// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages.ApplicationParts
{
    internal class ApplicationPartRegistry
    {
        // Page types that we could serve
        private static readonly Type _webPageType = typeof(WebPageRenderingBase);
        private readonly DictionaryBasedVirtualPathFactory _virtualPathFactory;
        private readonly ConcurrentDictionary<string, bool> _registeredVirtualPaths;
        private readonly ConcurrentDictionary<IResourceAssembly, ApplicationPart> _applicationParts;

        public ApplicationPartRegistry(DictionaryBasedVirtualPathFactory pathFactory)
        {
            _applicationParts = new ConcurrentDictionary<IResourceAssembly, ApplicationPart>();
            _registeredVirtualPaths = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            _virtualPathFactory = pathFactory;
        }

        public IEnumerable<ApplicationPart> RegisteredParts
        {
            get { return _applicationParts.Values; }
        }

        public ApplicationPart this[string name]
        {
            get { return _applicationParts.Values.FirstOrDefault(appPart => appPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); }
        }

        public ApplicationPart this[IResourceAssembly assembly]
        {
            get
            {
                ApplicationPart part;
                if (!_applicationParts.TryGetValue(assembly, out part))
                {
                    part = null;
                }
                return part;
            }
        }

        // Register an assembly as an application module, which makes its compiled web pages
        // and embedded resources available
        public void Register(ApplicationPart applicationPart)
        {
            Register(applicationPart, registerPageAction: null); // Use default action which creates a new page
        }

        // Register an assembly as an application module, which makes its compiled web pages
        // and embedded resources available
        internal void Register(ApplicationPart applicationPart, Func<object> registerPageAction)
        {
            // Throw if this assembly has been registered
            if (_applicationParts.ContainsKey(applicationPart.Assembly))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  WebPageResources.ApplicationPart_ModuleAlreadyRegistered, applicationPart.Assembly));
            }

            // Throw if the virtual path is already in use
            if (_registeredVirtualPaths.ContainsKey(applicationPart.RootVirtualPath))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  WebPageResources.ApplicationPart_ModuleAlreadyRegisteredForVirtualPath, applicationPart.RootVirtualPath));
            }

            // REVIEW: Should we register the app-part after we scan the assembly for webpages?
            // Add the part to the list
            if (_applicationParts.TryAdd(applicationPart.Assembly, applicationPart))
            {
                // We don't really care about the value
                _registeredVirtualPaths.TryAdd(applicationPart.RootVirtualPath, true);

                // Get all of the web page types
                var webPageTypes = from type in applicationPart.Assembly.GetTypes()
                                   where type.IsSubclassOf(_webPageType)
                                   select type;

                // Register each of page with the plan9
                foreach (Type webPageType in webPageTypes)
                {
                    RegisterWebPage(applicationPart, webPageType, registerPageAction);
                }
            }
        }

        private void RegisterWebPage(ApplicationPart module, Type webPageType, Func<object> registerPageAction)
        {
            var virtualPathAttribute = webPageType.GetCustomAttributes(typeof(PageVirtualPathAttribute), false)
                .Cast<PageVirtualPathAttribute>()
                .SingleOrDefault();

            // Ignore it if it doesn't have a PageVirtualPathAttribute
            if (virtualPathAttribute == null)
            {
                return;
            }

            // Get the path of the page relative to the module root
            string virtualPath = GetRootRelativeVirtualPath(module.RootVirtualPath, virtualPathAttribute.VirtualPath);

            // Create a factory for the page type
            Func<object> pageFactory = registerPageAction ?? NewTypeInstance(webPageType);

            // Register a page factory for it
            _virtualPathFactory.RegisterPath(virtualPath, pageFactory);
        }

        private static Func<object> NewTypeInstance(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
        }

        internal static string GetRootRelativeVirtualPath(string rootVirtualPath, string pageVirtualPath)
        {
            string virtualPath = pageVirtualPath;

            // Trim the ~/ prefix, since we want it to be relative to the module base path
            if (virtualPath.StartsWith("~/", StringComparison.Ordinal))
            {
                virtualPath = virtualPath.Substring(2);
            }

            if (!rootVirtualPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                rootVirtualPath += "/";
            }

            // Combine it with the root
            virtualPath = VirtualPathUtility.Combine(rootVirtualPath, virtualPath);
            return virtualPath;
        }
    }
}

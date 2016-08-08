// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using System.Web.WebPages.ApplicationParts;
using System.Web.WebPages.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages
{
    public class ApplicationPart
    {
        private const string ModuleRootSyntax = "@/";
        private const string ResourceVirtualPathRoot = "~/r.ashx/";
        private const string ResourceRoute = "r.ashx/{module}/{*path}";
        private static readonly LazyAction _initApplicationPart = new LazyAction(InitApplicationParts);
        private static ApplicationPartRegistry _partRegistry;
        private readonly Lazy<IDictionary<string, string>> _applicationPartResources;
        private readonly Lazy<string> _applicationPartName;

        public ApplicationPart(Assembly assembly, string rootVirtualPath)
            : this(new ResourceAssembly(assembly), rootVirtualPath)
        {
        }

        internal ApplicationPart(IResourceAssembly assembly, string rootVirtualPath)
        {
            if (String.IsNullOrEmpty(rootVirtualPath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "rootVirtualPath");
            }

            // Make sure the root path ends with a slash
            if (!rootVirtualPath.EndsWith("/", StringComparison.Ordinal))
            {
                rootVirtualPath += "/";
            }

            Assembly = assembly;
            RootVirtualPath = rootVirtualPath;
            _applicationPartResources = new Lazy<IDictionary<string, string>>(() => Assembly.GetManifestResourceNames().ToDictionary(key => key, key => key, StringComparer.OrdinalIgnoreCase));
            _applicationPartName = new Lazy<string>(() => Assembly.Name);
        }

        internal IResourceAssembly Assembly { get; private set; }

        internal string RootVirtualPath { get; private set; }

        internal string Name
        {
            get { return _applicationPartName.Value; }
        }

        internal IDictionary<string, string> ApplicationPartResources
        {
            get { return _applicationPartResources.Value; }
        }

        // REVIEW: Do we need an Unregister?
        // Register an assembly as an application module, which makes its compiled web pages
        // and embedded resources available
        public static void Register(ApplicationPart applicationPart)
        {
            // Ensure the registry is ready and the route handlers are set up
            _initApplicationPart.EnsurePerformed();
            Debug.Assert(_partRegistry != null, "Part registry should be initialized");

            _partRegistry.Register(applicationPart);
        }

        public static string ProcessVirtualPath(Assembly assembly, string baseVirtualPath, string virtualPath)
        {
            if (_partRegistry == null)
            {
                // This was called without registering a part.
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebPageResources.ApplicationPart_ModuleNotRegistered, assembly));
            }

            ApplicationPart applicationPart = _partRegistry[new ResourceAssembly(assembly)];
            if (applicationPart == null)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  WebPageResources.ApplicationPart_ModuleNotRegistered,
                                  assembly));
            }

            return applicationPart.ProcessVirtualPath(baseVirtualPath, virtualPath);
        }

        internal static IEnumerable<ApplicationPart> GetRegisteredParts()
        {
            _initApplicationPart.EnsurePerformed();
            return _partRegistry.RegisteredParts;
        }

        private string ProcessVirtualPath(string baseVirtualPath, string virtualPath)
        {
            virtualPath = ResolveVirtualPath(RootVirtualPath, baseVirtualPath, virtualPath);
            if (!virtualPath.StartsWith(RootVirtualPath, StringComparison.OrdinalIgnoreCase))
            {
                return virtualPath;
            }

            // Remove the root package path from the path, since the resource name doesn't use it
            // e.g. ~/admin/Debugger/Sub Folder/foo.jpg ==> ~/Sub Folder/foo.jpg
            string packageVirtualPath = "~/" + virtualPath.Substring(RootVirtualPath.Length);

            string resourceName = GetResourceNameFromVirtualPath(packageVirtualPath);

            // If the assembly doesn't contains that resource, don't change the path
            if (!ApplicationPartResources.ContainsKey(resourceName))
            {
                return virtualPath;
            }

            // The resource exists, so return a special path that will be handled by the resource handler
            return GetResourceVirtualPath(virtualPath);
        }

        /// <summary>
        /// Expands a virtual path by replacing a leading "@" with the application part root
        /// or combining it with the specified baseVirtualPath
        /// </summary>
        internal static string ResolveVirtualPath(string applicationRoot, string baseVirtualPath, string virtualPath)
        {
            // If it starts with @/, replace that with the package root
            // e.g. @/Sub Folder/foo.jpg ==> ~/admin/Debugger/Sub Folder/foo.jpg
            if (virtualPath.StartsWith(ModuleRootSyntax, StringComparison.OrdinalIgnoreCase))
            {
                return applicationRoot + virtualPath.Substring(ModuleRootSyntax.Length);
            }
            else
            {
                // Resolve if relative to the base
                return VirtualPathUtility.Combine(baseVirtualPath, virtualPath);
            }
        }

        internal Stream GetResourceStream(string virtualPath)
        {
            string resourceName = GetResourceNameFromVirtualPath(virtualPath);
            string normalizedResourceName;
            if (ApplicationPartResources.TryGetValue(resourceName, out normalizedResourceName))
            {
                // Return the resource stream
                return Assembly.GetManifestResourceStream(normalizedResourceName);
            }
            return null;
        }

        // Get the name of an embedded resource based on a virtual path
        private string GetResourceNameFromVirtualPath(string virtualPath)
        {
            return GetResourceNameFromVirtualPath(Name, virtualPath);
        }

        internal static string GetResourceNameFromVirtualPath(string moduleName, string virtualPath)
        {
            // Make sure path starts with ~/
            if (!virtualPath.StartsWith("~/", StringComparison.Ordinal))
            {
                virtualPath = "~/" + virtualPath;
            }

            // Get the directory part of the path
            // e.g. ~/Sub Folder/foo.jpg ==> ~/Sub Folder/
            string dir = VirtualPathUtility.GetDirectory(virtualPath);

            // Get rid of the starting ~/
            // e.g. ~/Sub Folder/ ==> Sub Folder/
            if (dir.Length >= 2)
            {
                dir = dir.Substring(2);
            }

            // Replace / with . and spaces with _
            // TODO: other special chars need to be replaced by _ as well
            // e.g. Sub Folder/ ==> Sub_Folder.
            dir = dir.Replace('/', '.');
            dir = dir.Replace(' ', '_');

            // Get the file name part.  That part of the resource names is the same as in the virtual path,
            // so no replacements are needed
            // e.g. ~/Sub Folder/foo.jpg ==> foo.jpg
            string fileName = Path.GetFileName(virtualPath);

            // Put them back together, and prepend the assembly name
            // e.g. DebuggerAssembly.Sub_Folder.foo.jpg
            return moduleName + "." + dir + fileName;
        }

        // Get a virtual path that uses the resource handler from a regular virtual path
        private string GetResourceVirtualPath(string virtualPath)
        {
            return GetResourceVirtualPath(Name, RootVirtualPath, virtualPath);
        }

        internal static string GetResourceVirtualPath(string moduleName, string moduleRoot, string virtualPath)
        {
            // The path should always start with the root of the module. Skip it.
            Debug.Assert(virtualPath.StartsWith(moduleRoot, StringComparison.OrdinalIgnoreCase));
            virtualPath = virtualPath.Substring(moduleRoot.Length).TrimStart('/');

            // Make a path to the resource through our resource route, e.g. ~/r.ashx/sub/foo.jpg
            // e.g. ~/admin/Debugger/Sub Folder/foo.jpg ==> ~/r.ashx/DebuggerPackageName/Sub Folder/foo.jpg
            return ResourceVirtualPathRoot + HttpUtility.UrlPathEncode(moduleName) + "/" + virtualPath;
        }

        private static void InitApplicationParts()
        {
            // Register the virtual path factory
            var virtualPathFactory = new DictionaryBasedVirtualPathFactory();
            VirtualPathFactoryManager.RegisterVirtualPathFactory(virtualPathFactory);

            // Intantiate the part registry
            _partRegistry = new ApplicationPartRegistry(virtualPathFactory);

            // Register the resource route
            RouteTable.Routes.Add(new Route(ResourceRoute, new ResourceRouteHandler(_partRegistry)));
        }
    }
}

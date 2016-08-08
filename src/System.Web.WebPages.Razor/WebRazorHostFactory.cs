// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.WebPages.Razor.Configuration;
using System.Web.WebPages.Razor.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Razor
{
    public class WebRazorHostFactory
    {
        private static ConcurrentDictionary<string, Func<WebRazorHostFactory>> _factories =
            new ConcurrentDictionary<string, Func<WebRazorHostFactory>>(StringComparer.OrdinalIgnoreCase);

        internal static Func<string, Type> TypeFactory = DefaultTypeFactory;

        public static WebPageRazorHost CreateDefaultHost(string virtualPath)
        {
            return CreateDefaultHost(virtualPath, null);
        }

        public static WebPageRazorHost CreateDefaultHost(string virtualPath, string physicalPath)
        {
            return CreateHostFromConfigCore(null, virtualPath, physicalPath);
        }

        public static WebPageRazorHost CreateHostFromConfig(string virtualPath)
        {
            return CreateHostFromConfig(virtualPath, null);
        }

        public static WebPageRazorHost CreateHostFromConfig(string virtualPath, string physicalPath)
        {
            if (String.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "virtualPath"), "virtualPath");
            }

            return CreateHostFromConfigCore(GetRazorSection(virtualPath), virtualPath, physicalPath);
        }

        public static WebPageRazorHost CreateHostFromConfig(RazorWebSectionGroup config, string virtualPath)
        {
            return CreateHostFromConfig(config, virtualPath, null);
        }

        public static WebPageRazorHost CreateHostFromConfig(RazorWebSectionGroup config, string virtualPath, string physicalPath)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (String.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "virtualPath"), "virtualPath");
            }

            return CreateHostFromConfigCore(config, virtualPath, physicalPath);
        }

        internal static WebPageRazorHost CreateHostFromConfigCore(RazorWebSectionGroup config, string virtualPath, string physicalPath)
        {
            // Use the virtual path to select a host environment for the generated code
            // Do this check here because the App_Code host can't be overridden.

            // Make the path app relative
            virtualPath = EnsureAppRelative(virtualPath);

            WebPageRazorHost host;
            if (virtualPath.StartsWith("~/App_Code", StringComparison.OrdinalIgnoreCase))
            {
                // Under App_Code => It's a Web Code file
                host = new WebCodeRazorHost(virtualPath, physicalPath);
            }
            else
            {
                WebRazorHostFactory factory = null;
                if (config != null && config.Host != null && !String.IsNullOrEmpty(config.Host.FactoryType))
                {
                    Func<WebRazorHostFactory> factoryCreator = _factories.GetOrAdd(config.Host.FactoryType, CreateFactory);
                    Debug.Assert(factoryCreator != null); // CreateFactory should throw if there's an error creating the factory
                    factory = factoryCreator();
                }

                host = (factory ?? new WebRazorHostFactory()).CreateHost(virtualPath, physicalPath);

                if (config != null && config.Pages != null)
                {
                    ApplyConfigurationToHost(config.Pages, host);
                }
            }

            return host;
        }

        private static Func<WebRazorHostFactory> CreateFactory(string typeName)
        {
            Type factoryType = TypeFactory(typeName);
            if (factoryType == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                                  RazorWebResources.Could_Not_Locate_FactoryType,
                                                                  typeName));
            }
            return Expression.Lambda<Func<WebRazorHostFactory>>(Expression.New(factoryType))
                .Compile();
        }

        public static void ApplyConfigurationToHost(RazorPagesSection config, WebPageRazorHost host)
        {
            host.DefaultPageBaseClass = config.PageBaseType;

            // Add imports
            foreach (string import in config.Namespaces.OfType<NamespaceInfo>().Select(ns => ns.Namespace))
            {
                host.NamespaceImports.Add(import);
            }
        }

        public virtual WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            return new WebPageRazorHost(virtualPath, physicalPath);
        }

        internal static RazorWebSectionGroup GetRazorSection(string virtualPath)
        {
            // Get the individual sections (we can only use GetSection in medium trust) and then reconstruct the section group
            return new RazorWebSectionGroup()
            {
                Host = (HostSection)WebConfigurationManager.GetSection(HostSection.SectionName, virtualPath),
                Pages = (RazorPagesSection)WebConfigurationManager.GetSection(RazorPagesSection.SectionName, virtualPath)
            };
        }

#if CODE_COVERAGE
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        // JUSTIFICATION: VirtualPathUtility.ToAppRelative is only available in ASP.Net environment
#endif

        private static string EnsureAppRelative(string virtualPath)
        {
            if (HostingEnvironment.IsHosted)
            {
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            }
            else
            {
                if (virtualPath.StartsWith("/", StringComparison.Ordinal))
                {
                    virtualPath = "~" + virtualPath;
                }
                else if (!virtualPath.StartsWith("~/", StringComparison.Ordinal))
                {
                    virtualPath = "~/" + virtualPath;
                }
            }
            return virtualPath;
        }

#if CODE_COVERAGE
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        // JUSTIFICATION: BuildManager.GetType is only available in ASP.Net environment
#endif

        private static Type DefaultTypeFactory(string typeName)
        {
            return BuildManager.GetType(typeName, false, false);
        }
    }
}

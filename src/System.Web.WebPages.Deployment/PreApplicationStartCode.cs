// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.WebPages.Deployment.Resources;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.Infrastructure;

namespace System.Web.WebPages.Deployment
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        /// <summary>
        /// Key used to indicate to tooling that the compile exception we throw to refresh the app domain originated from us so that they can deal with it correctly. 
        /// </summary>
        private const string ToolingIndicatorKey = "WebPages.VersionChange";

        // NOTE: Do not add public fields, methods, or other members to this class.
        // This class does not show up in Intellisense so members on it will not be
        // discoverable by users. Place new members on more appropriate classes that
        // relate to the public API (for example, a LoginUrl property should go on a
        // membership-related class).
        private static readonly IFileSystem _physicalFileSystem = new PhysicalFileSystem();

        private static bool _startWasCalled;

        public static void Start()
        {
            // Even though ASP.NET will only call each PreAppStart once, we sometimes internally call one PreAppStart from 
            // another PreAppStart to ensure that things get initialized in the right order. ASP.NET does not guarantee the 
            // order so we have to guard against multiple calls.
            // All Start calls are made on same thread, so no lock needed here.

            if (_startWasCalled)
            {
                return;
            }
            _startWasCalled = true;

            StartCore();
        }

        internal static bool StartCore()
        {
            var buildManager = new BuildManagerWrapper();
            NameValueCollection appSettings = WebConfigurationManager.AppSettings;
            Action<Version> loadWebPages = LoadWebPages;
            Action registerForChangeNotification = RegisterForChangeNotifications;
            IEnumerable<AssemblyName> loadedAssemblies = AssemblyUtils.GetLoadedAssemblies();

            return StartCore(_physicalFileSystem, HttpRuntime.AppDomainAppPath, HttpRuntime.BinDirectory, appSettings, loadedAssemblies,
                             buildManager, loadWebPages, registerForChangeNotification);
        }

        // Adds Parameter for unit tests
        internal static bool StartCore(IFileSystem fileSystem, string appDomainAppPath, string binDirectory, NameValueCollection appSettings, IEnumerable<AssemblyName> loadedAssemblies,
                                       IBuildManager buildManager, Action<Version> loadWebPages, Action registerForChangeNotification, Func<string, AssemblyName> getAssemblyNameThunk = null)
        {
            if (WebPagesDeployment.IsExplicitlyDisabled(appSettings))
            {
                // If WebPages is explicitly disabled, exit.
                Debug.WriteLine("WebPages Bootstrapper v{0}: not loading WebPages since it is disabled", AssemblyUtils.ThisAssemblyName.Version);
                return false;
            }

            Version maxWebPagesVersion = AssemblyUtils.GetMaxWebPagesVersion(loadedAssemblies);
            Debug.Assert(maxWebPagesVersion != null, "Function must return some max value.");
            if (AssemblyUtils.ThisAssemblyName.Version != maxWebPagesVersion)
            {
                // Always let the highest version determine what needs to be done. This would make future proofing simpler.
                Debug.WriteLine("WebPages Bootstrapper v{0}: Higher version v{1} is available.", AssemblyUtils.ThisAssemblyName.Version, maxWebPagesVersion);
                return false;
            }

            var webPagesEnabled = WebPagesDeployment.IsEnabled(fileSystem, appDomainAppPath, appSettings);
            Version binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, fileSystem, getAssemblyNameThunk);
            Version configVersion = WebPagesDeployment.GetVersionFromConfig(appSettings);
            Version version = configVersion ?? binVersion ?? AssemblyUtils.WebPagesV1Version;

            // Asserts to ensure unit tests are set up correctly. So essentially, we're unit testing the unit tests. 
            Debug.Assert(version != null, "GetVersion always returns a version");
            Debug.Assert(binVersion == null || binVersion <= maxWebPagesVersion, "binVersion cannot be higher than max version");

            if ((binVersion != null) && (binVersion != version))
            {
                // Determine if there's a version conflict. A conflict could occur if there's a version specified in the bin which is different from the version specified in the 
                // config that is different.
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.WebPagesVersionConflict, version, binVersion));
            }
            else if (binVersion != null)
            {
                // The rest of the code is only meant to be executed if we are executing from the GAC.
                // If a version is bin deployed, we don't need to do anything special to bootstrap.
                return false;
            }
            else if (!webPagesEnabled)
            {
                Debug.WriteLine("WebPages Bootstrapper v{0}: WebPages not enabled, registering for change notifications", AssemblyUtils.ThisAssemblyName.Version);
                // Register for change notifications under the application root
                registerForChangeNotification();
                return false;
            }
            else if (!AssemblyUtils.IsVersionAvailable(loadedAssemblies, version))
            {
                if (version == AssemblyUtils.WebPagesV1Version && configVersion == null && binVersion == null)
                {
                    // No version was specified. We're implicitly assuming that the site is a v1 site. However, the user does not have V1 binaries available. 
                    throw new InvalidOperationException(ConfigurationResources.WebPagesImplicitVersionFailure);
                }
                else 
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.WebPagesVersionNotFound, version, AssemblyUtils.ThisAssemblyName.Version));
                }
            }

            Debug.WriteLine("WebPages Bootstrapper v{0}: loading version {1}, loading WebPages", AssemblyUtils.ThisAssemblyName.Version, version);
            // If the version the application was compiled earlier was different, invalidate compilation results by adding a file to the bin.
            InvalidateCompilationResultsIfVersionChanged(buildManager, fileSystem, binDirectory, version);
            loadWebPages(version);
            return true;
        }

        /// <summary>
        /// WebPages stores the version to be compiled against in AppSettings as &gt;add key="webpages:version" value="1.0" /&lt;. 
        /// Changing values AppSettings does not cause recompilation therefore we could run into a state where we have files compiled against v1 but the application is 
        /// currently v2.
        /// </summary>
        private static void InvalidateCompilationResultsIfVersionChanged(IBuildManager buildManager, IFileSystem fileSystem, string binDirectory, Version currentVersion)
        {
            Version previousVersion = WebPagesDeployment.GetPreviousRuntimeVersion(buildManager);

            // Persist the current version number in BuildManager's cached file
            WebPagesDeployment.PersistRuntimeVersion(buildManager, currentVersion);

            if (previousVersion == null)
            {
                // Do nothing.
            }
            else if (previousVersion != currentVersion)
            {
                // If the previous runtime version is different, perturb the bin directory so that it forces recompilation.
                WebPagesDeployment.ForceRecompile(fileSystem, binDirectory);
                var httpCompileException = new HttpCompileException(ConfigurationResources.WebPagesVersionChanges);
                // Indicator for tooling
                httpCompileException.Data[ToolingIndicatorKey] = true;
                throw httpCompileException;
            }
        }

        // Copied from xsp\System\Web\Compilation\BuildManager.cs
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Copied from System.Web.dll")]
        internal static ICollection<MethodInfo> GetPreStartInitMethodsFromAssemblyCollection(IEnumerable<Assembly> assemblies)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (Assembly assembly in assemblies)
            {
                PreApplicationStartMethodAttribute[] attributes = null;
                try
                {
                    attributes = (PreApplicationStartMethodAttribute[])assembly.GetCustomAttributes(typeof(PreApplicationStartMethodAttribute), inherit: true);
                }
                catch
                {
                    // GetCustomAttributes invokes the constructors of the attributes, so it is possible that they might throw unexpected exceptions.
                    // (Dev10 bug 831981)
                }

                if (attributes != null && attributes.Length != 0)
                {
                    Debug.Assert(attributes.Length == 1);
                    PreApplicationStartMethodAttribute attribute = attributes[0];
                    Debug.Assert(attribute != null);

                    MethodInfo method = null;
                    // Ensure the Type on the attribute is in the same assembly as the attribute itself
                    if (attribute.Type != null && !String.IsNullOrEmpty(attribute.MethodName) && attribute.Type.Assembly == assembly)
                    {
                        method = FindPreStartInitMethod(attribute.Type, attribute.MethodName);
                    }

                    if (method != null)
                    {
                        methods.Add(method);
                    }

                    // No-op if the attribute is invalid
                    /*
                    else {
                        throw new HttpException(SR.GetString(SR.Invalid_PreApplicationStartMethodAttribute_value,
                            assembly.FullName,
                            (attribute.Type != null ? attribute.Type.FullName : String.Empty),
                            attribute.MethodName));
                    }
                    */
                }
            }
            return methods;
        }

        // Copied from xsp\System\Web\Compilation\BuildManager.cs
        internal static MethodInfo FindPreStartInitMethod(Type type, string methodName)
        {
            Debug.Assert(type != null);
            Debug.Assert(!String.IsNullOrEmpty(methodName));
            MethodInfo method = null;
            if (type.IsPublic)
            {
                // Verify that type is public to avoid allowing internal code execution. This implementation will not match
                // nested public types.
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                                        binder: null,
                                        types: Type.EmptyTypes,
                                        modifiers: null);
            }
            return method;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The cache disposes of the dependency")]
        private static void RegisterForChangeNotifications()
        {
            string physicalPath = HttpRuntime.AppDomainAppPath;

            CacheDependency cacheDependency = new CacheDependency(physicalPath, DateTime.UtcNow);
            var key = WebPagesDeployment.CacheKeyPrefix + physicalPath;

            HttpRuntime.Cache.Insert(key, physicalPath, cacheDependency,
                                     Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                     CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnChanged));
        }

        private static void OnChanged(string key, object value, CacheItemRemovedReason reason)
        {
            // Only handle case when the dependency has changed.
            if (reason != CacheItemRemovedReason.DependencyChanged)
            {
                return;
            }

            // Scan the app root for a webpages file
            if (WebPagesDeployment.AppRootContainsWebPagesFile(_physicalFileSystem, HttpRuntime.AppDomainAppPath))
            {
                // Unload the app domain so we register plan9 when the app restarts
                InfrastructureHelper.UnloadAppDomain();
            }
            else
            {
                // We need to re-register since the item was removed from the cache
                RegisterForChangeNotifications();
            }
        }

        private static void LoadWebPages(Version version)
        {
            IEnumerable<AssemblyName> assemblyList = AssemblyUtils.GetAssembliesForVersion(version);
            var assemblies = assemblyList.Select(LoadAssembly);

            foreach (var asm in assemblies)
            {
                BuildManager.AddReferencedAssembly(asm);
            }

            foreach (var m in GetPreStartInitMethodsFromAssemblyCollection(assemblies))
            {
                m.Invoke(null, null);
            }
        }

        private static Assembly LoadAssembly(AssemblyName name)
        {
            return Assembly.Load(name);
        }
    }
}

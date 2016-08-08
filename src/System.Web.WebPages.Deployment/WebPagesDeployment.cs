// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.WebPages.Deployment.Resources;
using Microsoft.Internal.Web.Utils;
using Microsoft.Win32;

namespace System.Web.WebPages.Deployment
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebPagesDeployment
    {
        private const string AppSettingsVersionKey = "webpages:Version";
        private const string AppSettingsEnabledKey = "webpages:Enabled";

        /// <summary>
        /// File name for a temporary file that we drop in bin to force recompilation.
        /// </summary>
        private const string ForceRecompilationFile = "WebPagesRecompilation.deleteme";

        private const string WebPagesRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET Web Pages\v{0}.{1}";
        internal static readonly string CacheKeyPrefix = "__System.Web.WebPages.Deployment__";
        private static readonly string[] _webPagesExtensions = new[] { ".cshtml", ".vbhtml" };
        private static readonly object _installPathNotFound = new object();
        private static readonly IFileSystem _fileSystem = new PhysicalFileSystem();

        /// <param name="path">Physical or virtual path to a directory where we need to determine the version of WebPages to be used.</param>
        /// <remarks>
        /// In a non-hosted scenario, this method would only look at a web.config that is present at the current path. Any config settings at an
        /// ancestor directory would not be considered.
        /// If we are unable to determine a version, we would assume that this is a v1 app.
        /// </remarks>
        public static Version GetVersionWithoutEnabledCheck(string path)
        {
            return GetVersionWithoutEnabledCheckInternal(path, AssemblyUtils.WebPagesV1Version);
        }

        public static Version GetExplicitWebPagesVersion(string path)
        {
            return GetVersionWithoutEnabledCheckInternal(path, defaultVersion: null);
        }

        [Obsolete("This method is obsolete and is meant for legacy code. Use GetVersionWithoutEnabled instead.")]
        public static Version GetVersion(string path)
        {
            return GetObsoleteVersionInternal(path, GetAppSettings(path), new PhysicalFileSystem());
        }

        /// <remarks>
        /// This is meant to test an obsolete method. Don't use this!
        /// </remarks>
        internal static Version GetObsoleteVersionInternal(string path, NameValueCollection configuration, IFileSystem fileSystem)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }

            var binDirectory = GetBinDirectory(path);
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, _fileSystem);
            var version = GetVersionInternal(configuration, binVersion, defaultVersion: null);

            if (version != null)
            {
                // If a webpages version is available in config or bin, return it.
                return version;
            }
            else if (AppRootContainsWebPagesFile(fileSystem, path))
            {
                // If the path points to a WebPages site, return v1 as a fixed version.
                return AssemblyUtils.WebPagesV1Version;
            }
            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This operation might be expensive since it has to reflect over Assembly names.")]
        public static Version GetMaxVersion()
        {
            return AssemblyUtils.GetMaxWebPagesVersion();
        }

        /// <summary>
        /// Determines if Asp.Net Web Pages is enabled.
        /// Web Pages is enabled if there's a webPages:Enabled key in AppSettings is set to "true" or if there's a cshtml file in the current path
        /// and the key is not present.
        /// </summary>
        /// <param name="path">The path at which to determine if web pages is enabled.</param>
        /// <remarks>
        /// In a non-hosted scenario, this method would only look at a web.config that is present at the current path. Any config settings at an
        /// ancestor directory would not be considered.
        /// </remarks>
        public static bool IsEnabled(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }

            return IsEnabled(_fileSystem, path, GetAppSettings(path));
        }

        /// <remarks>
        /// In a non-hosted scenario, this method would only look at a web.config that is present at the current path. Any config settings at an
        /// ancestor directory would not be considered.
        /// </remarks>
        public static bool IsExplicitlyDisabled(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }
            return IsExplicitlyDisabled(GetAppSettings(path));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDictionary<string, Version> GetIncompatibleDependencies(string appPath)
        {
            if (String.IsNullOrEmpty(appPath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "appPath");
            }
            var configFilePath = Path.Combine(appPath, "web.config");

            var assemblyReferences = AppDomainHelper.GetBinAssemblyReferences(appPath, configFilePath);
            return AssemblyUtils.GetAssembliesMatchingOtherVersions(assemblyReferences);
        }

        internal static bool IsExplicitlyDisabled(NameValueCollection appSettings)
        {
            bool? enabled = GetEnabled(appSettings);
            return enabled.HasValue && enabled.Value == false;
        }

        internal static bool IsEnabled(IFileSystem fileSystem, string path, NameValueCollection appSettings)
        {
            bool? enabled = GetEnabled(appSettings);
            if (!enabled.HasValue)
            {
                return AppRootContainsWebPagesFile(fileSystem, path);
            }
            return enabled.Value;
        }

        /// <summary>
        /// Returns the value for webPages:Enabled AppSetting value in web.config.
        /// </summary>
        private static bool? GetEnabled(NameValueCollection appSettings)
        {
            string enabledSetting = appSettings.Get(AppSettingsEnabledKey);
            if (String.IsNullOrEmpty(enabledSetting))
            {
                return null;
            }
            else
            {
                return Boolean.Parse(enabledSetting);
            }
        }

        /// <summary>
        /// Returns the version of WebPages to be used for a specified path.
        /// </summary>
        /// <remarks>
        /// This method would always returns a value regardless of web pages is explicitly disabled (via config) or implicitly disabled (by virtue of not having a cshtml file) at 
        /// the specified path.
        /// </remarks>
        internal static Version GetVersionInternal(NameValueCollection appSettings, Version binVersion, Version defaultVersion)
        {
            // Return version values with the following precedence: 
            // 1) Version in config
            // 2) Version in bin
            // 3) defaultVersion.
            return GetVersionFromConfig(appSettings) ?? binVersion ?? defaultVersion;
        }

        private static Version GetVersionWithoutEnabledCheckInternal(string path, Version defaultVersion)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }

            var binDirectory = GetBinDirectory(path);
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, _fileSystem);

            return GetVersionInternal(GetAppSettings(path), binVersion, defaultVersion);
        }

        /// <summary>
        /// Gets full path to a folder that contains ASP.NET WebPages assemblies for a given version. Used by
        /// WebMatrix and Visual Studio so they know what to copy to an app's Bin folder or deploy to a hoster. 
        /// </summary>
        public static string GetAssemblyPath(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            string webPagesRegistryKey = String.Format(CultureInfo.InvariantCulture, WebPagesRegistryKey, version.Major, version.Minor);

            object installPath = Registry.GetValue(webPagesRegistryKey, "InstallPath", _installPathNotFound);

            if (installPath == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                                  ConfigurationResources.WebPagesRegistryKeyDoesNotExist, webPagesRegistryKey));
            }
            else if (installPath == _installPathNotFound)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                                  ConfigurationResources.InstallPathNotFound, webPagesRegistryKey));
            }

            return Path.Combine((string)installPath, "Assemblies");
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This operation might be expensive since it has to reflect over Assembly names.")]
        public static IEnumerable<AssemblyName> GetWebPagesAssemblies()
        {
            return AssemblyUtils.GetAssembliesForVersion(AssemblyUtils.ThisAssemblyName.Version);
        }

        private static NameValueCollection GetAppSettings(string path)
        {
            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                // Path is virtual, assume we're hosted
                return (NameValueCollection)WebConfigurationManager.GetSection("appSettings", path);
            }
            else
            {
                // Path is physical, map it to an application
                WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
                fileMap.VirtualDirectories.Add("/", new VirtualDirectoryMapping(path, true));
                var config = WebConfigurationManager.OpenMappedWebConfiguration(fileMap, "/");

                var appSettingsSection = config.AppSettings;
                var appSettings = new NameValueCollection();

                foreach (KeyValueConfigurationElement element in appSettingsSection.Settings)
                {
                    appSettings.Add(element.Key, element.Value);
                }
                return appSettings;
            }
        }

        internal static Version GetVersionFromConfig(NameValueCollection appSettings)
        {
            string version = appSettings.Get(AppSettingsVersionKey);
            // Version will be null if the config section is registered but not present in app web.config.
            if (!String.IsNullOrEmpty(version))
            {
                // Build and Revision are optional in config but required by Fusion, so we set them to 0 if unspecified in config.
                // Valid in config: "1.0", "1.0.0", "1.0.0.0"
                var fullVersion = new Version(version);
                if (fullVersion.Build == -1 || fullVersion.Revision == -1)
                {
                    fullVersion = new Version(fullVersion.Major, fullVersion.Minor,
                                              fullVersion.Build == -1 ? 0 : fullVersion.Build,
                                              fullVersion.Revision == -1 ? 0 : fullVersion.Revision);
                }
                return fullVersion;
            }
            return null;
        }

        internal static bool AppRootContainsWebPagesFile(IFileSystem fileSystem, string path)
        {
            var files = fileSystem.EnumerateFiles(path);
            return files.Any(IsWebPagesFile);
        }

        private static bool IsWebPagesFile(string file)
        {
            var extension = Path.GetExtension(file);
            return _webPagesExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// HttpRuntime.BinDirectory is unavailable in design time and throws if we try to access it. To workaround this, if we aren't hosted,
        /// we will assume that the path that was passed to us is the application root.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetBinDirectory(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                return HttpRuntime.BinDirectory;
            }
            return Path.Combine(path, "bin");
        }

        /// <summary>
        /// Reads a previously persisted version number from build manager's cached directory.
        /// </summary>
        /// <returns>Null if a previous version number does not exist or is not a valid version number, read version number otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to throw an exception from this method.")]
        internal static Version GetPreviousRuntimeVersion(IBuildManager buildManagerFileSystem)
        {
            string fileName = GetCachedFileName();
            try
            {
                Stream stream = buildManagerFileSystem.ReadCachedFile(fileName);
                if (stream == null)
                {
                    return null;
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    string text = reader.ReadLine();
                    Version version;
                    if (Version.TryParse(text, out version))
                    {
                        return version;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// Persists the version number in a file under the build manager's cached directory.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to throw an exception from this method.")]
        internal static void PersistRuntimeVersion(IBuildManager buildManager, Version version)
        {
            string fileName = GetCachedFileName();
            try
            {
                Stream stream = buildManager.CreateCachedFile(fileName);
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(version.ToString());
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Forces recompilation of the application by dropping a file under bin.
        /// </summary>
        /// <param name="fileSystem">File system instance used to write a file to bin directory.</param>
        /// <param name="binDirectory">Path to bin directory of the application</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to throw an exception from this method.")]
        internal static void ForceRecompile(IFileSystem fileSystem, string binDirectory)
        {
            var fileToWrite = Path.Combine(binDirectory, ForceRecompilationFile);
            try
            {
                // Note: We should use BuildManager::ForceRecompile once that method makes it into System.Web.
                using (var writer = new StreamWriter(fileSystem.OpenFile(fileToWrite)))
                {
                    writer.WriteLine();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Name of the the temporary file used by BuildManager.CreateCachedFile / BuildManager.ReadCachedFile where we cache WebPages's version number. 
        /// </summary>
        /// <returns></returns>
        private static string GetCachedFileName()
        {
            return typeof(WebPagesDeployment).Namespace;
        }

        private static string RemoveTrailingSlash(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar);
            }
            return path;
        }
    }
}

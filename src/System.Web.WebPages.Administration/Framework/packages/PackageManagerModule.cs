// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Hosting;

namespace System.Web.WebPages.Administration.PackageManager
{
    internal static class PackageManagerModule
    {
        private const string DefaultSourceUrl = @"http://go.microsoft.com/fwlink/?LinkID=226946";
        private const string NuGetSourceUrl = @"http://go.microsoft.com/fwlink/?LinkID=226948";
        internal static readonly string PackageSourceFilePath = VirtualPathUtility.Combine(SiteAdmin.AdminSettingsFolder, "PackageSources.config");
        private static readonly object _sourceFileLock = new object();

        private static readonly IEnumerable<WebPackageSource> _defaultSources = new[]
        {
            new WebPackageSource(name: PackageManagerResources.DefaultPackageSourceName, source: DefaultSourceUrl) { FilterPreferredPackages = true },
            new WebPackageSource(name: PackageManagerResources.NuGetFeed, source: NuGetSourceUrl) { FilterPreferredPackages = false }
        };

        private static readonly PackageSourceFile _sourceFile = new PackageSourceFile(PackageSourceFilePath);
        private static ISet<WebPackageSource> _packageSources;

        public static IEnumerable<WebPackageSource> PackageSources
        {
            get
            {
                Debug.Assert(_packageSources != null, "InitFeedsFile must be called before Feeds can be accessed.");
                return _packageSources.Any() ? _packageSources : _defaultSources;
            }
        }

        /// <summary>
        /// Gets the first available PackageSource
        /// </summary>
        public static WebPackageSource ActiveSource
        {
            get { return PackageSources.First(); }
        }

        public static IEnumerable<WebPackageSource> DefaultSources
        {
            get { return _defaultSources; }
        }

        public static string ModuleName
        {
            get { return PackageManagerResources.ModuleTitle; }
        }

        public static string ModuleDescription
        {
            get { return PackageManagerResources.ModuleDesc; }
        }

        internal static string SiteRoot
        {
            get { return HostingEnvironment.MapPath("~/"); }
        }

        internal static bool Available
        {
            get { return (HttpContext.Current != null) && HttpContext.Current.Request.IsLocal; }
        }

        public static bool InitPackageSourceFile()
        {
            return InitPackageSourceFile(_sourceFile, ref _packageSources);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any exception that occurs indicates that the source file cannot be initialized.")]
        public static bool InitPackageSourceFile(IPackagesSourceFile sourceFile, ref ISet<WebPackageSource> packageSources)
        {
            if (packageSources != null)
            {
                return true;
            }
            try
            {
                lock (_sourceFileLock)
                {
                    // This method is invoked from Page_Start and ensures we have a feed file to read/write. 
                    // The call needs to be guarded against multiple simultaneous requests.
                    if (!sourceFile.Exists())
                    {
                        packageSources = new HashSet<WebPackageSource>(_defaultSources);
                        sourceFile.WriteSources(_packageSources);
                    }
                    else
                    {
                        packageSources = new HashSet<WebPackageSource>(sourceFile.ReadSources());
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static WebPackageSource GetSource(string sourceName)
        {
            Debug.Assert(_packageSources != null, "InitFeedsFile must be called before this method can be called.");
            return GetSource(PackageSources, sourceName);
        }

        public static WebPackageSource GetSource(IEnumerable<WebPackageSource> packageSources, string sourceName)
        {
            lock (_sourceFileLock)
            {
                return packageSources.Where(source => source.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
        }

        public static bool AddPackageSource(string source, string name)
        {
            Debug.Assert(_packageSources != null, "InitFeedsFile must be called before this method can be called.");
            name = String.IsNullOrEmpty(name) ? source.ToString() : name;
            var packageSource = new WebPackageSource(source: source, name: name);
            return AddPackageSource(_sourceFile, _packageSources, packageSource);
        }

        public static bool AddPackageSource(WebPackageSource packageSource)
        {
            Debug.Assert(!String.IsNullOrEmpty(packageSource.Name) && !String.IsNullOrEmpty(packageSource.Source));
            return AddPackageSource(_sourceFile, _packageSources, packageSource);
        }

        public static bool AddPackageSource(IPackagesSourceFile sourceFile, ISet<WebPackageSource> packageSources, WebPackageSource packageSource)
        {
            if (GetSource(packageSources, packageSource.Name) != null)
            {
                return false;
            }
            lock (_sourceFileLock)
            {
                packageSources.Add(packageSource);
                sourceFile.WriteSources(packageSources);
            }
            return true;
        }

        public static void RemovePackageSource(string sourceName)
        {
            Debug.Assert(_packageSources != null, "InitFeedsFile must be called before this method can be called.");
            RemovePackageSource(_sourceFile, _packageSources, sourceName);
        }

        public static void RemovePackageSource(IPackagesSourceFile sourceFile, ISet<WebPackageSource> packageSources, string name)
        {
            var packageSource = GetSource(packageSources, name);
            lock (_sourceFileLock)
            {
                if (packageSource == null)
                {
                    return;
                }
                packageSources.Remove(packageSource);
                sourceFile.WriteSources(packageSources);
            }
        }
    }
}

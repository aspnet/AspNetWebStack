// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Internal.Web.Utils;
using NuGet;
using NuGet.Runtime;

namespace System.Web.WebPages.Administration.PackageManager
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WebProjectManager : IWebProjectManager
    {
        private const string WebPagesPreferredTag = " aspnetwebpages ";
        private readonly IProjectManager _projectManager;
        private readonly string _siteRoot;

        public WebProjectManager(string remoteSource, string siteRoot)
        {
            if (String.IsNullOrEmpty(remoteSource))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "remoteSource");
            }
            if (String.IsNullOrEmpty(siteRoot))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "siteRoot");
            }

            _siteRoot = siteRoot;
            string webRepositoryDirectory = GetWebRepositoryDirectory(siteRoot);
            _projectManager = new ProjectManager(sourceRepository: PackageRepositoryFactory.Default.CreateRepository(remoteSource),
                                                 pathResolver: new DefaultPackagePathResolver(webRepositoryDirectory),
                                                 localRepository: PackageRepositoryFactory.Default.CreateRepository(webRepositoryDirectory),
                                                 project: new WebProjectSystem(siteRoot));
        }

        internal WebProjectManager(IProjectManager projectManager, string siteRoot)
        {
            if (String.IsNullOrEmpty(siteRoot))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "siteRoot");
            }

            if (projectManager == null)
            {
                throw new ArgumentNullException("projectManager");
            }

            _siteRoot = siteRoot;
            _projectManager = projectManager;
        }

        public IPackageRepository LocalRepository
        {
            get { return _projectManager.LocalRepository; }
        }

        public IPackageRepository SourceRepository
        {
            get { return _projectManager.SourceRepository; }
        }

        internal bool DoNotAddBindingRedirects { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#",
            Justification = "We want to ensure we get server-side counts for the IQueryable which can only be performed before we collapse versions.")]
        public virtual IQueryable<IPackage> GetRemotePackages(string searchTerms, bool filterPreferred)
        {
            var packages = GetPackages(SourceRepository, searchTerms);
            if (filterPreferred)
            {
                packages = packages.Where(p => p.Tags.ToLower().Contains(WebPagesPreferredTag));
            }

            // Order by download count and Id to allow collapsing 
            return packages.OrderByDescending(p => p.DownloadCount)
                .ThenBy(p => p.Id);
        }

        public IQueryable<IPackage> GetInstalledPackages(string searchTerms)
        {
            return GetPackages(LocalRepository, searchTerms);
        }

        public IEnumerable<IPackage> GetPackagesWithUpdates(string searchTerms, bool filterPreferredPackages)
        {
            var packagesToUpdate = GetPackages(LocalRepository, searchTerms);
            if (filterPreferredPackages)
            {
                packagesToUpdate = packagesToUpdate.Where(p => !String.IsNullOrEmpty(p.Tags) && p.Tags.ToLower().Contains(WebPagesPreferredTag));
            }
            return SourceRepository.GetUpdates(packagesToUpdate, includePrerelease: false).AsQueryable();
        }

        internal IEnumerable<string> InstallPackage(IPackage package)
        {
            return InstallPackage(package, AppDomain.CurrentDomain);
        }

        /// <summary>
        /// Installs and adds a package reference to the project
        /// </summary>
        /// <returns>Warnings encountered when installing the package.</returns>
        public IEnumerable<string> InstallPackage(IPackage package, AppDomain appDomain)
        {
            IEnumerable<string> result = PerformLoggedAction(() =>
            {
                _projectManager.AddPackageReference(package.Id, package.Version, ignoreDependencies: false, allowPrereleaseVersions: false);
                AddBindingRedirects(appDomain);
            });
            return result;
        }

        internal IEnumerable<string> UpdatePackage(IPackage package)
        {
            return UpdatePackage(package, AppDomain.CurrentDomain);
        }

        /// <summary>
        /// Updates a package reference. Installs the package to the App_Data repository if it does not already exist.
        /// </summary>
        /// <returns>Warnings encountered when updating the package.</returns>
        public IEnumerable<string> UpdatePackage(IPackage package, AppDomain appDomain)
        {
            return PerformLoggedAction(() =>
            {
                _projectManager.UpdatePackageReference(package.Id, package.Version, updateDependencies: true, allowPrereleaseVersions: false);
                AddBindingRedirects(appDomain);
            });
        }

        /// <summary>
        /// Removes a package reference and uninstalls the package
        /// </summary>
        /// <returns>Warnings encountered when uninstalling the package.</returns>
        public IEnumerable<string> UninstallPackage(IPackage package, bool removeDependencies)
        {
            return PerformLoggedAction(() =>
            {
                _projectManager.RemovePackageReference(package.Id, forceRemove: false, removeDependencies: removeDependencies);
            });
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "It seems more appropriate to deal with IPackages")]
        public bool IsPackageInstalled(IPackage package)
        {
            return LocalRepository.Exists(package);
        }

        public IPackage GetUpdate(IPackage package)
        {
            return SourceRepository.GetUpdates(new[] { package }, includePrerelease: false).SingleOrDefault();
        }

        private void AddBindingRedirects(AppDomain appDomain)
        {
            if (DoNotAddBindingRedirects)
            {
                return;
            }
            // We can't use HttpRuntime.BinDirectory since there is no runtime when installing via WebMatrix.
            var binDirectory = Path.Combine(_siteRoot, "bin");
            var assemblies = RemoteAssembly.GetAssembliesForBindingRedirect(appDomain, binDirectory);
            var bindingRedirects = BindingRedirectResolver.GetBindingRedirects(assemblies);

            if (bindingRedirects.Any())
            {
                // NuGet ends up reading our web.config file regardless of if any bindingRedirects are needed.
                var bindingRedirectManager = new BindingRedirectManager(_projectManager.Project, "web.config");
                bindingRedirectManager.AddBindingRedirects(bindingRedirects);
            }
        }

        private IEnumerable<string> PerformLoggedAction(Action action)
        {
            ErrorLogger logger = new ErrorLogger();
            _projectManager.Logger = logger;
            try
            {
                action();
            }
            finally
            {
                _projectManager.Logger = null;
            }
            return logger.Errors;
        }

        /// <remarks>
        /// Ensure that some form of sorting is applied to the IQueryable before this method is invoked.
        /// </remarks>
        /// <returns>A sequence with the most recent version for each package.</returns>
        public static IEnumerable<IPackage> CollapseVersions(IQueryable<IPackage> packages)
        {
            const int BufferSize = 30;
            return packages.Where(package => package.IsLatestVersion)
                .AsBufferedEnumerable(BufferSize)
                .DistinctLast(PackageEqualityComparer.Id, PackageComparer.Version);
        }

        internal IEnumerable<IPackage> GetPackagesRequiringLicenseAcceptance(IPackage package)
        {
            return GetPackagesRequiringLicenseAcceptance(package, localRepository: LocalRepository, sourceRepository: SourceRepository);
        }

        internal static IEnumerable<IPackage> GetPackagesRequiringLicenseAcceptance(IPackage package, IPackageRepository localRepository, IPackageRepository sourceRepository)
        {
            var dependencies = GetPackageDependencies(package, localRepository, sourceRepository);

            return from p in dependencies
                   where p.RequireLicenseAcceptance
                   select p;
        }

        private static IEnumerable<IPackage> GetPackageDependencies(IPackage package, IPackageRepository localRepository, IPackageRepository sourceRepository)
        {
            InstallWalker walker = new InstallWalker(localRepository: localRepository, sourceRepository: sourceRepository, logger: NullLogger.Instance,
                                                     ignoreDependencies: false, allowPrereleaseVersions: false);
            IEnumerable<PackageOperation> operations = walker.ResolveOperations(package);

            return from operation in operations
                   where operation.Action == PackageAction.Install
                   select operation.Package;
        }

        internal static IQueryable<IPackage> GetPackages(IPackageRepository repository, string searchTerm)
        {
            return GetPackages(repository.GetPackages(), searchTerm);
        }

        internal static IQueryable<IPackage> GetPackages(IQueryable<IPackage> packages, string searchTerm)
        {
            if (!String.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                packages = packages.Find(searchTerm);
            }
            return packages;
        }

        internal static string GetWebRepositoryDirectory(string siteRoot)
        {
            return Path.Combine(siteRoot, "App_Data", "packages");
        }

        private class ErrorLogger : ILogger
        {
            private readonly IList<string> _errors = new List<string>();

            public IEnumerable<string> Errors
            {
                get { return _errors; }
            }

            public void Log(MessageLevel level, string message, params object[] args)
            {
                if (level == MessageLevel.Warning)
                {
                    _errors.Add(String.Format(CultureInfo.CurrentCulture, message, args));
                }
            }
        }
    }
}

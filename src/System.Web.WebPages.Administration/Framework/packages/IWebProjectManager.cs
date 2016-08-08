// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NuGet;

namespace System.Web.WebPages.Administration.PackageManager
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IWebProjectManager
    {
        /// <summary>
        /// The repository where packages are installed.
        /// </summary>
        IPackageRepository LocalRepository { get; }

        /// <summary>
        /// Remote feed to fetch packages from. 
        /// </summary>
        IPackageRepository SourceRepository { get; }

        /// <summary>
        /// Gets packages from the SourceRepository
        /// </summary>
        /// <param name="searchTerms">One or more terms separated by a whitespace used to filter packages.</param>
        /// <param name="filterPreferred">Determines if packages are filtered by tag identifying packages for Asp.Net WebPages.</param>
        IQueryable<IPackage> GetRemotePackages(string searchTerms, bool filterPreferred);

        /// <summary>
        /// Gets packages from the LocalRepository.
        /// </summary>
        /// <param name="searchTerms">One or more terms separated by a whitespace used to filter packages.</param>
        IQueryable<IPackage> GetInstalledPackages(string searchTerms);

        /// <summary>
        /// Gets packages from the RemoteRepository that are updates to installed packages.
        /// </summary>
        /// <param name="searchTerms">One or more terms separated by a whitespace used to filter packages.</param>
        /// <param name="filterPreferredPackages"></param>
        IEnumerable<IPackage> GetPackagesWithUpdates(string searchTerms, bool filterPreferredPackages);

        /// <summary>
        /// Installs the package to the LocalRepository.
        /// </summary>
        /// <param name="package">The package to be installed.</param>
        /// <param name="appDomain">The AppDomain that is used to determine binding redirects. If null, the current AppDomain would be used.</param>
        /// <returns>A sequence of errors that occurred during the operation.</returns>
        IEnumerable<string> InstallPackage(IPackage package, AppDomain appDomain);

        /// <summary>
        /// Updates the package in the LocalRepository.
        /// </summary>
        /// <param name="package">The package to be installed.</param>
        /// <param name="appDomain">The AppDomain that is used to determine binding redirects. If null, the current AppDomain would be used.</param>
        /// <returns>A sequence of errors that occurred during the operation.</returns>
        IEnumerable<string> UpdatePackage(IPackage package, AppDomain appDomain);

        /// <summary>
        /// Removes the package from the LocalRepository.
        /// </summary>
        /// <returns>A sequence of errors that occurred during the operation.</returns>
        IEnumerable<string> UninstallPackage(IPackage package, bool removeDependencies);

        /// <summary>
        /// Gets the latest version of the package.
        /// </summary>
        /// <param name="package">The package to find updates for.</param>
        IPackage GetUpdate(IPackage package);

        /// <summary>
        /// Determines if any version of a package is installed in the LocalRepository.
        /// </summary>
        bool IsPackageInstalled(IPackage package);
    }
}

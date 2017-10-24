// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages.Administration.PackageManager;
using Microsoft.TestCommon;
using Moq;
using NuGet;

namespace System.Web.WebPages.Administration.Test
{
    public class WebPackageManagerTest
    {
        [Fact]
        public void ConstructorThrowsIfRemoteSourceIsNullOrEmpty()
        {
            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebProjectManager((string)null, "foo"), "remoteSource");
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebProjectManager("", @"D:\baz"), "remoteSource");
        }

        [Fact]
        public void ConstructorThrowsIfSiteRootIsNullOrEmpty()
        {
            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebProjectManager("foo", null), "siteRoot");
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebProjectManager("foo", ""), "siteRoot");
        }

        [Fact]
        public void AllowInstallingPackageWithToolsFolderDoNotThrow()
        {
            // Arrange
            var projectManager = new Mock<IProjectManager>();
            projectManager.Setup(p => p.AddPackageReference("A", new SemanticVersion("1.0"), false, false)).Verifiable();

            var webProjectManager = new WebProjectManager(projectManager.Object, @"x:\")
            {
                DoNotAddBindingRedirects = true
            };

            var packageFile1 = new Mock<IPackageFile>();
            packageFile1.Setup(p => p.Path).Returns("tools\\install.ps1");

            var packageFile2 = new Mock<IPackageFile>();
            packageFile2.Setup(p => p.Path).Returns("content\\A.txt");

            var package = new Mock<IPackage>();
            package.Setup(p => p.Id).Returns("A");
            package.Setup(p => p.Version).Returns(new SemanticVersion("1.0"));
            package.Setup(p => p.GetFiles()).Returns(new[] { packageFile1.Object, packageFile2.Object });

            // Act
            webProjectManager.InstallPackage(package.Object, appDomain: null);

            // Assert
            projectManager.Verify();
        }

        [Fact]
        public void GetLocalRepositoryReturnsPackagesFolderUnderAppData()
        {
            // Arrange
            var siteRoot = "my-site";

            // Act
            var repositoryFolder = WebProjectManager.GetWebRepositoryDirectory(siteRoot);

            Assert.Equal(@"my-site\App_Data\packages", repositoryFolder);
        }

        [Fact]
        public void GetPackagesReturnsAllItemsWhenNoSearchTermIsIncluded()
        {
            // Arrange
            var repository = GetRepository();

            // Act
            var result = WebProjectManager.GetPackages(repository, String.Empty);

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void GetPackagesReturnsItemsContainingSomeSearchToken()
        {
            // Arrange
            var repository = GetRepository();

            // Act
            var result = WebProjectManager.GetPackages(repository, "testing .NET");
            var package = result.SingleOrDefault();

            // Assert
            Assert.NotNull(package);
            Assert.Equal("A", package.Id);
        }

        [Fact]
        public void GetPackagesWithLicenseReturnsAllDependenciesWithRequiresAcceptance()
        {
            // Arrange
            var remoteRepository = GetRepository();
            var localRepository = new Mock<IPackageRepository>().Object;

            // Act
            var package = remoteRepository.GetPackages().Find("C").SingleOrDefault();
            var result = WebProjectManager.GetPackagesRequiringLicenseAcceptance(package, localRepository, remoteRepository);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.Id == "C");
            Assert.Contains(result, c => c.Id == "B");
        }

        [Fact]
        public void GetPackagesWithLicenseReturnsEmptyResultForPackageThatDoesNotRequireLicenses()
        {
            // Arrange
            var remoteRepository = GetRepository();
            var localRepository = new Mock<IPackageRepository>().Object;

            // Act
            var package = remoteRepository.GetPackages().Find("A").SingleOrDefault();
            var result = WebProjectManager.GetPackagesRequiringLicenseAcceptance(package, localRepository, remoteRepository);

            // Assert
            Assert.False(result.Any());
        }

        private static IPackageRepository GetRepository()
        {
            Mock<IPackageRepository> repository = new Mock<IPackageRepository>();
            var packages = new[]
            {
                GetPackage("A", desc: "testing"),
                GetPackage("B", version: "1.1", requiresLicense: true),
                GetPackage("C", requiresLicense: true, dependencies: new[]
                {
                    new PackageDependency("B", new VersionSpec { MinVersion = new SemanticVersion("1.0") })
                })
            };
            repository.Setup(c => c.GetPackages()).Returns(packages.AsQueryable());

            return repository.Object;
        }

        private static IPackage GetPackage(string id, string version = "1.0", string desc = null, bool requiresLicense = false, IEnumerable<PackageDependency> dependencies = null)
        {
            Mock<IPackage> package = new Mock<IPackage>();
            package.SetupGet(c => c.Id).Returns(id);
            package.SetupGet(c => c.Version).Returns(SemanticVersion.Parse(version));
            package.SetupGet(c => c.Description).Returns(desc ?? id);
            package.SetupGet(c => c.RequireLicenseAcceptance).Returns(requiresLicense);
            package.SetupGet(c => c.LicenseUrl).Returns(new Uri("http://www." + id + ".com"));
            package.SetupGet(c => c.Dependencies).Returns(dependencies ?? Enumerable.Empty<PackageDependency>());
            return package.Object;
        }
    }
}

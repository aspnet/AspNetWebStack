// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Deployment.Test
{
    public class AssemblyUtilsTest
    {
        private static readonly string LatestVersion = LatestRazorVersion.LatestVersion.ToString();

        private string LatestAssemblyName()
        {
            return AssemblyName(LatestVersion);
        }

        private string AssemblyName(string version, string culture = "neutral", string publicKeyToken = "31bf3856ad364e35")
        {
            return AssemblyNameFor("System.Web.WebPages.Deployment", version, culture, publicKeyToken);
        }

        private string AssemblyNameFor(string name)
        {
            return AssemblyNameFor(name, LatestVersion);
        }

        private string AssemblyNameFor(string name, string version, string culture = "neutral", string publicKeyToken = "31bf3856ad364e35")
        {
            string formatString = "{0}, Version={1}, Culture={2}, PublicKeyToken={3}";

            return string.Format(CultureInfo.InvariantCulture, formatString, name, version, culture, publicKeyToken);
        }

        [Fact]
        public void GetMaxAssemblyVersionReturnsMaximumAvailableVersion()
        {
            // Arrange
            var assemblies = new[]
            {
                new AssemblyName(AssemblyName("1.0.0.0")),
                new AssemblyName(AssemblyName("3.0.0.0")),
                new AssemblyName(LatestAssemblyName()),
                new AssemblyName(AssemblyName("2.0.0.0")),
            };

            // Act
            var maxVersion = AssemblyUtils.GetMaxWebPagesVersion(assemblies);

            // Assert
            Assert.Equal(LatestRazorVersion.LatestVersion, maxVersion);
        }

        [Fact]
        public void GetMaxAssemblyVersionMatchesExactName()
        {
            // Arrange
            var assemblies = new[]
            {
                new AssemblyName(AssemblyName("1.0.0.0")),
                new AssemblyName(AssemblyName("2.2.0.0")),
                new AssemblyName(AssemblyName("2.2.0.0", publicKeyToken: "7777777777777777")),
                new AssemblyName(AssemblyName("2.2.0.0", culture: "en-US")),
                new AssemblyName(AssemblyName("3.0.0.0")),
                new AssemblyName(LatestAssemblyName()),
            };

            // Act
            var maxVersion = AssemblyUtils.GetMaxWebPagesVersion(assemblies);

            // Assert
            Assert.Equal(LatestRazorVersion.LatestVersion, maxVersion);
        }

        [Fact]
        public void GetVersionFromBinReturnsNullIfNoFileWithDeploymentAssemblyNameIsFoundInBin()
        {
            // Arrange
            var binDirectory = @"X:\test\project";
            TestFileSystem fileSystem = new TestFileSystem();

            // Act
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, fileSystem, getAssemblyNameThunk: null);

            // Assert
            Assert.Null(binVersion);
        }

        [Fact]
        public void GetVersionFromBinReturnsVersionFromBinIfLower()
        {
            // Arrange
            var binDirectory = @"X:\test\project";
            TestFileSystem fileSystem = new TestFileSystem();
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName(AssemblyName("1.0.0.0"));

            // Act
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, fileSystem, getAssembyName);

            // Assert
            Assert.Equal(new Version("1.0.0.0"), binVersion);
        }

        [Fact]
        public void GetVersionFromBinReturnsVersionFromBinIfSameVersion()
        {
            // Arrange
            var binDirectory = @"X:\test\project";
            TestFileSystem fileSystem = new TestFileSystem();
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName(LatestAssemblyName());

            // Act
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, fileSystem, getAssembyName);

            // Assert
            Assert.Equal(LatestRazorVersion.LatestVersion, binVersion);
        }

        [Fact]
        public void GetVersionFromBinReturnsVersionFromBinIfHigherVersion()
        {
            // Arrange
            var binDirectory = @"X:\test\project";
            TestFileSystem fileSystem = new TestFileSystem();
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, fileSystem, getAssembyName);

            // Assert
            Assert.Equal(new Version("8.0.0.0"), binVersion);
        }

        [Fact]
        public void GetVersionFromBinReturnsNullIfFileInBinIsNotAValidBinary()
        {
            // Arrange
            var binDirectory = @"X:\test\project";
            TestFileSystem fileSystem = new TestFileSystem();
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            Func<string, AssemblyName> getAssembyName = _ => { throw new FileLoadException(); };

            // Act
            var binVersion = AssemblyUtils.GetVersionFromBin(binDirectory, fileSystem, getAssembyName);

            // Assert
            Assert.Null(binVersion);
        }

        [Fact]
        public void GetAssembliesForVersionReturnsCorrectSetForV1()
        {
            // Arrange
            var expectedAssemblies = new[]
            {
                "Microsoft.Web.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Web.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Web.Helpers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Web.WebPages, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Web.WebPages.Administration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "WebMatrix.Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "WebMatrix.WebData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
            };

            // Act 
            var assemblies = AssemblyUtils.GetAssembliesForVersion(new Version("1.0.0.0"))
                .Select(c => c.ToString())
                .ToArray();

            // Assert
            Assert.Equal(expectedAssemblies, assemblies);
        }

        [Fact]
        public void GetAssembliesForVersionReturnsCorrectSetForVCurrent()
        {
            // Arrange
            var expectedAssemblies = new[]
            {
                AssemblyNameFor("Microsoft.Web.Infrastructure", "1.0.0.0"),
                AssemblyNameFor("System.Web.Razor"),
                AssemblyNameFor("System.Web.Helpers"),
                AssemblyNameFor("System.Web.WebPages"),
                AssemblyNameFor("System.Web.WebPages.Administration"),
                AssemblyNameFor("System.Web.WebPages.Razor"),
                AssemblyNameFor("WebMatrix.Data"),
                AssemblyNameFor("WebMatrix.WebData"),
            };

            // Act 
            var assemblies = AssemblyUtils.GetAssembliesForVersion(LatestRazorVersion.LatestVersion)
                .Select(c => c.ToString())
                .ToArray();

            // Assert
            Assert.Equal(expectedAssemblies, assemblies);
        }

        [Fact]
        public void GetMatchingAssembliesReturnsEmptyDictionaryIfAssemblyReferencesCollectionIsNull()
        {
            // Arrange
            Dictionary<string, IEnumerable<string>> assemblyReferences = null;

            // Act
            var referencedAssemblies = AssemblyUtils.GetAssembliesMatchingOtherVersions(assemblyReferences);

            // Assert
            Assert.Empty(referencedAssemblies);
        }

        [Fact]
        public void GetMatchingAssembliesReturnsEmptyDictionaryIfAssemblyReferencesCollectionIsEmpty()
        {
            // Arrange
            var assemblyReferences = new Dictionary<string, IEnumerable<string>>();

            // Act
            var referencedAssemblies = AssemblyUtils.GetAssembliesMatchingOtherVersions(assemblyReferences);

            // Assert
            Assert.Empty(referencedAssemblies);
        }

        [Fact]
        public void GetMatchingAssembliesReturnsEmptyDictionaryIfNoReferencesMatchWebPagesAssemblies()
        {
            // Arrange
            var assemblyReferences = new Dictionary<string, IEnumerable<string>>
            {
                { @"x:\site\bin\A.dll", new List<string> { "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null" }},
                { @"x:\site\bin\B.dll", new List<string> { "System.Web.Mvc, Version=2.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" }},
            };

            // Act
            var referencedAssemblies = AssemblyUtils.GetAssembliesMatchingOtherVersions(assemblyReferences);

            // Assert
            Assert.Empty(referencedAssemblies);
        }

        [Fact]
        public void GetMatchingAssembliesReturnsReferencingAssemblyAndWebPagesVersionForMatchingReferences()
        {
            // Arrange
            var assemblyReferences = new Dictionary<string, IEnumerable<string>>
            {
                { @"x:\site\bin\A.dll", new[] { "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null" }},
                { @"x:\site\bin\B.dll", new[] 
                    {
                        "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null",
                        AssemblyNameFor("System.Web.WebPages", "1.0.0.0"),
                        AssemblyNameFor("System.Web.Helpers", "1.0.0.0"),
                    }
                },
            };

            // Act
            var referencedAssemblies = AssemblyUtils.GetAssembliesMatchingOtherVersions(assemblyReferences);

            // Assert
            Assert.Equal(1, referencedAssemblies.Count);
            Assert.Equal(@"x:\site\bin\B.dll", referencedAssemblies.Single().Key);
            Assert.Equal(new Version("1.0.0.0"), referencedAssemblies.Single().Value);
        }

        [Fact]
        public void GetMatchingAssembliesFiltersWebPagesVersionsThatMatch()
        {
            // Arrange
            var assemblyReferences = new Dictionary<string, IEnumerable<string>>
            {
                { @"x:\site\bin\A.dll", new[] { "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null" }},
                { @"x:\site\bin\B.dll", new[] 
                    { 
                        "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null",
                        String.Format(CultureInfo.InvariantCulture, "System.Web.WebPages, Version={0}, Culture=neutral, PublicKeyToken=31bf3856ad364e35", AssemblyUtils.ThisAssemblyName.Version),
                        String.Format(CultureInfo.InvariantCulture, "System.Web.Helpers, Version={0}, Culture=neutral, PublicKeyToken=31bf3856ad364e35", AssemblyUtils.ThisAssemblyName.Version)
                    }
                },
                { @"x:\site\bin\C.dll", new[] 
                    { 
                        "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null",
                        "System.Web.WebPages.Razor, Version=1.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                        "System.Web.WebPages.Razor, Version=1.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                    }
                },
            };

            // Act
            var referencedAssemblies = AssemblyUtils.GetAssembliesMatchingOtherVersions(assemblyReferences);

            // Assert
            Assert.Equal(1, referencedAssemblies.Count);
            Assert.Equal(@"x:\site\bin\C.dll", referencedAssemblies.Single().Key);
            Assert.Equal(new Version("1.2.0.0"), referencedAssemblies.Single().Value);
        }

        private static void EnsureDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

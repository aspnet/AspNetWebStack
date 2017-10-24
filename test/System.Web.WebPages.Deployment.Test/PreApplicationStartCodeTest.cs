// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Deployment.Test
{
    public class PreApplicationStartCodeTest
    {
        private const string DeploymentVersionFile = "System.Web.WebPages.Deployment";

        private readonly string LatestVersion = LatestRazorVersion.LatestVersion.ToString();

        [Fact]
        public void PreApplicationStartCodeDoesNothingIfWebPagesIsExplicitlyDisabled()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1", "2");

            var fileSystem = new TestFileSystem();
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: false, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeUsesVersionSpecifiedInConfigIfWebPagesIsImplicitlyEnabled()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.12.123.1234", LatestVersion);
            Version webPagesVersion = new Version("1.12.123.1234");

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Default.cshtml");
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: webPagesVersion);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, getAssemblyNameThunk: null);

            // Assert
            Assert.True(loaded);
            Assert.Equal(webPagesVersion, loadedVersion);
            Assert.False(registeredForChangeNotification);
            VerifyVersionFile(buildManager, webPagesVersion);
        }

        [Fact]
        public void PreApplicationStartCodeDoesNotLoadCurrentWebPagesIfOnlyVersionIsListedInConfigAndNoFilesAreFoundInSiteRoot()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            Version webPagesVersion = AssemblyUtils.ThisAssemblyName.Version;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(LatestVersion);

            var fileSystem = new TestFileSystem();
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: webPagesVersion);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Arrange
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.True(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeRegistersForChangeNotificationIfNotExplicitlyDisabledAndNoFilesFoundInSiteRoot()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(LatestVersion);

            var fileSystem = new TestFileSystem();
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.True(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeDoesNothingIfV1IsAvailableInBinAndSiteIsExplicitlyEnabled()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            var v1Version = new Version("1.0.0.0");
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", LatestVersion);

            var binDirectory = DeploymentUtil.GetBinDirectory();

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: true, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, getAssembyName);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeDoesNothingIfV1IsAvailableInBinAndFileExistsInRootOfWebSite()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            var v1Version = new Version("1.0.0.0");
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", LatestVersion);

            var binDirectory = DeploymentUtil.GetBinDirectory();

            var fileSystem = new TestFileSystem();
            var buildManager = new TestBuildManager();
            fileSystem.AddFile("Default.cshtml");
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, getAssembyName);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeDoesNothingIfItIsAvailableInBinAndFileExistsInRootOfWebSite()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            var webPagesVersion = AssemblyUtils.ThisAssemblyName.Version;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(AssemblyUtils.ThisAssemblyName.Version.ToString());

            var fileSystem = new TestFileSystem();
            var binDirectory = DeploymentUtil.GetBinDirectory();

            var buildManager = new TestBuildManager();
            fileSystem.AddFile("Default.vbhtml");
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=" + AssemblyUtils.ThisAssemblyName.Version.ToString() + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, getAssembyName);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeLoadsV1IfNoVersionIsSpecifiedAndCurrentAssemblyIsTheMaximumVersionAvailable()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            var webPagesVersion = AssemblyUtils.ThisAssemblyName.Version;
            var v1Version = new Version("1.0.0.0");
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", LatestVersion);

            // Note: For this test to work with future versions we would need to create corresponding embedded resources with that version in it.
            var fileSystem = new TestFileSystem();
            var buildManager = new TestBuildManager();
            fileSystem.AddFile("Index.cshtml");
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null);

            // Assert
            Assert.True(loaded);
            Assert.Equal(v1Version, loadedVersion);
            Assert.False(registeredForChangeNotification);
            VerifyVersionFile(buildManager, v1Version);
        }

        [Fact]
        public void PreApplicationStartCodeThrowsIfNoVersionIsSpecifiedAndV1IsNotAvailable()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            var webPagesVersion = AssemblyUtils.ThisAssemblyName.Version;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(LatestVersion);

            // Note: For this test to work with future versions we would need to create corresponding embedded resources with that version in it.
            var fileSystem = new TestFileSystem();
            var buildManager = new TestBuildManager();
            fileSystem.AddFile("Index.cshtml");
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
                PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null),
@"Could not determine which version of ASP.NET Web Pages to use.

In order to use this site, specify a version in the site’s web.config file. For more information, see the following article on the Microsoft support site: http://go.microsoft.com/fwlink/?LinkId=254126");
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeDoesNotLoadIfAHigherVersionIsAvailableInBin()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(LatestVersion, "8.0.0.0");

            var binDirectory = DeploymentUtil.GetBinDirectory();

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, getAssembyName);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeDoesNotLoadIfAHigherVersionIsAvailableInGac()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            // Hopefully we'd have figured out a better way to load Plan9 by v8.
            var webPagesVersion = new Version("8.0.0.0");
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", LatestVersion, "8.0.0.0");

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            var buildManager = new TestBuildManager();
            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: null);
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", "bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null);

            // Assert
            Assert.False(loaded);
            Assert.Null(loadedVersion);
            Assert.False(registeredForChangeNotification);
            Assert.Equal(0, buildManager.Stream.Length);
        }

        [Fact]
        public void PreApplicationStartCodeForcesRecompileIfPreviousVersionIsNotTheSameAsCurrentVersion()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(LatestVersion);

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            var buildManager = new TestBuildManager();
            var content = "1.0.0.0" + Environment.NewLine;
            buildManager.Stream = new MemoryStream(Encoding.Default.GetBytes(content));

            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: new Version(LatestVersion));
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            var ex = Assert.Throws<HttpCompileException>(() =>
                PreApplicationStartCode.StartCore(fileSystem, "", @"site\bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null)
            );

            // Assert
            Assert.Equal("Changes were detected in the Web Pages runtime version that require your application to be recompiled. Refresh your browser window to continue.", ex.Message);
            Assert.Equal((object)true, ex.Data["WebPages.VersionChange"]);
            Assert.False(registeredForChangeNotification);
            VerifyVersionFile(buildManager, new Version(LatestVersion));
            Assert.True(fileSystem.FileExists(@"site\bin\WebPagesRecompilation.deleteme"));
        }

        [Fact]
        public void PreApplicationStartCodeDoesNotForceRecompileIfNewVersionIsV1AndCurrentAssemblyIsNotMaxVersion()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies(LatestVersion, "5.0.0.0");

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            var buildManager = new TestBuildManager();
            var content = AssemblyUtils.ThisAssemblyName.Version + Environment.NewLine;
            buildManager.Stream = new MemoryStream(Encoding.Default.GetBytes(content));

            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: new Version("1.0.0"));
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };

            // Act
            bool loaded = PreApplicationStartCode.StartCore(fileSystem, "", @"site\bin", nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, null);

            // Assert
            Assert.False(loaded);
            Assert.False(registeredForChangeNotification);
            VerifyVersionFile(buildManager, AssemblyUtils.ThisAssemblyName.Version);
            Assert.False(fileSystem.FileExists(@"site\bin\WebPagesRecompilation.deleteme"));
        }

        [Fact]
        public void PreApplicationStartCodeThrowsIfWebPagesIsInBinAndDifferentVersionIsSpecifiedInConfig()
        {
            // Arrange
            Version loadedVersion = null;
            bool registeredForChangeNotification = false;
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", LatestVersion);

            var binDirectory = DeploymentUtil.GetBinDirectory();

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            var buildManager = new TestBuildManager();
            var content = AssemblyUtils.ThisAssemblyName.Version + Environment.NewLine;
            buildManager.Stream = new MemoryStream(Encoding.Default.GetBytes(content));

            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: new Version(LatestVersion));
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { registeredForChangeNotification = true; };
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
                PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager, loadWebPages, registerForChange, getAssembyName),
                @"Conflicting versions of ASP.NET Web Pages detected: specified version is """ + LatestVersion + @""", but the version in bin is ""1.0.0.0"". To continue, remove files from the application's bin directory or remove the version specification in web.config."
            );

            Assert.False(registeredForChangeNotification);
            Assert.Null(loadedVersion);
        }

        [Fact]
        public void PreApplicationStartCodeThrowsIfVersionIsSpecifiedInConfigAndDifferentVersionExistsInBin()
        {
            // Arrange
            Version loadedVersion = null;

            var binDirectory = DeploymentUtil.GetBinDirectory();
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", AssemblyUtils.ThisAssemblyName.Version.ToString());

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            fileSystem.AddFile(Path.Combine(binDirectory, "System.Web.WebPages.Deployment.dll"));
            var buildManager = new TestBuildManager();
            var content = AssemblyUtils.ThisAssemblyName.Version + Environment.NewLine;
            buildManager.Stream = new MemoryStream(Encoding.Default.GetBytes(content));

            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: new Version("1.0.0"));
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { };
            Func<string, AssemblyName> getAssembyName = _ => new AssemblyName("System.Web.WebPages.Deployment, Version=" + AssemblyUtils.ThisAssemblyName.Version + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
                                                              PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager: buildManager, loadWebPages: loadWebPages, registerForChangeNotification: registerForChange, getAssemblyNameThunk: getAssembyName),
                                                              String.Format(@"Conflicting versions of ASP.NET Web Pages detected: specified version is ""1.0.0.0"", but the version in bin is ""{0}"". To continue, remove files from the application's bin directory or remove the version specification in web.config.",
                                                                            AssemblyUtils.ThisAssemblyName.Version));
        }

        [Fact]
        public void PreApplicationStartCodeThrowsIfVersionSpecifiedInConfigIsNotAvailable()
        {
            // Arrange
            Version loadedVersion = null;

            var binDirectory = DeploymentUtil.GetBinDirectory();
            IEnumerable<AssemblyName> loadedAssemblies = GetAssemblies("1.0.0.0", AssemblyUtils.ThisAssemblyName.Version.ToString());

            var fileSystem = new TestFileSystem();
            fileSystem.AddFile("Index.cshtml");
            var buildManager = new TestBuildManager();
            var content = AssemblyUtils.ThisAssemblyName.Version + Environment.NewLine;
            buildManager.Stream = new MemoryStream(Encoding.Default.GetBytes(content));

            var nameValueCollection = GetAppSettings(enabled: null, webPagesVersion: new Version("1.5"));
            Action<Version> loadWebPages = (version) => { loadedVersion = version; };
            Action registerForChange = () => { };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
                                                              PreApplicationStartCode.StartCore(fileSystem, "", binDirectory, nameValueCollection, loadedAssemblies, buildManager: buildManager, loadWebPages: loadWebPages, registerForChangeNotification: registerForChange, getAssemblyNameThunk: null),
                                                              String.Format("Specified Web Pages version \"1.5.0.0\" could not be found. Update your web.config to specify a different version. Current version: \"{0}\".",
                                                                            AssemblyUtils.ThisAssemblyName.Version));
        }

        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }

        private static NameValueCollection GetAppSettings(bool? enabled, Version webPagesVersion)
        {
            var nameValueCollection = new NameValueCollection();
            if (enabled.HasValue)
            {
                nameValueCollection["webpages:enabled"] = enabled.Value ? "true" : "false";
            }
            if (webPagesVersion != null)
            {
                nameValueCollection["webpages:version"] = webPagesVersion.ToString();
            }

            return nameValueCollection;
        }

        private static void VerifyVersionFile(TestBuildManager buildManager, Version webPagesVersion)
        {
            var content = Encoding.UTF8.GetString(buildManager.Stream.ToArray());
            Version version = Version.Parse(content);
            Assert.Equal(webPagesVersion, version);
        }

        private class TestBuildManager : IBuildManager
        {
            private MemoryStream _memoryStream = new MemoryStream();

            public MemoryStream Stream
            {
                get { return _memoryStream; }
                set { _memoryStream = value; }
            }

            public Stream CreateCachedFile(string fileName)
            {
                Assert.Equal(DeploymentVersionFile, fileName);
                CopyMemoryStream();
                return _memoryStream;
            }

            public Stream ReadCachedFile(string fileName)
            {
                Assert.Equal(DeploymentVersionFile, fileName);
                CopyMemoryStream();
                return _memoryStream;
            }

            /// <summary>
            /// Need to do this because the MemoryStream is read and written to in consecutive calls which causes it to be closed / non-expandable.
            /// </summary>
            private void CopyMemoryStream()
            {
                var content = _memoryStream.ToArray();
                if (content.Length > 0)
                {
                    _memoryStream = new MemoryStream(_memoryStream.ToArray());
                }
                else
                {
                    _memoryStream = new MemoryStream();
                }
            }
        }

        private static IEnumerable<AssemblyName> GetAssemblies(params string[] versions)
        {
            return from version in versions
                   select new AssemblyName("System.Web.WebPages.Deployment, Version=" + version + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        }
    }
}

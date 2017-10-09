// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    public class ThemesTest
    {
        [Fact]
        public void Initialize_WithBadParams_Throws()
        {
            // Arrange
            var mockVpp = new Mock<VirtualPathProvider>().Object;
            var scope = new ScopeStorageDictionary();

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => new ThemesImplementation(mockVpp, scope).Initialize(null, "foo"), "themeDirectory");
            Assert.ThrowsArgumentNullOrEmptyString(() => new ThemesImplementation(mockVpp, scope).Initialize("", "foo"), "themeDirectory");

            Assert.ThrowsArgumentNullOrEmptyString(() => new ThemesImplementation(mockVpp, scope).Initialize("~/folder", null), "defaultTheme");
            Assert.ThrowsArgumentNullOrEmptyString(() => new ThemesImplementation(mockVpp, scope).Initialize("~/folder", ""), "defaultTheme");
        }

        [Fact]
        public void CurrentThemeThrowsIfAssignedNullOrEmpty()
        {
            // Arrange
            var mockVpp = new Mock<VirtualPathProvider>().Object;
            var scope = new ScopeStorageDictionary();
            var themesImpl = new ThemesImplementation(mockVpp, scope);

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => { themesImpl.CurrentTheme = null; }, "value");
            Assert.ThrowsArgumentNullOrEmptyString(() => { themesImpl.CurrentTheme = String.Empty; }, "value");
        }

        [Fact]
        public void InvokingPropertiesAndMethodsBeforeInitializationThrows()
        {
            // Arrange
            var mockVpp = new Mock<VirtualPathProvider>().Object;
            var scope = new ScopeStorageDictionary();
            var themesImpl = new ThemesImplementation(mockVpp, scope);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => themesImpl.CurrentTheme = "Foo",
                                                              @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            Assert.Throws<InvalidOperationException>(() => { var x = themesImpl.CurrentTheme; },
                                                              @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            Assert.Throws<InvalidOperationException>(() => { var x = themesImpl.AvailableThemes; },
                                                              @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            Assert.Throws<InvalidOperationException>(() => { var x = themesImpl.DefaultTheme; },
                                                              @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            Assert.Throws<InvalidOperationException>(() => { var x = themesImpl.GetResourcePath("baz"); },
                                                              @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");

            Assert.Throws<InvalidOperationException>(() => { var x = themesImpl.GetResourcePath("baz", "some-file"); },
                                                              @"You must call the ""Themes.Initialize"" method before you call any other method of the ""Themes"" class.");
        }

        [Fact]
        public void InitializeThrowsIfDefaultThemeDirectoryDoesNotExist()
        {
            // Arrange
            var defaultTheme = "default-theme";
            var themeDirectory = "theme-directory";

            var scope = new ScopeStorageDictionary();
            var themesImpl = new ThemesImplementation(GetVirtualPathProvider(themeDirectory, new Dir("not-default-theme")), scope);

            // Act And Assert
            Assert.ThrowsArgument(
                () => themesImpl.Initialize(themeDirectory: themeDirectory, defaultTheme: defaultTheme),
                "defaultTheme",
                "Unknown theme 'default-theme'. Ensure that a directory labeled 'default-theme' exists under the theme directory.");
        }

        [Fact]
        public void ThemesImplUsesScopeStorageToStoreProperties()
        {
            // Arrange
            var defaultTheme = "default-theme";
            var themeDirectory = "theme-directory";

            var scope = new ScopeStorageDictionary();
            var themesImpl = new ThemesImplementation(GetVirtualPathProvider(themeDirectory, new Dir(defaultTheme)), scope);

            // Act
            themesImpl.Initialize(themeDirectory: themeDirectory, defaultTheme: defaultTheme);

            // Ensure Theme use scope storage to store properties
            Assert.Equal((object)true, scope[ThemesImplementation.ThemesInitializedKey]);
            Assert.Equal(themeDirectory, scope[ThemesImplementation.ThemeDirectoryKey]);
            Assert.Equal(defaultTheme, scope[ThemesImplementation.DefaultThemeKey]);
        }

        [Fact]
        public void ThemesImplUsesDefaultThemeWhenNoCurrentThemeIsSpecified()
        {
            // Arrange
            var defaultTheme = "default-theme";
            var themeDirectory = "theme-directory";

            var scope = new ScopeStorageDictionary();
            var themesImpl = new ThemesImplementation(GetVirtualPathProvider(themeDirectory, new Dir(defaultTheme)), scope);
            themesImpl.Initialize(themeDirectory, defaultTheme);

            // Act and Assert
            // CurrentTheme falls back to default theme when null
            Assert.Equal(themesImpl.CurrentTheme, defaultTheme);
        }

        [Fact]
        public void ThemesImplThrowsIfCurrentThemeIsInvalid()
        {
            // Arrange
            var defaultTheme = "default-theme";
            var themeDirectory = "theme-directory";
            var themesImpl = new ThemesImplementation(GetVirtualPathProvider(themeDirectory, new Dir(defaultTheme), new Dir("not-a-random-value")), new ScopeStorageDictionary());
            themesImpl.Initialize(themeDirectory, defaultTheme);

            // Act and Assert
            Assert.ThrowsArgument(() => themesImpl.CurrentTheme = "random-value",
                                                    "value",
                                                    "Unknown theme 'random-value'. Ensure that a directory labeled 'random-value' exists under the theme directory.");
        }

        [Fact]
        public void ThemesImplUsesScopeStorageToStoreCurrentTheme()
        {
            // Arrange
            var defaultTheme = "default-theme";
            var themeDirectory = "theme-directory";
            var currentThemeDir = "custom-theme-dir";
            var scope = new ScopeStorageDictionary();
            var themesImpl = new ThemesImplementation(GetVirtualPathProvider(themeDirectory, new Dir(defaultTheme), new Dir("custom-theme-dir")), scope);

            // Act
            themesImpl.Initialize(themeDirectory, defaultTheme);
            themesImpl.CurrentTheme = currentThemeDir;

            // Assert
            Assert.Equal(scope[ThemesImplementation.CurrentThemeKey], currentThemeDir);
        }

        [Fact]
        public void GetResourcePathThrowsIfCurrentDirectoryIsNull()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile", "wp7.css")));
            themesImpl.Initialize("themes", "default");

            // Act and Assert
            Assert.ThrowsArgumentNull(() => themesImpl.GetResourcePath(folder: null, fileName: "wp7.css"), "folder");
        }

        [Fact]
        public void GetResourcePathThrowsIfFileNameIsNullOrEmpty()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile", "wp7.css")));
            themesImpl.Initialize("themes", "default");

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => themesImpl.GetResourcePath(folder: String.Empty, fileName: null), "fileName");
            Assert.ThrowsArgumentNullOrEmptyString(() => themesImpl.GetResourcePath(folder: String.Empty, fileName: String.Empty), "fileName");
        }

        [Fact]
        public void GetResourcePathReturnsItemFromThemeRootIfAvailable()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile", "wp7.css")));
            themesImpl.Initialize("themes", "default");

            // Act
            themesImpl.CurrentTheme = "mobile";
            var themePath = themesImpl.GetResourcePath(fileName: "wp7.css");

            // Assert
            Assert.Equal(@"themes/mobile/wp7.css", themePath);
        }

        [Fact]
        public void GetResourcePathReturnsItemFromCurrentThemeDirectoryIfAvailable()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile\styles", "wp7.css"), new Dir(@"default\styles", "main.css")));
            themesImpl.Initialize("themes", "default");

            // Act
            themesImpl.CurrentTheme = "mobile";
            var themePath = themesImpl.GetResourcePath(folder: "styles", fileName: "wp7.css");

            // Assert
            Assert.Equal(@"themes/mobile/styles/wp7.css", themePath);
        }

        [Fact]
        public void GetResourcePathReturnsItemFromDefaultThemeDirectoryIfNotFoundInCurrentThemeDirectory()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile\styles", "wp7.css"), new Dir(@"default\styles", "main.css")));
            themesImpl.Initialize("themes", "default");

            // Act
            themesImpl.CurrentTheme = "mobile";
            var themePath = themesImpl.GetResourcePath(folder: "styles", fileName: "main.css");

            // Assert
            Assert.Equal(@"themes/default/styles/main.css", themePath);
        }

        [Fact]
        public void GetResourcePathReturnsNullIfDirectoryDoesNotExist()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile\styles", "wp7.css"), new Dir(@"default\styles", "main.css")));
            themesImpl.Initialize("themes", "default");

            // Act
            themesImpl.CurrentTheme = "mobile";
            var themePath = themesImpl.GetResourcePath(folder: "does-not-exist", fileName: "main.css");

            // Assert
            Assert.Null(themePath);
        }

        [Fact]
        public void GetResourcePathReturnsNullIfItemNotFoundInCurrentAndDefaultThemeDirectories()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir(@"mobile\styles", "wp7.css"), new Dir(@"default\styles", "main.css")));
            themesImpl.Initialize("themes", "default");

            // Act
            themesImpl.CurrentTheme = "mobile";
            var themePath = themesImpl.GetResourcePath(folder: "styles", fileName: "awesome-blinking-text.css");

            // Assert
            Assert.Null(themePath);
        }

        [Fact]
        public void AvaliableThemesReturnsTopLevelDirectoriesUnderThemeDirectory()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("mobile"), new Dir("rotary-phone")));
            // Act
            themesImpl.Initialize("themes", "default");
            var themes = themesImpl.AvailableThemes;

            // Assert
            Assert.Equal(3, themes.Count);
            Assert.Equal("default", themes[0]);
            Assert.Equal("mobile", themes[1]);
            Assert.Equal("rotary-phone", themes[2]);
        }

        /// <remarks>
        /// // folder structure:
        /// // /root
        /// //   /foo
        /// //      /bar.cs
        /// // testing that a file specified as foo/bar in folder root will return null
        /// </remarks>
        [Fact]
        public void FileWithSlash_ReturnsNull()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("root"), new Dir(@"root\foo", "wp7.css"), new Dir(@"default\styles", "main.css")));

            // Act
            var actual = themesImpl.FindMatchingFile("root", "foo/bar.cs");

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void DirectoryWithNoFilesReturnsNull()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir("default"), new Dir("empty-dir")));

            // Act
            var actual = themesImpl.FindMatchingFile(@"themes\empty-dir", "main.css");

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void MatchingFiles_ReturnsCorrectFile()
        {
            // Arrange
            var themesImpl = new ThemesImplementation(scopeStorage: new ScopeStorageDictionary(),
                                                      vpp: GetVirtualPathProvider("themes", new Dir(@"nomatchingfiles", "foo.cs")));

            // Act
            var bar = themesImpl.FindMatchingFile(@"themes\nomatchingfiles", "bar.cs");
            var foo = themesImpl.FindMatchingFile(@"themes\nomatchingfiles", "foo.cs");

            // Assert
            Assert.Null(bar);
            Assert.Equal(@"themes/nomatchingfiles/foo.cs", foo);
        }

        private static VirtualPathProvider GetVirtualPathProvider(string themeRoot, params Dir[] fileSystem)
        {
            var mockVpp = new Mock<VirtualPathProvider>();
            var dirRoot = new Mock<VirtualDirectory>(themeRoot);

            var themeDirectories = new List<VirtualDirectory>();
            foreach (var directory in fileSystem)
            {
                var dir = new Mock<VirtualDirectory>(directory.Name);
                var directoryPath = themeRoot + '\\' + directory.Name;
                dir.SetupGet(d => d.Name).Returns(directory.Name);
                mockVpp.Setup(c => c.GetDirectory(It.Is<string>(p => p.Equals(directoryPath, StringComparison.OrdinalIgnoreCase)))).Returns(dir.Object);

                var fileList = new List<VirtualFile>();
                foreach (var item in directory.Files)
                {
                    var filePath = directoryPath + '\\' + item;
                    var file = new Mock<VirtualFile>(filePath);
                    file.SetupGet(f => f.Name).Returns(item);
                    fileList.Add(file.Object);
                }

                dir.SetupGet(c => c.Files).Returns(fileList);
                themeDirectories.Add(dir.Object);
            }

            dirRoot.SetupGet(c => c.Directories).Returns(themeDirectories);
            mockVpp.Setup(c => c.GetDirectory(themeRoot)).Returns(dirRoot.Object);

            return mockVpp.Object;
        }

        private class Dir
        {
            public Dir(string name, params string[] files)
            {
                Name = name;
                Files = files;
            }

            public string Name { get; private set; }

            public IEnumerable<string> Files { get; private set; }
        }
    }
}

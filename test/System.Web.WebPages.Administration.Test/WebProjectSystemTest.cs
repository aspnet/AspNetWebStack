// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.WebPages.Administration.PackageManager;
using System.Xml.Linq;
using Microsoft.TestCommon;
using Moq;
using NuGet;

namespace System.Web.WebPages.Administration.Test
{
    public class WebProjectSystemTest
    {
        [Fact]
        public void ResolvePathReturnsAppCodePathIfPathIsSourceFile()
        {
            // Arrange
            var path = "Foo.cs";
            var webProjectSystem = new WebProjectSystem(@"x:\");

            // Act
            var resolvedPath = webProjectSystem.ResolvePath(path);

            // Assert
            Assert.Equal(@"App_Code\Foo.cs", resolvedPath);
        }

        [Fact]
        public void ResolvePathReturnsOriginalPathIfSourceFilePathIsAlreadyUnderAppCode()
        {
            // Arrange
            var path = @"App_Code\Foo.cs";
            var webProjectSystem = new WebProjectSystem(@"x:\");

            // Act
            var resolvedPath = webProjectSystem.ResolvePath(path);

            // Assert
            Assert.Equal(path, resolvedPath);
        }

        [Fact]
        public void ResolvePathReturnsOriginalPathIfFileIsNotSource()
        {
            // Arrange
            var path = @"Foo.js";
            var webProjectSystem = new WebProjectSystem(@"x:\");

            // Act
            var resolvedPath = webProjectSystem.ResolvePath(path);

            // Assert
            Assert.Equal(path, resolvedPath);
        }

        [Fact]
        public void AddPackageWithFrameworkReferenceCreatesWebConfigIfItDoesNotExist()
        {
            // Arrange
            string webConfigPath = @"x:\my-website\web.config";
            MemoryStream memoryStream = new MemoryStream();

            var fileSystem = new Mock<IFileSystem>();
            fileSystem.SetupGet(f => f.Root).Returns("x:\\my-website");
            fileSystem.Setup(f => f.FileExists(It.Is<string>(p => p.Equals(webConfigPath)))).Returns(false).Verifiable();
            fileSystem.Setup(f => f.AddFile(It.Is<string>(p => p.Equals(webConfigPath)), It.IsAny<Stream>()))
                .Callback<string, Stream>((_, s) => { s.CopyTo(memoryStream); });

            var references = "System";

            // Act
            WebProjectSystem.AddReferencesToConfig(fileSystem.Object, references);

            // Assert
            memoryStream.Seek(0, SeekOrigin.Begin);
            XDocument document = XDocument.Load(memoryStream);

            var element = document.Root;
            Assert.Equal("configuration", element.Name);

            // Use SingleOrDefault to ensure there's exactly one element with that name
            var assemblies = document.Root
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("system.web"))
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("compilation"))
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("assemblies"));

            Assert.Equal(references, assemblies.Elements().First().Attribute("assembly").Value);
        }

        [Fact]
        public void AddPackageWithFrameworkReferenceCreatesWebConfigIfItExistsWithoutAssembliesNode()
        {
            // Arrange
            var webConfigPath = @"x:\my-website\web.config";
            var webConfigContent = @"<?xml version=""1.0""?>
                <configuration>
                    <connectionStrings>
                        <add name=""test"" />
                    </connectionStrings>
                    <system.web>
                        <profiles><add name=""awesomeprofile"" /></profiles>
                    </system.web>
                </configuration>

".AsStream();
            MemoryStream memoryStream = new MemoryStream();

            var fileSystem = new Mock<IFileSystem>();
            fileSystem.SetupGet(f => f.Root).Returns("x:\\my-website");
            fileSystem.Setup(f => f.FileExists(It.Is<string>(p => p.Equals(webConfigPath)))).Returns(true).Verifiable();
            fileSystem.Setup(f => f.OpenFile(It.Is<string>(p => p.Equals(webConfigPath)))).Returns(webConfigContent);
            fileSystem.Setup(f => f.AddFile(It.Is<string>(p => p.Equals(webConfigPath)), It.IsAny<Stream>()))
                .Callback<string, Stream>((_, s) => { s.CopyTo(memoryStream); });

            var references = "System.Data";

            // Act
            WebProjectSystem.AddReferencesToConfig(fileSystem.Object, references);

            // Assert
            memoryStream.Seek(0, SeekOrigin.Begin);
            XDocument document = XDocument.Load(memoryStream);

            var element = document.Root;
            Assert.Equal("configuration", element.Name);

            // Use SingleOrDefault to ensure there's exactly one element with that name
            var assemblies = document.Root
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("system.web"))
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("compilation"))
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("assemblies"));

            Assert.Equal(references, assemblies.Elements().First().Attribute("assembly").Value);

            // Make sure the original web.config content is unaffected
            Assert.Equal("test", document.Root
                                     .Elements().SingleOrDefault(e => e.Name.ToString().Equals("connectionStrings"))
                                     .Elements().SingleOrDefault(e => e.Name.ToString().Equals("add"))
                                     .Attributes().SingleOrDefault(e => e.Name.ToString().Equals("name")).Value);

            Assert.Equal("awesomeprofile", document.Root.Element("system.web").Element("profiles").Element("add").Attribute("name").Value);
        }

        [Fact]
        public void AddPackageWithFrameworkReferenceDoesNotAffectWebConfigIfReferencesAlreadyExist()
        {
            // Arrange
            var webConfigPath = @"x:\my-website\web.config";
            var memoryStream = new NeverCloseMemoryStream(@"<?xml version=""1.0""?>
                <configuration>
                    <connectionStrings>
                        <add name=""test"" />
                    </connectionStrings>
                    <system.web>
                        <compilation>
                            <assemblies>
                                <add assembly=""System.Data, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
                            </assemblies>
                        </compilation>
                        <profiles><add name=""awesomeprofile"" /></profiles>
                    </system.web>
                </configuration>

");

            var fileSystem = new Mock<IFileSystem>();
            fileSystem.SetupGet(f => f.Root).Returns("x:\\my-website");
            fileSystem.Setup(f => f.FileExists(It.Is<string>(p => p.Equals(webConfigPath)))).Returns(true);
            fileSystem.Setup(f => f.OpenFile(It.Is<string>(p => p.Equals(webConfigPath)))).Returns(() =>
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            });
            fileSystem.Setup(f => f.AddFile(It.Is<string>(p => p.Equals(webConfigPath)), It.IsAny<Stream>()))
                .Callback<string, Stream>((_, stream) => { memoryStream = new NeverCloseMemoryStream(stream.ReadToEnd()); });

            // Act
            WebProjectSystem.AddReferencesToConfig(fileSystem.Object, "System.Data");
            WebProjectSystem.AddReferencesToConfig(fileSystem.Object, "Microsoft.Abstractions");

            // Assert
            memoryStream.Seek(0, SeekOrigin.Begin);
            XDocument document = XDocument.Load(memoryStream);

            var element = document.Root;
            Assert.Equal("configuration", element.Name);

            // Use SingleOrDefault to ensure there's exactly one element with that name
            var assemblies = document.Root
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("system.web"))
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("compilation"))
                .Elements().SingleOrDefault(e => e.Name.ToString().Equals("assemblies"));

            Assert.Equal(2, assemblies.Elements("add").Count());
            Assert.Equal("System.Data, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35",
                         assemblies.Elements().First().Attribute("assembly").Value);
            Assert.Equal("Microsoft.Abstractions",
                         assemblies.Elements().Last().Attribute("assembly").Value);

            // Make sure the original web.config content is unaffected
            Assert.Equal("test", document.Root
                                     .Elements().SingleOrDefault(e => e.Name.ToString().Equals("connectionStrings"))
                                     .Elements().SingleOrDefault(e => e.Name.ToString().Equals("add"))
                                     .Attributes().SingleOrDefault(e => e.Name.ToString().Equals("name")).Value);

            Assert.Equal("awesomeprofile", document.Root.Element("system.web").Element("profiles").Element("add").Attribute("name").Value);
        }

        [Fact]
        public void ResolveAssemblyPartialNameForCommonAssemblies()
        {
            // Arrange
            var commonAssemblies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "System.Data", "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" },
                { "System.Data.Linq", "System.Data.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" },
                { "System.Net", "System.Net, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" },
                { "System.Runtime.Caching", "System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" },
                { "System.Xml", "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" },
                { "System.Web.DynamicData", "System.Web.DynamicData, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" },
            };

            // Act and Assert
            foreach (var item in commonAssemblies)
            {
                var resolvedName = WebProjectSystem.ResolvePartialAssemblyName(item.Key);

                Assert.Equal(item.Value, resolvedName);
            }
        }

        private static IFileSystem GetFileSystem()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(c => c.Root).Returns(@"X:\packages\");
            return fileSystem.Object;
        }

        private class NeverCloseMemoryStream : MemoryStream
        {
            public NeverCloseMemoryStream(string content)
                : base(Encoding.UTF8.GetBytes(content))
            {
            }

            protected override void Dispose(bool disposing)
            {
                // Do nothing
            }
        }
    }
}

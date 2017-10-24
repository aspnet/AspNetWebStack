// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using System.Web.WebPages.Razor.Configuration;
using System.Web.WebPages.Razor.Resources;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Razor.Test
{
    public class WebRazorHostFactoryTest
    {
        public class TestFactory : WebRazorHostFactory
        {
            public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath = null)
            {
                return new TestHost();
            }
        }

        public class TestHost : WebPageRazorHost
        {
            public TestHost()
                : base("Foo.cshtml")
            {
            }

            public new void RegisterSpecialFile(string fileName, Type baseType)
            {
                base.RegisterSpecialFile(fileName, baseType);
            }

            public new void RegisterSpecialFile(string fileName, string baseType)
            {
                base.RegisterSpecialFile(fileName, baseType);
            }
        }

        [Fact]
        public void CreateHostReturnsWebPageHostWithWebPageAsBaseClassIfVirtualPathIsNormalPage()
        {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/Baz.cshtml", null);

            // Assert
            Assert.IsType<WebPageRazorHost>(host);
            Assert.Equal(WebPageRazorHost.PageBaseClass, host.DefaultBaseClass);
        }

        [Fact]
        public void CreateHostReturnsWebPageHostWithInitPageAsBaseClassIfVirtualPathIsPageStart()
        {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/_pagestart.cshtml", null);

            // Assert
            Assert.IsType<WebPageRazorHost>(host);
            Assert.Equal(typeof(StartPage).FullName, host.DefaultBaseClass);
        }

        [Fact]
        public void CreateHostReturnsWebPageHostWithStartPageAsBaseClassIfVirtualPathIsAppStart()
        {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/_appstart.cshtml", null);

            // Assert
            Assert.IsType<WebPageRazorHost>(host);
            Assert.Equal(typeof(ApplicationStartPage).FullName, host.DefaultBaseClass);
        }

        [Fact]
        public void CreateHostPassesPhysicalPathOnToWebCodeRazorHost()
        {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/Baz/App_Code/Bar", @"C:\Foo.cshtml");

            // Assert
            Assert.Equal(@"C:\Foo.cshtml", host.PhysicalPath);
        }

        [Fact]
        public void CreateHostPassesPhysicalPathOnToWebPageRazorHost()
        {
            // Act
            WebPageRazorHost host = new WebRazorHostFactory().CreateHost("~/Foo/Bar/Baz/Bar", @"C:\Foo.cshtml");

            // Assert
            Assert.Equal(@"C:\Foo.cshtml", host.PhysicalPath);
        }

        [Fact]
        public void CreateHostFromConfigRequiresNonNullVirtualPath()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => WebRazorHostFactory.CreateHostFromConfig(virtualPath: null,
                                                                                                physicalPath: "foo"), "virtualPath");
            Assert.ThrowsArgumentNullOrEmptyString(() => WebRazorHostFactory.CreateHostFromConfig(config: new RazorWebSectionGroup(),
                                                                                                virtualPath: null,
                                                                                                physicalPath: "foo"), "virtualPath");
        }

        [Fact]
        public void CreateHostFromConfigRequiresNonEmptyVirtualPath()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => WebRazorHostFactory.CreateHostFromConfig(virtualPath: String.Empty,
                                                                                                physicalPath: "foo"), "virtualPath");
            Assert.ThrowsArgumentNullOrEmptyString(() => WebRazorHostFactory.CreateHostFromConfig(config: new RazorWebSectionGroup(),
                                                                                                virtualPath: String.Empty,
                                                                                                physicalPath: "foo"), "virtualPath");
        }

        [Fact]
        public void CreateHostFromConfigRequiresNonNullSectionGroup()
        {
            Assert.ThrowsArgumentNull(() => WebRazorHostFactory.CreateHostFromConfig(config: (RazorWebSectionGroup)null,
                                                                                         virtualPath: String.Empty,
                                                                                         physicalPath: "foo"), "config");
        }

        [Fact]
        public void CreateHostFromConfigReturnsWebCodeHostIfVirtualPathStartsWithAppCode()
        {
            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfigCore(null, "~/App_Code/Bar.cshtml", null);

            // Assert
            Assert.IsType<WebCodeRazorHost>(host);
        }

        [Fact]
        public void CreateHostFromConfigUsesDefaultFactoryIfNoRazorWebSectionGroupFound()
        {
            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfigCore(null, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsType<WebPageRazorHost>(host);
        }

        [Fact]
        public void CreateHostFromConfigUsesDefaultFactoryIfNoHostSectionFound()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = null,
                Pages = null
            };

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsType<WebPageRazorHost>(host);
        }

        [Fact]
        public void CreateHostFromConfigUsesDefaultFactoryIfNullFactoryType()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = new HostSection()
                {
                    FactoryType = null
                },
                Pages = null
            };

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsType<WebPageRazorHost>(host);
        }

        [Fact]
        public void CreateHostFromConfigUsesFactorySpecifiedInConfig()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = new HostSection()
                {
                    FactoryType = typeof(TestFactory).FullName
                },
                Pages = null
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.IsType<TestHost>(host);
        }

        [Fact]
        public void CreateHostFromConfigThrowsInvalidOperationExceptionIfFactoryTypeNotFound()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = new HostSection()
                {
                    FactoryType = "Foo"
                },
                Pages = null
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            Assert.Throws<InvalidOperationException>(
                () => WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null),
                String.Format(RazorWebResources.Could_Not_Locate_FactoryType, "Foo"));
        }

        [Fact]
        public void CreateHostFromConfigAppliesBaseTypeFromConfigToHost()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = null,
                Pages = new RazorPagesSection()
                {
                    PageBaseType = "System.Foo.Bar"
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.Equal("System.Foo.Bar", host.DefaultBaseClass);
        }

        [Fact]
        public void CreateHostFromConfigIgnoresBaseTypeFromConfigIfPageIsPageStart()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = null,
                Pages = new RazorPagesSection()
                {
                    PageBaseType = "System.Foo.Bar"
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/_pagestart.cshtml", null);

            // Assert
            Assert.Equal(typeof(StartPage).FullName, host.DefaultBaseClass);
        }

        [Fact]
        public void CreateHostFromConfigIgnoresBaseTypeFromConfigIfPageIsAppStart()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = null,
                Pages = new RazorPagesSection()
                {
                    PageBaseType = "System.Foo.Bar"
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/_appstart.cshtml", null);

            // Assert
            Assert.Equal(typeof(ApplicationStartPage).FullName, host.DefaultBaseClass);
        }

        [Fact]
        public void CreateHostFromConfigMergesNamespacesFromConfigToHost()
        {
            // Arrange
            RazorWebSectionGroup config = new RazorWebSectionGroup()
            {
                Host = null,
                Pages = new RazorPagesSection()
                {
                    Namespaces = new NamespaceCollection()
                    {
                        new NamespaceInfo("System"),
                        new NamespaceInfo("Foo")
                    }
                }
            };
            WebRazorHostFactory.TypeFactory = name => Assembly.GetExecutingAssembly().GetType(name, throwOnError: false);

            // Act
            WebPageRazorHost host = WebRazorHostFactory.CreateHostFromConfig(config, "/Foo/Bar.cshtml", null);

            // Assert
            Assert.True(host.NamespaceImports.Contains("System"));
            Assert.True(host.NamespaceImports.Contains("Foo"));
        }

        [Fact]
        public void HostFactoryTypeIsCorrectlyLoadedFromConfig()
        {
            // Act
            RazorWebSectionGroup group = GetRazorGroup();
            HostSection host = (HostSection)group.Host;

            // Assert
            Assert.NotNull(host);
            Assert.Equal("System.Web.WebPages.Razor.Test.TestRazorHostFactory, System.Web.WebPages.Razor.Test", host.FactoryType);
        }

        [Fact]
        public void PageBaseTypeIsCorrectlyLoadedFromConfig()
        {
            // Act
            RazorWebSectionGroup group = GetRazorGroup();
            RazorPagesSection pages = (RazorPagesSection)group.Pages;

            // Assert
            Assert.NotNull(pages);
            Assert.Equal("System.Web.WebPages.Razor.Test.TestPageBase, System.Web.WebPages.Razor.Test", pages.PageBaseType);
        }

        [Fact]
        public void NamespacesAreCorrectlyLoadedFromConfig()
        {
            // Act
            RazorWebSectionGroup group = GetRazorGroup();
            RazorPagesSection pages = (RazorPagesSection)group.Pages;

            // Assert
            Assert.NotNull(pages);
            NamespaceInfo namespaceInfo = Assert.IsType<NamespaceInfo>(Assert.Single(pages.Namespaces));
            Assert.Equal("System.Text.RegularExpressions", namespaceInfo.Namespace);
        }

        [Fact]
        public void RegisterSpecialFile_ThrowsOnNullFileName()
        {
            TestHost host = new TestHost();
            Assert.ThrowsArgumentNullOrEmptyString(() => host.RegisterSpecialFile(null, typeof(string)), "fileName");
            Assert.ThrowsArgumentNullOrEmptyString(() => host.RegisterSpecialFile(null, "string"), "fileName");
        }

        [Fact]
        public void RegisterSpecialFile_ThrowsOnEmptyFileName()
        {
            TestHost host = new TestHost();
            Assert.ThrowsArgumentNullOrEmptyString(() => host.RegisterSpecialFile(String.Empty, typeof(string)), "fileName");
            Assert.ThrowsArgumentNullOrEmptyString(() => host.RegisterSpecialFile(String.Empty, "string"), "fileName");
        }

        [Fact]
        public void RegisterSpecialFile_ThrowsOnNullBaseType()
        {
            TestHost host = new TestHost();
            Assert.ThrowsArgumentNull(() => host.RegisterSpecialFile("file", (Type)null), "baseType");
        }

        [Fact]
        public void RegisterSpecialFile_ThrowsOnNullBaseTypeName()
        {
            TestHost host = new TestHost();
            Assert.ThrowsArgumentNullOrEmptyString(() => host.RegisterSpecialFile("file", (string)null), "baseTypeName");
        }

        [Fact]
        public void RegisterSpecialFile_ThrowsOnEmptyBaseTypeName()
        {
            TestHost host = new TestHost();
            Assert.ThrowsArgumentNullOrEmptyString(() => host.RegisterSpecialFile("file", String.Empty), "baseTypeName");
        }

        private static RazorWebSectionGroup GetRazorGroup()
        {
            return (RazorWebSectionGroup)ConfigurationManager.OpenExeConfiguration(null).GetSectionGroup(RazorWebSectionGroup.GroupName);
        }
    }
}

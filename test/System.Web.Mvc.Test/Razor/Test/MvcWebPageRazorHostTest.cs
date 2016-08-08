// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Razor.Test
{
    public class MvcWebPageRazorHostTest
    {
        [Fact]
        public void Constructor()
        {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");

            Assert.Equal("foo.cshtml", host.VirtualPath);
            Assert.Equal("bar", host.PhysicalPath);
            Assert.Equal(typeof(WebViewPage).FullName, host.DefaultBaseClass);
        }

        [Fact]
        public void ConstructorRemovesUnwantedNamespaceImports()
        {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");

            Assert.False(host.NamespaceImports.Contains("System.Web.WebPages.Html"));

            // Even though MVC no longer needs to remove the following two namespaces
            // (because they are no longer imported by System.Web.WebPages), we want
            // to make sure that they don't get introduced again by default.
            Assert.False(host.NamespaceImports.Contains("WebMatrix.Data"));
            Assert.False(host.NamespaceImports.Contains("WebMatrix.WebData"));
        }

#if VB_ENABLED
        [Fact]
        public void DecorateGodeGenerator_ReplacesVBCodeGeneratorWithMvcSpecificOne() {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.vbhtml", "bar");
            var generator = new VBRazorCodeGenerator("someClass", "root.name", "foo.vbhtml", host);

            // Act
            var result = host.DecorateCodeGenerator(generator);

            // Assert
            Assert.IsType<MvcVBRazorCodeGenerator>(result);
            Assert.Equal("someClass", result.ClassName);
            Assert.Equal("root.name", result.RootNamespaceName);
            Assert.Equal("foo.vbhtml", result.SourceFileName);
            Assert.Same(host, result.Host);
        }
#endif

        [Fact]
        public void DecorateGodeGenerator_ReplacesCSharpCodeGeneratorWithMvcSpecificOne()
        {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            var generator = new CSharpRazorCodeGenerator("someClass", "root.name", "foo.cshtml", host);

            // Act
            var result = host.DecorateCodeGenerator(generator);

            // Assert
            Assert.IsType<MvcCSharpRazorCodeGenerator>(result);
            Assert.Equal("someClass", result.ClassName);
            Assert.Equal("root.name", result.RootNamespaceName);
            Assert.Equal("foo.cshtml", result.SourceFileName);
            Assert.Same(host, result.Host);
        }

        [Fact]
        public void DecorateCodeParser_ThrowsOnNull()
        {
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            Assert.ThrowsArgumentNull(delegate() { host.DecorateCodeParser(null); }, "incomingCodeParser");
        }

        [Fact]
        public void DecorateCodeParser_ReplacesCSharpCodeParserWithMvcSpecificOne()
        {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.cshtml", "bar");
            var parser = new CSharpCodeParser();

            // Act
            var result = host.DecorateCodeParser(parser);

            // Assert
            Assert.IsType<MvcCSharpRazorCodeParser>(result);
        }

#if VB_ENABLED
        [Fact]
        public void DecorateCodeParser_ReplacesVBCodeParserWithMvcSpecificOne() {
            // Arrange
            MvcWebPageRazorHost host = new MvcWebPageRazorHost("foo.vbhtml", "bar");
            var parser = new VBCodeParser();

            // Act
            var result = host.DecorateCodeParser(parser);

            // Assert
            Assert.IsType<MvcVBRazorCodeParser>(result);
        }
#endif
    }
}

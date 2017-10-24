// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Collections.Generic;
using System.Web.Razor;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Razor.Test
{
    public class MvcCSharpRazorCodeGeneratorTest
    {
        [Fact]
        public void Constructor()
        {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);

            // Assert
            Assert.Equal("FooClass", generator.ClassName);
            Assert.Equal("Root.Namespace", generator.RootNamespaceName);
            Assert.Equal("SomeSourceFile.cshtml", generator.SourceFileName);
            Assert.Same(mockHost.Object, generator.Host);
        }

        [Fact]
        public void Constructor_DoesNotSetBaseTypeForNonMvcHost()
        {
            // Arrange
            Mock<RazorEngineHost> mockHost = new Mock<RazorEngineHost>();
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);

            // Assert
            Assert.Empty(generator.Context.GeneratedClass.BaseTypes);
        }

        [Fact]
        public void Constructor_DoesNotSetBaseTypeForSpecialPage()
        {
            // Arrange
            Mock<MvcWebPageRazorHost> mockHost = new Mock<MvcWebPageRazorHost>("_viewStart.cshtml", "_viewStart.cshtml");
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "_viewStart.cshtml", mockHost.Object);

            // Assert
            Assert.Empty(generator.Context.GeneratedClass.BaseTypes);
        }

        [Fact]
        public void Constructor_SetsBaseTypeForRegularPage()
        {
            // Arrange
            Mock<MvcWebPageRazorHost> mockHost = new Mock<MvcWebPageRazorHost>("SomeSourceFile.cshtml", "SomeSourceFile.cshtml") { CallBase = true };
            mockHost.SetupGet(h => h.NamespaceImports).Returns(new HashSet<string>());

            // Act
            var generator = new MvcCSharpRazorCodeGenerator("FooClass", "Root.Namespace", "SomeSourceFile.cshtml", mockHost.Object);

            // Assert
            var baseType = Assert.IsType<CodeTypeReference>(Assert.Single(generator.Context.GeneratedClass.BaseTypes));
            Assert.Equal("System.Web.Mvc.WebViewPage<dynamic>", baseType.BaseType);
        }
    }
}

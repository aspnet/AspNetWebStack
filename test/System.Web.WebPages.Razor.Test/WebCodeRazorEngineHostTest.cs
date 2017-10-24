// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Linq;
using System.Web.Razor.Generator;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Razor.Test
{
    public class WebCodeRazorEngineHostTest
    {
        [Fact]
        public void ConstructorWithMalformedVirtualPathSetsDefaultProperties()
        {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost(@"~/Foo/App_Code\Bar\Baz\Qux.cshtml");

            // Assert
            Assert.Equal("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.Equal("ASP.Bar.Baz", host.DefaultNamespace);
            Assert.Equal("Qux", host.DefaultClassName);
            Assert.False(host.DefaultDebugCompilation);
            Assert.True(host.StaticHelpers);
        }

        [Fact]
        public void ConstructorWithFileOnlyVirtualPathSetsDefaultProperties()
        {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost(@"Foo.cshtml");

            // Assert
            Assert.Equal("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.Equal("ASP", host.DefaultNamespace);
            Assert.Equal("Foo", host.DefaultClassName);
            Assert.False(host.DefaultDebugCompilation);
        }

        [Fact]
        public void ConstructorWithVirtualPathSetsDefaultProperties()
        {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost("~/Foo/App_Code/Bar/Baz/Qux.cshtml");

            // Assert
            Assert.Equal("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.Equal("ASP.Bar.Baz", host.DefaultNamespace);
            Assert.Equal("Qux", host.DefaultClassName);
            Assert.False(host.DefaultDebugCompilation);
        }

        [Fact]
        public void ConstructorWithVirtualAndPhysicalPathSetsDefaultProperties()
        {
            // Act
            WebCodeRazorHost host = new WebCodeRazorHost("~/Foo/App_Code/Bar/Baz/Qux.cshtml", @"C:\Qux.doodad");

            // Assert
            Assert.Equal("System.Web.WebPages.HelperPage", host.DefaultBaseClass);
            Assert.Equal("ASP.Bar.Baz", host.DefaultNamespace);
            Assert.Equal("Qux", host.DefaultClassName);
            Assert.False(host.DefaultDebugCompilation);
        }

        [Fact]
        public void PostProcessGeneratedCodeRemovesExecuteMethod()
        {
            // Arrange
            WebCodeRazorHost host = new WebCodeRazorHost("Foo.cshtml");
            CodeGeneratorContext context = CodeGeneratorContext.Create(
                host,
                () => new CSharpCodeWriter(),
                "TestClass",
                "TestNamespace",
                "TestFile.cshtml",
                shouldGenerateLinePragmas: true);

            // Act
            host.PostProcessGeneratedCode(context);

            // Assert
            Assert.Empty(context.GeneratedClass.Members.OfType<CodeMemberMethod>());
        }

        [Fact]
        public void PostProcessGeneratedCodeAddsStaticApplicationInstanceProperty()
        {
            // Arrange
            WebCodeRazorHost host = new WebCodeRazorHost("Foo.cshtml");
            CodeGeneratorContext context =
                CodeGeneratorContext.Create(
                    host,
                    () => new CSharpCodeWriter(),
                    "TestClass",
                    "TestNamespace",
                    "Foo.cshtml",
                    shouldGenerateLinePragmas: true);

            // Act
            host.PostProcessGeneratedCode(context);

            // Assert
            CodeMemberProperty appInstance = context.GeneratedClass
                .Members
                .OfType<CodeMemberProperty>()
                .Where(p => p.Name.Equals("ApplicationInstance"))
                .SingleOrDefault();
            Assert.NotNull(appInstance);
            Assert.True(appInstance.Attributes.HasFlag(MemberAttributes.Static));
        }
    }
}

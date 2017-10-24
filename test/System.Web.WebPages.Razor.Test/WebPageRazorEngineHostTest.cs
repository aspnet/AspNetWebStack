// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Razor;
using System.Web.Razor.Generator;
using Microsoft.CSharp;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Razor.Test
{
    public class WebPageRazorEngineHostTest
    {
        [Fact]
        public void ConstructorRequiresNonNullOrEmptyVirtualPath()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebPageRazorHost(null), "virtualPath");
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebPageRazorHost(String.Empty), "virtualPath");
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebPageRazorHost(null, "foo"), "virtualPath");
            Assert.ThrowsArgumentNullOrEmptyString(() => new WebPageRazorHost(String.Empty, "foo"), "virtualPath");
        }

        [Fact]
        public void ConstructorWithVirtualPathUsesItToDetermineBaseClassClassNameAndLanguage()
        {
            // Act
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Bar.cshtml");

            // Assert
            Assert.Equal("_Page_Foo_Bar_cshtml", host.DefaultClassName);
            Assert.Equal("System.Web.WebPages.WebPage", host.DefaultBaseClass);
            Assert.IsType<CSharpRazorCodeLanguage>(host.CodeLanguage);
            Assert.False(host.StaticHelpers);
        }

        [Fact]
        public void PostProcessGeneratedCodeAddsGlobalImports()
        {
            // Arrange
            WebPageRazorHost.AddGlobalImport("Foo.Bar");
            WebPageRazorHost host = new WebPageRazorHost("Foo.cshtml");
            CodeGeneratorContext context = CodeGeneratorContext.Create(
                host,
                () => new CSharpCodeWriter(),
                "TestClass",
                "TestNs",
                "TestFile.cshtml",
                shouldGenerateLinePragmas: true);

            // Act
            host.PostProcessGeneratedCode(context);

            // Assert
            Assert.Contains(context.Namespace.Imports.OfType<CodeNamespaceImport>(), import => String.Equals("Foo.Bar", import.Namespace));
        }

        [Fact]
        public void PostProcessGeneratedCodeAddsApplicationInstanceProperty()
        {
            string expectedPropertyCode =
                Environment.NewLine
              + "protected Foo.Bar ApplicationInstance {" + Environment.NewLine
              + "    get {" + Environment.NewLine
              + "        return ((Foo.Bar)(Context.ApplicationInstance));" + Environment.NewLine
              + "    }" + Environment.NewLine
              + "}" + Environment.NewLine;

            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("Foo.cshtml")
            {
                GlobalAsaxTypeName = "Foo.Bar"
            };
            CodeGeneratorContext context = CodeGeneratorContext.Create(
                host,
                () => new CSharpCodeWriter(),
                "TestClass",
                "TestNs",
                "TestFile.cshtml",
                shouldGenerateLinePragmas: true);

            // Act
            host.PostProcessGeneratedCode(context);

            // Assert
            CodeMemberProperty property = context.GeneratedClass.Members[0] as CodeMemberProperty;
            Assert.NotNull(property);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringBuilder builder = new StringBuilder();
            using (StringWriter writer = new StringWriter(builder))
            {
                provider.GenerateCodeFromMember(property, writer, new CodeGeneratorOptions());
            }

            Assert.Equal(expectedPropertyCode, builder.ToString());
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using ASP;
using Microsoft.CSharp;
using Microsoft.TestCommon;
using Moq;

namespace ASP
{
    public class _Page_Foo_Test_cshtml
    {
    }
}

namespace System.Web.WebPages.Razor.Test
{
    public class RazorBuildProviderTest
    {
        private class MockAssemblyBuilder : IAssemblyBuilder
        {
            public BuildProvider BuildProvider { get; private set; }
            public CodeCompileUnit CompileUnit { get; private set; }
            public string LastTypeFactoryGenerated { get; private set; }

            public void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit)
            {
                BuildProvider = buildProvider;
                CompileUnit = compileUnit;
            }

            public void GenerateTypeFactory(string typeName)
            {
                LastTypeFactoryGenerated = typeName;
            }
        }

        [Fact]
        public void CodeCompilerTypeReturnsTypeFromCodeLanguage()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            // Act
            CompilerType type = provider.CodeCompilerType;

            // Assert
            Assert.Equal(typeof(CSharpCodeProvider), type.CodeDomProviderType);
        }

        [Fact]
        public void CodeCompilerTypeSetsDebugFlagInFullTrust()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            // Act
            CompilerType type = provider.CodeCompilerType;

            // Assert
            Assert.True(type.CompilerParameters.IncludeDebugInformation);
        }

        [Fact]
        public void GetGeneratedTypeUsesNameAndNamespaceFromHostToExtractType()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Test.cshtml", @"C:\Foo\Test.cshtml");
            RazorBuildProvider provider = new RazorBuildProvider() { Host = host };
            CompilerResults results = new CompilerResults(new TempFileCollection());
            results.CompiledAssembly = typeof(_Page_Foo_Test_cshtml).Assembly;

            // Act
            Type typ = provider.GetGeneratedType(results);

            // Assert
            Assert.Equal(typeof(_Page_Foo_Test_cshtml), typ);
        }

        [Fact]
        public void GenerateCodeCoreAddsGeneratedCodeToAssemblyBuilder()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = new RazorBuildProvider();
            CodeCompileUnit ccu = new CodeCompileUnit();
            MockAssemblyBuilder asmBuilder = new MockAssemblyBuilder();
            provider.Host = host;
            provider.GeneratedCode = ccu;

            // Act
            provider.GenerateCodeCore(asmBuilder);

            // Assert
            Assert.Same(provider, asmBuilder.BuildProvider);
            Assert.Same(ccu, asmBuilder.CompileUnit);
            Assert.Equal("ASP._Page_Foo_Baz_cshtml", asmBuilder.LastTypeFactoryGenerated);
        }

        [Fact]
        public void CodeGenerationStartedTest()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            // Expected original base dependencies
            var baseDependencies = new ArrayList();
            baseDependencies.Add("/Samples/Foo/Baz.cshtml");

            // Expected list of dependencies after GenerateCode is called
            var dependencies = new ArrayList();
            dependencies.Add(baseDependencies[0]);
            dependencies.Add("/Samples/Foo/Foo.cshtml");

            // Set up the event handler
            provider.CodeGenerationStartedInternal += (sender, e) =>
            {
                var bp = sender as RazorBuildProvider;
                bp.AddVirtualPathDependency("/Samples/Foo/Foo.cshtml");
            };

            // Set up the base dependency
            MockAssemblyBuilder builder = new MockAssemblyBuilder();
            typeof(BuildProvider).GetField("_virtualPath", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(provider, CreateVirtualPath("/Samples/Foo/Baz.cshtml"));

            // Test that VirtualPathDependencies returns the original dependency before GenerateCode is called
            Assert.True(baseDependencies.OfType<string>().SequenceEqual(provider.VirtualPathDependencies.OfType<string>()));

            // Act
            provider.GenerateCodeCore(builder);

            // Assert
            Assert.NotNull(provider.AssemblyBuilderInternal);
            Assert.Equal(builder, provider.AssemblyBuilderInternal);
            Assert.True(dependencies.OfType<string>().SequenceEqual(provider.VirtualPathDependencies.OfType<string>()));
            Assert.Equal("/Samples/Foo/Baz.cshtml", provider.VirtualPath);
        }

        [Fact]
        public void AfterGeneratedCodeEventGetsExecutedAtCorrectTime()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @bar baz");
            provider.Host = host;

            provider.CodeGenerationCompletedInternal += (sender, e) =>
            {
                Assert.Equal("~/Foo/Baz.cshtml", e.VirtualPath);
                e.GeneratedCode.Namespaces.Add(new CodeNamespace("DummyNamespace"));
            };

            // Act
            CodeCompileUnit generated = provider.GeneratedCode;

            // Assert
            Assert.NotNull(generated.Namespaces
                               .OfType<CodeNamespace>()
                               .SingleOrDefault(ns => String.Equals(ns.Name, "DummyNamespace")));
        }

        [Fact]
        public void GeneratedCodeThrowsHttpParseExceptionForLastParserError()
        {
            // Arrange
            WebPageRazorHost host = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo @{ if( baz");
            provider.Host = host;

            // Act
            Assert.Throws<HttpParseException>(() => { CodeCompileUnit ccu = provider.GeneratedCode; });
        }

        [Fact]
        public void BuildProviderFiresEventToAlterHostBeforeBuildingPath()
        {
            // Arrange
            WebPageRazorHost expected = new TestHost("~/Foo/Boz.cshtml", @"C:\Foo\Boz.cshtml");
            WebPageRazorHost expectedBefore = new WebPageRazorHost("~/Foo/Baz.cshtml", @"C:\Foo\Baz.cshtml");
            RazorBuildProvider provider = CreateBuildProvider("foo");
            typeof(BuildProvider).GetField("_virtualPath", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(provider, CreateVirtualPath("/Samples/Foo/Baz.cshtml"));
            Mock.Get(provider).Setup(p => p.GetHostFromConfig()).Returns(expectedBefore);
            bool called = false;
            EventHandler<CompilingPathEventArgs> handler = (sender, args) =>
            {
                Assert.Equal("/Samples/Foo/Baz.cshtml", args.VirtualPath);
                Assert.Same(expectedBefore, args.Host);
                args.Host = expected;
                called = true;
            };
            RazorBuildProvider.CompilingPath += handler;

            try
            {
                // Act
                CodeCompileUnit ccu = provider.GeneratedCode;

                // Assert
                Assert.Equal("Test", ccu.Namespaces[0].Name);
                Assert.Same(expected, provider.Host);
                Assert.True(called);
            }
            finally
            {
                RazorBuildProvider.CompilingPath -= handler;
            }
        }

        private static object CreateVirtualPath(string path)
        {
            var vPath = typeof(BuildProvider).Assembly.GetType("System.Web.VirtualPath");
            var method = vPath.GetMethod("CreateNonRelative", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return method.Invoke(null, new object[] { path });
        }

        private static RazorBuildProvider CreateBuildProvider(string razorContent)
        {
            Mock<RazorBuildProvider> mockProvider = new Mock<RazorBuildProvider>()
            {
                CallBase = true
            };
            mockProvider.Setup(p => p.InternalOpenReader())
                .Returns(() => new StringReader(razorContent));
            return mockProvider.Object;
        }

        private class TestHost : WebPageRazorHost
        {
            public TestHost(string virtualPath, string physicalPath) : base(virtualPath, physicalPath) { }

            public override void PostProcessGeneratedCode(Web.Razor.Generator.CodeGeneratorContext context)
            {
                context.CompileUnit.Namespaces.Insert(0, new CodeNamespace("Test"));
            }
        }
    }
}

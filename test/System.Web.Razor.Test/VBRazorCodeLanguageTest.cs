// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using Microsoft.TestCommon;
using Microsoft.VisualBasic;

namespace System.Web.Razor.Test
{
    public class VBRazorCodeLanguageTest
    {
        [Fact]
        public void CreateCodeParserReturnsNewVBCodeParser()
        {
            // Arrange
            RazorCodeLanguage service = new VBRazorCodeLanguage();

            // Act
            ParserBase parser = service.CreateCodeParser();

            // Assert
            Assert.NotNull(parser);
            Assert.IsType<VBCodeParser>(parser);
        }

        [Fact]
        public void CreateCodeGeneratorParserListenerReturnsNewCSharpCodeGeneratorParserListener()
        {
            // Arrange
            RazorCodeLanguage service = new VBRazorCodeLanguage();

            // Act
            RazorEngineHost host = new RazorEngineHost(new VBRazorCodeLanguage());
            RazorCodeGenerator generator = service.CreateCodeGenerator("Foo", "Bar", "Baz", host);

            // Assert
            Assert.NotNull(generator);
            Assert.IsType<VBRazorCodeGenerator>(generator);
            Assert.Equal("Foo", generator.ClassName);
            Assert.Equal("Bar", generator.RootNamespaceName);
            Assert.Equal("Baz", generator.SourceFileName);
            Assert.Same(host, generator.Host);
        }

        [Fact]
        public void CodeDomProviderTypeReturnsVBCodeProvider()
        {
            // Arrange
            RazorCodeLanguage service = new VBRazorCodeLanguage();

            // Assert
            Assert.Equal(typeof(VBCodeProvider), service.CodeDomProviderType);
        }
    }
}

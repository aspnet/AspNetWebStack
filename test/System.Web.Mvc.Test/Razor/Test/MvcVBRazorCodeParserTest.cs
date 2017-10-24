// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Razor.Test
{
    public class MvcVBRazorCodeParserTest
    {
        [Fact]
        public void Constructor_AddsModelKeyword()
        {
            var parser = new MvcVBRazorCodeParser();

            Assert.True(parser.IsDirectiveDefined(MvcVBRazorCodeParser.ModelTypeKeyword));
        }

        [Fact]
        public void ParseModelKeyword_HandlesSingleInstance()
        {
            // Arrange + Act
            var document = "@ModelType    Foo";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType    ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}(Of {1})"))
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_HandlesNullableTypes()
        {
            // Arrange + Act
            var document = "@ModelType Foo?\r\nBar";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo?\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo?", "{0}(Of {1})")),
                factory.Markup("Bar")
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_HandlesArrays()
        {
            // Arrange + Act
            var document = "@ModelType Foo(())()\r\nBar";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo(())()\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo(())()", "{0}(Of {1})")),
                factory.Markup("Bar")
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_HandlesVSTemplateSyntax()
        {
            // Arrange + Act
            var document = "@ModelType $rootnamespace$.MyModel";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("$rootnamespace$.MyModel")
                    .As(new SetModelTypeCodeGenerator("$rootnamespace$.MyModel", "{0}(Of {1})"))
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_ErrorOnMissingModelType()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document = "@ModelType   ";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType   ")
                    .Accepts(AcceptedCharacters.None),
                factory.EmptyVB()
                    .As(new SetModelTypeCodeGenerator(String.Empty, "{0}(Of {1})"))
                    .Accepts(AcceptedCharacters.Any)
            };
            var expectedErrors = new[]
            {
                new RazorError("The 'ModelType' keyword must be followed by a type name on the same line.", new SourceLocation(10, 0, 10), 1)
            };
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Equal(expectedErrors, errors.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_DoesNotAcceptNewlineIfInDesignTimeMode()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document = "@ModelType foo\r\n";
            var spans = ParseDocument(document, errors, designTimeMode: true);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("foo")
                    .As(new SetModelTypeCodeGenerator("foo", "{0}(Of {1})"))
                    .Accepts(AcceptedCharacters.Any),
                factory.Markup("\r\n")
            };
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Empty(errors);
        }

        [Fact]
        public void ParseModelKeyword_ErrorOnMultipleModelStatements()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
                "@ModelType Foo" + Environment.NewLine
              + "@ModelType Bar";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}(Of {1})")),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Bar")
                    .As(new SetModelTypeCodeGenerator("Bar", "{0}(Of {1})"))
            };

            var expectedErrors = new[]
            {
                new RazorError("Only one 'ModelType' statement is allowed in a file.", new SourceLocation(26, 1, 10), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.Equal(i.expected, i.span));
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Equal(expectedErrors, errors.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_ErrorOnModelFollowedByInherits()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
                "@ModelType Foo" + Environment.NewLine
              + "@Inherits Bar";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}(Of {1})")),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("Inherits ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Bar")
                    .As(new SetBaseTypeCodeGenerator("Bar"))
            };

            var expectedErrors = new[]
            {
                new RazorError("The 'inherits' keyword is not allowed when a 'ModelType' keyword is used.", new SourceLocation(25, 1, 9), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.Equal(i.expected, i.span));
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Equal(expectedErrors, errors.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_ErrorOnInheritsFollowedByModel()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
                "@Inherits Bar" + Environment.NewLine
              + "@ModelType Foo";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateVbHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("Inherits ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Bar\r\n")
                    .AsBaseType("Bar"),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("ModelType ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}(Of {1})"))
            };

            var expectedErrors = new[]
            {
                new RazorError("The 'inherits' keyword is not allowed when a 'ModelType' keyword is used.", new SourceLocation(9, 0, 9), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.Equal(i.expected, i.span));
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Equal(expectedErrors, errors.ToArray());
        }

        private static List<Span> ParseDocument(string documentContents, List<RazorError> errors = null, bool designTimeMode = false)
        {
            errors = errors ?? new List<RazorError>();
            var markupParser = new HtmlMarkupParser();
            var codeParser = new MvcVBRazorCodeParser();
            var context = new ParserContext(new SeekableTextReader(documentContents), codeParser, markupParser, markupParser);
            context.DesignTimeMode = designTimeMode;
            codeParser.Context = context;
            markupParser.Context = context;
            markupParser.ParseDocument();

            ParserResults results = context.CompleteParse();
            foreach (RazorError error in results.ParserErrors)
            {
                errors.Add(error);
            }
            return results.Document.Flatten().ToList();
        }
    }
}

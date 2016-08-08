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
    public class MvcCSharpRazorCodeParserTest
    {
        [Fact]
        public void Constructor_AddsModelKeyword()
        {
            var parser = new TestMvcCSharpRazorCodeParser();

            Assert.True(parser.HasDirective("model"));
        }

        [Fact]
        public void ParseModelKeyword_HandlesSingleInstance()
        {
            // Arrange + Act
            var document = "@model    Foo";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("   Foo")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}<{1}>"))
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_HandlesNullableTypes()
        {
            // Arrange + Act
            var document = "@model Foo?\r\nBar";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo?\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo?", "{0}<{1}>")),
                factory.Markup("Bar")
                    .With(new MarkupCodeGenerator())
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_HandlesArrays()
        {
            // Arrange + Act
            var document = "@model Foo[[]][]\r\nBar";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo[[]][]\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo[[]][]", "{0}<{1}>")),
                factory.Markup("Bar")
                    .With(new MarkupCodeGenerator())
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_HandlesVSTemplateSyntax()
        {
            // Arrange + Act
            var document = "@model $rootnamespace$.MyModel";
            var spans = ParseDocument(document);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("$rootnamespace$.MyModel")
                    .As(new SetModelTypeCodeGenerator("$rootnamespace$.MyModel", "{0}<{1}>"))
            };
            Assert.Equal(expectedSpans, spans.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_ErrorOnMissingModelType()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document = "@model   ";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("  ")
                    .As(new SetModelTypeCodeGenerator(String.Empty, "{0}<{1}>")),
            };
            var expectedErrors = new[]
            {
                new RazorError("The 'model' keyword must be followed by a type name on the same line.", new SourceLocation(9, 0, 9), 1)
            };
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Equal(expectedErrors, errors.ToArray());
        }

        [Fact]
        public void ParseModelKeyword_ErrorOnMultipleModelStatements()
        {
            // Arrange + Act
            List<RazorError> errors = new List<RazorError>();
            var document =
                "@model Foo" + Environment.NewLine
              + "@model Bar";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}<{1}>")),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Bar")
                    .As(new SetModelTypeCodeGenerator("Bar", "{0}<{1}>"))
            };

            var expectedErrors = new[]
            {
                new RazorError("Only one 'model' statement is allowed in a file.", new SourceLocation(18, 1, 6), 1)
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
                "@model Foo" + Environment.NewLine
              + "@inherits Bar";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo\r\n")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}<{1}>")),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("inherits ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Bar")
                    .As(new SetBaseTypeCodeGenerator("Bar"))
            };

            var expectedErrors = new[]
            {
                new RazorError("The 'inherits' keyword is not allowed when a 'model' keyword is used.", new SourceLocation(21, 1, 9), 1)
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
                "@inherits Bar" + Environment.NewLine
              + "@model Foo";
            var spans = ParseDocument(document, errors);

            // Assert
            var factory = SpanFactory.CreateCsHtml();
            var expectedSpans = new Span[]
            {
                factory.EmptyHtml(),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("inherits ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Bar" + Environment.NewLine)
                    .As(new SetBaseTypeCodeGenerator("Bar")),
                factory.CodeTransition(SyntaxConstants.TransitionString)
                    .Accepts(AcceptedCharacters.None),
                factory.MetaCode("model ")
                    .Accepts(AcceptedCharacters.None),
                factory.Code("Foo")
                    .As(new SetModelTypeCodeGenerator("Foo", "{0}<{1}>"))
            };

            var expectedErrors = new[]
            {
                new RazorError("The 'inherits' keyword is not allowed when a 'model' keyword is used.", new SourceLocation(9, 0, 9), 1)
            };
            expectedSpans.Zip(spans, (exp, span) => new { expected = exp, span = span }).ToList().ForEach(i => Assert.Equal(i.expected, i.span));
            Assert.Equal(expectedSpans, spans.ToArray());
            Assert.Equal(expectedErrors, errors.ToArray());
        }

        private static List<Span> ParseDocument(string documentContents, IList<RazorError> errors = null)
        {
            errors = errors ?? new List<RazorError>();
            var markupParser = new HtmlMarkupParser();
            var codeParser = new TestMvcCSharpRazorCodeParser();
            var context = new ParserContext(new SeekableTextReader(documentContents), codeParser, markupParser, markupParser);
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

        private sealed class TestMvcCSharpRazorCodeParser : MvcCSharpRazorCodeParser
        {
            public bool HasDirective(string directive)
            {
                Action handler;
                return TryGetDirectiveHandler(directive, out handler);
            }
        }
    }
}

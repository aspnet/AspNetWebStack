// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Razor.Test.Parser
{
    public class RazorParserTest
    {
        [Fact]
        public void ConstructorRequiresNonNullCodeParser()
        {
            Assert.ThrowsArgumentNull(() => new RazorParser(null, new HtmlMarkupParser()), "codeParser");
        }

        [Fact]
        public void ConstructorRequiresNonNullMarkupParser()
        {
            Assert.ThrowsArgumentNull(() => new RazorParser(new CSharpCodeParser(), null), "markupParser");
        }

        [Fact]
        public void ParseMethodCallsParseDocumentOnMarkupParserAndReturnsResults()
        {
            var factory = SpanFactory.CreateCsHtml();

            // Arrange
            RazorParser parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());

            // Act/Assert
            ParserTestBase.EvaluateResults(parser.Parse(new StringReader("foo @bar baz")),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("bar")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void Parse_ParsesDocumentAndInvokesVisitor()
        {
            // Arrange
            var factory = SpanFactory.CreateCsHtml();
            var visitor = new Mock<ParserVisitor> { CallBase = true };
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());

            // Act
            parser.Parse(new StringReader("foo @bar baz"), visitor.Object);

            // Assert
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Markup && s.Content == "foo ")));
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Transition && s.Content == "@")));
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Code && s.Content == "bar")));
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Markup && s.Content == " baz")));
        }

        [Fact]
        public async Task CreateParseTask_ParsesDocumentAndInvokesVisitor()
        {
            // Arrange
            var factory = SpanFactory.CreateCsHtml();
            var visitor = new Mock<ParserVisitor> { CallBase = true };
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());

            // Act
            var task = parser.CreateParseTask(new StringReader("foo @bar baz"), visitor.Object);
            task.Start();
            await task;

            // Assert
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Markup && s.Content == "foo ")));
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Transition && s.Content == "@")));
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Code && s.Content == "bar")));
            visitor.Verify(v => v.VisitSpan(It.Is<Span>(s => s.Kind == SpanKind.Markup && s.Content == " baz")));
        }

        [Fact]
        public async Task CreateParseTask_ReturnsWithoutThrowingIfTaskIsCancelled()
        {
            // Arrange
            var factory = SpanFactory.CreateCsHtml();
            var visitor = new Mock<ParserVisitor> { CallBase = true };
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());
            var cancelTokenSource = new CancellationTokenSource();
            var visited = false;

            // Act
            var task = parser.CreateParseTask(new StringReader("foo @bar baz"),
                                              (span) =>
                                              {
                                                  visited = true;
                                                  Assert.Equal("foo ", span.Content);
                                                  cancelTokenSource.Cancel();
                                              },
                                              errorCallback: null,
                                              cancelToken: cancelTokenSource.Token);
            task.Start();
            await task;

            // Assert
            Assert.True(visited);
        }

        [Fact]
        public void ParseMethodUsesProvidedParserListenerIfSpecified()
        {
            var factory = SpanFactory.CreateCsHtml();

            // Arrange
            RazorParser parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());

            // Act
            ParserResults results = parser.Parse(new StringReader("foo @bar baz"));

            // Assert
            ParserTestBase.EvaluateResults(results,
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("bar")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ParseMethodSetsUpRunWithSpecifiedCodeParserMarkupParserAndListenerAndPassesToMarkupParser()
        {
            RunParseWithListenerTest((parser, reader) => parser.Parse(reader));
        }

        private static void RunParseWithListenerTest(Action<RazorParser, TextReader> parserAction)
        {
            // Arrange
            ParserBase markupParser = new MockMarkupParser();
            ParserBase codeParser = new CSharpCodeParser();
            RazorParser parser = new RazorParser(codeParser, markupParser);
            TextReader expectedReader = new StringReader("foo");

            // Act
            parserAction(parser, expectedReader);

            // Assert
            ParserContext actualContext = markupParser.Context;
            Assert.NotNull(actualContext);
            Assert.Same(markupParser, actualContext.MarkupParser);
            Assert.Same(markupParser, actualContext.ActiveParser);
            Assert.Same(codeParser, actualContext.CodeParser);
        }

        private class MockMarkupParser : ParserBase
        {
            public override bool IsMarkupParser
            {
                get
                {
                    return true;
                }
            }

            public override void ParseDocument()
            {
                using (Context.StartBlock(BlockType.Markup))
                {
                }
            }

            public override void ParseSection(Tuple<string, string> nestingSequences, bool caseSensitive = true)
            {
                using (Context.StartBlock(BlockType.Markup))
                {
                }
            }

            public override void ParseBlock()
            {
                using (Context.StartBlock(BlockType.Markup))
                {
                }
            }

            protected override ParserBase OtherParser
            {
                get { return Context.CodeParser; }
            }

            public override void BuildSpan(SpanBuilder span, Razor.Text.SourceLocation start, string content)
            {
                throw new NotImplementedException();
            }
        }
    }
}

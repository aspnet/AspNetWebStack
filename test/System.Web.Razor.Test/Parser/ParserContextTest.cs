// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer.Symbols;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Razor.Test.Parser
{
    public class ParserContextTest
    {
        [Fact]
        public void ConstructorRequiresNonNullSource()
        {
            var codeParser = new CSharpCodeParser();
            Assert.ThrowsArgumentNull(() => new ParserContext(null, codeParser, new HtmlMarkupParser(), codeParser), "source");
        }

        [Fact]
        public void ConstructorRequiresNonNullCodeParser()
        {
            var codeParser = new CSharpCodeParser();
            Assert.ThrowsArgumentNull(() => new ParserContext(new SeekableTextReader(TextReader.Null), null, new HtmlMarkupParser(), codeParser), "codeParser");
        }

        [Fact]
        public void ConstructorRequiresNonNullMarkupParser()
        {
            var codeParser = new CSharpCodeParser();
            Assert.ThrowsArgumentNull(() => new ParserContext(new SeekableTextReader(TextReader.Null), codeParser, null, codeParser), "markupParser");
        }

        [Fact]
        public void ConstructorRequiresNonNullActiveParser()
        {
            Assert.ThrowsArgumentNull(() => new ParserContext(new SeekableTextReader(TextReader.Null), new CSharpCodeParser(), new HtmlMarkupParser(), null), "activeParser");
        }

        [Fact]
        public void ConstructorThrowsIfActiveParserIsNotCodeOrMarkupParser()
        {
            Assert.ThrowsArgument(() => new ParserContext(new SeekableTextReader(TextReader.Null), new CSharpCodeParser(), new HtmlMarkupParser(), new CSharpCodeParser()),
                                                    "activeParser",
                                                    RazorResources.ActiveParser_Must_Be_Code_Or_Markup_Parser);
        }

        [Fact]
        public void ConstructorAcceptsActiveParserIfIsSameAsEitherCodeOrMarkupParser()
        {
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            new ParserContext(new SeekableTextReader(TextReader.Null), codeParser, markupParser, codeParser);
            new ParserContext(new SeekableTextReader(TextReader.Null), codeParser, markupParser, markupParser);
        }

        [Fact]
        public void ConstructorInitializesProperties()
        {
            // Arrange
            SeekableTextReader expectedBuffer = new SeekableTextReader(TextReader.Null);
            CSharpCodeParser expectedCodeParser = new CSharpCodeParser();
            HtmlMarkupParser expectedMarkupParser = new HtmlMarkupParser();

            // Act
            ParserContext context = new ParserContext(expectedBuffer, expectedCodeParser, expectedMarkupParser, expectedCodeParser);

            // Assert
            Assert.NotNull(context.Source);
            Assert.Same(expectedCodeParser, context.CodeParser);
            Assert.Same(expectedMarkupParser, context.MarkupParser);
            Assert.Same(expectedCodeParser, context.ActiveParser);
        }

        [Fact]
        public void CurrentCharacterReturnsCurrentCharacterInTextBuffer()
        {
            // Arrange
            ParserContext context = SetupTestContext("bar", b => b.Read());

            // Act
            char actual = context.CurrentCharacter;

            // Assert
            Assert.Equal('a', actual);
        }

        [Fact]
        public void CurrentCharacterReturnsNulCharacterIfTextBufferAtEOF()
        {
            // Arrange
            ParserContext context = SetupTestContext("bar", b => b.ReadToEnd());

            // Act
            char actual = context.CurrentCharacter;

            // Assert
            Assert.Equal('\0', actual);
        }

        [Fact]
        public void EndOfFileReturnsFalseIfTextBufferNotAtEOF()
        {
            // Arrange
            ParserContext context = SetupTestContext("bar");

            // Act/Assert
            Assert.False(context.EndOfFile);
        }

        [Fact]
        public void EndOfFileReturnsTrueIfTextBufferAtEOF()
        {
            // Arrange
            ParserContext context = SetupTestContext("bar", b => b.ReadToEnd());

            // Act/Assert
            Assert.True(context.EndOfFile);
        }

        [Fact]
        public void StartBlockCreatesNewBlock()
        {
            // Arrange
            ParserContext context = SetupTestContext("phoo");

            // Act
            context.StartBlock(BlockType.Expression);

            // Assert
            BlockBuilder blockBuilder = Assert.Single(context.BlockStack);
            Assert.Equal(BlockType.Expression, blockBuilder.Type);
        }

        [Fact]
        public void EndBlockAddsCurrentBlockToParentBlock()
        {
            // Arrange
            Mock<ParserVisitor> mockListener = new Mock<ParserVisitor>();
            ParserContext context = SetupTestContext("phoo");

            // Act
            context.StartBlock(BlockType.Expression);
            context.StartBlock(BlockType.Statement);
            context.EndBlock();

            // Assert
            BlockBuilder blockBuilder = Assert.Single(context.BlockStack);
            Assert.Equal(BlockType.Expression, blockBuilder.Type);
            SyntaxTreeNode node = Assert.Single(blockBuilder.Children);
            Assert.Equal(BlockType.Statement, Assert.IsType<Block>(node).Type);
        }

        [Fact]
        public void AddSpanAddsSpanToCurrentBlockBuilder()
        {
            // Arrange
            var factory = SpanFactory.CreateCsHtml();
            Mock<ParserVisitor> mockListener = new Mock<ParserVisitor>();
            ParserContext context = SetupTestContext("phoo");

            SpanBuilder builder = new SpanBuilder()
            {
                Kind = SpanKind.Code
            };
            builder.Accept(new CSharpSymbol(1, 0, 1, "foo", CSharpSymbolType.Identifier));
            Span added = builder.Build();

            using (context.StartBlock(BlockType.Functions))
            {
                context.AddSpan(added);
            }

            BlockBuilder expected = new BlockBuilder()
            {
                Type = BlockType.Functions,
            };
            expected.Children.Add(added);

            // Assert
            ParserTestBase.EvaluateResults(context.CompleteParse(), expected.Build());
        }

        [Fact]
        public void SwitchActiveParserSetsMarkupParserAsActiveIfCodeParserCurrentlyActive()
        {
            // Arrange
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            ParserContext context = SetupTestContext("barbazbiz", b => b.Read(), codeParser, markupParser, codeParser);
            Assert.Same(codeParser, context.ActiveParser);

            // Act
            context.SwitchActiveParser();

            // Assert
            Assert.Same(markupParser, context.ActiveParser);
        }

        [Fact]
        public void SwitchActiveParserSetsCodeParserAsActiveIfMarkupParserCurrentlyActive()
        {
            // Arrange
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            ParserContext context = SetupTestContext("barbazbiz", b => b.Read(), codeParser, markupParser, markupParser);
            Assert.Same(markupParser, context.ActiveParser);

            // Act
            context.SwitchActiveParser();

            // Assert
            Assert.Same(codeParser, context.ActiveParser);
        }

        private ParserContext SetupTestContext(string document)
        {
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            return SetupTestContext(document, b => { }, codeParser, markupParser, codeParser);
        }

        private ParserContext SetupTestContext(string document, Action<TextReader> positioningAction)
        {
            var codeParser = new CSharpCodeParser();
            var markupParser = new HtmlMarkupParser();
            return SetupTestContext(document, positioningAction, codeParser, markupParser, codeParser);
        }

        private ParserContext SetupTestContext(string document, Action<TextReader> positioningAction, ParserBase codeParser, ParserBase markupParser, ParserBase activeParser)
        {
            ParserContext context = new ParserContext(new SeekableTextReader(new StringReader(document)), codeParser, markupParser, activeParser);
            positioningAction(context.Source);
            return context;
        }
    }
}

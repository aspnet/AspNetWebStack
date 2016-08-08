// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Generator;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.Html
{
    public class HtmlErrorTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void ParseBlockAllowsInvalidTagNamesAsLongAsParserCanIdentifyEndTag()
        {
            SingleSpanBlockTest("<1-foo+bar>foo</1-foo+bar>", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [Fact]
        public void ParseBlockThrowsErrorIfStartTextTagContainsTextAfterName()
        {
            ParseBlockTest("<text foo bar></text>",
                new MarkupBlock(
                    Factory.MarkupTransition("<text").Accepts(AcceptedCharacters.Any),
                    Factory.Markup(" foo bar>"),
                    Factory.MarkupTransition("</text>")),
                new RazorError(RazorResources.ParseError_TextTagCannotContainAttributes, SourceLocation.Zero));
        }

        [Fact]
        public void ParseBlockThrowsErrorIfEndTextTagContainsTextAfterName()
        {
            ParseBlockTest("<text></text foo bar>",
                new MarkupBlock(
                    Factory.MarkupTransition("<text>"),
                    Factory.MarkupTransition("</text").Accepts(AcceptedCharacters.Any),
                    Factory.Markup(" ")),
                new RazorError(RazorResources.ParseError_TextTagCannotContainAttributes, 6, 0, 6));
        }

        [Fact]
        public void ParseBlockThrowsExceptionIfBlockDoesNotStartWithTag()
        {
            ParseBlockTest("foo bar <baz>",
                new MarkupBlock(),
                new RazorError(RazorResources.ParseError_MarkupBlock_Must_Start_With_Tag, SourceLocation.Zero));
        }

        [Fact]
        public void ParseBlockStartingWithEndTagProducesRazorErrorThenOutputsMarkupSegmentAndEndsBlock()
        {
            ParseBlockTest("</foo> bar baz",
                new MarkupBlock(
                    Factory.Markup("</foo> ").Accepts(AcceptedCharacters.None)),
                new RazorError(String.Format(RazorResources.ParseError_UnexpectedEndTag, "foo"), SourceLocation.Zero));
        }

        [Fact]
        public void ParseBlockWithUnclosedTopLevelTagThrowsMissingEndTagParserExceptionOnOutermostUnclosedTag()
        {
            ParseBlockTest("<p><foo></bar>",
                new MarkupBlock(
                    Factory.Markup("<p><foo></bar>").Accepts(AcceptedCharacters.None)),
                new RazorError(String.Format(RazorResources.ParseError_MissingEndTag, "p"), new SourceLocation(0, 0, 0)));
        }

        [Fact]
        public void ParseBlockWithUnclosedTagAtEOFThrowsMissingEndTagException()
        {
            ParseBlockTest("<foo>blah blah blah blah blah",
                new MarkupBlock(
                    Factory.Markup("<foo>blah blah blah blah blah")),
                new RazorError(String.Format(RazorResources.ParseError_MissingEndTag, "foo"), new SourceLocation(0, 0, 0)));
        }

        [Fact]
        public void ParseBlockWithUnfinishedTagAtEOFThrowsIncompleteTagException()
        {
            ParseBlockTest("<foo bar=baz",
                new MarkupBlock(
                    Factory.Markup("<foo"),
                    new MarkupBlock(new AttributeBlockCodeGenerator("bar", new LocationTagged<string>(" bar=", 4, 0, 4), new LocationTagged<string>(String.Empty, 12, 0, 12)),
                        Factory.Markup(" bar=").With(SpanCodeGenerator.Null),
                        Factory.Markup("baz").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 9, 0, 9), new LocationTagged<string>("baz", 9, 0, 9))))),
                new RazorError(String.Format(RazorResources.ParseError_UnfinishedTag, "foo"), new SourceLocation(0, 0, 0)));
        }
    }
}

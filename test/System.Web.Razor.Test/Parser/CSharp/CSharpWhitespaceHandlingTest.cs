// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Tokenizer.Symbols;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpWhitespaceHandlingTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void StatementBlockDoesNotAcceptTrailingNewlineIfNewlinesAreSignificantToAncestor()
        {
            ParseBlockTest("@: @if (true) { }" + Environment.NewLine
                         + "}",
                           new MarkupBlock(
                               Factory.MarkupTransition()
                                   .Accepts(AcceptedCharacters.None),
                               Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                               Factory.Markup(" "),
                               new StatementBlock(
                                   Factory.CodeTransition()
                                       .Accepts(AcceptedCharacters.None),
                                   Factory.Code("if (true) { }")
                                       .AsStatement()
                                   ),
                               Factory.Markup("\r\n")
                                   .Accepts(AcceptedCharacters.None)));
        }
    }
}

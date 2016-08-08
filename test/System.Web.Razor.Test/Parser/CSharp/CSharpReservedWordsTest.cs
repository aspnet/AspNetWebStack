// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpReservedWordsTest : CsHtmlCodeParserTestBase
    {
        [Theory]
        [InlineData("namespace")]
        [InlineData("class")]
        public void ReservedWords(string word)
        {
            ParseBlockTest(word,
                           new DirectiveBlock(
                               Factory.MetaCode(word).Accepts(AcceptedCharacters.None)
                               ),
                           new RazorError(String.Format(RazorResources.ParseError_ReservedWord, word), SourceLocation.Zero));
        }

        [Theory]
        [InlineData("Namespace")]
        [InlineData("Class")]
        [InlineData("NAMESPACE")]
        [InlineData("CLASS")]
        [InlineData("nameSpace")]
        [InlineData("NameSpace")]
        private void ReservedWordsAreCaseSensitive(string word)
        {
            ParseBlockTest(word,
                           new ExpressionBlock(
                               Factory.Code(word)
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)
                               ));
        }
    }
}

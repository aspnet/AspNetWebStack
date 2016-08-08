// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.VB
{
    public class VBImplicitExpressionTest : VBHtmlCodeParserTestBase
    {
        [Fact]
        public void VB_Simple_ImplicitExpression()
        {
            ParseBlockTest("@foo not-part-of-the-block",
                new ExpressionBlock(
                    Factory.CodeTransition(),
                    Factory.Code("foo")
                           .AsImplicitExpression(VBCodeParser.DefaultKeywords)
                           .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }

        [Fact]
        public void VB_ImplicitExpression_With_Keyword_At_Start()
        {
            ParseBlockTest("@Partial",
                new ExpressionBlock(
                    Factory.CodeTransition(),
                    Factory.Code("Partial")
                           .AsImplicitExpression(VBCodeParser.DefaultKeywords)
                           .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }

        [Fact]
        public void VB_ImplicitExpression_With_Keyword_In_Body()
        {
            ParseBlockTest("@Html.Partial",
                new ExpressionBlock(
                    Factory.CodeTransition(),
                    Factory.Code("Html.Partial")
                           .AsImplicitExpression(VBCodeParser.DefaultKeywords)
                           .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }

        [Fact]
        public void VB_ImplicitExpression_With_MethodCallOrArrayIndex()
        {
            ParseBlockTest("@foo(42) not-part-of-the-block",
                new ExpressionBlock(
                    Factory.CodeTransition(),
                    Factory.Code("foo(42)")
                           .AsImplicitExpression(VBCodeParser.DefaultKeywords)
                           .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }

        [Fact]
        public void VB_ImplicitExpression_Terminates_If_Trailing_Dot_Not_Followed_By_Valid_Token()
        {
            ParseBlockTest("@foo(42). ",
                new ExpressionBlock(
                    Factory.CodeTransition(),
                    Factory.Code("foo(42)")
                           .AsImplicitExpression(VBCodeParser.DefaultKeywords)
                           .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }

        [Fact]
        public void VB_ImplicitExpression_Supports_Complex_Expressions()
        {
            ParseBlockTest("@foo(42).bar(Biz.Boz / 42 * 8)(1).Burf not part of the block",
                new ExpressionBlock(
                    Factory.CodeTransition()
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("foo(42).bar(Biz.Boz / 42 * 8)(1).Burf")
                           .AsImplicitExpression(VBCodeParser.DefaultKeywords)
                           .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }
    }
}

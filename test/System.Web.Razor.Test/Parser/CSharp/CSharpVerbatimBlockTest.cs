// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpVerbatimBlockTest : CsHtmlCodeParserTestBase
    {
        private const string TestExtraKeyword = "model";

        [Fact]
        public void VerbatimBlock()
        {
            ParseBlockTest("@{ foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.MetaCode("{")
                                   .Accepts(AcceptedCharacters.None),
                               Factory.Code(" foo(); ")
                                   .AsStatement(),
                               Factory.MetaCode("}")
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void InnerImplicitExpressionWithOnlySingleAtOutputsZeroLengthCodeSpan()
        {
            ParseBlockTest("{@}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.EmptyCSharp().AsStatement(),
                               new ExpressionBlock(
                                   Factory.CodeTransition(),
                                   Factory.EmptyCSharp().AsImplicitExpression(KeywordSet, acceptTrailingDot: true).Accepts(AcceptedCharacters.NonWhiteSpace)
                                   ),
                               Factory.EmptyCSharp().AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                           designTimeParser: true,
                           expectedErrors: new[]
                           {
                               new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS, "}"), new SourceLocation(2, 0, 2))
                           });
        }

        [Fact]
        public void InnerImplicitExpressionDoesNotAcceptDotAfterAt()
        {
            ParseBlockTest("{@.}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.EmptyCSharp().AsStatement(),
                               new ExpressionBlock(
                                   Factory.CodeTransition(),
                                   Factory.EmptyCSharp().AsImplicitExpression(KeywordSet, acceptTrailingDot: true).Accepts(AcceptedCharacters.NonWhiteSpace)
                                   ),
                               Factory.Code(".").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                           designTimeParser: true,
                           expectedErrors: new[]
                           {
                               new RazorError(String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS, "."), new SourceLocation(2, 0, 2))
                           });
        }

        [Fact]
        public void InnerImplicitExpressionWithOnlySingleAtAcceptsSingleSpaceOrNewlineAtDesignTime()
        {
            ParseBlockTest("{" + Environment.NewLine
                         + "    @" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\n    ").AsStatement(),
                               new ExpressionBlock(
                                   Factory.CodeTransition(),
                                   Factory.EmptyCSharp().AsImplicitExpression(KeywordSet, acceptTrailingDot: true).Accepts(AcceptedCharacters.NonWhiteSpace)
                                   ),
                               Factory.Code("\r\n").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                /* designTimeParser */ true,
                           new RazorError(RazorResources.ParseError_Unexpected_WhiteSpace_At_Start_Of_CodeBlock_CS, 8, 1, 5));
        }

        [Fact]
        public void InnerImplicitExpressionDoesNotAcceptTrailingNewlineInRunTimeMode()
        {
            ParseBlockTest("{@foo." + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.EmptyCSharp().AsStatement(),
                               new ExpressionBlock(
                                   Factory.CodeTransition(),
                                   Factory.Code("foo.").AsImplicitExpression(KeywordSet, acceptTrailingDot: true).Accepts(AcceptedCharacters.NonWhiteSpace)),
                               Factory.Code("\r\n").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void InnerImplicitExpressionAcceptsTrailingNewlineInDesignTimeMode()
        {
            ParseBlockTest("{@foo." + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.EmptyCSharp().AsStatement(),
                               new ExpressionBlock(
                                   Factory.CodeTransition(),
                                   Factory.Code("foo.").AsImplicitExpression(KeywordSet, acceptTrailingDot: true).Accepts(AcceptedCharacters.NonWhiteSpace)),
                               Factory.Code("\r\n").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                           designTimeParser: true);
        }
    }
}

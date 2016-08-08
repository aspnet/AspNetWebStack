// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.VB
{
    // VB Continue Statement: http://msdn.microsoft.com/en-us/library/801hyx6f.aspx
    public class VBContinueStatementTest : VBHtmlCodeParserTestBase
    {
        [Fact]
        public void VB_Do_Statement_With_Continue()
        {
            ParseBlockTest("@Do While True" + Environment.NewLine
                         + "    Continue Do" + Environment.NewLine
                         + "Loop" + Environment.NewLine
                         + "' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("Do While True\r\n    Continue Do\r\nLoop\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.AnyExceptNewline)));
        }

        [Fact]
        public void VB_For_Statement_With_Continue()
        {
            ParseBlockTest("@For i = 1 To 12" + Environment.NewLine
                         + "    Continue For" + Environment.NewLine
                         + "Next i" + Environment.NewLine
                         + "' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("For i = 1 To 12\r\n    Continue For\r\nNext i\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.AnyExceptNewline)));
        }

        [Fact]
        public void VB_While_Statement_With_Continue()
        {
            ParseBlockTest("@While True" + Environment.NewLine
                         + "    Continue While" + Environment.NewLine
                         + "End While" + Environment.NewLine
                         + "' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("While True\r\n    Continue While\r\nEnd While\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }
    }
}

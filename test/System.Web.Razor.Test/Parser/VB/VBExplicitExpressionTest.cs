// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.VB
{
    public class VBExplicitExpressionTest : VBHtmlCodeParserTestBase
    {
        [Fact]
        public void VB_Simple_ExplicitExpression()
        {
            ParseBlockTest("@(foo)",
                new ExpressionBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("(").Accepts(AcceptedCharacters.None),
                    Factory.Code("foo").AsExpression(),
                    Factory.MetaCode(")").Accepts(AcceptedCharacters.None)));
        }
    }
}

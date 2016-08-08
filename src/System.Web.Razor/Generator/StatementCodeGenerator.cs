// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public class StatementCodeGenerator : SpanCodeGenerator
    {
        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            context.FlushBufferedStatement();

            string generatedCode = context.BuildCodeString(cw =>
            {
                cw.WriteSnippet(target.Content);
            });

            int startGeneratedCode = target.Start.CharacterIndex;
            int paddingCharCount;
            generatedCode = CodeGeneratorPaddingHelper.PadStatement(context.Host, generatedCode, target, ref startGeneratedCode, out paddingCharCount);

            context.AddStatement(
                generatedCode,
                context.GenerateLinePragma(target, paddingCharCount));
        }

        public override string ToString()
        {
            return "Stmt";
        }

        public override bool Equals(object obj)
        {
            return obj is StatementCodeGenerator;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

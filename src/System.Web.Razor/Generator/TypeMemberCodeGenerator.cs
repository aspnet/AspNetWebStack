// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Diagnostics.Contracts;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public class TypeMemberCodeGenerator : SpanCodeGenerator
    {
        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            string generatedCode = context.BuildCodeString(cw =>
            {
                cw.WriteSnippet(target.Content);
            });

            int paddingCharCount;
            string paddedCode = CodeGeneratorPaddingHelper.Pad(context.Host, generatedCode, target, out paddingCharCount);

            Contract.Assert(paddingCharCount > 0);

            context.GeneratedClass.Members.Add(
                new CodeSnippetTypeMember(paddedCode)
                {
                    LinePragma = context.GenerateLinePragma(target, paddingCharCount)
                });
        }

        public override string ToString()
        {
            return "TypeMember";
        }

        public override bool Equals(object obj)
        {
            return obj is TypeMemberCodeGenerator;
        }

        // C# complains at us if we don't provide an implementation, even one like this
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

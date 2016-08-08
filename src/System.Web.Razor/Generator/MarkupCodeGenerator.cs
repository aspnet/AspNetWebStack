// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public class MarkupCodeGenerator : SpanCodeGenerator
    {
        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            if (!context.Host.DesignTimeMode && String.IsNullOrEmpty(target.Content))
            {
                return;
            }

            if (context.Host.EnableInstrumentation)
            {
                context.AddContextCall(target, context.Host.GeneratedClassContext.BeginContextMethodName, isLiteral: true);
            }

            if (!String.IsNullOrEmpty(target.Content) && !context.Host.DesignTimeMode)
            {
                string code = context.BuildCodeString(cw =>
                {
                    if (!String.IsNullOrEmpty(context.TargetWriterName))
                    {
                        cw.WriteStartMethodInvoke(context.Host.GeneratedClassContext.WriteLiteralToMethodName);
                        cw.WriteSnippet(context.TargetWriterName);
                        cw.WriteParameterSeparator();
                    }
                    else
                    {
                        cw.WriteStartMethodInvoke(context.Host.GeneratedClassContext.WriteLiteralMethodName);
                    }
                    cw.WriteStringLiteral(target.Content);
                    cw.WriteEndMethodInvoke();
                    cw.WriteEndStatement();
                });
                context.AddStatement(code);
            }

            if (context.Host.EnableInstrumentation)
            {
                context.AddContextCall(target, context.Host.GeneratedClassContext.EndContextMethodName, isLiteral: true);
            }
        }

        public override string ToString()
        {
            return "Markup";
        }

        public override bool Equals(object obj)
        {
            return obj is MarkupCodeGenerator;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

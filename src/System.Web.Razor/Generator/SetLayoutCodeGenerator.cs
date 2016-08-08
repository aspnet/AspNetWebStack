// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public class SetLayoutCodeGenerator : SpanCodeGenerator
    {
        public SetLayoutCodeGenerator(string layoutPath)
        {
            LayoutPath = layoutPath;
        }

        public string LayoutPath { get; set; }

        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            if (!context.Host.DesignTimeMode && !String.IsNullOrEmpty(context.Host.GeneratedClassContext.LayoutPropertyName))
            {
                context.TargetMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodePropertyReferenceExpression(null, context.Host.GeneratedClassContext.LayoutPropertyName),
                        new CodePrimitiveExpression(LayoutPath)));
            }
        }

        public override string ToString()
        {
            return "Layout: " + LayoutPath;
        }

        public override bool Equals(object obj)
        {
            SetLayoutCodeGenerator other = obj as SetLayoutCodeGenerator;
            return other != null && String.Equals(other.LayoutPath, LayoutPath, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return LayoutPath.GetHashCode();
        }
    }
}

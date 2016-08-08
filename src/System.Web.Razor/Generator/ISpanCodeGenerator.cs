// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public interface ISpanCodeGenerator
    {
        void GenerateCode(Span target, CodeGeneratorContext context);
    }
}

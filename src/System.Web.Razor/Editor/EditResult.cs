// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Editor
{
    public class EditResult
    {
        public EditResult(PartialParseResult result, SpanBuilder editedSpan)
        {
            Result = result;
            EditedSpan = editedSpan;
        }

        public PartialParseResult Result { get; set; }
        public SpanBuilder EditedSpan { get; set; }
    }
}

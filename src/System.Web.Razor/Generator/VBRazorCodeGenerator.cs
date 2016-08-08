// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Razor.Generator
{
    public class VBRazorCodeGenerator : RazorCodeGenerator
    {
        public VBRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
        }

        internal override Func<CodeWriter> CodeWriterFactory
        {
            get { return () => new VBCodeWriter(); }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Razor.Generator
{
    public class CodeGenerationCompleteEventArgs : EventArgs
    {
        public CodeGenerationCompleteEventArgs(string virtualPath, string physicalPath, CodeCompileUnit generatedCode)
        {
            if (String.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "virtualPath");
            }
            if (generatedCode == null)
            {
                throw new ArgumentNullException("generatedCode");
            }
            VirtualPath = virtualPath;
            PhysicalPath = physicalPath;
            GeneratedCode = generatedCode;
        }

        public CodeCompileUnit GeneratedCode { get; private set; }
        public string VirtualPath { get; private set; }
        public string PhysicalPath { get; private set; }
    }
}

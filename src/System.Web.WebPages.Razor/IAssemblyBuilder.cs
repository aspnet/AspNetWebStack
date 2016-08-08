// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.Compilation;

namespace System.Web.WebPages.Razor
{
    internal interface IAssemblyBuilder
    {
        void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit);
        void GenerateTypeFactory(string typeName);
    }
}

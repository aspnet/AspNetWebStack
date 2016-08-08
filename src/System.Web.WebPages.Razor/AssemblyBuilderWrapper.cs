// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.Compilation;

namespace System.Web.WebPages.Razor
{
    internal sealed class AssemblyBuilderWrapper : IAssemblyBuilder
    {
        public AssemblyBuilderWrapper(AssemblyBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            InnerBuilder = builder;
        }

        internal AssemblyBuilder InnerBuilder { get; set; }

        public void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit)
        {
            InnerBuilder.AddCodeCompileUnit(buildProvider, compileUnit);
        }

        public void GenerateTypeFactory(string typeName)
        {
            InnerBuilder.GenerateTypeFactory(typeName);
        }
    }
}

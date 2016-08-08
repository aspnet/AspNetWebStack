// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.UI;

namespace System.Web.Mvc
{
    internal sealed class ViewMasterPageControlBuilder : FileLevelMasterPageControlBuilder, IMvcControlBuilder
    {
        public string Inherits { get; set; }

        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
            if (!String.IsNullOrWhiteSpace(Inherits))
            {
                derivedType.BaseTypes[0] = new CodeTypeReference(Inherits);
            }
        }
    }
}

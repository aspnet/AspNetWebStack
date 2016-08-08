// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Tokenizer
{
    public interface ITokenizer
    {
        ISymbol NextSymbol();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer;
using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Test.Tokenizer
{
    public abstract class VBTokenizerTestBase : TokenizerTestBase<VBSymbol, VBSymbolType>
    {
        private static VBSymbol _ignoreRemaining = new VBSymbol(0, 0, 0, String.Empty, VBSymbolType.Unknown);

        protected override VBSymbol IgnoreRemaining
        {
            get { return _ignoreRemaining; }
        }

        protected override Tokenizer<VBSymbol, VBSymbolType> CreateTokenizer(ITextDocument source)
        {
            return new VBTokenizer(source);
        }

        protected void TestSingleToken(string text, VBSymbolType expectedSymbolType)
        {
            TestTokenizer(text, new VBSymbol(0, 0, 0, text, expectedSymbolType));
        }
    }
}

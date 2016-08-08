// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer;
using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Test.Tokenizer
{
    public abstract class HtmlTokenizerTestBase : TokenizerTestBase<HtmlSymbol, HtmlSymbolType>
    {
        private static HtmlSymbol _ignoreRemaining = new HtmlSymbol(0, 0, 0, String.Empty, HtmlSymbolType.Unknown);

        protected override HtmlSymbol IgnoreRemaining
        {
            get { return _ignoreRemaining; }
        }

        protected override Tokenizer<HtmlSymbol, HtmlSymbolType> CreateTokenizer(ITextDocument source)
        {
            return new HtmlTokenizer(source);
        }

        protected void TestSingleToken(string text, HtmlSymbolType expectedSymbolType)
        {
            TestTokenizer(text, new HtmlSymbol(0, 0, 0, text, expectedSymbolType));
        }
    }
}

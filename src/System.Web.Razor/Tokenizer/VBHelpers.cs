// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Razor.Tokenizer
{
    public static class VBHelpers
    {
        public static bool IsSingleQuote(char character)
        {
            return character == '\'' || character == '‘' || character == '’';
        }

        public static bool IsDoubleQuote(char character)
        {
            return character == '"' || character == '“' || character == '”';
        }

        public static bool IsOctalDigit(char character)
        {
            return character >= '0' && character <= '7';
        }
    }
}

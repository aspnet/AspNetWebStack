// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Razor.Tokenizer.Symbols
{
    public enum VBSymbolType
    {
        Unknown,
        WhiteSpace,
        NewLine,
        LineContinuation,
        Comment,
        Identifier,
        Keyword,
        IntegerLiteral,
        FloatingPointLiteral,
        StringLiteral,
        CharacterLiteral,
        DateLiteral,
        LeftParenthesis,
        RightBrace,
        LeftBrace,
        RightParenthesis,
        Hash,
        Bang,
        Comma,
        Dot,
        Colon,
        Concatenation,
        QuestionMark,
        Subtract,
        Multiply,
        Add,
        Divide,
        IntegerDivide,
        Exponentiation,
        LessThan,
        GreaterThan,
        Equal,
        RightBracket,
        LeftBracket,
        Dollar,
        Transition,

        RazorCommentTransition,
        RazorCommentStar,
        RazorComment
    }
}

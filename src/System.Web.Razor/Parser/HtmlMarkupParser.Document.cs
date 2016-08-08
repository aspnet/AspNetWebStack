// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Parser
{
    public partial class HtmlMarkupParser
    {
        public override void ParseDocument()
        {
            if (Context == null)
            {
                throw new InvalidOperationException(RazorResources.Parser_Context_Not_Set);
            }

            using (PushSpanConfig(DefaultMarkupSpan))
            {
                using (Context.StartBlock(BlockType.Markup))
                {
                    NextToken();
                    while (!EndOfFile)
                    {
                        SkipToAndParseCode(HtmlSymbolType.OpenAngle);
                        ScanTagInDocumentContext();
                    }
                    AddMarkerSymbolIfNecessary();
                    Output(SpanKind.Markup);
                }
            }
        }

        /// <summary>
        /// Reads the content of a tag (if present) in the MarkupDocument (or MarkupSection) context,
        /// where we don't care about maintaining a stack of tags.
        /// </summary>
        /// <returns>A boolean indicating if we scanned at least one tag.</returns>
        private bool ScanTagInDocumentContext()
        {
            if (Optional(HtmlSymbolType.OpenAngle))
            {
                if (At(HtmlSymbolType.Bang))
                {
                    BangTag();
                    return true;
                }
                else if (At(HtmlSymbolType.QuestionMark))
                {
                    XmlPI();
                    return true;
                }
                else if (!At(HtmlSymbolType.Solidus))
                {
                    bool scriptTag = At(HtmlSymbolType.Text) &&
                                     String.Equals(CurrentSymbol.Content, "script", StringComparison.OrdinalIgnoreCase);
                    Optional(HtmlSymbolType.Text);
                    TagContent(); // Parse the tag, don't care about the content
                    Optional(HtmlSymbolType.Solidus);
                    Optional(HtmlSymbolType.CloseAngle);
                    if (scriptTag)
                    {
                        SkipToEndScriptAndParseCode();
                    }
                    return true;
                }
            }
            return false;
        }
    }
}

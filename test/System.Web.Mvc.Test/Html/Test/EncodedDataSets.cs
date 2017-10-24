// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Html.Test
{
    public static class EncodedDataSets
    {
        private static readonly List<StringSet> StringSets = new List<StringSet>
        {
            // Needs extra URL-escaping due to routing special cases.
            new StringSet("&'\"", "&amp;&#39;&quot;", "&amp;&#39;&quot;", "___", "%26&#39;%22"),

            // High ASCII
            new StringSet(" ¡ÿĀ", " ¡ÿĀ", "&#160;&#161;&#255;Ā", "____"),

            // Surrogate pair, Ugaritic letter beta, &x10381;
            new StringSet("\xD800\xDF81", "𐎁", "&#66433;", "__"),

            new StringSet(
                "<blink>text</blink>",
                "&lt;blink>text&lt;/blink>",
                "&lt;blink&gt;text&lt;/blink&gt;",
                "_blink_text__blink_"),

            // Remaining examples are only id-sanitized, not attribute- or HTML-encoded.
            new StringSet("Simple Display Text", "Simple Display Text", "Simple Display Text", "Simple_Display_Text"),
            new StringSet("Chinese西雅图Chars", "Chinese西雅图Chars", "Chinese西雅图Chars", "Chinese___Chars"), // Seattle
            new StringSet(
                "Unicode؃Format؃Char", // class Cf
                "Unicode؃Format؃Char",
                "Unicode؃Format؃Char",
                "Unicode_Format_Char"),
            new StringSet(
                "UnicodeῼTitlecaseῼChar", // class Lt
                "UnicodeῼTitlecaseῼChar",
                "UnicodeῼTitlecaseῼChar",
                "Unicode_Titlecase_Char"),
            new StringSet(
                "UnicodeःCombiningःChar", // class Mc
                "UnicodeःCombiningःChar",
                "UnicodeःCombiningःChar",
                "Unicode_Combining_Char"),
        };

        public static TheoryDataSet<string, bool, string, string> AttributeAndHtmlEncodedData
        {
            get
            {
                var result = new TheoryDataSet<string, bool, string, string>();
                foreach (var stringSet in StringSets)
                {
                    // Same results whether ModelMetadata.HtmlEncode is true or false.
                    result.Add(
                        stringSet.Text,
                        false,
                        stringSet.AttributeEncodedText,
                        stringSet.HtmlEncodedText);
                    result.Add(
                        stringSet.Text,
                        true,
                        stringSet.AttributeEncodedText,
                        stringSet.HtmlEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, bool, string> AttributeEncodedData
        {
            get
            {
                var result = new TheoryDataSet<string, bool, string>();
                foreach (var stringSet in StringSets)
                {
                    // Same results whether ModelMetadata.HtmlEncode is true or false.
                    result.Add(stringSet.Text, false, stringSet.AttributeEncodedText);
                    result.Add(stringSet.Text, true, stringSet.AttributeEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, bool> AttributeEncodedData_NoEncodedText
        {
            get
            {
                var dataSet = new TheoryDataSet<string, bool>();
                foreach (var stringSet in StringSets)
                {
                    dataSet.Add(stringSet.Text, false);
                    dataSet.Add(stringSet.Text, true);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<string, string> AttributeEncodedData_NoHtmlEncode
        {
            get
            {
                var dataSet = new TheoryDataSet<string, string>();
                foreach (var stringSet in StringSets)
                {
                    dataSet.Add(stringSet.Text, stringSet.AttributeEncodedText);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<string, bool, string> ConditionallyHtmlEncodedData
        {
            get
            {
                var result = new TheoryDataSet<string, bool, string>();
                foreach (var stringSet in StringSets)
                {
                    result.Add(stringSet.Text, false, stringSet.Text);
                    result.Add(stringSet.Text, true, stringSet.HtmlEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, bool> ConditionallyHtmlEncodedData_NoEncodedText
        {
            get
            {
                var dataSet = new TheoryDataSet<string, bool>();
                foreach (var stringSet in StringSets)
                {
                    dataSet.Add(stringSet.Text, false);
                    dataSet.Add(stringSet.Text, true);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<string, bool, string> HtmlEncodedData
        {
            get
            {
                var result = new TheoryDataSet<string, bool, string>();
                foreach (var stringSet in StringSets)
                {
                    // Same results whether ModelMetadata.HtmlEncode is true or false.
                    result.Add(stringSet.Text, false, stringSet.HtmlEncodedText);
                    result.Add(stringSet.Text, true, stringSet.HtmlEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, string> HtmlEncodedData_NoHtmlEncode
        {
            get
            {
                var dataSet = new TheoryDataSet<string, string>();
                foreach (var stringSet in StringSets)
                {
                    dataSet.Add(stringSet.Text, stringSet.HtmlEncodedText);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<string> HtmlEncodedData_JustText
        {
            get
            {
                var dataSet = new TheoryDataSet<string>();
                foreach (var stringSet in StringSets)
                {
                    dataSet.Add(stringSet.Text);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<string, bool, string> IdEncodedData
        {
            get
            {
                var result = new TheoryDataSet<string, bool, string>();
                foreach (var encodedString in StringSets)
                {
                    // Same results whether ModelMetadata.HtmlEncode is true or false.
                    // Add leading 'a' to avoid sanitizing to an empty string.
                    result.Add("a" + encodedString.Text, false, "a" + encodedString.IdEncodedText);
                    result.Add("a" + encodedString.Text, true, "a" + encodedString.IdEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, string> IdEncodedData_NoHtmlEncode
        {
            get
            {
                var result = new TheoryDataSet<string, string>();
                foreach (var encodedString in StringSets)
                {
                    // Add leading 'a' to avoid sanitizing to an empty string.
                    result.Add("a" + encodedString.Text, "a" + encodedString.IdEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, bool, string> UrlEncodedData
        {
            get
            {
                var result = new TheoryDataSet<string, bool, string>();
                foreach (var encodedString in StringSets)
                {
                    // Same results whether ModelMetadata.HtmlEncode is true or false.
                    result.Add(encodedString.Text, false, encodedString.UrlEncodedText);
                    result.Add(encodedString.Text, true, encodedString.UrlEncodedText);
                }

                return result;
            }
        }

        public static TheoryDataSet<string, string> UrlEncodedData_NoHtmlEncode
        {
            get
            {
                var result = new TheoryDataSet<string, string>();
                foreach (var encodedString in StringSets)
                {
                    result.Add(encodedString.Text, encodedString.UrlEncodedText);
                }

                return result;
            }
        }

        private class StringSet
        {
            public StringSet(string text, string attributeEncodedText, string htmlEncodedText, string idEncodedText)
                : this(text, attributeEncodedText, htmlEncodedText, idEncodedText, Uri.EscapeUriString(text))
            {
            }

            public StringSet(
                string text,
                string attributeEncodedText,
                string htmlEncodedText,
                string idEncodedText,
                string urlEncodedText)
            {
                Contract.Assert(!String.IsNullOrEmpty(text));
                Contract.Assert(!String.IsNullOrEmpty(attributeEncodedText));
                Contract.Assert(!String.IsNullOrEmpty(htmlEncodedText));
                Contract.Assert(!String.IsNullOrEmpty(idEncodedText));
                Contract.Assert(!String.IsNullOrEmpty(urlEncodedText));

                // Override default UrlEncodedText.
                Text = text;
                AttributeEncodedText = attributeEncodedText;
                HtmlEncodedText = htmlEncodedText;
                IdEncodedText = idEncodedText;
                UrlEncodedText = urlEncodedText;
            }

            public string Text { get; private set; }

            public string AttributeEncodedText { get; private set; }

            public string HtmlEncodedText { get; private set; }

            public string IdEncodedText { get; private set; }

            public string UrlEncodedText { get; private set; }
        }
    }
}

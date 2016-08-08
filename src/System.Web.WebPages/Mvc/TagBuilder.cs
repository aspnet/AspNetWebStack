// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.WebPages.Html;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Mvc
{
    [TypeForwardedFrom("System.Web.Mvc, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class TagBuilder
    {
        private string _idAttributeDotReplacement;

        private string _innerHtml;

        public TagBuilder(string tagName)
        {
            if (String.IsNullOrEmpty(tagName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "tagName");
            }

            TagName = tagName;
            Attributes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        public IDictionary<string, string> Attributes { get; private set; }

        public string IdAttributeDotReplacement
        {
            get
            {
                if (String.IsNullOrEmpty(_idAttributeDotReplacement))
                {
                    _idAttributeDotReplacement = HtmlHelper.IdAttributeDotReplacement;
                }
                return _idAttributeDotReplacement;
            }
            set { _idAttributeDotReplacement = value; }
        }

        public string InnerHtml
        {
            get { return _innerHtml ?? String.Empty; }
            set { _innerHtml = value; }
        }

        public string TagName { get; private set; }

        public void AddCssClass(string value)
        {
            string currentValue;

            if (Attributes.TryGetValue("class", out currentValue))
            {
                Attributes["class"] = value + " " + currentValue;
            }
            else
            {
                Attributes["class"] = value;
            }
        }

        public static string CreateSanitizedId(string originalId)
        {
            return CreateSanitizedId(originalId, HtmlHelper.IdAttributeDotReplacement);
        }

        public static string CreateSanitizedId(string originalId, string invalidCharReplacement)
        {
            if (String.IsNullOrEmpty(originalId))
            {
                return null;
            }

            if (invalidCharReplacement == null)
            {
                throw new ArgumentNullException("invalidCharReplacement");
            }

            char firstChar = originalId[0];
            if (!Html401IdUtil.IsLetter(firstChar))
            {
                // the first character must be a letter
                return null;
            }

            StringBuilder sb = new StringBuilder(originalId.Length);
            sb.Append(firstChar);

            for (int i = 1; i < originalId.Length; i++)
            {
                char thisChar = originalId[i];
                if (Html401IdUtil.IsValidIdCharacter(thisChar))
                {
                    sb.Append(thisChar);
                }
                else
                {
                    sb.Append(invalidCharReplacement);
                }
            }

            return sb.ToString();
        }

        public void GenerateId(string name)
        {
            if (!Attributes.ContainsKey("id"))
            {
                string sanitizedId = CreateSanitizedId(name, IdAttributeDotReplacement);
                if (!String.IsNullOrEmpty(sanitizedId))
                {
                    Attributes["id"] = sanitizedId;
                }
            }
        }

        private void AppendAttributes(StringBuilder sb)
        {
            foreach (var attribute in Attributes)
            {
                string key = attribute.Key;
                if (String.Equals(key, "id", StringComparison.Ordinal /* case-sensitive */) && String.IsNullOrEmpty(attribute.Value))
                {
                    continue; // DevDiv Bugs #227595: don't output empty IDs
                }
                string value = HttpUtility.HtmlAttributeEncode(attribute.Value);
                sb.Append(' ')
                    .Append(key)
                    .Append("=\"")
                    .Append(value)
                    .Append('"');
            }
        }

        public void MergeAttribute(string key, string value)
        {
            MergeAttribute(key, value, replaceExisting: false);
        }

        public void MergeAttribute(string key, string value, bool replaceExisting)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }

            if (replaceExisting || !Attributes.ContainsKey(key))
            {
                Attributes[key] = value;
            }
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes)
        {
            MergeAttributes(attributes, replaceExisting: false);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
        {
            if (attributes != null)
            {
                foreach (var entry in attributes)
                {
                    string key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    string value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);
                    MergeAttribute(key, value, replaceExisting);
                }
            }
        }

        public void SetInnerText(string innerText)
        {
            InnerHtml = HttpUtility.HtmlEncode(innerText);
        }

        internal HtmlString ToHtmlString(TagRenderMode renderMode)
        {
            return new HtmlString(ToString(renderMode));
        }

        public override string ToString()
        {
            return ToString(TagRenderMode.Normal);
        }

        public string ToString(TagRenderMode renderMode)
        {
            StringBuilder sb = new StringBuilder();
            switch (renderMode)
            {
                case TagRenderMode.StartTag:
                    sb.Append('<')
                        .Append(TagName);
                    AppendAttributes(sb);
                    sb.Append('>');
                    break;
                case TagRenderMode.EndTag:
                    sb.Append("</")
                        .Append(TagName)
                        .Append('>');
                    break;
                case TagRenderMode.SelfClosing:
                    sb.Append('<')
                        .Append(TagName);
                    AppendAttributes(sb);
                    sb.Append(" />");
                    break;
                default:
                    sb.Append('<')
                        .Append(TagName);
                    AppendAttributes(sb);
                    sb.Append('>')
                        .Append(InnerHtml)
                        .Append("</")
                        .Append(TagName)
                        .Append('>');
                    break;
            }
            return sb.ToString();
        }

        // Valid IDs are defined in http://www.w3.org/TR/html401/types.html#type-id
        private static class Html401IdUtil
        {
            private static bool IsAllowableSpecialCharacter(char c)
            {
                switch (c)
                {
                    case '-':
                    case '_':
                    case ':':
                        // note that we're specifically excluding the '.' character
                        return true;

                    default:
                        return false;
                }
            }

            private static bool IsDigit(char c)
            {
                return ('0' <= c && c <= '9');
            }

            public static bool IsLetter(char c)
            {
                return (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'));
            }

            public static bool IsValidIdCharacter(char c)
            {
                return (IsLetter(c) || IsDigit(c) || IsAllowableSpecialCharacter(c));
            }
        }
    }
}

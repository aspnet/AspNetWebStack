// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Web.UI;

namespace System.Web.Helpers
{
    internal class HtmlElement
    {
        public HtmlElement(string tagName)
        {
            TagName = tagName;
            Attributes = new Dictionary<string, string>();
            Children = new List<HtmlElement>();
        }

        internal string TagName { get; set; }

        internal string InnerText { get; set; }

        public IList<HtmlElement> Children { get; set; }

        private IDictionary<string, string> Attributes { get; set; }

        public string this[string name]
        {
            get { return Attributes[name]; }
            set { MergeAttribute(name, value); }
        }

        public HtmlElement SetInnerText(string innerText)
        {
            InnerText = innerText;
            Children.Clear();
            return this;
        }

        public HtmlElement AppendChild(HtmlElement e)
        {
            Children.Add(e);
            return this;
        }

        public HtmlElement AppendChild(string innerText)
        {
            AppendChild(CreateSpan(innerText));
            return this;
        }

        private void MergeAttribute(string name, string value)
        {
            Attributes[name] = value;
        }

        public HtmlElement AddCssClass(string className)
        {
            string currentValue;
            if (!Attributes.TryGetValue("class", out currentValue))
            {
                Attributes["class"] = className;
            }
            else
            {
                Attributes["class"] = currentValue + " " + className;
            }
            return this;
        }

        public IHtmlString ToHtmlString()
        {
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                WriteTo(sw);
                return new HtmlString(sw.ToString());
            }
        }

        public void WriteTo(TextWriter writer)
        {
            WriteToInternal(new HtmlTextWriter(writer));
        }

        private void WriteToInternal(HtmlTextWriter writer)
        {
            foreach (var a in Attributes)
            {
                writer.AddAttribute(a.Key, a.Value, true);
            }
            writer.RenderBeginTag(TagName);
            if (!String.IsNullOrEmpty(InnerText))
            {
                writer.WriteEncodedText(InnerText);
            }
            else
            {
                foreach (var e in Children)
                {
                    e.WriteToInternal(writer);
                }
            }
            writer.RenderEndTag();
        }

        public override string ToString()
        {
            return ToHtmlString().ToString();
        }

        internal static HtmlElement CreateSpan(string innerText, string cssClass = null)
        {
            var span = new HtmlElement("span");
            span.SetInnerText(innerText);
            if (!String.IsNullOrEmpty(cssClass))
            {
                span.AddCssClass(cssClass);
            }
            return span;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Helpers.Resources;

namespace System.Web.Helpers
{
    internal class HtmlObjectPrinter : ObjectVisitor
    {
        private const string Styles =
            @"<style type=""text/css"">       
    .objectinfo { font-size: 13px; }
    .objectinfo .type { color: #0000ff; }
    .objectinfo .complexType { color: #2b91af; }
    .objectinfo .name { color: Black; }
    .objectinfo .value { color: Black; }
    .objectinfo .quote { color: Brown; }
    .objectinfo .null { color: Red; }
    .objectinfo .exception { color:Red; }
    .objectinfo .typeContainer { border-left: solid 2px #7C888A; padding-left: 3px; margin-left:3px; }
    .objectinfo h3, h2 { margin:0; padding:0; }
    .objectinfo ul { margin-top:0; margin-bottom:0; list-style-type:none; padding-left:10px; margin-left:10px; }
</style>
";

        private static readonly HtmlElement _nullSpan = HtmlElement.CreateSpan("(null)", "null");
        // List of chars to escape within strings
        private static readonly Dictionary<char, string> _printableEscapeChars = new Dictionary<char, string>
        {
            { '\0', "\\0" },
            { '\\', "\\\\" },
            { '\'', "'" },
            { '\"', "\\\"" },
            { '\a', "\\a" },
            { '\b', "\\b" },
            { '\f', "\\f" },
            { '\n', "\\n" },
            { '\r', "\\r" },
            { '\t', "\\t" },
            { '\v', "\\v" },
        };

        // We want to exclude the type name next to the value for members
        private bool _excludeTypeName;
        private Stack<HtmlElement> _elementStack = new Stack<HtmlElement>();

        public HtmlObjectPrinter(int recursionLimit, int enumerationLimit)
            : base(recursionLimit, enumerationLimit)
        {
        }

        private HtmlElement Current
        {
            get
            {
                Debug.Assert(_elementStack.Count > 0);
                return _elementStack.Peek();
            }
        }

        public void WriteTo(object value, TextWriter writer)
        {
            HtmlElement rootElement = new HtmlElement("div");
            rootElement.AddCssClass("objectinfo");

            PushElement(rootElement);
            Visit(value, 0);
            PopElement();

            Debug.Assert(_elementStack.Count == 0, "Stack should be empty");

            // REVIEW: We should only do this once per page/request
            writer.Write(Styles);
            rootElement.WriteTo(writer);
        }

        public override void VisitKeyValues(object value, IEnumerable<object> keys, Func<object, object> valueSelector, int depth)
        {
            string id = GetObjectId(value);
            HtmlElement ul = new HtmlElement("ul");
            ul.AddCssClass("typeEnumeration");
            ul["id"] = id;

            PushElement(ul);
            base.VisitKeyValues(value, keys, valueSelector, depth);
            PopElement();

            Current.AppendChild(ul);
        }

        public override void VisitKeyValue(object key, object value, int depth)
        {
            HtmlElement keyElement = new HtmlElement("span");
            PushElement(keyElement);
            Visit(key, depth);
            PopElement();

            HtmlElement valueElement = new HtmlElement("span");
            PushElement(valueElement);
            Visit(value, depth);
            PopElement();

            // Append the elements to the li
            HtmlElement li = new HtmlElement("li");
            li.AppendChild(keyElement);
            li.AppendChild(" = ");
            li.AppendChild(valueElement);
            Current.AppendChild(li);
        }

        public override void VisitEnumerable(IEnumerable enumerable, int depth)
        {
            string id = GetObjectId(enumerable);

            HtmlElement ul = new HtmlElement("ul");
            ul.AddCssClass("typeEnumeration");
            ul["id"] = id;

            PushElement(ul);
            base.VisitEnumerable(enumerable, depth);
            PopElement();

            Current.AppendChild(ul);
        }

        public override void VisitIndexedEnumeratedValue(int index, object item, int depth)
        {
            HtmlElement li = new HtmlElement("li");
            li.AppendChild(String.Format(CultureInfo.InvariantCulture, "[{0}] = ", index));
            PushElement(li);
            base.VisitIndexedEnumeratedValue(index, item, depth);
            PopElement();
            Current.AppendChild(li);
        }

        public override void VisitEnumeratedValue(object item, int depth)
        {
            HtmlElement li = new HtmlElement("li");
            PushElement(li);
            base.VisitEnumeratedValue(item, depth);
            PopElement();
            Current.AppendChild(li);
        }

        public override void VisitEnumeratonLimitExceeded()
        {
            HtmlElement li = new HtmlElement("li");
            li.AppendChild("...");
            Current.AppendChild(li);
        }

        public override void VisitMembers(IEnumerable<string> names, Func<string, Type> typeSelector, Func<string, object> valueSelector, int depth)
        {
            HtmlElement ul = new HtmlElement("ul");
            ul.AddCssClass("typeProperties");

            PushElement(ul);
            base.VisitMembers(names, typeSelector, valueSelector, depth);
            PopElement();

            Current.AppendChild(ul);
        }

        public override void VisitMember(string name, Type type, object value, int depth)
        {
            HtmlElement li = new HtmlElement("li");

            if (type != null)
            {
                li.AppendChild(CreateTypeNameSpan(type));
                li.AppendChild(" ");
            }

            li.AppendChild(CreateNameSpan(name));
            li.AppendChild(" = ");

            PushElement(li);

            _excludeTypeName = true;
            base.VisitMember(name, type, value, depth);
            _excludeTypeName = false;

            PopElement();

            Current.AppendChild(li);
        }

        public override void VisitComplexObject(object value, int depth)
        {
            string id = GetObjectId(value);

            HtmlElement objectElement = new HtmlElement("div");
            objectElement.AddCssClass("typeContainer");
            objectElement["id"] = id;

            PushElement(objectElement);
            base.VisitComplexObject(value, depth);
            PopElement();

            if (objectElement.Children.Any())
            {
                Current.AppendChild(objectElement);
            }
        }

        public override void VisitNull()
        {
            Current.AppendChild(_nullSpan);
        }

        public override void VisitStringValue(string stringValue)
        {
            // Convert the string escape sequences
            stringValue = "\"" + ConvertEscapseSequences(stringValue) + "\"";
            Current.AppendChild(CreateQuotedSpan(stringValue));
        }

        public override void VisitVisitedObject(string id, object value)
        {
            Current.AppendChild(CreateVisitedLink(id));
        }

        public override void Visit(object value, int depth)
        {
            if (value != null)
            {
                if (!_excludeTypeName)
                {
                    Current.AppendChild(CreateTypeNameSpan(value.GetType()));
                    Current.AppendChild(" ");
                }
                _excludeTypeName = false;
            }

            base.Visit(value, depth);
        }

        public override void VisitObjectVisitorException(ObjectVisitorException exception)
        {
            Current.AppendChild(CreateExceptionSpan(exception));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Making the value lowercase has nothing to do with normalization. It's used to show true or false instead of the Title case version")]
        public override void VisitConvertedValue(object value, string convertedValue)
        {
            Type type = value.GetType();
            if (type.Equals(typeof(bool)))
            {
                // Convert True or False to lowercase
                convertedValue = convertedValue.ToLowerInvariant();
                Current.AppendChild(CreateTypeSpan(convertedValue));
                return;
            }

            if (type.Equals(typeof(char)))
            {
                string charValue = GetCharValue((char)value);
                Current.AppendChild(CreateQuotedSpan("'" + charValue + "'"));
                return;
            }

            // See if the value is a Type itself
            Type valueAsType = value as Type;
            if (valueAsType != null)
            {
                // For types we're going to generate elements that print typeof(TypeName)
                Current.AppendChild(CreateParentSpan(CreateTypeSpan("typeof"),
                                                     CreateOperatorSpan("("),
                                                     CreateTypeNameSpan(valueAsType),
                                                     CreateOperatorSpan(")")));
            }
            else
            {
                Current.AppendChild(CreateValueSpan(convertedValue));
            }
        }

        private static HtmlElement CreateParentSpan(params HtmlElement[] elements)
        {
            HtmlElement span = new HtmlElement("span");
            foreach (var e in elements)
            {
                span.AppendChild(e);
            }
            return span;
        }

        private static HtmlElement CreateNameSpan(string name)
        {
            return HtmlElement.CreateSpan(name, "name");
        }

        private static HtmlElement CreateOperatorSpan(string @operator)
        {
            return HtmlElement.CreateSpan(@operator, "operator");
        }

        private static HtmlElement CreateValueSpan(string value)
        {
            return HtmlElement.CreateSpan(value, "value");
        }

        private static HtmlElement CreateExceptionSpan(ObjectVisitorException exception)
        {
            HtmlElement span = new HtmlElement("span");
            span.AppendChild(HelpersResources.ObjectInfo_PropertyThrewException);
            span.AppendChild(HtmlElement.CreateSpan(exception.InnerException.Message, "exception"));
            return span;
        }

        private static HtmlElement CreateQuotedSpan(string value)
        {
            return HtmlElement.CreateSpan(value, "quote");
        }

        private static HtmlElement CreateLink(string href, string linkText, string cssClass = null)
        {
            HtmlElement a = new HtmlElement("a");
            a.SetInnerText(linkText);
            a["href"] = href;
            if (!String.IsNullOrEmpty(cssClass))
            {
                a.AddCssClass(cssClass);
            }
            return a;
        }

        private static HtmlElement CreateVisitedLink(string id)
        {
            string text = String.Format(CultureInfo.InvariantCulture, "[{0}]", HelpersResources.ObjectInfo_PreviousDisplayed);
            return CreateLink("#" + id, text);
        }

        private static HtmlElement CreateTypeSpan(string value)
        {
            return HtmlElement.CreateSpan(value, "type");
        }

        private static HtmlElement CreateTypeNameSpan(Type type)
        {
            string typeName = GetTypeName(type);
            HtmlElement span = new HtmlElement("span");
            StringBuilder sb = new StringBuilder();
            // Convert the type name into html elements with different css classes
            foreach (var ch in typeName)
            {
                if (IsOperator(ch))
                {
                    if (sb.Length > 0)
                    {
                        span.AppendChild(CreateTypeSpan(sb.ToString()));
                        sb.Clear();
                    }
                    span.AppendChild(CreateOperatorSpan(ch.ToString()));
                }
                else
                {
                    sb.Append(ch);
                }
            }
            if (sb.Length > 0)
            {
                span.AppendChild(CreateTypeSpan(sb.ToString()));
            }
            return span;
        }

        private static bool IsOperator(char ch)
        {
            // These are the operators we expect to see within type names
            return ch == '[' || ch == ']' || ch == '<' || ch == '>' || ch == '&' || ch == '*';
        }

        internal void PushElement(HtmlElement element)
        {
            _elementStack.Push(element);
        }

        internal HtmlElement PopElement()
        {
            Debug.Assert(_elementStack.Count > 0);
            return _elementStack.Pop();
        }

        internal static string ConvertEscapseSequences(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ch in value)
            {
                sb.Append(GetCharValue(ch));
            }
            return sb.ToString();
        }

        private static string GetCharValue(char ch)
        {
            string value;
            if (_printableEscapeChars.TryGetValue(ch, out value))
            {
                return value;
            }
            // REVIEW: Perf?
            return ch.ToString();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        private enum InputType
        {
            Text,
            Password,
            Hidden
        }

        public IHtmlString TextBox(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildInputField(name, InputType.Text, value: null, isExplicitValue: false,
                                   attributes: (IDictionary<string, object>)null);
        }

        public IHtmlString TextBox(string name, object value)
        {
            return TextBox(name, value, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString TextBox(string name, object value, object htmlAttributes)
        {
            return TextBox(name, value, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString TextBox(string name, object value, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildInputField(name, InputType.Text, value, isExplicitValue: true, attributes: htmlAttributes);
        }

        public IHtmlString Hidden(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildInputField(name, InputType.Hidden, value: null, isExplicitValue: false,
                                   attributes: (IDictionary<string, object>)null);
        }

        public IHtmlString Hidden(string name, object value)
        {
            return Hidden(name, value, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString Hidden(string name, object value, object htmlAttributes)
        {
            return Hidden(name, value, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString Hidden(string name, object value, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildInputField(name, InputType.Hidden, GetHiddenFieldValue(value), isExplicitValue: true,
                                   attributes: htmlAttributes);
        }

        private static object GetHiddenFieldValue(object value)
        {
            Binary binaryValue = value as Binary;
            if (binaryValue != null)
            {
                value = binaryValue.ToArray();
            }

            byte[] byteArrayValue = value as byte[];
            if (byteArrayValue != null)
            {
                value = Convert.ToBase64String(byteArrayValue);
            }

            return value;
        }

        public IHtmlString Password(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildInputField(name, InputType.Password, null, isExplicitValue: false,
                                   attributes: (IDictionary<string, object>)null);
        }

        public IHtmlString Password(string name, object value)
        {
            return Password(name, value, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString Password(string name, object value, object htmlAttributes)
        {
            return Password(name, value, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString Password(string name, object value, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildInputField(name, InputType.Password, value, isExplicitValue: true, attributes: htmlAttributes);
        }

        private IHtmlString BuildInputField(string name, InputType type, object value, bool isExplicitValue,
                                            IDictionary<string, object> attributes)
        {
            TagBuilder tagBuilder = new TagBuilder("input");
            // Implicit parameters
            tagBuilder.MergeAttribute("type", GetInputTypeString(type));
            tagBuilder.GenerateId(name);

            // Overwrite implicit
            tagBuilder.MergeAttributes(attributes, replaceExisting: true);

            if (UnobtrusiveJavaScriptEnabled)
            {
                // Add validation attributes
                var validationAttributes = _validationHelper.GetUnobtrusiveValidationAttributes(name);
                tagBuilder.MergeAttributes(validationAttributes, replaceExisting: false);
            }

            // Function arguments
            tagBuilder.MergeAttribute("name", name, replaceExisting: true);
            var modelState = ModelState[name];
            if ((type != InputType.Password) && modelState != null)
            {
                // Don't use model values for passwords
                value = value ?? modelState.Value ?? String.Empty;
            }

            if ((type != InputType.Password) || ((type == InputType.Password) && (value != null)))
            {
                // Review: Do we really need to be this pedantic about sticking to mvc?
                tagBuilder.MergeAttribute("value", (string)ConvertTo(value, typeof(string)), replaceExisting: isExplicitValue);
            }

            AddErrorClass(tagBuilder, name);
            return tagBuilder.ToHtmlString(TagRenderMode.SelfClosing);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Input types are specified in lower case")]
        private static string GetInputTypeString(InputType inputType)
        {
            if (!Enum.IsDefined(typeof(InputType), inputType))
            {
                inputType = InputType.Text;
            }
            return inputType.ToString().ToLowerInvariant();
        }
    }
}

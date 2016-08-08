// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        public IHtmlString RadioButton(string name, object value)
        {
            return RadioButton(name, value, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString RadioButton(string name, object value, object htmlAttributes)
        {
            return RadioButton(name, value, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString RadioButton(string name, object value, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildRadioButton(name, value, isChecked: null, attributes: htmlAttributes);
        }

        public IHtmlString RadioButton(string name, object value, bool isChecked)
        {
            return RadioButton(name, value, isChecked, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString RadioButton(string name, object value, bool isChecked, object htmlAttributes)
        {
            return RadioButton(name, value, isChecked, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString RadioButton(string name, object value, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            if (name == null)
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            return BuildRadioButton(name, value, isChecked, htmlAttributes);
        }

        private IHtmlString BuildRadioButton(string name, object value, bool? isChecked, IDictionary<string, object> attributes)
        {
            string valueString = ConvertTo(value, typeof(string)) as string;

            TagBuilder builder = new TagBuilder("input");
            builder.MergeAttribute("type", "radio", true);
            builder.GenerateId(name);
            builder.MergeAttributes(attributes, replaceExisting: true);

            builder.MergeAttribute("value", valueString, replaceExisting: true);
            builder.MergeAttribute("name", name, replaceExisting: true);

            if (UnobtrusiveJavaScriptEnabled)
            {
                // Add validation attributes
                var validationAttributes = _validationHelper.GetUnobtrusiveValidationAttributes(name);
                builder.MergeAttributes(validationAttributes, replaceExisting: false);
            }

            var modelState = ModelState[name];
            string modelValue = null;
            if (modelState != null)
            {
                modelValue = ConvertTo(modelState.Value, typeof(string)) as string;
                isChecked = isChecked ?? String.Equals(modelValue, valueString, StringComparison.OrdinalIgnoreCase);
            }

            if (isChecked.HasValue)
            {
                // Overrides attribute values
                if (isChecked.Value)
                {
                    builder.MergeAttribute("checked", "checked", true);
                }
                else
                {
                    builder.Attributes.Remove("checked");
                }
            }

            AddErrorClass(builder, name);

            return builder.ToHtmlString(TagRenderMode.SelfClosing);
        }
    }
}

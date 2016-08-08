// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        public IHtmlString CheckBox(string name)
        {
            return CheckBox(name, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString CheckBox(string name, object htmlAttributes)
        {
            return CheckBox(name, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString CheckBox(string name, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildCheckBox(name, null, htmlAttributes);
        }

        public IHtmlString CheckBox(string name, bool isChecked)
        {
            return CheckBox(name, isChecked, (IDictionary<string, object>)null);
        }

        public IHtmlString CheckBox(string name, bool isChecked, object htmlAttributes)
        {
            return CheckBox(name, isChecked, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString CheckBox(string name, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            return BuildCheckBox(name, isChecked, htmlAttributes);
        }

        private IHtmlString BuildCheckBox(string name, bool? isChecked, IDictionary<string, object> attributes)
        {
            TagBuilder builder = new TagBuilder("input");
            builder.MergeAttribute("type", "checkbox", replaceExisting: true);
            builder.GenerateId(name);
            builder.MergeAttributes(attributes, replaceExisting: true);
            builder.MergeAttribute("name", name, replaceExisting: true);

            if (UnobtrusiveJavaScriptEnabled)
            {
                var validationAttributes = _validationHelper.GetUnobtrusiveValidationAttributes(name);
                builder.MergeAttributes(validationAttributes, replaceExisting: false);
            }

            var model = ModelState[name];
            if (model != null && model.Value != null)
            {
                bool modelValue = (bool)ConvertTo(model.Value, typeof(bool));
                isChecked = isChecked ?? modelValue;
            }
            if (isChecked.HasValue)
            {
                if (isChecked.Value == true)
                {
                    builder.MergeAttribute("checked", "checked", replaceExisting: true);
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

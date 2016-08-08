// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        // Values from mvc
        private const int TextAreaRows = 2;
        private const int TextAreaColumns = 20;

        private static readonly IDictionary<string, object> _implicitRowsAndColumns = new Dictionary<string, object>
        {
            { "rows", TextAreaRows.ToString(CultureInfo.InvariantCulture) },
            { "cols", TextAreaColumns.ToString(CultureInfo.InvariantCulture) },
        };

        private static IDictionary<string, object> GetRowsAndColumnsDictionary(int rows, int columns)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (rows > 0)
            {
                result.Add("rows", rows.ToString(CultureInfo.InvariantCulture));
            }
            if (columns > 0)
            {
                result.Add("cols", columns.ToString(CultureInfo.InvariantCulture));
            }
            return result;
        }

        public IHtmlString TextArea(string name)
        {
            return TextArea(name, value: null, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString TextArea(string name, object htmlAttributes)
        {
            return TextArea(name, value: null, htmlAttributes: AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString TextArea(string name, IDictionary<string, object> htmlAttributes)
        {
            return TextArea(name, value: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString TextArea(string name, string value)
        {
            return TextArea(name, value, (IDictionary<string, object>)null);
        }

        public IHtmlString TextArea(string name, string value, object htmlAttributes)
        {
            return TextArea(name, value, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString TextArea(string name, string value, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildTextArea(name, value, _implicitRowsAndColumns, htmlAttributes);
        }

        public IHtmlString TextArea(string name, string value, int rows, int columns,
                                    object htmlAttributes)
        {
            return TextArea(name, value, rows, columns, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString TextArea(string name, string value, int rows, int columns,
                                    IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            return BuildTextArea(name, value, GetRowsAndColumnsDictionary(rows, columns), htmlAttributes);
        }

        private IHtmlString BuildTextArea(string name, string value, IDictionary<string, object> rowsAndColumnsDictionary,
                                          IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("textarea");

            if (UnobtrusiveJavaScriptEnabled)
            {
                // Add validation attributes
                var validationAttributes = _validationHelper.GetUnobtrusiveValidationAttributes(name);
                tagBuilder.MergeAttributes(validationAttributes, replaceExisting: false);
            }

            // Add user specified htmlAttributes
            tagBuilder.MergeAttributes(htmlAttributes);

            tagBuilder.MergeAttributes(rowsAndColumnsDictionary, rowsAndColumnsDictionary != _implicitRowsAndColumns);

            // Value becomes the inner html of the textarea element
            var modelState = ModelState[name];
            if (modelState != null)
            {
                value = value ?? Convert.ToString(ModelState[name].Value, CultureInfo.CurrentCulture);
            }
            tagBuilder.InnerHtml = Encode(value);

            //Assign name and id
            tagBuilder.MergeAttribute("name", name);
            tagBuilder.GenerateId(name);

            AddErrorClass(tagBuilder, name);

            return tagBuilder.ToHtmlString(TagRenderMode.Normal);
        }
    }
}

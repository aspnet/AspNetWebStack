// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        public IHtmlString ListBox(string name, IEnumerable<SelectListItem> selectList)
        {
            return ListBox(name, defaultOption: null, selectList: selectList, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList)
        {
            return ListBox(name, defaultOption: defaultOption, selectList: selectList, selectedValues: null,
                           htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ListBox(string name, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return ListBox(name, defaultOption: null, selectList: selectList, selectedValues: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ListBox(string name, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            return ListBox(name, defaultOption: null, selectList: selectList, selectedValues: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                   IDictionary<string, object> htmlAttributes)
        {
            return ListBox(name, defaultOption, selectList, selectedValues: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return ListBox(name, defaultOption: defaultOption, selectList: selectList, selectedValues: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList, object selectedValues,
                                   IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            return BuildListBox(name, defaultOption: defaultOption, selectList: selectList,
                                selectedValues: selectedValues, size: null, allowMultiple: false, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList, object selectedValues,
                                   object htmlAttributes)
        {
            return ListBox(name, defaultOption: defaultOption, selectList: selectList,
                           selectedValues: selectedValues, htmlAttributes: AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString ListBox(string name, IEnumerable<SelectListItem> selectList,
                                   object selectedValues, int size, bool allowMultiple)
        {
            return ListBox(name, defaultOption: null, selectList: selectList, selectedValues: selectedValues, size: size,
                           allowMultiple: allowMultiple, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                   object selectedValues, int size, bool allowMultiple)
        {
            return ListBox(name, defaultOption: defaultOption, selectList: selectList, selectedValues: selectedValues,
                           size: size, allowMultiple: allowMultiple, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                   object selectedValues, int size, bool allowMultiple, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }

            return BuildListBox(name, defaultOption, selectList, selectedValues, size, allowMultiple, htmlAttributes);
        }

        public IHtmlString ListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                   object selectedValues, int size, bool allowMultiple, object htmlAttributes)
        {
            return ListBox(name, defaultOption, selectList, selectedValues, size, allowMultiple, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        private IHtmlString BuildListBox(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                         object selectedValues, int? size, bool allowMultiple, IDictionary<string, object> htmlAttributes)
        {
            var modelState = ModelState[name];
            if (modelState != null)
            {
                selectedValues = selectedValues ?? ModelState[name].Value;
            }

            if (selectedValues != null)
            {
                IEnumerable values = (allowMultiple) ? ConvertTo(selectedValues, typeof(string[])) as string[]
                                         : new[] { ConvertTo(selectedValues, typeof(string)) };

                HashSet<string> selectedValueSet = new HashSet<string>(from object value in values
                                                                       select Convert.ToString(value, CultureInfo.CurrentCulture),
                                                                       StringComparer.OrdinalIgnoreCase);
                List<SelectListItem> newSelectList = new List<SelectListItem>();

                bool previousSelected = false;
                foreach (SelectListItem item in selectList)
                {
                    bool selected = false;
                    // If the user's specified allowed multiple to be false
                    // only pick up the first item that was selected.
                    if (allowMultiple || !previousSelected)
                    {
                        selected = item.Selected || selectedValueSet.Contains(item.Value ?? item.Text);
                    }
                    previousSelected = previousSelected | selected;

                    newSelectList.Add(new SelectListItem(item) { Selected = selected });
                }
                selectList = newSelectList;
            }

            TagBuilder tagBuilder = new TagBuilder("select")
            {
                InnerHtml = BuildListOptions(selectList, defaultOption)
            };

            if (UnobtrusiveJavaScriptEnabled)
            {
                // Add validation attributes
                var validationAttributes = _validationHelper.GetUnobtrusiveValidationAttributes(name);
                tagBuilder.MergeAttributes(validationAttributes, replaceExisting: false);
            }

            tagBuilder.GenerateId(name);
            tagBuilder.MergeAttributes(htmlAttributes);

            tagBuilder.MergeAttribute("name", name, replaceExisting: true);
            if (size.HasValue)
            {
                tagBuilder.MergeAttribute("size", size.ToString(), true);
            }
            if (allowMultiple)
            {
                tagBuilder.MergeAttribute("multiple", "multiple");
            }
            else if (tagBuilder.Attributes.ContainsKey("multiple"))
            {
                tagBuilder.Attributes.Remove("multiple");
            }

            // If there are any errors for a named field, we add the css attribute.
            AddErrorClass(tagBuilder, name);

            return tagBuilder.ToHtmlString(TagRenderMode.Normal);
        }

        public IHtmlString DropDownList(string name, IEnumerable<SelectListItem> selectList)
        {
            return DropDownList(name, defaultOption: null, selectList: selectList, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString DropDownList(string name, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return DropDownList(name, defaultOption: null, selectList: selectList, selectedValue: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString DropDownList(string name, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            return DropDownList(name, defaultOption: null, selectList: selectList, selectedValue: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString DropDownList(string name, string defaultOption, IEnumerable<SelectListItem> selectList)
        {
            return DropDownList(name, defaultOption, selectList, selectedValue: null, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString DropDownList(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                        IDictionary<string, object> htmlAttributes)
        {
            return DropDownList(name, defaultOption, selectList, selectedValue: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString DropDownList(string name, string defaultOption, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return DropDownList(name, defaultOption: defaultOption, selectList: selectList, selectedValue: null, htmlAttributes: htmlAttributes);
        }

        public IHtmlString DropDownList(string name, string defaultOption, IEnumerable<SelectListItem> selectList, object selectedValue,
                                        IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            return BuildDropDownList(name, defaultOption, selectList, selectedValue, htmlAttributes: htmlAttributes);
        }

        public IHtmlString DropDownList(string name, string defaultOption, IEnumerable<SelectListItem> selectList, object selectedValue,
                                        object htmlAttributes)
        {
            return DropDownList(name, defaultOption, selectList, selectedValue, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        private IHtmlString BuildDropDownList(string name, string defaultOption, IEnumerable<SelectListItem> selectList,
                                              object selectedValue, IDictionary<string, object> htmlAttributes)
        {
            var modelState = ModelState[name];
            if (modelState != null)
            {
                selectedValue = selectedValue ?? ModelState[name].Value;
            }
            selectedValue = ConvertTo(selectedValue, typeof(string));

            if (selectedValue != null)
            {
                var newSelectList = new List<SelectListItem>(from item in selectList
                                                             select new SelectListItem(item));
                var comparer = StringComparer.InvariantCultureIgnoreCase;
                var selectedItem = newSelectList.FirstOrDefault(item => item.Selected || comparer.Equals(item.Value ?? item.Text, selectedValue));
                if (selectedItem != default(SelectListItem))
                {
                    selectedItem.Selected = true;
                    selectList = newSelectList;
                }
            }

            TagBuilder tagBuilder = new TagBuilder("select")
            {
                InnerHtml = BuildListOptions(selectList, defaultOption)
            };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", name, replaceExisting: true);
            tagBuilder.GenerateId(name);
            if (UnobtrusiveJavaScriptEnabled)
            {
                var validationAttributes = _validationHelper.GetUnobtrusiveValidationAttributes(name);
                tagBuilder.MergeAttributes(validationAttributes, replaceExisting: false);
            }

            // If there are any errors for a named field, we add the css attribute.
            AddErrorClass(tagBuilder, name);

            return tagBuilder.ToHtmlString(TagRenderMode.Normal);
        }

        private static string BuildListOptions(IEnumerable<SelectListItem> selectList, string optionText)
        {
            StringBuilder builder = new StringBuilder().AppendLine();
            if (optionText != null)
            {
                builder.AppendLine(ListItemToOption(new SelectListItem { Text = optionText, Value = String.Empty }));
            }
            if (selectList != null)
            {
                foreach (var item in selectList)
                {
                    builder.AppendLine(ListItemToOption(item));
                }
            }
            return builder.ToString();
        }

        private static string ListItemToOption(SelectListItem item)
        {
            TagBuilder builder = new TagBuilder("option")
            {
                InnerHtml = HttpUtility.HtmlEncode(item.Text)
            };
            if (item.Value != null)
            {
                builder.Attributes["value"] = item.Value;
            }
            if (item.Selected)
            {
                builder.Attributes["selected"] = "selected";
            }
            return builder.ToString(TagRenderMode.Normal);
        }
    }
}

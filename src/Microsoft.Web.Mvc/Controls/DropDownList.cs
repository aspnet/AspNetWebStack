// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Controls
{
    // TODO: Have ListBoxBase class to use with DropDownList and ListBox?
    // TODO: Do we need a way to explicitly specify the items? And only get the selected value(s) from ViewData?

    public class DropDownList : MvcControl
    {
        private string _name;
        private string _optionLabel;

        [DefaultValue("")]
        public string Name
        {
            get { return _name ?? String.Empty; }
            set { _name = value; }
        }

        [DefaultValue("")]
        public string OptionLabel
        {
            get { return _optionLabel ?? String.Empty; }
            set { _optionLabel = value; }
        }

        private object GetModelStateValue(string key, Type destinationType)
        {
            ModelState modelState;
            if (ViewData.ModelState.TryGetValue(key, out modelState))
            {
                return modelState.Value.ConvertTo(destinationType, null /* culture */);
            }
            return null;
        }

        private IEnumerable<SelectListItem> GetSelectData(string name)
        {
            object o = null;
            if (ViewData != null)
            {
                o = ViewData.Eval(name);
            }
            if (o == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.HtmlHelper_MissingSelectData,
                        name,
                        "IEnumerable<SelectListItem>"));
            }
            IEnumerable<SelectListItem> selectList = o as IEnumerable<SelectListItem>;
            if (selectList == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.HtmlHelper_WrongSelectDataType,
                        name,
                        o.GetType().FullName,
                        "IEnumerable<SelectListItem>"));
            }
            return selectList;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode && String.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException(MvcResources.CommonControls_NameRequired);
            }

            if (DesignMode)
            {
                RenderDesignMode(writer);
            }
            else
            {
                RenderRuntime(writer);
            }
        }

        private void RenderDesignMode(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Select);
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            if (String.IsNullOrEmpty(OptionLabel))
            {
                writer.WriteEncodedText(MvcResources.DropDownList_SampleItem);
            }
            else
            {
                writer.WriteEncodedText(OptionLabel);
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void RenderRuntime(HtmlTextWriter writer)
        {
            // TODO: Move this to the base class once it exists
            bool allowMultiple = false;

            SortedDictionary<string, string> attrs = new SortedDictionary<string, string>();

            foreach (KeyValuePair<string, string> attribute in Attributes)
            {
                attrs.Add(attribute.Key, attribute.Value);
            }

            attrs.Add("name", Name);
            if (!String.IsNullOrEmpty(ID))
            {
                attrs.Add("id", ID);
            }
            if (allowMultiple)
            {
                attrs.Add("multiple", "multiple");
            }

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (ViewData.ModelState.TryGetValue(Name, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    string currentValue;

                    if (attrs.TryGetValue("class", out currentValue))
                    {
                        attrs["class"] = HtmlHelper.ValidationInputCssClassName + " " + currentValue;
                    }
                    else
                    {
                        attrs["class"] = HtmlHelper.ValidationInputCssClassName;
                    }
                }
            }

            foreach (KeyValuePair<string, string> attribute in attrs)
            {
                writer.AddAttribute(attribute.Key, Convert.ToString(attribute.Value, CultureInfo.CurrentCulture));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            // Use ViewData to get the list of items
            IEnumerable<SelectListItem> selectList = GetSelectData(Name);

            object defaultValue = (allowMultiple) ? GetModelStateValue(Name, typeof(string[])) : GetModelStateValue(Name, typeof(string));

            if (defaultValue != null)
            {
                IEnumerable defaultValues = (allowMultiple) ? defaultValue as IEnumerable : new[] { defaultValue };
                IEnumerable<string> values = from object value in defaultValues
                                             select Convert.ToString(value, CultureInfo.CurrentCulture);
                HashSet<string> selectedValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
                List<SelectListItem> newSelectList = new List<SelectListItem>();

                foreach (SelectListItem item in selectList)
                {
                    item.Selected = (item.Value != null) ? selectedValues.Contains(item.Value) : selectedValues.Contains(item.Text);
                    newSelectList.Add(item);
                }
                selectList = newSelectList;
            }

            // Render the option label if it exists
            if (!String.IsNullOrEmpty(OptionLabel))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, String.Empty);
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.WriteEncodedText(OptionLabel);
                writer.RenderEndTag();
            }

            // Render out the list items
            foreach (SelectListItem listItem in selectList)
            {
                if (listItem.Value != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, listItem.Value);
                }
                if (listItem.Selected)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.WriteEncodedText(listItem.Text);
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }
    }
}

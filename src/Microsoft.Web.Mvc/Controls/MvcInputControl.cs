// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Controls
{
    public abstract class MvcInputControl : MvcControl
    {
        private string _format;
        private string _name;

        protected MvcInputControl(string inputType)
        {
            if (String.IsNullOrEmpty(inputType))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "inputType");
            }
            InputType = inputType;
        }

        [DefaultValue("")]
        public string Format
        {
            get { return _format ?? String.Empty; }
            set { _format = value; }
        }

        [Browsable(false)]
        public string InputType { get; private set; }

        [DefaultValue("")]
        public string Name
        {
            get { return _name ?? String.Empty; }
            set { _name = value; }
        }

        private ModelState GetModelState()
        {
            return ViewData.ModelState[Name];
        }

        private object GetModelStateValue(Type destinationType)
        {
            ModelState modelState = GetModelState();
            if (modelState != null)
            {
                return modelState.Value.ConvertTo(destinationType, null /* culture */);
            }
            return null;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode && String.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException(MvcResources.CommonControls_NameRequired);
            }

            SortedDictionary<string, string> attrs = new SortedDictionary<string, string>();

            foreach (KeyValuePair<string, string> attribute in Attributes)
            {
                attrs.Add(attribute.Key, attribute.Value);
            }

            if (!Attributes.ContainsKey("type"))
            {
                attrs.Add("type", InputType);
            }
            attrs.Add("name", Name);
            if (!String.IsNullOrEmpty(ID))
            {
                attrs.Add("id", ID);
            }

            if (DesignMode)
            {
                // Use a dummy value in design mode
                attrs.Add("value", "TextBox");
            }
            else
            {
                string attemptedValue = (string)GetModelStateValue(typeof(string));

                if (attemptedValue != null)
                {
                    // Never format the attempted value since it was already formatted in the previous request
                    attrs.Add("value", attemptedValue);
                }
                else
                {
                    // Use an explicit value attribute if it is available. Otherwise get it from ViewData.
                    string attributeValue;
                    Attributes.TryGetValue("value", out attributeValue);
                    object rawValue = attributeValue ?? ViewData.Eval(Name);
                    string stringValue;

                    if (String.IsNullOrEmpty(Format))
                    {
                        stringValue = Convert.ToString(rawValue, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        stringValue = String.Format(CultureInfo.CurrentCulture, Format, rawValue);
                    }

                    // The HtmlTextWriter will automatically encode this value
                    attrs.Add("value", stringValue);
                }

                // If there are any errors for a named field, we add the CSS attribute.
                ModelState modelState = GetModelState();
                if ((modelState != null) && (modelState.Errors.Count > 0))
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

            writer.RenderBeginTag(HtmlTextWriterTag.Input);

            writer.RenderEndTag();
        }
    }
}

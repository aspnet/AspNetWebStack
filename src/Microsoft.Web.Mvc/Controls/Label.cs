// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Controls
{
    public class Label : MvcControl
    {
        private string _format;
        private string _name;
        private int _truncateLength = -1;
        private string _truncateText = "...";

        [DefaultValue(EncodeType.Html)]
        public EncodeType EncodeType { get; set; }

        [DefaultValue("")]
        public string Format
        {
            get { return _format ?? String.Empty; }
            set { _format = value; }
        }

        [DefaultValue("")]
        public string Name
        {
            get { return _name ?? String.Empty; }
            set { _name = value; }
        }

        [DefaultValue(-1)]
        [Description("The length of the text at which to truncate the value. Set to -1 to never truncate.")]
        public int TruncateLength
        {
            get { return _truncateLength; }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", "The TruncateLength property must be greater than or equal to -1.");
                }
                _truncateLength = value;
            }
        }

        [DefaultValue("...")]
        [Description("The text to display at the end of the string if it is truncated. This text is never encoded.")]
        public string TruncateText
        {
            get { return _truncateText ?? String.Empty; }
            set { _truncateText = value; }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode && String.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException(MvcResources.CommonControls_NameRequired);
            }

            string stringValue = String.Empty;
            if (ViewData != null)
            {
                object rawValue = ViewData.Eval(Name);

                if (String.IsNullOrEmpty(Format))
                {
                    stringValue = Convert.ToString(rawValue, CultureInfo.CurrentCulture);
                }
                else
                {
                    stringValue = String.Format(CultureInfo.CurrentCulture, Format, rawValue);
                }
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Name, Name);
            if (!String.IsNullOrEmpty(ID))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ID);
            }

            bool wasTruncated = false;
            if ((TruncateLength >= 0) && (stringValue.Length > TruncateLength))
            {
                stringValue = stringValue.Substring(0, TruncateLength);
                wasTruncated = true;
            }

            switch (EncodeType)
            {
                case EncodeType.Html:
                    writer.Write(HttpUtility.HtmlEncode(stringValue));
                    break;
                case EncodeType.HtmlAttribute:
                    writer.Write(HttpUtility.HtmlAttributeEncode(stringValue));
                    break;
                case EncodeType.None:
                    writer.Write(stringValue);
                    break;
            }

            if (wasTruncated)
            {
                writer.Write(TruncateText);
            }
        }
    }
}

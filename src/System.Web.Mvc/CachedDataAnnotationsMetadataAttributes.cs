// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Web.Mvc
{
    public class CachedDataAnnotationsMetadataAttributes
    {
        public CachedDataAnnotationsMetadataAttributes(Attribute[] attributes)
        {
            DataType = attributes.OfType<DataTypeAttribute>().FirstOrDefault();
            Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            DisplayColumn = attributes.OfType<DisplayColumnAttribute>().FirstOrDefault();
            DisplayFormat = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            DisplayName = attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
            Editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            HiddenInput = attributes.OfType<HiddenInputAttribute>().FirstOrDefault();
            ReadOnly = attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
            Required = attributes.OfType<RequiredAttribute>().FirstOrDefault();
            ScaffoldColumn = attributes.OfType<ScaffoldColumnAttribute>().FirstOrDefault();

            var uiHintAttributes = attributes.OfType<UIHintAttribute>();
            UIHint = uiHintAttributes.FirstOrDefault(a => String.Equals(a.PresentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))
                     ?? uiHintAttributes.FirstOrDefault(a => String.IsNullOrEmpty(a.PresentationLayer));

            if (DisplayFormat == null && DataType != null)
            {
                DisplayFormat = DataType.DisplayFormat;
            }
        }

        public DataTypeAttribute DataType { get; protected set; }

        public DisplayAttribute Display { get; protected set; }

        public DisplayColumnAttribute DisplayColumn { get; protected set; }

        public DisplayFormatAttribute DisplayFormat { get; protected set; }

        public DisplayNameAttribute DisplayName { get; protected set; }

        public EditableAttribute Editable { get; protected set; }

        public HiddenInputAttribute HiddenInput { get; protected set; }

        public ReadOnlyAttribute ReadOnly { get; protected set; }

        public RequiredAttribute Required { get; protected set; }

        public ScaffoldColumnAttribute ScaffoldColumn { get; protected set; }

        public UIHintAttribute UIHint { get; protected set; }
    }
}

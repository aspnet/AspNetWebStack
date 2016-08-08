// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Web.Mvc
{
    public class DataAnnotationsModelMetadataProvider : AssociatedMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            List<Attribute> attributeList = new List<Attribute>(attributes);
            DisplayColumnAttribute displayColumnAttribute = attributeList.OfType<DisplayColumnAttribute>().FirstOrDefault();
            DataAnnotationsModelMetadata result = new DataAnnotationsModelMetadata(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);

            // Do [HiddenInput] before [UIHint], so you can override the template hint
            HiddenInputAttribute hiddenInputAttribute = attributeList.OfType<HiddenInputAttribute>().FirstOrDefault();
            if (hiddenInputAttribute != null)
            {
                result.TemplateHint = "HiddenInput";
                result.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
            }

            // We prefer [UIHint("...", PresentationLayer = "MVC")] but will fall back to [UIHint("...")]
            IEnumerable<UIHintAttribute> uiHintAttributes = attributeList.OfType<UIHintAttribute>();
            UIHintAttribute uiHintAttribute = uiHintAttributes.FirstOrDefault(a => String.Equals(a.PresentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))
                                              ?? uiHintAttributes.FirstOrDefault(a => String.IsNullOrEmpty(a.PresentationLayer));
            if (uiHintAttribute != null)
            {
                result.TemplateHint = uiHintAttribute.UIHint;
            }

            EditableAttribute editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            if (editable != null)
            {
                result.IsReadOnly = !editable.AllowEdit;
            }
            else
            {
                ReadOnlyAttribute readOnlyAttribute = attributeList.OfType<ReadOnlyAttribute>().FirstOrDefault();
                if (readOnlyAttribute != null)
                {
                    result.IsReadOnly = readOnlyAttribute.IsReadOnly;
                }
            }

            DataTypeAttribute dataTypeAttribute = attributeList.OfType<DataTypeAttribute>().FirstOrDefault();
            DisplayFormatAttribute displayFormatAttribute = attributeList.OfType<DisplayFormatAttribute>().FirstOrDefault();
            SetFromDataTypeAndDisplayAttributes(result, dataTypeAttribute, displayFormatAttribute);

            ScaffoldColumnAttribute scaffoldColumnAttribute = attributeList.OfType<ScaffoldColumnAttribute>().FirstOrDefault();
            if (scaffoldColumnAttribute != null)
            {
                result.ShowForDisplay = result.ShowForEdit = scaffoldColumnAttribute.Scaffold;
            }

            DisplayAttribute display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            string name = null;
            if (display != null)
            {
                result.Description = display.GetDescription();
                result.ShortDisplayName = display.GetShortName();
                result.Watermark = display.GetPrompt();
                result.Order = display.GetOrder() ?? ModelMetadata.DefaultOrder;

                name = display.GetName();
            }

            if (name != null)
            {
                result.DisplayName = name;
            }
            else
            {
                DisplayNameAttribute displayNameAttribute = attributeList.OfType<DisplayNameAttribute>().FirstOrDefault();
                if (displayNameAttribute != null)
                {
                    result.DisplayName = displayNameAttribute.DisplayName;
                }
            }

            RequiredAttribute requiredAttribute = attributeList.OfType<RequiredAttribute>().FirstOrDefault();
            if (requiredAttribute != null)
            {
                result.IsRequired = true;
            }

            return result;
        }

        private static void SetFromDataTypeAndDisplayAttributes(DataAnnotationsModelMetadata result,
            DataTypeAttribute dataTypeAttribute, DisplayFormatAttribute displayFormatAttribute)
        {
            if (dataTypeAttribute != null)
            {
                result.DataTypeName = dataTypeAttribute.ToDataTypeName();
            }

            if (displayFormatAttribute == null && dataTypeAttribute != null)
            {
                displayFormatAttribute = dataTypeAttribute.DisplayFormat;

                // If DisplayFormat value was non-null and this [DataType] is of a subclass, assume the [DataType]
                // constructor used the protected DisplayFormat setter to override its default. Note deriving from
                // [DataType] but preserving DataFormatString and ApplyFormatInEditMode results in
                // HasNonDefaultEditFormat==true.
                if (displayFormatAttribute != null && dataTypeAttribute.GetType() != typeof(DataTypeAttribute))
                {
                    result.HasNonDefaultEditFormat = true;
                }
            }
            else if (displayFormatAttribute != null)
            {
                result.HasNonDefaultEditFormat = true;
            }

            if (displayFormatAttribute != null)
            {
                result.NullDisplayText = displayFormatAttribute.NullDisplayText;
                result.DisplayFormatString = displayFormatAttribute.DataFormatString;
                result.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;
                result.HtmlEncode = displayFormatAttribute.HtmlEncode;

                if (displayFormatAttribute.ApplyFormatInEditMode)
                {
                    result.EditFormatString = displayFormatAttribute.DataFormatString;
                }

                if (!displayFormatAttribute.HtmlEncode && String.IsNullOrWhiteSpace(result.DataTypeName))
                {
                    result.DataTypeName = DataTypeUtil.HtmlTypeName;
                }

                // Regardless of HasNonDefaultEditFormat calculation above, treat missing EditFormatString as the
                // default.  Note the corner case of a [DataType] subclass overriding a non-empty default to apply a
                // [DisplayFormat] lacking DataFormatString or with ApplyFormatInEditMode==false results in
                // HasNonDefaultEditFormat==false.
                if (String.IsNullOrEmpty(result.EditFormatString))
                {
                    result.HasNonDefaultEditFormat = false;
                }
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Web.Mvc;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        private void AddErrorClass(TagBuilder tagBuilder, string name)
        {
            if (!ModelState.IsValidField(name))
            {
                tagBuilder.AddCssClass(ValidationInputCssClassName);
            }
        }

        private static object ConvertTo(object value, Type type)
        {
            Debug.Assert(type != null);
            return UnwrapPossibleArrayType(value, type, CultureInfo.InvariantCulture);
        }

        private static object UnwrapPossibleArrayType(object value, Type destinationType, CultureInfo culture)
        {
            if (value == null || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            // array conversion results in four cases, as below
            Array valueAsArray = value as Array;
            if (destinationType.IsArray)
            {
                Type destinationElementType = destinationType.GetElementType();
                if (valueAsArray != null)
                {
                    // case 1: both destination + source type are arrays, so convert each element
                    IList converted = Array.CreateInstance(destinationElementType, valueAsArray.Length);
                    for (int i = 0; i < valueAsArray.Length; i++)
                    {
                        converted[i] = ConvertSimpleType(valueAsArray.GetValue(i), destinationElementType, culture);
                    }
                    return converted;
                }
                else
                {
                    // case 2: destination type is array but source is single element, so wrap element in array + convert
                    object element = ConvertSimpleType(value, destinationElementType, culture);
                    IList converted = Array.CreateInstance(destinationElementType, 1);
                    converted[0] = element;
                    return converted;
                }
            }
            else if (valueAsArray != null)
            {
                // case 3: destination type is single element but source is array, so extract first element + convert
                if (valueAsArray.Length > 0)
                {
                    value = valueAsArray.GetValue(0);
                    return ConvertSimpleType(value, destinationType, culture);
                }
                else
                {
                    // case 3(a): source is empty array, so can't perform conversion
                    return null;
                }
            }
            // case 4: both destination + source type are single elements, so convert
            return ConvertSimpleType(value, destinationType, culture);
        }

        private static object ConvertSimpleType(object value, Type destinationType, CultureInfo culture)
        {
            if (value == null || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            // if this is a user-input value but the user didn't type anything, return no value
            string valueAsString = value as string;
            if (valueAsString != null && valueAsString.Trim().Length == 0)
            {
                return null;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool canConvertFrom = converter.CanConvertFrom(value.GetType());
            if (!canConvertFrom)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!(canConvertFrom || converter.CanConvertTo(destinationType)))
            {
                string message = String.Format(CultureInfo.CurrentCulture, WebPageResources.HtmlHelper_NoConverterExists,
                                               value.GetType().FullName, destinationType.FullName);
                throw new InvalidOperationException(message);
            }

            try
            {
                object convertedValue = (canConvertFrom)
                                            ? converter.ConvertFrom(context: null, culture: culture, value: value)
                                            : converter.ConvertTo(context: null, culture: culture, value: value, destinationType: destinationType);
                return convertedValue;
            }
            catch (Exception ex)
            {
                string message = String.Format(CultureInfo.CurrentUICulture, WebPageResources.HtmlHelper_ConversionThrew,
                                               value.GetType().FullName, destinationType.FullName);
                throw new InvalidOperationException(message, ex);
            }
        }
    }
}

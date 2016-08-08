// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Web.Http.Properties;

namespace System.Web.Http.ValueProviders
{
    [Serializable]
    public class ValueProviderResult
    {
        private static readonly CultureInfo _staticCulture = CultureInfo.InvariantCulture;
        private CultureInfo _instanceCulture;

        // default constructor so that subclassed types can set the properties themselves
        protected ValueProviderResult()
        {
        }

        public ValueProviderResult(object rawValue, string attemptedValue, CultureInfo culture)
        {
            RawValue = rawValue;
            AttemptedValue = attemptedValue;
            Culture = culture;
        }

        public string AttemptedValue { get; protected set; }

        public CultureInfo Culture
        {
            get
            {
                if (_instanceCulture == null)
                {
                    _instanceCulture = _staticCulture;
                }
                return _instanceCulture;
            }
            protected set { _instanceCulture = value; }
        }

        public object RawValue { get; protected set; }

        private static object ConvertSimpleType(CultureInfo culture, object value, Type destinationType)
        {
            if (value == null || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            // if this is a user-input value but the user didn't type anything, return no value
            string valueAsString = value as string;

            if (valueAsString != null && String.IsNullOrWhiteSpace(valueAsString))
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
                // EnumConverter cannot convert integer, so we verify manually
                if (destinationType.IsEnum && value is int)
                {
                    return Enum.ToObject(destinationType, (int)value);
                }

                // In case of a Nullable object, we try again with its underlying type.
                Type underlyingType = Nullable.GetUnderlyingType(destinationType);
                if (underlyingType != null)
                {
                    return ConvertSimpleType(culture, value, underlyingType);
                }

                throw Error.InvalidOperation(SRResources.ValueProviderResult_NoConverterExists, value.GetType(), destinationType);
            }

            try
            {
                return canConvertFrom
                           ? converter.ConvertFrom(null, culture, value)
                           : converter.ConvertTo(null, culture, value, destinationType);
            }
            catch (Exception ex)
            {
                throw Error.InvalidOperation(ex, SRResources.ValueProviderResult_ConversionThrew, value.GetType(), destinationType);
            }
        }

        public object ConvertTo(Type type)
        {
            return ConvertTo(type, null /* culture */);
        }

        public virtual object ConvertTo(Type type, CultureInfo culture)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            object value = RawValue;
            if (value == null)
            {
                // treat null route parameters as though they were the default value for the type
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }

            if (type.IsInstanceOfType(value))
            {
                return value;
            }

            CultureInfo cultureToUse = culture ?? Culture;
            return UnwrapPossibleListType(cultureToUse, value, type);
        }

        private static object UnwrapPossibleListType(CultureInfo culture, object value, Type destinationType)
        {
            // array conversion results in four cases, as below
            IList valueAsList = value as IList;
            if (destinationType.IsArray)
            {
                Type destinationElementType = destinationType.GetElementType();
                if (valueAsList != null)
                {
                    // case 1: both destination + source type are lists, so convert each element
                    IList converted = Array.CreateInstance(destinationElementType, valueAsList.Count);
                    for (int i = 0; i < valueAsList.Count; i++)
                    {
                        converted[i] = ConvertSimpleType(culture, valueAsList[i], destinationElementType);
                    }
                    return converted;
                }
                else
                {
                    // case 2: destination type is array but source is single element, so wrap element in array + convert
                    object element = ConvertSimpleType(culture, value, destinationElementType);
                    IList converted = Array.CreateInstance(destinationElementType, 1);
                    converted[0] = element;
                    return converted;
                }
            }
            else if (valueAsList != null)
            {
                // case 3: destination type is single element but source is array, so extract first element + convert
                if (valueAsList.Count > 0)
                {
                    value = valueAsList[0];
                    return ConvertSimpleType(culture, value, destinationType);
                }
                else
                {
                    // case 3(a): source is empty array, so can't perform conversion
                    return null;
                }
            }

            // case 4: both destination + source type are single elements, so convert
            return ConvertSimpleType(culture, value, destinationType);
        }
    }
}

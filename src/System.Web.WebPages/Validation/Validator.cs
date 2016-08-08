// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Mvc;
using System.Web.WebPages.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages
{
    public abstract class Validator
    {
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Required(string errorMessage = null)
        {
            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_Required);
            var clientAttributes = new ModelClientValidationRequiredRule(errorMessage);
            // We don't care if the value is unsafe when verifying that it is required.
            return new ValidationAttributeAdapter(new RequiredAttribute(), errorMessage, clientAttributes, useUnvalidatedValues: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Range(int minValue, int maxValue, string errorMessage = null)
        {
            errorMessage = String.Format(CultureInfo.CurrentCulture, DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_IntegerRange), minValue, maxValue);
            var clientAttributes = new ModelClientValidationRangeRule(errorMessage, minValue, maxValue);
            return new ValidationAttributeAdapter(new RangeAttribute(minValue, maxValue), errorMessage, clientAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Range(double minValue, double maxValue, string errorMessage = null)
        {
            errorMessage = String.Format(CultureInfo.CurrentCulture, DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_FloatRange), minValue, maxValue);
            var clientAttributes = new ModelClientValidationRangeRule(errorMessage, minValue, maxValue);
            return new ValidationAttributeAdapter(new RangeAttribute(minValue, maxValue), errorMessage, clientAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator StringLength(int maxLength, int minLength = 0, string errorMessage = null)
        {
            if (minLength == 0)
            {
                errorMessage = String.Format(CultureInfo.CurrentCulture, DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_StringLength), maxLength);
            }
            else
            {
                errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_StringLengthRange);
                errorMessage = String.Format(CultureInfo.CurrentCulture, errorMessage, minLength, maxLength);
            }
            var clientAttributes = new ModelClientValidationStringLengthRule(errorMessage, minLength, maxLength);

            // We don't care if the value is unsafe when checking the length of the request field passed to us.
            return new ValidationAttributeAdapter(new StringLengthAttribute(maxLength) { MinimumLength = minLength }, errorMessage, clientAttributes,
                                                  useUnvalidatedValues: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Regex(string pattern, string errorMessage = null)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "pattern");
            }

            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_Regex);
            var clientAttributes = new ModelClientValidationRegexRule(errorMessage, pattern);
            return new ValidationAttributeAdapter(new RegularExpressionAttribute(pattern), errorMessage, clientAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator EqualsTo(string otherFieldName, string errorMessage = null)
        {
            if (String.IsNullOrEmpty(otherFieldName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "otherFieldName");
            }

            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_EqualsTo);
            return new CompareValidator(otherFieldName, errorMessage);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator DateTime(string errorMessage = null)
        {
            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_DataType);
            return new DataTypeValidator(DataTypeValidator.SupportedValidationDataType.DateTime, errorMessage);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Decimal(string errorMessage = null)
        {
            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_DataType);
            return new DataTypeValidator(DataTypeValidator.SupportedValidationDataType.Decimal, errorMessage);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Integer(string errorMessage = null)
        {
            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_DataType);
            return new DataTypeValidator(DataTypeValidator.SupportedValidationDataType.Integer, errorMessage);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Url(string errorMessage = null)
        {
            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_DataType);
            return new DataTypeValidator(DataTypeValidator.SupportedValidationDataType.Url, errorMessage);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are ok using default parameters for helpers")]
        public static IValidator Float(string errorMessage = null)
        {
            errorMessage = DefaultIfEmpty(errorMessage, WebPageResources.ValidationDefault_DataType);
            return new DataTypeValidator(DataTypeValidator.SupportedValidationDataType.Float, errorMessage);
        }

        private static string DefaultIfEmpty(string errorMessage, string defaultErrorMessage)
        {
            if (String.IsNullOrEmpty(errorMessage))
            {
                return defaultErrorMessage;
            }
            return errorMessage;
        }
    }
}

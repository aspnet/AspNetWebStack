// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class ClientDataTypeModelValidatorProvider : ModelValidatorProvider
    {
        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>(new Type[]
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        });

        private static string _resourceClassKey;

        public static string ResourceClassKey
        {
            get { return _resourceClassKey ?? String.Empty; }
            set { _resourceClassKey = value; }
        }

        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return GetValidatorsImpl(metadata, context);
        }

        private static IEnumerable<ModelValidator> GetValidatorsImpl(ModelMetadata metadata, ControllerContext context)
        {
            Type type = metadata.ModelType;

            if (IsDateTimeType(type, metadata))
            {
                yield return new DateModelValidator(metadata, context);
            }

            if (IsNumericType(type))
            {
                yield return new NumericModelValidator(metadata, context);
            }
        }

        private static bool IsNumericType(Type type)
        {
            return _numericTypes.Contains(GetTypeToValidate(type));
        }

        private static bool IsDateTimeType(Type type, ModelMetadata metadata)
        {
            return typeof(DateTime) == GetTypeToValidate(type)
                && !String.Equals(metadata.DataTypeName, "Time", StringComparison.OrdinalIgnoreCase);
        }

        private static Type GetTypeToValidate(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type; // strip off the Nullable<>
        }

        // If the user specified a ResourceClassKey try to load the resource they specified.
        // If the class key is invalid, an exception will be thrown.
        // If the class key is valid but the resource is not found, it returns null, in which
        // case it will fall back to the MVC default error message.
        private static string GetUserResourceString(ControllerContext controllerContext, string resourceName)
        {
            string result = null;

            if (!String.IsNullOrEmpty(ResourceClassKey) && (controllerContext != null) && (controllerContext.HttpContext != null))
            {
                result = controllerContext.HttpContext.GetGlobalResourceObject(ResourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string;
            }

            return result;
        }

        private static string GetFieldMustBeNumericResource(ControllerContext controllerContext)
        {
            return GetUserResourceString(controllerContext, "FieldMustBeNumeric") ?? MvcResources.ClientDataTypeModelValidatorProvider_FieldMustBeNumeric;
        }

        private static string GetFieldMustBeDateResource(ControllerContext controllerContext)
        {
            return GetUserResourceString(controllerContext, "FieldMustBeDate") ?? MvcResources.ClientDataTypeModelValidatorProvider_FieldMustBeDate;
        }

        internal class ClientModelValidator : ModelValidator
        {
            private string _errorMessage;
            private string _validationType;

            public ClientModelValidator(ModelMetadata metadata, ControllerContext controllerContext, string validationType, string errorMessage)
                : base(metadata, controllerContext)
            {
                if (String.IsNullOrEmpty(validationType))
                {
                    throw new ArgumentException(MvcResources.Common_NullOrEmpty, "validationType");
                }

                if (String.IsNullOrEmpty(errorMessage))
                {
                    throw new ArgumentException(MvcResources.Common_NullOrEmpty, "errorMessage");
                }

                _validationType = validationType;
                _errorMessage = errorMessage;
            }

            public sealed override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
            {
                ModelClientValidationRule rule = new ModelClientValidationRule()
                {
                    ValidationType = _validationType,
                    ErrorMessage = FormatErrorMessage(Metadata.GetDisplayName())
                };

                return new ModelClientValidationRule[] { rule };
            }

            private string FormatErrorMessage(string displayName)
            {
                // use CurrentCulture since this message is intended for the site visitor
                return String.Format(CultureInfo.CurrentCulture, _errorMessage, displayName);
            }

            public sealed override IEnumerable<ModelValidationResult> Validate(object container)
            {
                // this is not a server-side validator
                return Enumerable.Empty<ModelValidationResult>();
            }
        }

        internal sealed class DateModelValidator : ClientModelValidator
        {
            public DateModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext, "date", GetFieldMustBeDateResource(controllerContext))
            {
            }
        }

        internal sealed class NumericModelValidator : ClientModelValidator
        {
            public NumericModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext, "number", GetFieldMustBeNumericResource(controllerContext))
            {
            }
        }
    }
}

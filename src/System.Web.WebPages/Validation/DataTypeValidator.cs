// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    internal class DataTypeValidator : RequestFieldValidatorBase
    {
        private readonly SupportedValidationDataType _dataType;

        public DataTypeValidator(SupportedValidationDataType type, string errorMessage = null)
            : base(errorMessage)
        {
            _dataType = type;
        }

        public enum SupportedValidationDataType
        {
            DateTime,
            Decimal,
            Url,
            Integer,
            Float
        }

        protected override bool IsValid(HttpContextBase httpContext, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return true;
            }

            switch (_dataType)
            {
                case SupportedValidationDataType.DateTime:
                    return value.IsDateTime();
                case SupportedValidationDataType.Float:
                    return value.IsFloat();
                case SupportedValidationDataType.Decimal:
                    return value.IsDecimal();
                case SupportedValidationDataType.Integer:
                    return value.IsInt();
                case SupportedValidationDataType.Url:
                    return Uri.IsWellFormedUriString(value, UriKind.Absolute);
            }
            return true;
        }
    }
}

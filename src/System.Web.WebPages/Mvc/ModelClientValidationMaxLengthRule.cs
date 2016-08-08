// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class ModelClientValidationMaxLengthRule : ModelClientValidationRule
    {
        public ModelClientValidationMaxLengthRule(string errorMessage, int maximumLength)
        {
            ErrorMessage = errorMessage;
            ValidationType = "maxlength";
            ValidationParameters["max"] = maximumLength;
        }
    }
}

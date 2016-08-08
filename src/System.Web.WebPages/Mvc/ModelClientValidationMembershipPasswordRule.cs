// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    internal class ModelClientValidationMembershipPasswordRule : ModelClientValidationRule
    {
        public ModelClientValidationMembershipPasswordRule(string errorMessage, int minRequiredPasswordLength, int minRequiredNonAlphanumericCharacters, string passwordStrengthRegularExpression)
        {
            ErrorMessage = errorMessage;
            ValidationType = "password";

            if (minRequiredPasswordLength != 0)
            {
                ValidationParameters["min"] = minRequiredPasswordLength;
            }

            if (minRequiredNonAlphanumericCharacters != 0)
            {
                ValidationParameters["nonalphamin"] = minRequiredNonAlphanumericCharacters;
            }

            if (!String.IsNullOrEmpty(passwordStrengthRegularExpression))
            {
                ValidationParameters["regex"] = passwordStrengthRegularExpression;
            }
        }
    }
}

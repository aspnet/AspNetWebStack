// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CreditCardAttribute : DataTypeAttribute, IClientValidatable
    {
        public CreditCardAttribute()
            : base("creditcard")
        {
            ErrorMessage = MvcResources.CreditCardAttribute_Invalid;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            yield return new ModelClientValidationRule
            {
                ValidationType = "creditcard",
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName())
            };
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            string creditCardNumber = value as string;
            if (creditCardNumber == null)
            {
                return false;
            }
            creditCardNumber = creditCardNumber.Replace("-", String.Empty);

            int checksum = 0;
            bool evenDigit = false;

            // http://www.beachnet.com/~hstiles/cardtype.html
            foreach (char digit in creditCardNumber.Reverse())
            {
                if (!Char.IsDigit(digit))
                {
                    return false;
                }

                int digitValue = (digit - '0') * (evenDigit ? 2 : 1);
                evenDigit = !evenDigit;

                while (digitValue > 0)
                {
                    checksum += digitValue % 10;
                    digitValue /= 10;
                }
            }

            return (checksum % 10) == 0;
        }
    }
}

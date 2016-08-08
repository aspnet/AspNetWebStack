// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace System.Web.WebPages
{
    internal class ValidationAttributeAdapter : RequestFieldValidatorBase
    {
        private readonly ValidationAttribute _attribute;
        private readonly ModelClientValidationRule _clientValidationRule;

        public ValidationAttributeAdapter(ValidationAttribute attribute, string errorMessage, ModelClientValidationRule clientValidationRule)
            :
                this(attribute, errorMessage, clientValidationRule, useUnvalidatedValues: false)
        {
        }

        public ValidationAttributeAdapter(ValidationAttribute attribute, string errorMessage, ModelClientValidationRule clientValidationRule, bool useUnvalidatedValues)
            : base(errorMessage, useUnvalidatedValues)
        {
            _attribute = attribute;
            _clientValidationRule = clientValidationRule;
        }

        public ValidationAttribute Attribute
        {
            get { return _attribute; }
        }

        public override ModelClientValidationRule ClientValidationRule
        {
            get { return _clientValidationRule; }
        }

        protected override bool IsValid(HttpContextBase httpContext, string value)
        {
            return _attribute.IsValid(value);
        }
    }
}

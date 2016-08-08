// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Web.Mvc
{
    public class DataAnnotationsModelValidator : ModelValidator
    {
        public DataAnnotationsModelValidator(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            Attribute = attribute;
        }

        protected internal ValidationAttribute Attribute { get; private set; }

        protected internal string ErrorMessage
        {
            get { return Attribute.FormatErrorMessage(Metadata.GetDisplayName()); }
        }

        public override bool IsRequired
        {
            get { return Attribute is RequiredAttribute; }
        }

        internal static ModelValidator Create(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
        {
            return new DataAnnotationsModelValidator(metadata, context, attribute);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            IEnumerable<ModelClientValidationRule> results = base.GetClientValidationRules();

            IClientValidatable clientValidatable = Attribute as IClientValidatable;
            if (clientValidatable != null)
            {
                results = results.Concat(clientValidatable.GetClientValidationRules(Metadata, ControllerContext));
            }

            return results;
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            // Per the WCF RIA Services team, instance can never be null (if you have
            // no parent, you pass yourself for the "instance" parameter).
            string memberName = Metadata.PropertyName ?? Metadata.ModelType.Name;
            ValidationContext context = new ValidationContext(container ?? Metadata.Model)
            {
                DisplayName = Metadata.GetDisplayName(),
                MemberName = memberName
            };

            ValidationResult result = Attribute.GetValidationResult(Metadata.Model, context);
            if (result != ValidationResult.Success)
            {
                // ModelValidationResult.MemberName is used by invoking validators (such as ModelValidator) to 
                // construct the ModelKey for ModelStateDictionary. When validating at type level we want to append the 
                // returned MemberNames if specified (e.g. person.Address.FirstName). For property validation, the 
                // ModelKey can be constructed using the ModelMetadata and we should ignore MemberName (we don't want 
                // (person.Name.Name). However the invoking validator does not have a way to distinguish between these two 
                // cases. Consequently we'll only set MemberName if this validation returns a MemberName that is different
                // from the property being validated.

                string errorMemberName = result.MemberNames.FirstOrDefault();
                if (String.Equals(errorMemberName, memberName, StringComparison.Ordinal))
                {
                    errorMemberName = null;
                }

                var validationResult = new ModelValidationResult
                {
                    Message = result.ErrorMessage,
                    MemberName = errorMemberName
                };

                return new ModelValidationResult[] { validationResult };
            }

            return Enumerable.Empty<ModelValidationResult>();
        }
    }
}

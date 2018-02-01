// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation.Validators
{
    public class DataAnnotationsModelValidator : ModelValidator
    {
        internal static readonly string UseLegacyValidationMemberNameKey = "webapi:UseLegacyValidationMemberName";
        private static bool _useLegacyValidationMemberName =
            GetUseLegacyValidationMemberName(ConfigurationManager.AppSettings);

        public DataAnnotationsModelValidator(
            IEnumerable<ModelValidatorProvider> validatorProviders,
            ValidationAttribute attribute)
            : base(validatorProviders)
        {
            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            Attribute = attribute;
        }

        // Internal for testing
        internal static bool UseLegacyValidationMemberName
        {
            get { return _useLegacyValidationMemberName; }
            set { _useLegacyValidationMemberName = value; }
        }

        protected internal ValidationAttribute Attribute { get; private set; }

        public override bool IsRequired
        {
            get { return Attribute is RequiredAttribute; }
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            string memberName;
            if (_useLegacyValidationMemberName)
            {
                // Using member name from a Display or DisplayFormat attribute is generally incorrect. This
                // (configuration-controlled) override is provided only for corner cases where strict
                // back-compatibility is required.
                memberName = metadata.GetDisplayName();
            }
            else
            {
                // MemberName expression matches GetDisplayName() except it ignores Display and DisplayName
                // attributes. ModelType fallback isn't great and can separate errors and attempted values in the
                // ModelState. But, expression matches MVC, avoids a null MemberName, and is mostly back-compatible.
                memberName = metadata.PropertyName ?? metadata.ModelType.Name;
            }

            // Per the WCF RIA Services team, instance can never be null (if you have
            // no parent, you pass yourself for the "instance" parameter).
            ValidationContext context = new ValidationContext(instance: container ?? metadata.Model)
            {
                DisplayName = metadata.GetDisplayName(),
                MemberName = memberName,
            };

            ValidationResult result = Attribute.GetValidationResult(metadata.Model, context);

            if (result != ValidationResult.Success)
            {
                // ModelValidationResult.MemberName is used by invoking validators (such as ModelValidationNode) to
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

        // Internal for testing
        internal static bool GetUseLegacyValidationMemberName(NameValueCollection appSettings)
        {
            var useLegacyMemberNameArray = appSettings.GetValues(UseLegacyValidationMemberNameKey);
            if (useLegacyMemberNameArray != null &&
                useLegacyMemberNameArray.Length > 0)
            {
                bool useLegacyMemberName;
                if (bool.TryParse(useLegacyMemberNameArray[0], out useLegacyMemberName) &&
                    useLegacyMemberName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

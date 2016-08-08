// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Metadata;
using System.Web.Http.Validation.Validators;

namespace System.Web.Http.Validation.Providers
{
    public class RequiredMemberModelValidatorProvider : ModelValidatorProvider
    {
        private IRequiredMemberSelector _requiredMemberSelector;

        public RequiredMemberModelValidatorProvider(IRequiredMemberSelector requiredMemberSelector)
        {
            _requiredMemberSelector = requiredMemberSelector;
        }

        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            string propertyName = metadata.PropertyName;
            // if we're validating a property and not a type
            if (propertyName != null)
            {
                PropertyInfo metadataProperty = metadata.ContainerType.GetProperty(propertyName);
                if (_requiredMemberSelector.IsRequiredMember(metadataProperty))
                {
                    return new ModelValidator[] { new RequiredMemberModelValidator(validatorProviders) };
                }
            }

            return Enumerable.Empty<ModelValidator>();
        }
    }
}

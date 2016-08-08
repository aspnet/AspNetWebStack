// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.Internal;
using System.Web.Http.Metadata;
using System.Web.Http.Validation.Validators;

namespace System.Web.Http.Validation.Providers
{
    /// <summary>
    /// This <see cref="ModelValidatorProvider"/> provides a required ModelValidator for members marked as [DataMember(IsRequired=true)].
    /// </summary>
    public class DataMemberModelValidatorProvider : AssociatedValidatorProvider
    {
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
            // Types cannot be required; only properties can
            if (metadata.ContainerType == null || String.IsNullOrEmpty(metadata.PropertyName))
            {
                return Enumerable.Empty<ModelValidator>();
            }

            if (IsRequiredDataMember(metadata.ContainerType, attributes))
            {
                return new[] { new RequiredMemberModelValidator(validatorProviders) };
            }

            return Enumerable.Empty<ModelValidator>();
        }

        internal static bool IsRequiredDataMember(Type containerType, IEnumerable<Attribute> attributes)
        {
            DataMemberAttribute dataMemberAttribute = attributes.OfType<DataMemberAttribute>().FirstOrDefault();
            if (dataMemberAttribute != null)
            {
                // isDataContract == true iff the container type has at least one DataContractAttribute
                bool isDataContract = TypeDescriptorHelper.Get(containerType).GetAttributes().OfType<DataContractAttribute>().Any();
                if (isDataContract && dataMemberAttribute.IsRequired)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

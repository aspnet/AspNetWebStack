// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Internal;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;

namespace System.Web.Http.Validation.Providers
{
    public abstract class AssociatedValidatorProvider : ModelValidatorProvider
    {
        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return TypeDescriptorHelper.Get(type);
        }

        public sealed override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            if (metadata.ContainerType != null && !String.IsNullOrEmpty(metadata.PropertyName))
            {
                return GetValidatorsForProperty(metadata, validatorProviders);
            }

            return GetValidatorsForType(metadata, validatorProviders);
        }

        protected abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes);

        private IEnumerable<ModelValidator> GetValidatorsForProperty(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(metadata.ContainerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(metadata.PropertyName, true);
            if (property == null)
            {
                throw Error.Argument("metadata", SRResources.Common_PropertyNotFound, metadata.ContainerType, metadata.PropertyName);
            }

            return GetValidators(metadata, validatorProviders, property.Attributes.OfType<Attribute>());
        }

        private IEnumerable<ModelValidator> GetValidatorsForType(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            return GetValidators(metadata, validatorProviders, GetTypeDescriptor(metadata.ModelType).GetAttributes().Cast<Attribute>());
        }
    }
}

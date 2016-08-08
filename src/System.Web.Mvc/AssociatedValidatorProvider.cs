// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public abstract class AssociatedValidatorProvider : ModelValidatorProvider
    {
        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return TypeDescriptorHelper.Get(type);
        }

        public sealed override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (metadata.ContainerType != null && !String.IsNullOrEmpty(metadata.PropertyName))
            {
                return GetValidatorsForProperty(metadata, context);
            }

            return GetValidatorsForType(metadata, context);
        }

        protected abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes);

        private IEnumerable<ModelValidator> GetValidatorsForProperty(ModelMetadata metadata, ControllerContext context)
        {
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(metadata.ContainerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(metadata.PropertyName, true);
            if (property == null)
            {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.Common_PropertyNotFound,
                        metadata.ContainerType.FullName, metadata.PropertyName),
                    "metadata");
            }
            return GetValidators(metadata, context, new AttributeList(property.Attributes));
        }

        private IEnumerable<ModelValidator> GetValidatorsForType(ModelMetadata metadata, ControllerContext context)
        {
            return GetValidators(metadata, context, new AttributeList(GetTypeDescriptor(metadata.ModelType).GetAttributes()));
        }
    }
}

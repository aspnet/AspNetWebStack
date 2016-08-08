// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.Mvc
{
    public abstract class ModelValidator
    {
        protected ModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            Metadata = metadata;
            ControllerContext = controllerContext;
        }

        protected internal ControllerContext ControllerContext { get; private set; }

        public virtual bool IsRequired
        {
            get { return false; }
        }

        protected internal ModelMetadata Metadata { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public static ModelValidator GetModelValidator(ModelMetadata metadata, ControllerContext context)
        {
            return new CompositeModelValidator(metadata, context);
        }

        public abstract IEnumerable<ModelValidationResult> Validate(object container);

        private class CompositeModelValidator : ModelValidator
        {
            public CompositeModelValidator(ModelMetadata metadata, ControllerContext controllerContext)
                : base(metadata, controllerContext)
            {
            }

            private static ModelValidationResult CreateSubPropertyResult(ModelMetadata propertyMetadata, ModelValidationResult propertyResult)
            {
                return new ModelValidationResult
                {
                    MemberName = DefaultModelBinder.CreateSubPropertyName(propertyMetadata.PropertyName, propertyResult.MemberName),
                    Message = propertyResult.Message
                };
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
            {
                bool propertiesValid = true;

                ModelMetadata[] properties = Metadata.PropertiesAsArray;

                // Performance sensitive loops
                for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
                {
                    ModelMetadata propertyMetadata = properties[propertyIndex];
                    foreach (ModelValidator propertyValidator in propertyMetadata.GetValidators(ControllerContext))
                    {
                        foreach (ModelValidationResult propertyResult in propertyValidator.Validate(Metadata.Model))
                        {
                            propertiesValid = false;
                            yield return CreateSubPropertyResult(propertyMetadata, propertyResult);
                        }
                    }
                }

                if (propertiesValid)
                {
                    foreach (ModelValidator typeValidator in Metadata.GetValidators(ControllerContext))
                    {
                        foreach (ModelValidationResult typeResult in typeValidator.Validate(container))
                        {
                            yield return typeResult;
                        }
                    }
                }
            }
        }
    }
}

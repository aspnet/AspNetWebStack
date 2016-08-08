// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;
using System.Web.Http.Validation.Validators;

namespace System.Web.Http.Validation.Providers
{
    // A factory for validators based on ValidationAttribute
    public delegate ModelValidator DataAnnotationsModelValidationFactory(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute);

    // A factory for validators based on IValidatableObject
    public delegate ModelValidator DataAnnotationsValidatableObjectAdapterFactory(IEnumerable<ModelValidatorProvider> validatorProviders);

    /// <summary>
    /// An implementation of <see cref="ModelValidatorProvider"/> which providers validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>.
    /// </summary>
    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider
    {
        // Factories for validation attributes

        internal DataAnnotationsModelValidationFactory DefaultAttributeFactory =
            (validationProviders, attribute) => new DataAnnotationsModelValidator(validationProviders, attribute);

        internal Dictionary<Type, DataAnnotationsModelValidationFactory> AttributeFactories =
            new Dictionary<Type, DataAnnotationsModelValidationFactory>();

        // Factories for IValidatableObject models
        internal DataAnnotationsValidatableObjectAdapterFactory DefaultValidatableFactory =
            (validationProviders) => new ValidatableObjectAdapter(validationProviders);

        internal Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory> ValidatableFactories =
            new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
                List<ModelValidator> results = new List<ModelValidator>();

                // Produce a validator for each validation attribute we find
                foreach (ValidationAttribute attribute in attributes.OfType<ValidationAttribute>())
                {
                    DataAnnotationsModelValidationFactory factory;
                    if (!AttributeFactories.TryGetValue(attribute.GetType(), out factory))
                    {
                        factory = DefaultAttributeFactory;
                    }
                    results.Add(factory(validatorProviders, attribute));
                }

                // Produce a validator if the type supports IValidatableObject
                if (typeof(IValidatableObject).IsAssignableFrom(metadata.ModelType))
                {
                    DataAnnotationsValidatableObjectAdapterFactory factory;
                    if (!ValidatableFactories.TryGetValue(metadata.ModelType, out factory))
                    {
                        factory = DefaultValidatableFactory;
                    }
                    results.Add(factory(validatorProviders));
                }

                return results;
            }

        #region Validation attribute adapter registration

        public void RegisterAdapter(Type attributeType, Type adapterType)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(attributeType, adapterType);

                AttributeFactories[attributeType] = (context, attribute) => (ModelValidator)constructor.Invoke(new object[] { context, attribute });
            }

        public void RegisterAdapterFactory(Type attributeType, DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeFactory(factory);

                AttributeFactories[attributeType] = factory;
            }

        public void RegisterDefaultAdapter(Type adapterType)
        {
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(typeof(ValidationAttribute), adapterType);

            DefaultAttributeFactory = (context, attribute) => (ModelValidator)constructor.Invoke(new object[] { context, attribute });
        }

        public void RegisterDefaultAdapterFactory(DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeFactory(factory);

            DefaultAttributeFactory = factory;
        }

        // Helpers 

        private static ConstructorInfo GetAttributeAdapterConstructor(Type attributeType, Type adapterType)
        {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(IEnumerable<ModelValidatorProvider>), attributeType });
            if (constructor == null)
            {
                throw Error.Argument("adapterType", SRResources.DataAnnotationsModelValidatorProvider_ConstructorRequirements, adapterType.Name, typeof(ModelMetadata).Name, "IEnumerable<" + typeof(ModelValidatorProvider).Name + ">", attributeType.Name);
            }

            return constructor;
        }

        private static void ValidateAttributeAdapterType(Type adapterType)
        {
            if (adapterType == null)
            {
                throw Error.ArgumentNull("adapterType");
            }

            if (!typeof(ModelValidator).IsAssignableFrom(adapterType))
            {
                throw Error.Argument("adapterType", SRResources.Common_TypeMustDriveFromType, adapterType.Name, typeof(ModelValidator).Name);
            }
        }

        private static void ValidateAttributeType(Type attributeType)
        {
            if (attributeType == null)
            {
                throw Error.ArgumentNull("attributeType");
            }

            if (!typeof(ValidationAttribute).IsAssignableFrom(attributeType))
            {
                throw Error.Argument("attributeType", SRResources.Common_TypeMustDriveFromType, attributeType.Name, typeof(ValidationAttribute).Name);
            }
        }

        private static void ValidateAttributeFactory(DataAnnotationsModelValidationFactory factory)
        {
            if (factory == null)
            {
                throw Error.ArgumentNull("factory");
            }
        }

        #endregion

        #region IValidatableObject adapter registration

        /// <summary>
        /// Registers an adapter type for the given <paramref name="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="HttpActionContext"/>.
        /// </summary>
        public void RegisterValidatableObjectAdapter(Type modelType, Type adapterType)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

                ValidatableFactories[modelType] = context => (ModelValidator)constructor.Invoke(new object[] { context });
            }

        /// <summary>
        /// Registers an adapter factory for the given <paramref name="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>.
        /// </summary>
        public void RegisterValidatableObjectAdapterFactory(Type modelType, DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableFactory(factory);

                ValidatableFactories[modelType] = factory;
            }

        /// <summary>
        /// Registers the default adapter type for objects which implement
        /// <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="HttpActionContext"/>.
        /// </summary>
        public void RegisterDefaultValidatableObjectAdapter(Type adapterType)
        {
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            DefaultValidatableFactory = context => (ModelValidator)constructor.Invoke(new object[] { context });
        }

        /// <summary>
        /// Registers the default adapter factory for objects which implement
        /// <see cref="IValidatableObject"/>.
        /// </summary>
        public void RegisterDefaultValidatableObjectAdapterFactory(DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableFactory(factory);

            DefaultValidatableFactory = factory;
        }

        // Helpers 

        private static ConstructorInfo GetValidatableAdapterConstructor(Type adapterType)
        {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(IEnumerable<ModelValidatorProvider>) });
            if (constructor == null)
            {
                throw Error.Argument("adapterType", SRResources.DataAnnotationsModelValidatorProvider_ValidatableConstructorRequirements, adapterType.Name, typeof(ModelMetadata).Name, "IEnumerable<" + typeof(ModelValidatorProvider).Name + ">");
            }

            return constructor;
        }

        private static void ValidateValidatableAdapterType(Type adapterType)
        {
            if (adapterType == null)
            {
                throw Error.ArgumentNull("adapterType");
            }
            if (!typeof(ModelValidator).IsAssignableFrom(adapterType))
            {
                throw Error.Argument("adapterType", SRResources.Common_TypeMustDriveFromType, adapterType.Name, typeof(ModelValidator).Name);
            }
        }

        private static void ValidateValidatableModelType(Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (!typeof(IValidatableObject).IsAssignableFrom(modelType))
            {
                throw Error.Argument("modelType", SRResources.Common_TypeMustDriveFromType, modelType.Name, typeof(IValidatableObject).Name);
            }
        }

        private static void ValidateValidatableFactory(DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            if (factory == null)
            {
                throw Error.ArgumentNull("factory");
            }
        }

        #endregion
    }
}

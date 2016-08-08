// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Validation.Validators;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Validation.Providers
{
    public class DataAnnotationsModelValidatorProviderTest
    {
        private static DataAnnotationsModelMetadataProvider _metadataProvider = new DataAnnotationsModelMetadataProvider();
        private static IEnumerable<ModelValidatorProvider> _noValidatorProviders = Enumerable.Empty<ModelValidatorProvider>();

        // Validation attribute adapter registration

        private class MyValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                throw new NotImplementedException();
            }
        }

        private class MyValidationAttributeAdapter : DataAnnotationsModelValidator
        {
            public MyValidationAttributeAdapter(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute)
                : base(validatorProviders, attribute)
            {
            }
        }

        private class MyValidationAttributeAdapterBadCtor : ModelValidator
        {
            public MyValidationAttributeAdapterBadCtor(IEnumerable<ModelValidatorProvider> validatorProviders)
                : base(validatorProviders)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
            {
                throw new NotImplementedException();
            }
        }

        private class MyDefaultValidationAttributeAdapter : DataAnnotationsModelValidator
        {
            public MyDefaultValidationAttributeAdapter(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute)
                : base(validatorProviders, attribute)
            {
            }
        }

        [MyValidation]
        private class MyValidatedClass
        {
        }

        [Fact]
        public void RegisterAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            provider.AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();

            // Act
            provider.RegisterAdapter(typeof(MyValidationAttribute), typeof(MyValidationAttributeAdapter));

            // Assert
            var type = provider.AttributeFactories.Keys.Single();
            Assert.Equal(typeof(MyValidationAttribute), type);

            var factory = provider.AttributeFactories.Values.Single();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(object));
            var attribute = new MyValidationAttribute();
            var validator = factory(_noValidatorProviders, attribute);
            Assert.IsType<MyValidationAttributeAdapter>(validator);
        }

        [Fact]
        public void RegisterAdapterGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterAdapter(null, typeof(MyValidationAttributeAdapter)),
                "attributeType");

            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterAdapter(typeof(MyValidationAttribute), null),
                "adapterType");

            // Validation attribute must derive from ValidationAttribute
            Assert.ThrowsArgument(
                () => provider.RegisterAdapter(typeof(object), typeof(MyValidationAttributeAdapter)),
                "attributeType",
                "The type Object must derive from ValidationAttribute");

            // Adapter must derive from ModelValidator
            Assert.ThrowsArgument(
                () => provider.RegisterAdapter(typeof(MyValidationAttribute), typeof(object)),
                "adapterType",
                "The type Object must derive from ModelValidator");

            // Adapter must have the expected constructor
            Assert.ThrowsArgument(
                () => provider.RegisterAdapter(typeof(MyValidationAttribute), typeof(MyValidationAttributeAdapterBadCtor)),
                "adapterType",
                "The type MyValidationAttributeAdapterBadCtor must have a public constructor which accepts three parameters of types ModelMetadata, IEnumerable<ModelValidatorProvider>, and MyValidationAttribute");
        }

        [Fact]
        public void RegisterAdapterFactory()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            provider.AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();
            DataAnnotationsModelValidationFactory factory = delegate { return null; };

            // Act
            provider.RegisterAdapterFactory(typeof(MyValidationAttribute), factory);

            // Assert
            var type = provider.AttributeFactories.Keys.Single();
            Assert.Equal(typeof(MyValidationAttribute), type);
            Assert.Same(factory, provider.AttributeFactories.Values.Single());
        }

        [Fact]
        public void RegisterAdapterFactoryGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();
            DataAnnotationsModelValidationFactory factory = (validatorProviders, attribute) => null;

            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterAdapterFactory(null, factory),
                "attributeType");

            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterAdapterFactory(typeof(MyValidationAttribute), null),
                "factory");

            // Validation attribute must derive from ValidationAttribute
            Assert.ThrowsArgument(
                () => provider.RegisterAdapterFactory(typeof(object), factory),
                "attributeType",
                "The type Object must derive from ValidationAttribute");
        }

        [Fact]
        public void RegisterDefaultAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(MyValidatedClass));
            provider.RegisterDefaultAdapter(typeof(MyDefaultValidationAttributeAdapter));

            // Act
            var result = provider.GetValidators(metadata, _noValidatorProviders).Single();

            // Assert
            Assert.IsType<MyDefaultValidationAttributeAdapter>(result);
        }

        [Fact]
        public void RegisterDefaultAdapterGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterDefaultAdapter(null),
                "adapterType");

            // Adapter must derive from ModelValidator
            Assert.ThrowsArgument(
                () => provider.RegisterDefaultAdapter(typeof(object)),
                "adapterType",
                "The type Object must derive from ModelValidator");

            // Adapter must have the expected constructor
            Assert.ThrowsArgument(
                () => provider.RegisterDefaultAdapter(typeof(MyValidationAttributeAdapterBadCtor)),
                "adapterType",
                "The type MyValidationAttributeAdapterBadCtor must have a public constructor which accepts three parameters of types ModelMetadata, IEnumerable<ModelValidatorProvider>, and ValidationAttribute");
        }

        [Fact]
        public void RegisterDefaultAdapterFactory()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(MyValidatedClass));
            ModelValidator validator = new Mock<ModelValidator>(_noValidatorProviders).Object;
            DataAnnotationsModelValidationFactory factory = delegate { return validator; };
            provider.RegisterDefaultAdapterFactory(factory);

            // Act
            var result = provider.GetValidators(metadata, _noValidatorProviders).Single();

            // Assert
            Assert.Same(validator, result);
        }

        [Fact]
        public void RegisterDefaultAdapterFactoryGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterDefaultAdapterFactory(null),
                "factory");
        }

        // IValidatableObject adapter registration

        private class MyValidatableAdapter : ModelValidator
        {
            public MyValidatableAdapter(IEnumerable<ModelValidatorProvider> validatorProviders)
                : base(validatorProviders)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
            {
                throw new NotImplementedException();
            }
        }

        private class MyValidatableAdapterBadCtor : ModelValidator
        {
            public MyValidatableAdapterBadCtor(IEnumerable<ModelValidatorProvider> validatorProviders, int unused)
                : base(validatorProviders)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
            {
                throw new NotImplementedException();
            }
        }

        private class MyValidatableClass : IValidatableObject
        {
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void RegisterValidatableObjectAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            provider.ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();
            IValidatableObject validatable = new Mock<IValidatableObject>().Object;

            // Act
            provider.RegisterValidatableObjectAdapter(validatable.GetType(), typeof(MyValidatableAdapter));

            // Assert
            var type = provider.ValidatableFactories.Keys.Single();
            Assert.Equal(validatable.GetType(), type);

            var factory = provider.ValidatableFactories.Values.Single();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(object));
            var validator = factory(_noValidatorProviders);
            Assert.IsType<MyValidatableAdapter>(validator);
        }

        [Fact]
        public void RegisterValidatableObjectAdapterGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterValidatableObjectAdapter(null, typeof(MyValidatableAdapter)),
                "modelType");

            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), null),
                "adapterType");

            // Validation attribute must derive from ValidationAttribute
            Assert.ThrowsArgument(
                () => provider.RegisterValidatableObjectAdapter(typeof(object), typeof(MyValidatableAdapter)),
                "modelType",
                "The type Object must derive from IValidatableObject.");

            // Adapter must derive from ModelValidator
            Assert.ThrowsArgument(
                () => provider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), typeof(object)),
                "adapterType",
                "The type Object must derive from ModelValidator");

            // Adapter must have the expected constructor
            Assert.ThrowsArgument(
                () => provider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), typeof(MyValidatableAdapterBadCtor)),
                "adapterType",
                "The type MyValidatableAdapterBadCtor must have a public constructor which accepts two parameters of types ModelMetadata and IEnumerable<ModelValidatorProvider>");
        }

        [Fact]
        public void RegisterValidatableObjectAdapterFactory()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            provider.ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();
            DataAnnotationsValidatableObjectAdapterFactory factory = delegate { return null; };

            // Act
            provider.RegisterValidatableObjectAdapterFactory(typeof(MyValidatableClass), factory);

            // Assert
            var type = provider.ValidatableFactories.Keys.Single();
            Assert.Equal(typeof(MyValidatableClass), type);
            Assert.Same(factory, provider.ValidatableFactories.Values.Single());
        }

        [Fact]
        public void RegisterValidatableObjectAdapterFactoryGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();
            DataAnnotationsValidatableObjectAdapterFactory factory = (context) => null;

            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterValidatableObjectAdapterFactory(null, factory),
                "modelType");

            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterValidatableObjectAdapterFactory(typeof(MyValidatableClass), null),
                "factory");

            // Validation attribute must derive from ValidationAttribute
            Assert.ThrowsArgument(
                () => provider.RegisterValidatableObjectAdapterFactory(typeof(object), factory),
                "modelType",
                "The type Object must derive from IValidatableObject");
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(MyValidatableClass));
            provider.RegisterDefaultValidatableObjectAdapter(typeof(MyValidatableAdapter));

            // Act
            var result = provider.GetValidators(metadata, _noValidatorProviders).Single();

            // Assert
            Assert.IsType<MyValidatableAdapter>(result);
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapterGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterDefaultValidatableObjectAdapter(null),
                "adapterType");

            // Adapter must derive from ModelValidator
            Assert.ThrowsArgument(
                () => provider.RegisterDefaultValidatableObjectAdapter(typeof(object)),
                "adapterType",
                "The type Object must derive from ModelValidator");

            // Adapter must have the expected constructor
            Assert.ThrowsArgument(
                () => provider.RegisterDefaultValidatableObjectAdapter(typeof(MyValidatableAdapterBadCtor)),
                "adapterType",
                "The type MyValidatableAdapterBadCtor must have a public constructor which accepts two parameters of types ModelMetadata and IEnumerable<ModelValidatorProvider>");
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapterFactory()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(MyValidatableClass));
            ModelValidator validator = new Mock<ModelValidator>(_noValidatorProviders).Object;
            DataAnnotationsValidatableObjectAdapterFactory factory = delegate { return validator; };
            provider.RegisterDefaultValidatableObjectAdapterFactory(factory);

            // Act
            var result = provider.GetValidators(metadata, _noValidatorProviders).Single();

            // Assert
            Assert.Same(validator, result);
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapterFactoryGuardClauses()
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => provider.RegisterDefaultValidatableObjectAdapterFactory(null),
                "factory");
        }

        // Default adapter factory for unknown attribute type

        [Fact]
        public void UnknownValidationAttributeGetsDefaultAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForType(() => null, typeof(DummyClassWithDummyValidationAttribute));

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, _noValidatorProviders);

            // Assert
            var validator = validators.Single();
            Assert.IsType<DataAnnotationsModelValidator>(validator);
        }

        private class DummyValidationAttribute : ValidationAttribute
        {
        }

        [DummyValidation]
        private class DummyClassWithDummyValidationAttribute
        {
        }

        // Default IValidatableObject adapter factory

        [Fact]
        public void IValidatableObjectGetsAValidator()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var mockValidatable = new Mock<IValidatableObject>();
            var metadata = _metadataProvider.GetMetadataForType(() => null, mockValidatable.Object.GetType());

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, _noValidatorProviders);

            // Assert
            Assert.Single(validators);
        }

        // Integration with metadata system

        [Fact]
        public void DoesNotReadPropertyValue()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var model = new ObservableModel();
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(() => model.TheProperty, typeof(ObservableModel), "TheProperty");

            // Act
            ModelValidator[] validators = provider.GetValidators(metadata, _noValidatorProviders).ToArray();
            ModelValidationResult[] results = validators.SelectMany(o => o.Validate(metadata, model)).ToArray();

            // Assert
            Assert.Empty(validators);
            Assert.False(model.PropertyWasRead());
        }

        private class ObservableModel
        {
            private bool _propertyWasRead;

            public string TheProperty
            {
                get
                {
                    _propertyWasRead = true;
                    return "Hello";
                }
            }

            public bool PropertyWasRead()
            {
                return _propertyWasRead;
            }
        }

        private class BaseModel
        {
            public virtual string MyProperty { get; set; }
        }

        private class DerivedModel : BaseModel
        {
            [StringLength(10)]
            public override string MyProperty
            {
                get { return base.MyProperty; }
                set { base.MyProperty = value; }
            }
        }
    }
}

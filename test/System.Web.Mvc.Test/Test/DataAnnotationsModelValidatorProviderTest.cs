// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;
using Microsoft.TestCommon;
using Moq;
using DataAnnotationsCompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class DataAnnotationsModelValidatorProviderTest
    {
        // Validation attribute adapter registration

        private class MyValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                throw new NotImplementedException();
            }
        }

        private class MyValidationAttributeAdapter : DataAnnotationsModelValidator<MyValidationAttribute>
        {
            public MyValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, MyValidationAttribute attribute)
                : base(metadata, context, attribute)
            {
            }
        }

        private class MyValidationAttributeAdapterBadCtor : ModelValidator
        {
            public MyValidationAttributeAdapterBadCtor(ModelMetadata metadata, ControllerContext context)
                : base(metadata, context)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
            {
                throw new NotImplementedException();
            }
        }

        private class MyDefaultValidationAttributeAdapter : DataAnnotationsModelValidator
        {
            public MyDefaultValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
                : base(metadata, context, attribute)
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
            var oldFactories = DataAnnotationsModelValidatorProvider.AttributeFactories;

            try
            {
                // Arrange
                DataAnnotationsModelValidatorProvider.AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();

                // Act
                DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), typeof(MyValidationAttributeAdapter));

                // Assert
                var type = DataAnnotationsModelValidatorProvider.AttributeFactories.Keys.Single();
                Assert.Equal(typeof(MyValidationAttribute), type);

                var factory = DataAnnotationsModelValidatorProvider.AttributeFactories.Values.Single();
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
                var context = new ControllerContext();
                var attribute = new MyValidationAttribute();
                var validator = factory(metadata, context, attribute);
                Assert.IsType<MyValidationAttributeAdapter>(validator);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.AttributeFactories = oldFactories;
            }
        }

        [Fact]
        public void RegisterAdapterGuardClauses()
        {
            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(null, typeof(MyValidationAttributeAdapter)),
                "attributeType");

            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), null),
                "adapterType");

            // Validation attribute must derive from ValidationAttribute
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(object), typeof(MyValidationAttributeAdapter)),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.ValidationAttribute\r\nParameter name: attributeType");

            // Adapter must derive from ModelValidator
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(MyValidationAttribute), typeof(MyValidationAttributeAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidationAttributeAdapterBadCtor must have a public constructor which accepts three parameters of types System.Web.Mvc.ModelMetadata, System.Web.Mvc.ControllerContext, and System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidationAttribute\r\nParameter name: adapterType");
        }

        [Fact]
        public void RegisterAdapterFactory()
        {
            var oldFactories = DataAnnotationsModelValidatorProvider.AttributeFactories;

            try
            {
                // Arrange
                DataAnnotationsModelValidatorProvider.AttributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();
                DataAnnotationsModelValidationFactory factory = delegate { return null; };

                // Act
                DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(MyValidationAttribute), factory);

                // Assert
                var type = DataAnnotationsModelValidatorProvider.AttributeFactories.Keys.Single();
                Assert.Equal(typeof(MyValidationAttribute), type);
                Assert.Same(factory, DataAnnotationsModelValidatorProvider.AttributeFactories.Values.Single());
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.AttributeFactories = oldFactories;
            }
        }

        [Fact]
        public void RegisterAdapterFactoryGuardClauses()
        {
            DataAnnotationsModelValidationFactory factory = (metadata, context, attribute) => null;

            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(null, factory),
                "attributeType");

            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(MyValidationAttribute), null),
                "factory");

            // Validation attribute must derive from ValidationAttribute
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(object), factory),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.ValidationAttribute\r\nParameter name: attributeType");
        }

        [Fact]
        public void RegisterDefaultAdapter()
        {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultAttributeFactory;

            try
            {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatedClass));
                var context = new ControllerContext();
                DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(MyDefaultValidationAttributeAdapter));

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.IsType<MyDefaultValidationAttributeAdapter>(result);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.DefaultAttributeFactory = oldFactory;
            }
        }

        [Fact]
        public void RegisterDefaultAdapterGuardClauses()
        {
            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(null),
                "adapterType");

            // Adapter must derive from ModelValidator
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(MyValidationAttributeAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidationAttributeAdapterBadCtor must have a public constructor which accepts three parameters of types System.Web.Mvc.ModelMetadata, System.Web.Mvc.ControllerContext, and System.ComponentModel.DataAnnotations.ValidationAttribute\r\nParameter name: adapterType");
        }

        [Fact]
        public void RegisterDefaultAdapterFactory()
        {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultAttributeFactory;

            try
            {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatedClass));
                var context = new ControllerContext();
                ModelValidator validator = new Mock<ModelValidator>(metadata, context).Object;
                DataAnnotationsModelValidationFactory factory = delegate { return validator; };
                DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory(factory);

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.Same(validator, result);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.DefaultAttributeFactory = oldFactory;
            }
        }

        [Fact]
        public void RegisterDefaultAdapterFactoryGuardClauses()
        {
            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory(null),
                "factory");
        }

        // IValidatableObject adapter registration

        private class MyValidatableAdapter : ModelValidator
        {
            public MyValidatableAdapter(ModelMetadata metadata, ControllerContext context)
                : base(metadata, context)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
            {
                throw new NotImplementedException();
            }
        }

        private class MyValidatableAdapterBadCtor : ModelValidator
        {
            public MyValidatableAdapterBadCtor(ModelMetadata metadata, ControllerContext context, int unused)
                : base(metadata, context)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
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
            var oldFactories = DataAnnotationsModelValidatorProvider.ValidatableFactories;

            try
            {
                // Arrange
                DataAnnotationsModelValidatorProvider.ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();
                IValidatableObject validatable = new Mock<IValidatableObject>().Object;

                // Act
                DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(validatable.GetType(), typeof(MyValidatableAdapter));

                // Assert
                var type = DataAnnotationsModelValidatorProvider.ValidatableFactories.Keys.Single();
                Assert.Equal(validatable.GetType(), type);

                var factory = DataAnnotationsModelValidatorProvider.ValidatableFactories.Values.Single();
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
                var context = new ControllerContext();
                var validator = factory(metadata, context);
                Assert.IsType<MyValidatableAdapter>(validator);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.ValidatableFactories = oldFactories;
            }
        }

        [Fact]
        public void RegisterValidatableObjectAdapterGuardClauses()
        {
            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(null, typeof(MyValidatableAdapter)),
                "modelType");

            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), null),
                "adapterType");

            // Validation attribute must derive from ValidationAttribute
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(object), typeof(MyValidatableAdapter)),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.IValidatableObject\r\nParameter name: modelType");

            // Adapter must derive from ModelValidator
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapter(typeof(MyValidatableClass), typeof(MyValidatableAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidatableAdapterBadCtor must have a public constructor which accepts two parameters of types System.Web.Mvc.ModelMetadata and System.Web.Mvc.ControllerContext.\r\nParameter name: adapterType");
        }

        [Fact]
        public void RegisterValidatableObjectAdapterFactory()
        {
            var oldFactories = DataAnnotationsModelValidatorProvider.ValidatableFactories;

            try
            {
                // Arrange
                DataAnnotationsModelValidatorProvider.ValidatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();
                DataAnnotationsValidatableObjectAdapterFactory factory = delegate { return null; };

                // Act
                DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(typeof(MyValidatableClass), factory);

                // Assert
                var type = DataAnnotationsModelValidatorProvider.ValidatableFactories.Keys.Single();
                Assert.Equal(typeof(MyValidatableClass), type);
                Assert.Same(factory, DataAnnotationsModelValidatorProvider.ValidatableFactories.Values.Single());
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.ValidatableFactories = oldFactories;
            }
        }

        [Fact]
        public void RegisterValidatableObjectAdapterFactoryGuardClauses()
        {
            DataAnnotationsValidatableObjectAdapterFactory factory = (metadata, context) => null;

            // Attribute type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(null, factory),
                "modelType");

            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(typeof(MyValidatableClass), null),
                "factory");

            // Validation attribute must derive from ValidationAttribute
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterValidatableObjectAdapterFactory(typeof(object), factory),
                "The type System.Object must derive from System.ComponentModel.DataAnnotations.IValidatableObject\r\nParameter name: modelType");
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapter()
        {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultValidatableFactory;

            try
            {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatableClass));
                var context = new ControllerContext();
                DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(typeof(MyValidatableAdapter));

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.IsType<MyValidatableAdapter>(result);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.DefaultValidatableFactory = oldFactory;
            }
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapterGuardClauses()
        {
            // Adapter type cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(null),
                "adapterType");

            // Adapter must derive from ModelValidator
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(typeof(object)),
                "The type System.Object must derive from System.Web.Mvc.ModelValidator\r\nParameter name: adapterType");

            // Adapter must have the expected constructor
            Assert.Throws<ArgumentException>(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapter(typeof(MyValidatableAdapterBadCtor)),
                "The type System.Web.Mvc.Test.DataAnnotationsModelValidatorProviderTest+MyValidatableAdapterBadCtor must have a public constructor which accepts two parameters of types System.Web.Mvc.ModelMetadata and System.Web.Mvc.ControllerContext.\r\nParameter name: adapterType");
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapterFactory()
        {
            var oldFactory = DataAnnotationsModelValidatorProvider.DefaultValidatableFactory;

            try
            {
                // Arrange
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(MyValidatableClass));
                var context = new ControllerContext();
                ModelValidator validator = new Mock<ModelValidator>(metadata, context).Object;
                DataAnnotationsValidatableObjectAdapterFactory factory = delegate { return validator; };
                DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory(factory);

                // Act
                var result = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, context).Single();

                // Assert
                Assert.Same(validator, result);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.DefaultValidatableFactory = oldFactory;
            }
        }

        [Fact]
        public void RegisterDefaultValidatableObjectAdapterFactoryGuardClauses()
        {
            // Factory cannot be null
            Assert.ThrowsArgumentNull(
                () => DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory(null),
                "factory");
        }

        // Pre-configured adapters

        public static TheoryDataSet<Type, ValidationAttribute, Type, string> KnownAdapterTypeData
        {
            get
            {
                return new TheoryDataSet<Type, ValidationAttribute, Type, string>
                {
                    { typeof(RangeAttribute), new RangeAttribute(1, 100), typeof(RangeAttributeAdapter), null },
                    { typeof(RegularExpressionAttribute), new RegularExpressionAttribute("abc"), typeof(RegularExpressionAttributeAdapter), null },
                    { typeof(RequiredAttribute), new RequiredAttribute(), typeof(RequiredAttributeAdapter), null },
                    { typeof(StringLengthAttribute), new StringLengthAttribute(6), typeof(StringLengthAttributeAdapter), null },
                    { typeof(MaxLengthAttribute), new MaxLengthAttribute(), typeof(MaxLengthAttributeAdapter), null },
                    { typeof(MinLengthAttribute), new MinLengthAttribute(1), typeof(MinLengthAttributeAdapter), null },
                    { typeof(MembershipPasswordAttribute), new MembershipPasswordAttribute(), typeof(MembershipPasswordAttributeAdapter), null },
                    { typeof(DataAnnotationsCompareAttribute), new DataAnnotationsCompareAttribute("other"), typeof(CompareAttributeAdapter), null },
                    { typeof(FileExtensionsAttribute), new FileExtensionsAttribute(), typeof(FileExtensionsAttributeAdapter), null },
                    { typeof(CreditCardAttribute), new CreditCardAttribute(), typeof(DataTypeAttributeAdapter), "creditcard" },
                    { typeof(EmailAddressAttribute), new EmailAddressAttribute(), typeof(DataTypeAttributeAdapter), "email" },
                    { typeof(PhoneAttribute), new PhoneAttribute(), typeof(DataTypeAttributeAdapter), "phone" },
                    { typeof(UrlAttribute), new UrlAttribute(), typeof(DataTypeAttributeAdapter), "url" },
                };
            }
        }

        [Theory]
        [PropertyData("KnownAdapterTypeData")]
        public void AdapterForKnownTypeRegistered(Type attributeType, ValidationAttribute validationAttr,
            Type expectedAdapterType, string expectedRuleName)
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(object));
            var context = new ControllerContext();
            var adapters = DataAnnotationsModelValidatorProvider.AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == attributeType).Value;

            // Act
            var adapter = adapterFactory(metadata, context, validationAttr);

            // Assert
            Assert.IsType(expectedAdapterType, adapter);
            if (expectedRuleName != null)
            {
                DataTypeAttributeAdapter dataTypeAdapter = adapter as DataTypeAttributeAdapter;
                Assert.NotNull(dataTypeAdapter);
                Assert.Equal(expectedRuleName, dataTypeAdapter.RuleName);
            }
        }

        // Default adapter factory for unknown attribute type

        [Fact]
        public void UnknownValidationAttributeGetsDefaultAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(DummyClassWithDummyValidationAttribute));

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

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
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, mockValidatable.Object.GetType());

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            Assert.Single(validators);
        }

        // Implicit [Required] attribute

        [Fact]
        public void ReferenceTypesDontGetImplicitRequiredAttribute()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            Assert.Empty(validators);
        }

        [Fact]
        public void NonNullableValueTypesGetImplicitRequiredAttribute()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(DummyRequiredAttributeHelperClass), "WithoutAttribute");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelValidator validator = validators.Single();
            ModelClientValidationRule rule = validator.GetClientValidationRules().Single();
            Assert.IsType<ModelClientValidationRequiredRule>(rule);
        }

        [Fact]
        public void NonNullableValueTypesWithExplicitRequiredAttributeDoesntGetImplictRequiredAttribute()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(DummyRequiredAttributeHelperClass), "WithAttribute");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelValidator validator = validators.Single();
            ModelClientValidationRule rule = validator.GetClientValidationRules().Single();
            Assert.IsType<ModelClientValidationRequiredRule>(rule);
            Assert.Equal("Custom Required Message", rule.ErrorMessage);
        }

        [Fact]
        public void NonNullableValueTypeDoesntGetImplicitRequiredAttributeWhenFlagIsOff()
        {
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            try
            {
                // Arrange
                var provider = new DataAnnotationsModelValidatorProvider();
                var context = new ControllerContext();
                var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(DummyRequiredAttributeHelperClass), "WithoutAttribute");

                // Act
                IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

                // Assert
                Assert.Empty(validators);
            }
            finally
            {
                DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = true;
            }
        }

        private class DummyRequiredAttributeHelperClass
        {
            [Required(ErrorMessage = "Custom Required Message")]
            public int WithAttribute { get; set; }

            public int WithoutAttribute { get; set; }
        }

        // Integration with metadata system

        [Fact]
        public void DoesNotReadPropertyValue()
        {
            // Arrange
            ObservableModel model = new ObservableModel();
            ModelMetadata metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForProperty(() => model.TheProperty, typeof(ObservableModel), "TheProperty");
            ControllerContext controllerContext = new ControllerContext();

            // Act
            ModelValidator[] validators = new DataAnnotationsModelValidatorProvider().GetValidators(metadata, controllerContext).ToArray();
            ModelValidationResult[] results = validators.SelectMany(o => o.Validate(model)).ToArray();

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

            [MinLength(2)]
            [MaxLength(4)]
            public List<DateTime> MinMaxProperty { get; set; }
        }

        [Fact]
        public void GetValidatorsReturnsClientValidatorForDerivedTypeAppliedAgainstBaseTypeViaFromLambdaExpression()
        { // Dev10 Bug #868619
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var viewdata = new ViewDataDictionary<DerivedModel>();
            var metadata = ModelMetadata.FromLambdaExpression(m => m.MyProperty, viewdata); // Bug is in FromLambdaExpression

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelValidator validator = validators.Single();
            ModelClientValidationRule clientRule = validator.GetClientValidationRules().Single();
            Assert.IsType<ModelClientValidationStringLengthRule>(clientRule);
            Assert.Equal(10, clientRule.ValidationParameters["max"]);
        }

        [Fact]
        public void GetValidatorsReturnsValidationRulesForPropertiesWithMinAndMaxLength()
        { // Dev10 Bug #868619
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var context = new ControllerContext();
            var viewdata = new ViewDataDictionary<DerivedModel>();
            var metadata = ModelMetadata.FromLambdaExpression(m => m.MinMaxProperty, viewdata);

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, context);

            // Assert
            ModelClientValidationRule[] clientRule = validators.SelectMany(v => v.GetClientValidationRules())
                                                               .OrderBy(t => t.GetType().Name)
                                                               .ToArray();
            Assert.IsType<ModelClientValidationMaxLengthRule>(clientRule[0]);
            Assert.IsType<ModelClientValidationMinLengthRule>(clientRule[1]);
        }
    }
}

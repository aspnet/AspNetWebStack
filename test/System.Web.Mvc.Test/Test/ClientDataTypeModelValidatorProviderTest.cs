// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ClientDataTypeModelValidatorProviderTest
    {
        private static readonly ModelMetadataProvider _metadataProvider = new DataAnnotationsModelMetadataProvider();
        private static readonly ModelValidatorProvider _validatorProvider = new ClientDataTypeModelValidatorProvider();

        private bool ReturnsValidator<TValidator>(string propertyName)
        {
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(null, typeof(SampleModel), propertyName);
            IEnumerable<ModelValidator> validators = _validatorProvider.GetValidators(metadata, new ControllerContext());
            return validators.Any(v => v is TValidator);
        }

        [Theory]
        [InlineData("Byte"), InlineData("SByte"), InlineData("Int16"), InlineData("UInt16")]
        [InlineData("Int32"), InlineData("UInt32"), InlineData("Int64"), InlineData("UInt64")]
        [InlineData("Single"), InlineData("Double"), InlineData("Decimal"), InlineData("NullableInt32")]
        public void GetValidators_NumericValidatorTypes(string propertyName)
        {
            // Act & assert
            Assert.True(ReturnsValidator<ClientDataTypeModelValidatorProvider.NumericModelValidator>(propertyName));
        }

        [Theory]
        [InlineData("String"), InlineData("Object"), InlineData("DateTime"), InlineData("NullableDateTime")]
        public void GetValidators_NonNumericValidatorTypes(string propertyName)
        {
            // Act & assert
            Assert.False(ReturnsValidator<ClientDataTypeModelValidatorProvider.NumericModelValidator>(propertyName));
        }

        [Theory]
        [InlineData("DateTime"), InlineData("NullableDateTime")]
        public void GetValidators_DateTimeValidatorTypes(string propertyName)
        {
            // Act & assert
            Assert.True(ReturnsValidator<ClientDataTypeModelValidatorProvider.DateModelValidator>(propertyName));
        }

        [Theory]
        [InlineData("Int32"), InlineData("NullableInt32"), InlineData("String"), InlineData("Object"), InlineData("Time")]
        public void GetValidators_NonDateTimeValidatorTypes(string propertyName)
        {
            // Act & assert
            Assert.False(ReturnsValidator<ClientDataTypeModelValidatorProvider.DateModelValidator>(propertyName));
        }

        [Fact]
        public void GuardClauses()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForType(null, typeof(SampleModel));

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new ClientDataTypeModelValidatorProvider.ClientModelValidator(metadata, new ControllerContext(), "testValidationType", errorMessage: null),
                "errorMessage");

            Assert.ThrowsArgumentNullOrEmpty(
                () => new ClientDataTypeModelValidatorProvider.ClientModelValidator(metadata, new ControllerContext(), "testValidationType", errorMessage: String.Empty),
                "errorMessage");

            Assert.ThrowsArgumentNullOrEmpty(
                () => new ClientDataTypeModelValidatorProvider.ClientModelValidator(metadata, new ControllerContext(), validationType: null, errorMessage: "testErrorMessage"),
                "validationType");

            Assert.ThrowsArgumentNullOrEmpty(
                () => new ClientDataTypeModelValidatorProvider.ClientModelValidator(metadata, new ControllerContext(), validationType: String.Empty, errorMessage: "testErrorMessage"),
                "validationType");
        }

        [Fact]
        public void GetValidators_ThrowsIfContextIsNull()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForType(null, typeof(SampleModel));

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => _validatorProvider.GetValidators(metadata, null),
                "context");
        }

        [Fact]
        public void GetValidators_ThrowsIfMetadataIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => _validatorProvider.GetValidators(null, new ControllerContext()),
                "metadata");
        }

        [Fact]
        public void NumericValidator_GetClientValidationRules()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(null, typeof(SampleModel), "Int32");
            var validator = new ClientDataTypeModelValidatorProvider.NumericModelValidator(metadata, new ControllerContext());

            // Act
            ModelClientValidationRule[] rules = validator.GetClientValidationRules().ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("number", rule.ValidationType);
            Assert.Empty(rule.ValidationParameters);
            Assert.Equal("The field Int32 must be a number.", rule.ErrorMessage);
        }

        [Fact]
        public void DateValidator_GetClientValidationRules()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(null, typeof(SampleModel), "DateTime");
            var validator = new ClientDataTypeModelValidatorProvider.DateModelValidator(metadata, new ControllerContext());

            // Act
            ModelClientValidationRule[] rules = validator.GetClientValidationRules().ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("date", rule.ValidationType);
            Assert.Empty(rule.ValidationParameters);
            Assert.Equal("The field DateTime must be a date.", rule.ErrorMessage);
        }

        [Fact]
        public void ClientModelValidator_Validate_DoesNotReadPropertyValue()
        {
            // Arrange
            ObservableModel model = new ObservableModel();
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(() => model.TheProperty, typeof(ObservableModel), "TheProperty");
            ControllerContext controllerContext = new ControllerContext();

            // Act
            ModelValidator[] validators = new ClientDataTypeModelValidatorProvider().GetValidators(metadata, controllerContext).ToArray();
            ModelValidationResult[] results = validators.SelectMany(o => o.Validate(model)).ToArray();

            // Assert
            Assert.Equal(new Type[] { typeof(ClientDataTypeModelValidatorProvider.NumericModelValidator) }, Array.ConvertAll(validators, o => o.GetType()));
            Assert.Empty(results);
            Assert.False(model.PropertyWasRead());
        }

        [Fact]
        public void ClientModelValidator_Validate_ReturnsEmptyCollection()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForType(null, typeof(object));
            var validator = new ClientDataTypeModelValidatorProvider.ClientModelValidator(metadata, new ControllerContext(), "testValidationType", "testErrorMessage");

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.Empty(result);
        }

        private class SampleModel
        {
            // these should have 'numeric' validators associated with them
            public byte Byte { get; set; }
            public sbyte SByte { get; set; }
            public short Int16 { get; set; }
            public ushort UInt16 { get; set; }
            public int Int32 { get; set; }
            public uint UInt32 { get; set; }
            public long Int64 { get; set; }
            public ulong UInt64 { get; set; }
            public float Single { get; set; }
            public double Double { get; set; }
            public decimal Decimal { get; set; }

            // this should also have a 'numeric' validator
            public int? NullableInt32 { get; set; }

            // this should have a 'date' validator associated with it
            public DateTime DateTime { get; set; }

            // this should also have a 'date' validator associated with it
            public DateTime? NullableDateTime { get; set; }

            [DataType(DataType.Time)]
            public DateTime Time { get; set; }

            // these shouldn't have any validators
            public string String { get; set; }
            public object Object { get; set; }
        }

        private class ObservableModel
        {
            private bool _propertyWasRead;

            public int TheProperty
            {
                get
                {
                    _propertyWasRead = true;
                    return 42;
                }
            }

            public bool PropertyWasRead()
            {
                return _propertyWasRead;
            }
        }
    }
}

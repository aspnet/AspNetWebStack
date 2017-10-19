// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class DataAnnotationsModelValidatorTest
    {
        [Fact]
        public void ConstructorGuards()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(object));
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new DataAnnotationsModelValidator(null, context, attribute),
                "metadata");
            Assert.ThrowsArgumentNull(
                () => new DataAnnotationsModelValidator(metadata, null, attribute),
                "controllerContext");
            Assert.ThrowsArgumentNull(
                () => new DataAnnotationsModelValidator(metadata, context, null),
                "attribute");
        }

        [Fact]
        public void ValuesSet()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute);

            // Assert
            Assert.Same(attribute, validator.Attribute);
            Assert.Equal(attribute.FormatErrorMessage("Length"), validator.ErrorMessage);
        }

        public static TheoryDataSet<ModelMetadata, string> ValidateSetsMemberNamePropertyDataSet
        {
            get
            {
                return new TheoryDataSet<ModelMetadata, string>
                {
                    {
                        ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length"),
                        "Length"
                    },
                    {
                        ModelMetadataProviders.Current.GetMetadataForType(() => new object(), typeof(SampleModel)),
                        "SampleModel"
                    }
                };
            }
        }

        [Theory]
        [PropertyData("ValidateSetsMemberNamePropertyDataSet")]
        public void ValidateSetsMemberNamePropertyOfValidationContextForProperties(ModelMetadata metadata, string expectedMemberName)
        {
            // Arrange
            metadata.DisplayName = "Some-random-name";
            ControllerContext context = new ControllerContext();
            var attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                     .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                     .Callback((object o, ValidationContext validationContext) =>
                     {
                         Assert.Equal(expectedMemberName, validationContext.MemberName);
                     })
                     .Returns(ValidationResult.Success)
                     .Verifiable();
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> results = validator.Validate(container: null);

            // Assert
            Assert.Empty(results);
            attribute.VerifyAll();
        }

        [Fact]
        public void NoClientRulesByDefault()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute);

            // Assert
            Assert.Empty(validator.GetClientValidationRules());
        }

        [Fact]
        public void ValidateWithIsValidTrue()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Setup(a => a.IsValid(metadata.Model)).Returns(true);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ValidateWithIsValidFalse()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Setup(a => a.IsValid(metadata.Model)).Returns(false);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            var validationResult = result.Single();
            Assert.Equal("", validationResult.MemberName);
            Assert.Equal(attribute.Object.FormatErrorMessage("Length"), validationResult.Message);
        }

        [Fact]
        public void ValidatateWithValidationResultSuccess()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                .Returns(ValidationResult.Success);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ValidateReturnsSingleValidationResultIfMemberNameSequenceIsEmpty()
        {
            // Arrange
            const string errorMessage = "Some error message";
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                     .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                     .Returns(new ValidationResult(errorMessage, memberNames: null));
            var validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> results = validator.Validate(container: null);

            // Assert
            ModelValidationResult validationResult = Assert.Single(results);
            Assert.Equal(errorMessage, validationResult.Message);
            Assert.Empty(validationResult.MemberName);
        }

        [Fact]
        public void ValidateReturnsSingleValidationResultIfOneMemberNameIsSpecified()
        {
            // Arrange
            const string errorMessage = "A different error message";
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(() => new object(), typeof(object));
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                     .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                     .Returns(new ValidationResult(errorMessage, new[] { "FirstName" }));
            var validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> results = validator.Validate(container: null);

            // Assert
            ModelValidationResult validationResult = Assert.Single(results);
            Assert.Equal(errorMessage, validationResult.Message);
            Assert.Equal("FirstName", validationResult.MemberName);
        }

        [Fact]
        public void ValidateReturnsMemberNameIfItIsDifferentFromDisplayName()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(() => new SampleModel(), typeof(SampleModel));
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                     .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                     .Returns(new ValidationResult("Name error", new[] { "Name" }));
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> results = validator.Validate(container: null);

            // Assert
            ModelValidationResult validationResult = Assert.Single(results);
            Assert.Equal("Name", validationResult.MemberName);
        }

        [Fact]
        public void IsRequiredTests()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();

            // Act & Assert
            Assert.False(new DataAnnotationsModelValidator(metadata, context, new RangeAttribute(10, 20)).IsRequired);
            Assert.True(new DataAnnotationsModelValidator(metadata, context, new RequiredAttribute()).IsRequired);
            Assert.True(new DataAnnotationsModelValidator(metadata, context, new DerivedRequiredAttribute()).IsRequired);
        }

        [Fact]
        public void AttributeWithIClientValidatableGetsClientValidationRules()
        {
            // Arrange
            var expected = new ModelClientValidationStringLengthRule("Error", 1, 10);
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));
            var attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.As<IClientValidatable>()
                .Setup(cv => cv.GetClientValidationRules(metadata, context))
                .Returns(new[] { expected })
                .Verifiable();
            var validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            ModelClientValidationRule actual = validator.GetClientValidationRules().Single();

            // Assert
            attribute.Verify();
            Assert.Same(expected, actual);
        }

        class DerivedRequiredAttribute : RequiredAttribute
        {
        }

        class SampleModel
        {
            public string Name { get; set; }
        }
    }
}

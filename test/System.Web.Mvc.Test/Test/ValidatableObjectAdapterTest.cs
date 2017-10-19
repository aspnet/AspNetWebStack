// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class ValidatableObjectAdapterTest
    {
        // IValidatableObject support

        [Fact]
        public void NonIValidatableObjectInsideMetadataThrows()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => 42, typeof(IValidatableObject));
            var validator = new ValidatableObjectAdapter(metadata, context);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => validator.Validate(null),
                "The model object inside the metadata claimed to be compatible with System.ComponentModel.DataAnnotations.IValidatableObject, but was actually System.Int32.");
        }

        [Fact]
        public void IValidatableObjectGetsAProperlyPopulatedValidationContext()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => validatable.Object, validatable.Object.GetType());
            var validator = new ValidatableObjectAdapter(metadata, context);
            ValidationContext validationContext = null;
            validatable.Setup(vo => vo.Validate(It.IsAny<ValidationContext>()))
                .Callback<ValidationContext>(vc => validationContext = vc)
                .Returns(Enumerable.Empty<ValidationResult>())
                .Verifiable();

            // Act
            validator.Validate(null);

            // Assert
            validatable.Verify();
            Assert.Same(validatable.Object, validationContext.ObjectInstance);
            Assert.Null(validationContext.MemberName);
        }

        [Fact]
        public void IValidatableObjectWithNoErrors()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => validatable.Object, validatable.Object.GetType());
            var validator = new ValidatableObjectAdapter(metadata, context);
            validatable.Setup(vo => vo.Validate(It.IsAny<ValidationContext>()))
                .Returns(Enumerable.Empty<ValidationResult>());

            // Act
            IEnumerable<ModelValidationResult> results = validator.Validate(null);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void IValidatableObjectWithModelLevelError()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => validatable.Object, validatable.Object.GetType());
            var validator = new ValidatableObjectAdapter(metadata, context);
            validatable.Setup(vo => vo.Validate(It.IsAny<ValidationContext>()))
                .Returns(new ValidationResult[] { new ValidationResult("Error Message") });

            // Act
            ModelValidationResult result = validator.Validate(null).Single();

            // Assert
            Assert.Equal("Error Message", result.Message);
            Assert.Equal(String.Empty, result.MemberName);
        }

        [Fact]
        public void IValidatableObjectWithMultipleModelLevelErrors()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => validatable.Object, validatable.Object.GetType());
            var validator = new ValidatableObjectAdapter(metadata, context);
            validatable.Setup(vo => vo.Validate(It.IsAny<ValidationContext>()))
                .Returns(new ValidationResult[]
                {
                    new ValidationResult("Error Message 1"),
                    new ValidationResult("Error Message 2")
                });

            // Act
            ModelValidationResult[] results = validator.Validate(null).ToArray();

            // Assert
            Assert.Equal(2, results.Length);
            Assert.Equal("Error Message 1", results[0].Message);
            Assert.Equal("Error Message 2", results[1].Message);
        }

        [Fact]
        public void IValidatableObjectWithMultiPropertyValidationFailure()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => validatable.Object, validatable.Object.GetType());
            var validator = new ValidatableObjectAdapter(metadata, context);
            validatable.Setup(vo => vo.Validate(It.IsAny<ValidationContext>()))
                .Returns(new[] { new ValidationResult("Error Message", new[] { "Property1", "Property2" }) })
                .Verifiable();

            // Act
            ModelValidationResult[] results = validator.Validate(null).ToArray();

            // Assert
            validatable.Verify();
            Assert.Equal(2, results.Length);
            Assert.Equal("Error Message", results[0].Message);
            Assert.Equal("Property1", results[0].MemberName);
            Assert.Equal("Error Message", results[1].Message);
            Assert.Equal("Property2", results[1].MemberName);
        }

        [Fact]
        public void IValidatableObjectWhichIsNullReturnsNoErrors()
        {
            // Arrange
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(IValidatableObject));
            var validator = new ValidatableObjectAdapter(metadata, context);

            // Act
            IEnumerable<ModelValidationResult> results = validator.Validate(null);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void IValidatableObjectWhichReturnsValidationResultSuccessReturnsNoErrors()
        {
            // Arrange
            var context = new ControllerContext();
            var validatable = new Mock<IValidatableObject>();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => validatable.Object, validatable.Object.GetType());
            var validator = new ValidatableObjectAdapter(metadata, context);
            validatable.Setup(vo => vo.Validate(It.IsAny<ValidationContext>()))
                .Returns(new[] { ValidationResult.Success })
                .Verifiable();

            // Act
            ModelValidationResult[] results = validator.Validate(null).ToArray();

            // Assert
            validatable.Verify();
            Assert.Empty(results);
        }
    }
}

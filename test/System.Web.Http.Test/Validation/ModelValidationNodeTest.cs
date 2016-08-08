// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation
{
    public class ModelValidationNodeTest
    {
        [Fact]
        public void ConstructorSetsCollectionInstance()
        {
            // Arrange
            ModelMetadata metadata = GetModelMetadata();
            string modelStateKey = "someKey";
            ModelValidationNode[] childNodes = new[]
            {
                new ModelValidationNode(metadata, "someKey0"),
                new ModelValidationNode(metadata, "someKey1")
            };

            // Act
            ModelValidationNode node = new ModelValidationNode(metadata, modelStateKey, childNodes);

            // Assert
            Assert.Equal(childNodes, node.ChildNodes.ToArray());
        }

        [Fact]
        public void ConstructorThrowsIfModelMetadataIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new ModelValidationNode(null, "someKey"),
                "modelMetadata");
        }

        [Fact]
        public void ConstructorThrowsIfModelStateKeyIsNull()
        {
            // Arrange
            ModelMetadata metadata = GetModelMetadata();

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new ModelValidationNode(metadata, null),
                "modelStateKey");
        }

        [Fact]
        public void PropertiesAreSet()
        {
            // Arrange
            ModelMetadata metadata = GetModelMetadata();
            string modelStateKey = "someKey";

            // Act
            ModelValidationNode node = new ModelValidationNode(metadata, modelStateKey);

            // Assert
            Assert.Equal(metadata, node.ModelMetadata);
            Assert.Equal(modelStateKey, node.ModelStateKey);
            Assert.NotNull(node.ChildNodes);
            Assert.Empty(node.ChildNodes);
        }

        [Fact]
        public void CombineWith()
        {
            // Arrange
            List<string> log = new List<string>();

            ModelValidationNode[] allChildNodes = new[]
            {
                new ModelValidationNode(GetModelMetadata(), "key1"),
                new ModelValidationNode(GetModelMetadata(), "key2"),
                new ModelValidationNode(GetModelMetadata(), "key3"),
            };

            ModelValidationNode parentNode1 = new ModelValidationNode(GetModelMetadata(), "parent1");
            parentNode1.ChildNodes.Add(allChildNodes[0]);
            parentNode1.Validating += (sender, e) => log.Add("Validating parent1.");
            parentNode1.Validated += (sender, e) => log.Add("Validated parent1.");

            ModelValidationNode parentNode2 = new ModelValidationNode(GetModelMetadata(), "parent2");
            parentNode2.ChildNodes.Add(allChildNodes[1]);
            parentNode2.ChildNodes.Add(allChildNodes[2]);
            parentNode2.Validating += (sender, e) => log.Add("Validating parent2.");
            parentNode2.Validated += (sender, e) => log.Add("Validated parent2.");

            // Act
            parentNode1.CombineWith(parentNode2);
            parentNode1.Validate(ContextUtil.CreateActionContext());

            // Assert
            Assert.Equal(new[] { "Validating parent1.", "Validating parent2.", "Validated parent1.", "Validated parent2." }, log.ToArray());
            Assert.Equal(allChildNodes, parentNode1.ChildNodes.ToArray());
        }

        [Fact]
        public void CombineWith_OtherNodeIsSuppressed_DoesNothing()
        {
            // Arrange
            List<string> log = new List<string>();

            ModelValidationNode[] allChildNodes = new[]
            {
                new ModelValidationNode(GetModelMetadata(), "key1"),
                new ModelValidationNode(GetModelMetadata(), "key2"),
                new ModelValidationNode(GetModelMetadata(), "key3"),
            };

            ModelValidationNode[] expectedChildNodes = new[]
            {
                allChildNodes[0]
            };

            ModelValidationNode parentNode1 = new ModelValidationNode(GetModelMetadata(), "parent1");
            parentNode1.ChildNodes.Add(allChildNodes[0]);
            parentNode1.Validating += (sender, e) => log.Add("Validating parent1.");
            parentNode1.Validated += (sender, e) => log.Add("Validated parent1.");

            ModelValidationNode parentNode2 = new ModelValidationNode(GetModelMetadata(), "parent2");
            parentNode2.ChildNodes.Add(allChildNodes[1]);
            parentNode2.ChildNodes.Add(allChildNodes[2]);
            parentNode2.Validating += (sender, e) => log.Add("Validating parent2.");
            parentNode2.Validated += (sender, e) => log.Add("Validated parent2.");
            parentNode2.SuppressValidation = true;

            // Act
            parentNode1.CombineWith(parentNode2);
            parentNode1.Validate(ContextUtil.CreateActionContext());

            // Assert
            Assert.Equal(new[] { "Validating parent1.", "Validated parent1." }, log.ToArray());
            Assert.Equal(expectedChildNodes, parentNode1.ChildNodes.ToArray());
        }

        [Fact]
        public void Validate_Ordering()
        {
            // Proper order of invocation:
            // 1. OnValidating()
            // 2. Child validators
            // 3. This validator
            // 4. OnValidated()

            // Arrange
            List<string> log = new List<string>();
            LoggingValidatableObject model = new LoggingValidatableObject(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);
            ModelMetadata childMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(() => model, model.GetType(), "ValidStringProperty");
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey");
            node.Validating += (sender, e) => log.Add("In OnValidating()");
            node.Validated += (sender, e) => log.Add("In OnValidated()");
            node.ChildNodes.Add(new ModelValidationNode(childMetadata, "theKey.ValidStringProperty"));

            // Act
            node.Validate(ContextUtil.CreateActionContext());

            // Assert
            Assert.Equal(new[] { "In OnValidating()", "In LoggingValidatonAttribute.IsValid()", "In IValidatableObject.Validate()", "In OnValidated()" }, log.ToArray());
        }

        [Fact]
        public void Validate_SkipsRemainingValidationIfModelStateIsInvalid()
        {
            // Because a property validator fails, the model validator shouldn't run

            // Arrange
            List<string> log = new List<string>();
            LoggingValidatableObject model = new LoggingValidatableObject(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);
            ModelMetadata childMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(() => model, model.GetType(), "InvalidStringProperty");
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey");
            node.ChildNodes.Add(new ModelValidationNode(childMetadata, "theKey.InvalidStringProperty"));
            node.Validating += (sender, e) => log.Add("In OnValidating()");
            node.Validated += (sender, e) => log.Add("In OnValidated()");
            HttpActionContext context = ContextUtil.CreateActionContext();

            // Act
            node.Validate(context);

            // Assert
            Assert.Equal(new[] { "In OnValidating()", "In IValidatableObject.Validate()", "In OnValidated()" }, log.ToArray());
            Assert.Equal("Sample error message", context.ModelState["theKey.InvalidStringProperty"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_SkipsValidationIfHandlerCancels()
        {
            // Arrange
            List<string> log = new List<string>();
            LoggingValidatableObject model = new LoggingValidatableObject(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey");
            node.Validating += (sender, e) =>
            {
                log.Add("In OnValidating()");
                e.Cancel = true;
            };
            node.Validated += (sender, e) => log.Add("In OnValidated()");

            // Act
            node.Validate(ContextUtil.CreateActionContext());

            // Assert
            Assert.Equal(new[] { "In OnValidating()" }, log.ToArray());
        }

        [Fact]
        public void Validate_SkipsValidationIfSuppressed()
        {
            // Arrange
            List<string> log = new List<string>();
            LoggingValidatableObject model = new LoggingValidatableObject(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey")
            {
                SuppressValidation = true
            };

            node.Validating += (sender, e) => log.Add("In OnValidating()");
            node.Validated += (sender, e) => log.Add("In OnValidated()");

            // Act
            node.Validate(ContextUtil.CreateActionContext());

            // Assert
            Assert.Empty(log);
        }

        [Fact]
        public void Validate_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            ModelValidationNode node = new ModelValidationNode(GetModelMetadata(), "someKey");

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => node.Validate(null),
                "actionContext");
        }

        [Fact]
        [ReplaceCulture]
        public void Validate_ValidateAllProperties_AddsValidationErrors()
        {
            // Arrange
            ValidateAllPropertiesModel model = new ValidateAllPropertiesModel
            {
                RequiredString = null /* error */,
                RangedInt = 0 /* error */,
                ValidString = "dog"
            };

            ModelMetadata modelMetadata = GetModelMetadata(model);
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey")
            {
                ValidateAllProperties = true
            };
            context.ModelState.AddModelError("theKey.RequiredString.Dummy", "existing Error Text");

            // Act
            node.Validate(context);

            // Assert
            Assert.Null(context.ModelState["theKey.RequiredString"]);
            Assert.Equal("existing Error Text", context.ModelState["theKey.RequiredString.Dummy"].Errors[0].ErrorMessage);
            Assert.Equal("The field RangedInt must be between 10 and 30.", context.ModelState["theKey.RangedInt"].Errors[0].ErrorMessage);
            Assert.Null(context.ModelState["theKey.ValidString"]);
            Assert.Null(context.ModelState["theKey"]);
        }

        private static ModelMetadata GetModelMetadata()
        {
            return new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object));
        }

        private static ModelMetadata GetModelMetadata(object o)
        {
            return new DataAnnotationsModelMetadataProvider().GetMetadataForType(() => o, o.GetType());
        }

        private sealed class LoggingValidatableObject : IValidatableObject
        {
            private readonly IList<string> _log;

            public LoggingValidatableObject(IList<string> log)
            {
                _log = log;
            }

            [LoggingValidation]
            public string ValidStringProperty { get; set; }
            public string InvalidStringProperty { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                _log.Add("In IValidatableObject.Validate()");
                yield return new ValidationResult("Sample error message", new[] { "InvalidStringProperty" });
            }

            private sealed class LoggingValidationAttribute : ValidationAttribute
            {
                protected override ValidationResult IsValid(object value, ValidationContext validationContext)
                {
                    LoggingValidatableObject lvo = (LoggingValidatableObject)value;
                    lvo._log.Add("In LoggingValidatonAttribute.IsValid()");
                    return ValidationResult.Success;
                }
            }
        }

        private class ValidateAllPropertiesModel
        {
            [Required]
            public string RequiredString { get; set; }

            [Range(10, 30)]
            public int RangedInt { get; set; }

            [RegularExpression("dog")]
            public string ValidString { get; set; }
        }
    }
}

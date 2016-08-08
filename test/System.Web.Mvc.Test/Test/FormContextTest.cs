// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class FormContextTest
    {
        [Fact]
        public void FieldValidatorsProperty()
        {
            // Arrange
            FormContext context = new FormContext();

            // Act
            IDictionary<String, FieldValidationMetadata> fieldValidators = context.FieldValidators;

            // Assert
            Assert.NotNull(fieldValidators);
            Assert.Empty(fieldValidators);
        }

        [Fact]
        public void ReplaceValidationSummaryProperty()
        {
            // Arrange
            FormContext context = new FormContext();

            // Act & Assert
            MemberHelper.TestBooleanProperty(context, "ReplaceValidationSummary", false, false);
        }

        [Fact]
        public void GetJsonValidationMetadata_NoValidationSummary()
        {
            // Arrange
            FormContext context = new FormContext() { FormId = "theFormId" };

            ModelClientValidationRule rule = new ModelClientValidationRule() { ValidationType = "ValidationType1", ErrorMessage = "Error Message" };
            rule.ValidationParameters["theParam"] = new { FirstName = "John", LastName = "Doe", Age = 32 };
            FieldValidationMetadata metadata = new FieldValidationMetadata() { FieldName = "theFieldName", ValidationMessageId = "theFieldName_ValidationMessage" };
            metadata.ValidationRules.Add(rule);
            context.FieldValidators["theFieldName"] = metadata;

            // Act
            string jsonMetadata = context.GetJsonValidationMetadata();

            // Assert
            string expected = @"{""Fields"":[{""FieldName"":""theFieldName"",""ReplaceValidationMessageContents"":false,""ValidationMessageId"":""theFieldName_ValidationMessage"",""ValidationRules"":[{""ErrorMessage"":""Error Message"",""ValidationParameters"":{""theParam"":{""FirstName"":""John"",""LastName"":""Doe"",""Age"":32}},""ValidationType"":""ValidationType1""}]}],""FormId"":""theFormId"",""ReplaceValidationSummary"":false}";
            Assert.Equal(expected, jsonMetadata);
        }

        [Fact]
        public void GetJsonValidationMetadata_ValidationSummary()
        {
            // Arrange
            FormContext context = new FormContext() { FormId = "theFormId", ValidationSummaryId = "validationSummary" };

            ModelClientValidationRule rule = new ModelClientValidationRule() { ValidationType = "ValidationType1", ErrorMessage = "Error Message" };
            rule.ValidationParameters["theParam"] = new { FirstName = "John", LastName = "Doe", Age = 32 };
            FieldValidationMetadata metadata = new FieldValidationMetadata() { FieldName = "theFieldName", ValidationMessageId = "theFieldName_ValidationMessage" };
            metadata.ValidationRules.Add(rule);
            context.FieldValidators["theFieldName"] = metadata;

            // Act
            string jsonMetadata = context.GetJsonValidationMetadata();

            // Assert
            string expected = @"{""Fields"":[{""FieldName"":""theFieldName"",""ReplaceValidationMessageContents"":false,""ValidationMessageId"":""theFieldName_ValidationMessage"",""ValidationRules"":[{""ErrorMessage"":""Error Message"",""ValidationParameters"":{""theParam"":{""FirstName"":""John"",""LastName"":""Doe"",""Age"":32}},""ValidationType"":""ValidationType1""}]}],""FormId"":""theFormId"",""ReplaceValidationSummary"":false,""ValidationSummaryId"":""validationSummary""}";
            Assert.Equal(expected, jsonMetadata);
        }

        [Fact]
        public void GetValidationMetadataForField_Create_CreatesNewMetadataIfNotFound()
        {
            // Arrange
            FormContext context = new FormContext();

            // Act
            FieldValidationMetadata result = context.GetValidationMetadataForField("fieldName", true /* createIfNotFound */);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fieldName", result.FieldName);

            Assert.Single(context.FieldValidators);
            Assert.Equal(result, context.FieldValidators["fieldName"]);
        }

        [Fact]
        public void GetValidationMetadataForField_NoCreate_ReturnsMetadataIfFound()
        {
            // Arrange
            FormContext context = new FormContext();
            FieldValidationMetadata metadata = new FieldValidationMetadata();
            context.FieldValidators["fieldName"] = metadata;

            // Act
            FieldValidationMetadata result = context.GetValidationMetadataForField("fieldName");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(metadata, result);
        }

        [Fact]
        public void GetValidationMetadataForField_NoCreate_ReturnsNullIfNotFound()
        {
            // Arrange
            FormContext context = new FormContext();

            // Act
            FieldValidationMetadata result = context.GetValidationMetadataForField("fieldName");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetValidationMetadataForFieldThrowsIfFieldNameIsEmpty()
        {
            // Arrange
            FormContext context = new FormContext();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { context.GetValidationMetadataForField(String.Empty); }, "fieldName");
        }

        [Fact]
        public void GetValidationMetadataForFieldThrowsIfFieldNameIsNull()
        {
            // Arrange
            FormContext context = new FormContext();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { context.GetValidationMetadataForField(null); }, "fieldName");
        }

        // RenderedField

        [Fact]
        public void RenderedFieldIsFalseByDefault()
        {
            // Arrange
            var context = new FormContext();

            // Act
            bool result = context.RenderedField(Guid.NewGuid().ToString());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanSetRenderedFieldToBeTrue()
        {
            // Arrange
            var context = new FormContext();
            var name = Guid.NewGuid().ToString();
            context.RenderedField(name, true);

            // Act
            bool result = context.RenderedField(name);

            // Assert
            Assert.True(result);
        }
    }
}

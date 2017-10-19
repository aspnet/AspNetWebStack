// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class MaxLengthAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithMaxLengthAttribute()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string), "Length");
            var context = new ControllerContext();
            var attribute = new MaxLengthAttribute(10);
            var adapter = new MaxLengthAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("maxlength", rule.ValidationType);
            Assert.Equal(1, rule.ValidationParameters.Count);
            Assert.Equal(10, rule.ValidationParameters["max"]);
            Assert.Equal("The field Length must be a string or array type with a maximum length of '10'.", rule.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithMaxLengthAttributeAndCustomMessage()
        {
            // Arrange
            string propertyName = "Length";
            string message = "{0} must be at most {1}";
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string[]), propertyName);
            var context = new ControllerContext();
            var attribute = new MaxLengthAttribute(5) { ErrorMessage = message };
            var adapter = new MaxLengthAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("maxlength", rule.ValidationType);
            Assert.Equal(1, rule.ValidationParameters.Count);
            Assert.Equal(5, rule.ValidationParameters["max"]);
            Assert.Equal("Length must be at most 5", rule.ErrorMessage);
        }


    }
}

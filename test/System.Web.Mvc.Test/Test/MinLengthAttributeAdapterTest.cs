// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class MinLengthAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithMinLengthAttribute()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string), "Length");
            var context = new ControllerContext();
            var attribute = new MinLengthAttribute(6);
            var adapter = new MinLengthAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("minlength", rule.ValidationType);
            Assert.Equal(1, rule.ValidationParameters.Count);
            Assert.Equal(6, rule.ValidationParameters["min"]);
            Assert.Equal("The field Length must be a string or array type with a minimum length of '6'.", rule.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithMinLengthAttributeAndCustomMessage()
        {
            // Arrange
            string propertyName = "Length";
            string message = "Array must have at least {1} items.";
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(int[]), propertyName);
            var context = new ControllerContext();
            var attribute = new MinLengthAttribute(2) { ErrorMessage = message };
            var adapter = new MinLengthAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("minlength", rule.ValidationType);
            Assert.Equal(1, rule.ValidationParameters.Count);
            Assert.Equal(2, rule.ValidationParameters["min"]);
            Assert.Equal("Array must have at least 2 items.", rule.ErrorMessage);
        }


    }
}

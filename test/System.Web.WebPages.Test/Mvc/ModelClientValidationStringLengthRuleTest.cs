// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelClientValidationStringLengthRuleTest
    {
        [Fact]
        public void ModelClientValidationStringLengthRuleTestDoesNotAddMinLengthParameterIfSpecifiedValueIs0()
        {
            // Arrange
            var clientValidationRule = new ModelClientValidationStringLengthRule("Test message", 0, 10);

            // Assert
            Assert.Equal(1, clientValidationRule.ValidationParameters.Count);
            Assert.Equal(10, clientValidationRule.ValidationParameters["max"]);
        }

        [Fact]
        public void ModelClientValidationStringLengthRuleTestDoesNotAddMaxLengthIfSpecifiedValueIsMaxValue()
        {
            // Arrange
            var clientValidationRule = new ModelClientValidationStringLengthRule("Test message", 3, Int32.MaxValue);

            // Assert
            Assert.Equal(1, clientValidationRule.ValidationParameters.Count);
            Assert.Equal(3, clientValidationRule.ValidationParameters["min"]);
        }

        [Fact]
        public void ModelClientValidationStringLengthRuleTestAddsMinLengthParameter()
        {
            // Arrange
            string message = "Password must contain only letters and must be between 3-8 characters long";
            var clientValidationRule = new ModelClientValidationStringLengthRule(message, 3, 8);

            // Assert
            Assert.Equal(2, clientValidationRule.ValidationParameters.Count);
            Assert.Equal(message, clientValidationRule.ErrorMessage);
            Assert.Equal(3, clientValidationRule.ValidationParameters["min"]);
            Assert.Equal(8, clientValidationRule.ValidationParameters["max"]);
        }

    }
}

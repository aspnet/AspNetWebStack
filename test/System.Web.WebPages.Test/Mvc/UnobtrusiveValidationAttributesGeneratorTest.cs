// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test.Mvc
{
    public class UnobtrusiveValidationAttributesGeneratorTest
    {
        [Fact]
        public void GetValidationAttributesReturnsAttributesWhenMinAndMaxLengthRulesAreApplied()
        {
            // Arrange
            var clientRules = new ModelClientValidationRule[]
            {
                new ModelClientValidationMaxLengthRule("Max length message", 8),
                new ModelClientValidationMinLengthRule("Min length message", 2)
            };
            var attributes = new Dictionary<string, object>();

            // Act
            UnobtrusiveValidationAttributesGenerator.GetValidationAttributes(clientRules, attributes);

            // Assert
            Assert.Equal(5, attributes.Count);
            Assert.Equal("true", attributes["data-val"]);
            Assert.Equal("Max length message", attributes["data-val-maxlength"]);
            Assert.Equal(8, attributes["data-val-maxlength-max"]);
            Assert.Equal("Min length message", attributes["data-val-minlength"]);
            Assert.Equal(2, attributes["data-val-minlength-min"]);
        }

        [Fact]
        public void GetValidationAttributesReturnsAttributesWhenMinAndStringLengthRulesAreApplied()
        {
            // Arrange
            var clientRules = new ModelClientValidationRule[]
            {
                new ModelClientValidationMinLengthRule("Min length message", 2),
                new ModelClientValidationStringLengthRule("String length rule", 2, 6)
            };
            var attributes = new Dictionary<string, object>();

            // Act
            UnobtrusiveValidationAttributesGenerator.GetValidationAttributes(clientRules, attributes);

            // Assert
            Assert.Equal(6, attributes.Count);
            Assert.Equal("true", attributes["data-val"]);
            Assert.Equal("String length rule", attributes["data-val-length"]);
            Assert.Equal(2, attributes["data-val-length-min"]);
            Assert.Equal(6, attributes["data-val-length-max"]);
            Assert.Equal("Min length message", attributes["data-val-minlength"]);
            Assert.Equal(2, attributes["data-val-minlength-min"]);
        }
    }
}

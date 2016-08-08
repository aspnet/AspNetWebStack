// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class CreditCardAttributeTest
    {
        [Fact]
        public void ClientRule()
        {
            // Arrange
            var attribute = new CreditCardAttribute();
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.Equal("creditcard", clientRule.ValidationType);
            Assert.Equal("The PropertyName field is not a valid credit card number.", clientRule.ErrorMessage);
            Assert.Empty(clientRule.ValidationParameters);
        }

        [Fact]
        public void IsValidTests()
        {
            // Arrange
            var attribute = new CreditCardAttribute();

            // Act & Assert
            Assert.True(attribute.IsValid(null)); // Optional values are always valid
            Assert.True(attribute.IsValid("0000000000000000")); // Simplest valid value
            Assert.True(attribute.IsValid("1234567890123452")); // Good checksum
            Assert.True(attribute.IsValid("1234-5678-9012-3452")); // Good checksum, with dashes
            Assert.False(attribute.IsValid("0000000000000001")); // Bad checksum
            Assert.False(attribute.IsValid(0)); // Non-string
            Assert.False(attribute.IsValid("000%000000000001")); // Non-digit
        }
    }
}

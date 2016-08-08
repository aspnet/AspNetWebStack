// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class EmailAddressAttribueTest
    {
        [Fact]
        public void ClientRule()
        {
            // Arrange
            var attribute = new EmailAddressAttribute();
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.Equal("email", clientRule.ValidationType);
            Assert.Equal("The PropertyName field is not a valid e-mail address.", clientRule.ErrorMessage);
            Assert.Empty(clientRule.ValidationParameters);
        }

        [Fact]
        public void IsValidTests()
        {
            // Arrange
            var attribute = new EmailAddressAttribute();

            // Act & Assert
            Assert.True(attribute.IsValid(null)); // Optional values are always valid
            Assert.True(attribute.IsValid("joe@contoso.com"));
            Assert.True(attribute.IsValid("joe%fred@contoso.com"));
            Assert.False(attribute.IsValid("joe"));
            Assert.False(attribute.IsValid("joe@"));
            Assert.False(attribute.IsValid("joe@contoso"));
        }
    }
}

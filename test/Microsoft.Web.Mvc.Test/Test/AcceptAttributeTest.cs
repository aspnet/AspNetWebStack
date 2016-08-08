// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class AcceptAttributeTest
    {
        [Fact]
        public void DefaultIsEmpty()
        {
            Assert.True(String.IsNullOrEmpty(new AcceptAttribute().MimeTypes));
        }

        [Fact]
        public void ClientRule()
        {
            // Arrange
            var attribute = new AcceptAttribute { MimeTypes = " text/html , application/javascript " };
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.Equal("accept", clientRule.ValidationType);
            Assert.Equal("The PropertyName field only accepts files with one of the following content types: text/html, application/javascript.", clientRule.ErrorMessage);
            Assert.Single(clientRule.ValidationParameters);
            Assert.Equal("text/html,application/javascript", clientRule.ValidationParameters["mimetype"]);
        }

        [Fact]
        public void IsValidTests()
        {
            // Arrange
            var attribute = new AcceptAttribute { MimeTypes = " text/html , application/javascript " };

            // Act & Assert
            Assert.True(attribute.IsValid(null)); // Optional values are always valid
            Assert.True(attribute.IsValid("text/html"));
            Assert.True(attribute.IsValid("application/javascript"));
            Assert.False(attribute.IsValid("text/css"));
            Assert.False(attribute.IsValid("\0text/html")); // Illegal character
        }
    }
}

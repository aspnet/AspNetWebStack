// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class UrlAttributeTest
    {
        [Fact]
        public void ClientRule()
        {
            // Arrange
            var attribute = new UrlAttribute();
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.Equal("url", clientRule.ValidationType);
            Assert.Equal("The PropertyName field is not a valid fully-qualified http, https, or ftp URL.", clientRule.ErrorMessage);
            Assert.Empty(clientRule.ValidationParameters);
        }

        [Fact]
        public void IsValidTests()
        {
            // Arrange
            var attribute = new UrlAttribute();

            // Act & Assert
            Assert.True(attribute.IsValid(null)); // Optional values are always valid
            Assert.True(attribute.IsValid("http://foo.bar"));
            Assert.True(attribute.IsValid("https://foo.bar"));
            Assert.True(attribute.IsValid("ftp://foo.bar"));
            Assert.False(attribute.IsValid("file:///foo.bar"));
            Assert.False(attribute.IsValid("http://user%password@foo.bar/"));
            Assert.False(attribute.IsValid("foo.png"));
            Assert.False(attribute.IsValid("\0foo.png")); // Illegal character
        }
    }
}

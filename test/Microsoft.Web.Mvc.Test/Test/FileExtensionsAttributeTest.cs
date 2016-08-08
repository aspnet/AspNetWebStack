// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class FileExtensionsAttributeTest
    {
        [Fact]
        public void DefaultExtensions()
        {
            Assert.Equal("png,jpg,jpeg,gif", new FileExtensionsAttribute().Extensions);
        }

        [Fact]
        public void ClientRule()
        {
            // Arrange
            var attribute = new FileExtensionsAttribute { Extensions = " FoO, .bar,baz " };
            var provider = new Mock<ModelMetadataProvider>();
            var metadata = new ModelMetadata(provider.Object, null, null, typeof(string), "PropertyName");

            // Act
            ModelClientValidationRule clientRule = attribute.GetClientValidationRules(metadata, null).Single();

            // Assert
            Assert.Equal("extension", clientRule.ValidationType);
            Assert.Equal("The PropertyName field only accepts files with the following extensions: .foo, .bar, .baz", clientRule.ErrorMessage);
            Assert.Single(clientRule.ValidationParameters);
            Assert.Equal("foo,bar,baz", clientRule.ValidationParameters["extension"]);
        }

        [Fact]
        public void IsValidTests()
        {
            // Arrange
            var attribute = new FileExtensionsAttribute();

            // Act & Assert
            Assert.True(attribute.IsValid(null)); // Optional values are always valid
            Assert.True(attribute.IsValid("foo.png"));
            Assert.True(attribute.IsValid("foo.jpeg"));
            Assert.True(attribute.IsValid("foo.jpg"));
            Assert.True(attribute.IsValid("foo.gif"));
            Assert.True(attribute.IsValid(@"C:\Foo\baz.jpg"));
            Assert.False(attribute.IsValid("foo"));
            Assert.False(attribute.IsValid("foo.png.pif"));
            Assert.False(attribute.IsValid(@"C:\foo.png\bar"));
            Assert.False(attribute.IsValid("\0foo.png")); // Illegal character
        }
    }
}

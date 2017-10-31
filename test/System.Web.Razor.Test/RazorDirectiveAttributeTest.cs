﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test
{
    public class RazorDirectiveAttributeTest
    {
        [Fact]
        public void ConstructorThrowsIfNameIsNullOrEmpty()
        {
            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => new RazorDirectiveAttribute(name: null, value: "blah"), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => new RazorDirectiveAttribute(name: "", value: "blah"), "name");
        }

        [Fact]
        public void EnsureRazorDirectiveProperties()
        {
            // Arrange
            var attribute = (AttributeUsageAttribute)typeof(RazorDirectiveAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
                                                                                     .SingleOrDefault();

            // Assert
            Assert.True(attribute.AllowMultiple);
            Assert.Equal(AttributeTargets.Class, attribute.ValidOn);
            Assert.True(attribute.Inherited);
        }

        [Fact]
        public void EqualsAndGetHashCodeIgnoresCase()
        {
            // Arrange
            var attribute1 = new RazorDirectiveAttribute("foo", "bar");
            var attribute2 = new RazorDirectiveAttribute("fOo", "BAr");

            // Act
            var hashCode1 = attribute1.GetHashCode();
            var hashCode2 = attribute2.GetHashCode();

            // Assert
            Assert.Equal(attribute1, attribute2);
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void EqualsAndGetHashCodeDoNotThrowIfValueIsNullOrEmpty()
        {
            // Arrange
            var attribute1 = new RazorDirectiveAttribute("foo", null);
            var attribute2 = new RazorDirectiveAttribute("foo", "BAr");

            // Act
            bool result = attribute1.Equals(attribute2);
            var hashCode = attribute1.GetHashCode();

            // Assert
            Assert.False(result);
            // If we've got this far, GetHashCode did not throw
        }

        [Fact]
        public void EqualsAndGetHashCodeReturnDifferentValuesForNullAndEmpty()
        {
            // Arrange
            var attribute1 = new RazorDirectiveAttribute("foo", null);
            var attribute2 = new RazorDirectiveAttribute("foo", "");

            // Act
            bool result = attribute1.Equals(attribute2);
            var hashCode1 = attribute1.GetHashCode();
            var hashCode2 = attribute2.GetHashCode();

            // Assert
            Assert.False(result);
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}

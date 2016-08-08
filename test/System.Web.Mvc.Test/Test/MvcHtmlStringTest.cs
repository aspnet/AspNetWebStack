// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class MvcHtmlStringTest
    {
        // IsNullOrEmpty

        [Fact]
        public void IsNullOrEmptyTests()
        {
            // Act & Assert
            Assert.True(MvcHtmlString.IsNullOrEmpty(null));
            Assert.True(MvcHtmlString.IsNullOrEmpty(MvcHtmlString.Empty));
            Assert.True(MvcHtmlString.IsNullOrEmpty(MvcHtmlString.Create("")));
            Assert.False(MvcHtmlString.IsNullOrEmpty(MvcHtmlString.Create(" ")));
        }

        // ToHtmlString

        [Fact]
        public void ToHtmlStringReturnsOriginalString()
        {
            // Arrange
            MvcHtmlString htmlString = MvcHtmlString.Create("some value");

            // Act
            string retVal = htmlString.ToHtmlString();

            // Assert
            Assert.Equal("some value", retVal);
        }

        // ToString

        [Fact]
        public void ToStringReturnsOriginalString()
        {
            // Arrange
            MvcHtmlString htmlString = MvcHtmlString.Create("some value");

            // Act
            string retVal = htmlString.ToString();

            // Assert
            Assert.Equal("some value", retVal);
        }

        [Fact]
        public void ToStringReturnsEmptyStringIfOriginalStringWasNull()
        {
            // Arrange
            MvcHtmlString htmlString = MvcHtmlString.Create(null);

            // Act
            string retVal = htmlString.ToString();

            // Assert
            Assert.Equal("", retVal);
        }
    }
}

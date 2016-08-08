// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class ElementalValueProviderTest
    {
        [Fact]
        public void ContainsPrefix()
        {
            // Arrange
            ElementalValueProvider valueProvider = new ElementalValueProvider("foo", 42, null);

            // Act & assert
            Assert.True(valueProvider.ContainsPrefix("foo"));
            Assert.False(valueProvider.ContainsPrefix("bar"));
        }

        [Fact]
        public void GetValue_NameDoesNotMatch_ReturnsNull()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            DateTime rawValue = new DateTime(2001, 1, 2);
            ElementalValueProvider valueProvider = new ElementalValueProvider("foo", rawValue, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValue_NameMatches_ReturnsValueProviderResult()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            DateTime rawValue = new DateTime(2001, 1, 2);
            ElementalValueProvider valueProvider = new ElementalValueProvider("foo", rawValue, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("FOO");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(rawValue, vpResult.RawValue);
            Assert.Equal("02/01/2001 00:00:00", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }
    }
}

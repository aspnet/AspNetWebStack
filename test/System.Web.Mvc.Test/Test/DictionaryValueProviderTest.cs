// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class DictionaryValueProviderTest
    {
        private static readonly Dictionary<string, object> _backingStore = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "forty.two", 42 },
            { "nineteen.eighty.four", new DateTime(1984, 1, 1) }
        };

        [Fact]
        public void Constructor_ThrowsIfDictionaryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new DictionaryValueProvider<object>(null, CultureInfo.InvariantCulture); }, "dictionary");
        }

        [Fact]
        public void ContainsPrefix()
        {
            // Arrange
            DictionaryValueProvider<object> valueProvider = new DictionaryValueProvider<object>(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("forty");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_DoesNotContainEmptyPrefixIfBackingStoreIsEmpty()
        {
            // Arrange
            DictionaryValueProvider<object> valueProvider = new DictionaryValueProvider<object>(new Dictionary<string, object>(), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_ThrowsIfPrefixIsNull()
        {
            // Arrange
            DictionaryValueProvider<object> valueProvider = new DictionaryValueProvider<object>(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.ContainsPrefix(null); }, "prefix");
        }

        [Fact]
        public void GetValue()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            DictionaryValueProvider<object> valueProvider = new DictionaryValueProvider<object>(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("nineteen.eighty.four");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new DateTime(1984, 1, 1), vpResult.RawValue);
            Assert.Equal("01/01/1984 00:00:00", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            DictionaryValueProvider<object> valueProvider = new DictionaryValueProvider<object>(_backingStore, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("nineteen.eighty");

            // Assert
            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValue_ThrowsIfKeyIsNull()
        {
            // Arrange
            DictionaryValueProvider<object> valueProvider = new DictionaryValueProvider<object>(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.GetValue(null); }, "key");
        }
    }
}

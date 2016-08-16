// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValuePairsValueProviderTest
    {
        private static readonly IEnumerable<KeyValuePair<string, string>> _backingStore = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("foo", "fooValue1"),
            new KeyValuePair<string, string>("foo", "fooValue2"),
            new KeyValuePair<string, string>("bar.baz", "someOtherValue"),
            new KeyValuePair<string, string>("null_value", null),
            new KeyValuePair<string, string>("prefix.null_value", null)
        };

        [Fact]
        public void Constructor_GuardClauses()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new NameValuePairsValueProvider(values: (IEnumerable<KeyValuePair<string, string>>)null, culture: CultureInfo.InvariantCulture),
                "values");

            Assert.ThrowsArgumentNull(
                () => new NameValuePairsValueProvider(valuesFactory: null, culture: CultureInfo.InvariantCulture),
                "valuesFactory");
        }

        [Fact]
        public void ContainsPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.ContainsPrefix(null),
                "prefix");
        }

        [Fact]
        public void ContainsPrefix_WithEmptyCollection_ReturnsFalseForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(Enumerable.Empty<KeyValuePair<string, string>>(), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForKnownPrefixes()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & Assert
            Assert.True(valueProvider.ContainsPrefix("foo"));
            Assert.True(valueProvider.ContainsPrefix("bar"));
            Assert.True(valueProvider.ContainsPrefix("bar.baz"));
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsFalseForUnknownPrefix()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("biff");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetKeysFromPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.GetKeysFromPrefix(null),
                "prefix");
        }

        [Fact]
        public void GetKeysFromPrefix_EmptyPrefix_ReturnsAllPrefixes()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("");

            // Assert
            Assert.Equal<KeyValuePair<string, string>>(
                result.OrderBy(kvp => kvp.Key),
                new Dictionary<string, string> { { "bar", "bar" }, { "foo", "foo" }, { "null_value", "null_value" }, { "prefix", "prefix" } });
        }

        [Fact]
        public void GetKeysFromPrefix_UnknownPrefix_ReturnsEmptyDictionary()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("abc");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetKeysFromPrefix_KnownPrefix_ReturnsMatchingItems()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("bar");

            // Assert
            KeyValuePair<string, string> kvp = Assert.Single(result);
            Assert.Equal("baz", kvp.Key);
            Assert.Equal("bar.baz", kvp.Value);
        }

        [Fact]
        public void GetValue_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.GetValue(null),
                "key");
        }

        [Fact]
        public void GetValue_SingleValue()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValuePairsValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar.baz");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal("someOtherValue", vpResult.RawValue);
            Assert.Equal("someOtherValue", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_MultiValue()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValuePairsValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new List<string>() { "fooValue1", "fooValue2" }, (List<string>)vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Theory]
        [InlineData("null_value")]
        [InlineData("prefix.null_value")]
        public void GetValue_NullValue(string key)
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValuePairsValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue(key);

            // Assert
            Assert.NotNull(vpResult);
            Assert.Null(vpResult.RawValue);
            Assert.Null(vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_NullMultipleValue()
        {
            // Arrange
            var backingStore = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("key", null),
                new KeyValuePair<string, string>("key", null),
                new KeyValuePair<string, string>("key", "value")
            };
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValuePairsValueProvider(backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("key");

            // Assert
            Assert.Equal(new[] { null, null, "value" }, vpResult.RawValue as IEnumerable<string>);
            Assert.Equal(",,value", vpResult.AttemptedValue);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }
    }
}

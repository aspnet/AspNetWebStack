// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class NameValueCollectionValueProviderTest
    {
        private static readonly NameValueCollection _backingStore = new NameValueCollection()
        {
            { "foo", "fooValue1" },
            { "foo", "fooValue2" },
            { "bar.baz", "someOtherValue" }
        };

        [Fact]
        public void Constructor_ThrowsIfCollectionIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new NameValueCollectionValueProvider(null, CultureInfo.InvariantCulture); }, "collection");
        }

        [Fact]
        public void ContainsPrefix()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("bar");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_DoesNotContainEmptyPrefixIfBackingStoreIsEmpty()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(new NameValueCollection(), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_ThrowsIfPrefixIsNull()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.ContainsPrefix(null); }, "prefix");
        }

        [Fact]
        public void GetValue()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(_backingStore.GetValues("foo"), (string[])vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_NonValidating()
        {
            // Arrange
            NameValueCollection unvalidatedCollection = new NameValueCollection()
            {
                { "foo", "fooValue3" },
                { "foo", "fooValue4" }
            };

            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, unvalidatedCollection, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo", skipValidation: true);

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new[] { "fooValue3", "fooValue4" }, (string[])vpResult.RawValue);
            Assert.Equal("fooValue3,fooValue4", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_NonValidating_NoUnvalidatedCollectionSpecified_UsesDefaultCollectionValue()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo", skipValidation: true);

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(_backingStore.GetValues("foo"), (string[])vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Theory]
        [InlineData("foo", "fooValue", "foo", "fooValue")]
        [InlineData("fooArray[0][bar1][bar3]", "barValue1", "fooArray[0].bar1.bar3", "barValue1")]
        [InlineData("fooArray[0][bar2]", "barValue2", "fooArray[0].bar2", "barValue2")]
        [InlineData(
            "fooArray[1][bar1][0][nested]", "nestedArrayValue", "fooArray[1].bar1[0].nested", "nestedArrayValue")]
        [InlineData("fooArray[2].bar1", "noSquareBracesValue", "fooArray[2].bar1", "noSquareBracesValue")]
        [InlineData("foo.bar", "fooBarValue", "foo.bar", "fooBarValue")]
        public void GetValue_NonValidating_WithArraysInCollection(
                            string name, string value, string index, string expectedAttemptedValue)
        {
            // Arrange
            string[] expectedRawValue = new[] { expectedAttemptedValue };
            NameValueCollection unvalidatedCollection = new NameValueCollection();
            unvalidatedCollection.Add(name, value);

            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = 
                    new NameValueCollectionValueProvider(_backingStore, unvalidatedCollection, culture, true);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue(index, skipValidation: true);
            
            // Asserts
            Assert.NotNull(vpResult);
            Assert.Equal(culture, vpResult.Culture);
            Assert.Equal(expectedRawValue, (string[])vpResult.RawValue);
            Assert.Equal(expectedAttemptedValue, vpResult.AttemptedValue);
        }

        [Fact]
        public void GetValue_NonValidating_WithArraysInCollection_Error()
        {
            // Arrange
            NameValueCollection unvalidatedCollection = new NameValueCollection()
            {
                { "foo", "fooValue3" },
                { "fooArray[0][bar1", "barValue1" }
            };

            NameValueCollectionValueProvider valueProvider =
                new NameValueCollectionValueProvider(
                                    _backingStore,
                                    unvalidatedCollection,
                                    culture: null,
                                    jQueryToMvcRequestNormalizationRequired: true);

            // Act & Assert
            Assert.ThrowsArgument(
                () => valueProvider.GetValue("foo", skipValidation: true),
                "key",
                "The key is invalid JQuery syntax because it is missing a closing bracket.");
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValue_ThrowsIfKeyIsNull()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.GetValue(null); }, "key");
        }
    }
}

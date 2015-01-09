// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders.Providers
{
    public class ElementalValueProviderTest
    {
        [Theory]
        [InlineData("MyProperty", "MyProperty")]
        [InlineData("MyProperty.SubProperty", "MyProperty")]
        [InlineData("MyProperty[0]", "MyProperty")]
        public void ContainsPrefix_ReturnsTrue_IfElementNameStartsWithPrefix(string elementName, string prefix)
        {
            // Arrange
            CultureInfo culture = new CultureInfo("en-US");
            ElementalValueProvider elementalValueProvider = new ElementalValueProvider(elementName,
                                                                                       new object(),
                                                                                       culture);

            // Act
            bool containsPrefix = elementalValueProvider.ContainsPrefix(prefix);

            // Assert
            Assert.True(containsPrefix);
        }

        [Theory]
        [InlineData("MyProperty", "MyProperty1")]
        [InlineData("MyPropertyTest", "MyProperty")]
        [InlineData("Random", "MyProperty")]
        public void ContainsPrefix_ReturnsFalse_IfElementCannotSpecifyValuesForPrefix(string elementName, string prefix)
        {
            // Arrange
            CultureInfo culture = new CultureInfo("en-US");
            ElementalValueProvider elementalValueProvider = new ElementalValueProvider(elementName,
                                                                                       new object(),
                                                                                       culture);

            // Act
            bool containsPrefix = elementalValueProvider.ContainsPrefix(prefix);

            // Assert
            Assert.False(containsPrefix);
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

        [Theory]
        [InlineData("foo")]
        [InlineData("FOO")]
        [InlineData("FoO")]
        public void GetValue_NameMatches_ReturnsValueProviderResult(string name)
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            DateTime rawValue = new DateTime(2001, 1, 2);
            ElementalValueProvider valueProvider = new ElementalValueProvider("foo", rawValue, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue(name);

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(rawValue, vpResult.RawValue);
            Assert.Equal("02/01/2001 00:00:00", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }
    }
}

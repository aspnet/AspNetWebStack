// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ValueProviderResultTest
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            // Arrange
            object rawValue = new object();
            string attemptedValue = "some string";
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");

            // Act
            ValueProviderResult result = new ValueProviderResult(rawValue, attemptedValue, culture);

            // Assert
            Assert.Same(rawValue, result.RawValue);
            Assert.Same(attemptedValue, result.AttemptedValue);
            Assert.Same(culture, result.Culture);
        }

        [Fact]
        public void ConvertToCanConvertArraysToArrays()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new int[] { 1, 20, 42 }, "", CultureInfo.InvariantCulture);

            // Act
            string[] converted = (string[])vpr.ConvertTo(typeof(string[]));

            // Assert
            Assert.NotNull(converted);
            Assert.Equal(3, converted.Length);
            Assert.Equal("1", converted[0]);
            Assert.Equal("20", converted[1]);
            Assert.Equal("42", converted[2]);
        }

        [Fact]
        public void ConvertToCanConvertArraysToSingleElements()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new int[] { 1, 20, 42 }, "", CultureInfo.InvariantCulture);

            // Act
            string converted = (string)vpr.ConvertTo(typeof(string));

            // Assert
            Assert.Equal("1", converted);
        }

        [Fact]
        public void ConvertToCanConvertSingleElementsToArrays()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(42, "", CultureInfo.InvariantCulture);

            // Act
            string[] converted = (string[])vpr.ConvertTo(typeof(string[]));

            // Assert
            Assert.NotNull(converted);
            string value = Assert.Single(converted);
            Assert.Equal("42", value);
        }

        [Fact]
        public void ConvertToCanConvertSingleElementsToSingleElements()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(42, "", CultureInfo.InvariantCulture);

            // Act
            string converted = (string)vpr.ConvertTo(typeof(string));

            // Assert
            Assert.NotNull(converted);
            Assert.Equal("42", converted);
        }

        [Fact]
        public void ConvertToChecksTypeConverterCanConvertFrom()
        {
            // Arrange
            object original = "someValue";
            ValueProviderResult vpr = new ValueProviderResult(original, null, CultureInfo.GetCultureInfo("fr-FR"));

            // Act
            DefaultModelBinderTest.StringContainer returned = (DefaultModelBinderTest.StringContainer)vpr.ConvertTo(typeof(DefaultModelBinderTest.StringContainer));

            // Assert
            Assert.Equal("someValue (fr-FR)", returned.Value);
        }

        [Fact]
        public void ConvertingNullStringToNullableIntReturnsNull()
        {
            // Arrange
            object original = null;
            ValueProviderResult vpr = new ValueProviderResult(original, "", CultureInfo.InvariantCulture);

            // Act
            int? returned = (int?)vpr.ConvertTo(typeof(int?));

            // Assert
            Assert.Null(returned);
        }

        [Fact]
        public void ConvertingWhiteSpaceStringToNullableIntReturnsNull()
        {
            // Arrange
            object original = " ";
            ValueProviderResult vpr = new ValueProviderResult(original, "", CultureInfo.InvariantCulture);

            // Act
            int? returned = (int?)vpr.ConvertTo(typeof(int?));

            // Assert
            Assert.Null(returned);
        }

        [Fact]
        public void ConvertToChecksTypeConverterCanConvertTo()
        {
            // Arrange
            object original = new DefaultModelBinderTest.StringContainer("someValue");
            ValueProviderResult vpr = new ValueProviderResult(original, "", CultureInfo.GetCultureInfo("en-US"));

            // Act
            string returned = (string)vpr.ConvertTo(typeof(string));

            // Assert
            Assert.Equal("someValue (en-US)", returned);
        }

        [Fact]
        public void ConvertToReturnsNullIfArrayElementValueIsNull()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new string[] { null }, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsNullIfTryingToConvertEmptyArrayToSingleElement()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new int[0], "", CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsNullIfValueIsEmptyString()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult("", null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsNullIfTrimmedValueIsEmptyString()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(" \t \r\n ", null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsNullIfValueIsNull()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(null /* rawValue */, null /* attemptedValue */, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int[]));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementIsIntegerAndDestinationTypeIsEnum()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new object[] { 1 }, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(MyEnum));

            // Assert
            Assert.Equal(MyEnum.Value1, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementIsStringValueAndDestinationTypeIsEnum()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new object[] { "1" }, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(MyEnum));

            // Assert
            Assert.Equal(MyEnum.Value1, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementIsStringKeyAndDestinationTypeIsEnum()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new object[] { "Value1" }, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(MyEnum));

            // Assert
            Assert.Equal(MyEnum.Value1, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsStringAndDestionationIsNullableInteger()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult("12", null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(12, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsStringAndDestionationIsNullableDouble()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult("12.5", null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(double?));

            // Assert
            Assert.Equal(12.5, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalAndDestionationIsNullableInteger()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(12M, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(12, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalAndDestionationIsNullableDouble()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(12.5M, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(double?));

            // Assert
            Assert.Equal(12.5, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalDoubleAndDestionationIsNullableInteger()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(12.5M, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(12, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalDoubleAndDestionationIsNullableLong()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(12.5M, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(long?));

            // Assert
            Assert.Equal(12L, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementInstanceOfDestinationType()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult(new object[] { "some string" }, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(string));

            // Assert
            Assert.Equal("some string", outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfInstanceOfDestinationType()
        {
            // Arrange
            string[] original = new string[] { "some string" };
            ValueProviderResult vpr = new ValueProviderResult(original, null, CultureInfo.InvariantCulture);

            // Act
            object outValue = vpr.ConvertTo(typeof(string[]));

            // Assert
            Assert.Same(original, outValue);
        }

        [Fact]
        public void ConvertToThrowsIfConverterThrows()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult("x", null, CultureInfo.InvariantCulture);
            Type destinationType = typeof(DefaultModelBinderTest.StringContainer);

            // Act & Assert
            // Will throw since the custom converter assumes the first 5 characters to be digits
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                delegate { vpr.ConvertTo(destinationType); },
                "The parameter conversion from type 'System.String' to type 'System.Web.Mvc.Test.DefaultModelBinderTest+StringContainer' failed. See the inner exception for more information.");

            Exception innerException = exception.InnerException;
            Assert.Equal("Value must have at least 3 characters.", innerException.Message);
        }

        [Fact]
        public void ConvertToThrowsIfNoConverterExists()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult("x", null, CultureInfo.InvariantCulture);
            Type destinationType = typeof(MyClassWithoutConverter);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { vpr.ConvertTo(destinationType); },
                "The parameter conversion from type 'System.String' to type 'System.Web.Mvc.Test.ValueProviderResultTest+MyClassWithoutConverter' failed because no type converter can convert between these types.");
        }

        [Fact]
        public void ConvertToThrowsIfTypeIsNull()
        {
            // Arrange
            ValueProviderResult vpr = new ValueProviderResult("x", null, CultureInfo.InvariantCulture);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { vpr.ConvertTo(null); }, "type");
        }

        [Fact]
        public void ConvertToUsesProvidedCulture()
        {
            // Arrange
            object original = "someValue";
            CultureInfo gbCulture = CultureInfo.GetCultureInfo("en-GB");
            ValueProviderResult vpr = new ValueProviderResult(original, null, CultureInfo.GetCultureInfo("fr-FR"));

            // Act
            DefaultModelBinderTest.StringContainer returned = (DefaultModelBinderTest.StringContainer)vpr.ConvertTo(typeof(DefaultModelBinderTest.StringContainer), gbCulture);

            // Assert
            Assert.Equal("someValue (en-GB)", returned.Value);
        }

        [Fact]
        public void CulturePropertyDefaultsToInvariantCulture()
        {
            // Arrange
            ValueProviderResult result = new ValueProviderResult(null, null, null);

            // Act & assert
            Assert.Same(CultureInfo.InvariantCulture, result.Culture);
        }

        [Theory]
        [PropertyData("IntrinsicConversionData")]
        public void ConvertToCanConvertIntrinsics<T>(object initialValue, T expectedValue)
        {
            // Arrange
            var result = new ValueProviderResult(initialValue, "", CultureInfo.InvariantCulture);

            // Act & Assert
            Assert.Equal(expectedValue, result.ConvertTo(typeof(T)));
        }

        public static IEnumerable<object[]> IntrinsicConversionData
        {
            get
            {
                return new TheoryDataSet<object, object>
                {
                    { 42, 42M },
                    { 42, 42L },
                    { 42, (float)42.0 },
                    { 42, (double)42.0 },
                    { 42M, 42 },
                    { 42L, 42 },
                    { (float)42.0, 42 },
                    { (double)42.0, 42 }
                };
            }
        }

        private class MyClassWithoutConverter
        {
        }

        private enum MyEnum
        {
            Value0 = 0,
            Value1 = 1
        }
    }
}

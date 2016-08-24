// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders
{
    public class ValueProviderResultTest
    {
        [Fact]
        public void ConvertTo_ReturnsNullForReferenceTypes_WhenValueIsNull()
        {
            var valueProviderResult = new ValueProviderResult(null, null, CultureInfo.InvariantCulture);

            var convertedValue = valueProviderResult.ConvertTo(typeof(string));

            Assert.Null(convertedValue);
        }

        [Fact]
        public void ConvertTo_ReturnsDefaultForValueTypes_WhenValueIsNull()
        {
            var valueProviderResult = new ValueProviderResult(null, null, CultureInfo.InvariantCulture);

            var convertedValue = valueProviderResult.ConvertTo(typeof(int));

            Assert.Equal(0, convertedValue);
        }

        [Fact]
        public void ConvertTo_PopulatesArray_WhenSingleNeedsToBeConvertedToArray()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(DayOfWeek.Friday, "It's Friday!", CultureInfo.InvariantCulture);

            // Act
            var convertedValue = (DayOfWeek[])valueProviderResult.ConvertTo(typeof(DayOfWeek[]));

            // Assert
            Assert.Single(convertedValue, DayOfWeek.Friday);
        }

        [Fact]
        public void ConvertTo_PopulatesArray_WhenSourceArrayNeedsToBeConvertedToArray()
        {
            // Arrange
            string[] values = new[] { "3", "2", "1" };
            var valueProviderResult = new ValueProviderResult(values, values.ToString(), CultureInfo.InvariantCulture);

            // Act
            var convertedValue = valueProviderResult.ConvertTo(typeof(int[]));

            // Assert
            Assert.Equal(new[] { 3, 2, 1 }, convertedValue);
        }

        [Fact]
        public void ConvertTo_PopulatesArray_WhenListNeedsToBeConvertedToArray()
        {
            // Arrange
            List<string> values = new List<string> { "-1", "0", "1" };
            var valueProviderResult = new ValueProviderResult(values, values.ToString(), CultureInfo.InvariantCulture);

            // Act
            var convertedValue = valueProviderResult.ConvertTo(typeof(int[]));

            // Assert
            Assert.IsType<int[]>(convertedValue);
            Assert.Equal(new[] { -1, 0, 1 }, convertedValue);
        }

        public static TheoryDataSet<IList, object> SingleValueBoundToArrayData
        {
            get
            {
                return new TheoryDataSet<IList, object>
                {
                    { new List<string> { "Foo", "Bar" }, "Foo" },
                    { new string[] { "baz", "qux" }, "baz" },
                    { new List<int> { -17, 34 }, -17 },
                    { new string[] { "30", "15" }, 30 }
                };
            }
        }

        [Theory]
        [PropertyData("SingleValueBoundToArrayData")]
        public void ConvertTo_ReturnsFirstValue_WhenSequenceNeedsToConvertedToSingleValue(IList value, object expected)
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(value, value.ToString(), CultureInfo.InvariantCulture);

            // Act
            var convertedValue = valueProviderResult.ConvertTo(expected.GetType());

            // Assert
            Assert.Equal(expected, convertedValue);
        }
    }
}
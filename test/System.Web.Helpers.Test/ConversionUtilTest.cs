// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Drawing;
using System.Globalization;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class ConversionUtilTest
    {
        [Fact]
        public void ConversionUtilReturnsStringTypes()
        {
            // Arrange
            string original = "Foo";

            // Act
            object result;
            bool success = ConversionUtil.TryFromString(typeof(String), original, out result);

            // Assert
            Assert.True(success);
            Assert.Equal(original, result);
        }

        [Fact]
        public void ConversionUtilConvertsStringsToColor()
        {
            // Arrange
            string original = "Blue";

            // Act
            object result;
            bool success = ConversionUtil.TryFromString(typeof(Color), original, out result);

            // Assert
            Assert.True(success);
            Assert.Equal(Color.Blue, result);
        }

        [Fact]
        public void ConversionUtilConvertsEnumValues()
        {
            // Arrange
            string original = "Weekday";

            // Act
            object result;
            bool success = ConversionUtil.TryFromString(typeof(TestEnum), original, out result);

            // Assert
            Assert.True(success);
            Assert.Equal(TestEnum.Weekday, result);
        }

        [Fact]
        public void ConversionUtilUsesTypeConverterToConvertArbitraryTypes()
        {
            // Arrange
            var date = new DateTime(2010, 01, 01);
            string original = date.ToString(CultureInfo.InvariantCulture);

            // Act
            object result;
            bool success = ConversionUtil.TryFromString(typeof(DateTime), original, out result);

            // Assert
            Assert.True(success);
            Assert.Equal(date, result);
        }

        private enum TestEnum
        {
            Weekend,
            Weekday
        }
    }
}

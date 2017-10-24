// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class StringExtensionsTest
    {
        [Fact]
        public void IsIntTests()
        {
            Assert.False("1.3".IsInt());
            Assert.False(".13".IsInt());
            Assert.False("0.0".IsInt());
            Assert.False("12345678900123456".IsInt());
            Assert.False("gooblygook".IsInt());
            Assert.True("0".IsInt());
            Assert.True("123456".IsInt());
            Assert.True(Int32.MaxValue.ToString().IsInt());
            Assert.True(Int32.MinValue.ToString().IsInt());
            Assert.False(((string)null).IsInt());
        }

        [Fact]
        public void AsIntBasicTests()
        {
            Assert.Equal(-123, "-123".AsInt());
            Assert.Equal(12345, "12345".AsInt());
            Assert.Equal(0, "0".AsInt());
        }

        [Fact]
        public void AsIntDefaultTests()
        {
            // Illegal values default to 0
            Assert.Equal(0, "-100000000000000000000000".AsInt());

            // Illegal values default to 0
            Assert.Equal(0, "adlfkj".AsInt());

            Assert.Equal(-1, "adlfkj".AsInt(-1));
            Assert.Equal(-1, "-100000000000000000000000".AsInt(-1));
        }

        [Fact]
        public void IsDecimalTests()
        {
            Assert.True(1.3m.ToString("0.0").IsDecimal());
            Assert.True(0.13m.ToString(".00").IsDecimal());
            Assert.True(0m.ToString("0.0").IsDecimal());
            Assert.True("12345678900123456".IsDecimal());
            Assert.True("0".IsDecimal());
            Assert.True("123456".IsDecimal());
            Assert.True(decimal.MaxValue.ToString().IsDecimal());
            Assert.True(decimal.MinValue.ToString().IsDecimal());
            Assert.False("gooblygook".IsDecimal());
            Assert.False("..0".IsDecimal());
            Assert.False(((string)null).IsDecimal());
        }

        [Fact]
        public void AsDecimalBasicTests()
        {
            Assert.Equal(-123m, -123m.ToString().AsDecimal());
            Assert.Equal(9.99m, 9.99m.ToString().AsDecimal());
            Assert.Equal(0m, "0".AsDecimal());
            Assert.Equal(-1.1111m, -1.1111m.ToString().AsDecimal());
        }

        [Fact]
        public void AsDecimalDefaultTests()
        {
            // Illegal values default to 0
            Assert.Equal(0m, "abc".AsDecimal());

            Assert.Equal(-1.11m, "adlfkj".AsDecimal(-1.11m));
        }

        [Fact]
        public void AsDecimalUsesCurrentCulture()
        {
            decimal value = 12345.00M;
            using (new CultureReplacer("ar-DZ"))
            {
                Assert.Equal("12345.00", value.ToString(CultureInfo.CurrentCulture));
                Assert.Equal("12345.00", value.ToString());
            }

            using (new CultureReplacer("bg-BG"))
            {
                Assert.Equal("12345,00", value.ToString(CultureInfo.CurrentCulture));
                Assert.Equal("12345,00", value.ToString());
            }
        }

        [Fact]
        public void IsAndAsDecimalsUsesCurrentCulture()
        {
            // Pretty identical to the earlier test case. This was a post on the forums, making sure it works.
            using (new CultureReplacer(culture: "lt-LT"))
            {
                Assert.False("1.2".IsDecimal());
                Assert.True("1,2".IsDecimal());

                Assert.Equal(1.2M, "1,2".AsDecimal());
                Assert.Equal(0, "1.2".AsDecimal());
            }
        }

        [Fact]
        public void IsFloatTests()
        {
            Assert.True(1.3f.ToString("0.0").IsFloat());
            Assert.True(0.13f.ToString(".00").IsFloat());
            Assert.True(0f.ToString("0.0").IsFloat());
            Assert.True("12345678900123456".IsFloat());
            Assert.True("0".IsFloat());
            Assert.True("123456".IsFloat());
            Assert.True(float.MaxValue.ToString().IsFloat());
            Assert.True(float.MinValue.ToString().IsFloat());
            Assert.True(float.NegativeInfinity.ToString().IsFloat());
            Assert.True(float.PositiveInfinity.ToString().IsFloat());
            Assert.False("gooblygook".IsFloat());
            Assert.False(((string)null).IsFloat());
        }

        [Fact]
        public void AsFloatBasicTests()
        {
            Assert.Equal(-123f, -123f.ToString().AsFloat());
            Assert.Equal(9.99f, 9.99f.ToString().AsFloat());
            Assert.Equal(0f, "0".AsFloat());
            Assert.Equal(-1.1111f, -1.1111f.ToString().AsFloat());
        }

        [Fact]
        public void AsFloatDefaultTests()
        {
            // Illegal values default to 0
            Assert.Equal(0f, "abc".AsFloat());

            Assert.Equal(-1.11f, "adlfkj".AsFloat(-1.11f));
        }

        [Fact]
        public void IsDateTimeTests()
        {
            using (new CultureReplacer())
            {
                Assert.True("Sat, 01 Nov 2008 19:35:00 GMT".IsDateTime());
                Assert.True("1/5/1979".IsDateTime());
                Assert.False("0".IsDateTime());
                Assert.True(DateTime.MaxValue.ToString().IsDateTime());
                Assert.True(DateTime.MinValue.ToString().IsDateTime());
                Assert.True(new DateTime(2010, 12, 21).ToUniversalTime().ToString().IsDateTime());
                Assert.False("gooblygook".IsDateTime());
                Assert.False(((string)null).IsDateTime());
            }
        }

        /// <remarks>Tests for bug 153439</remarks>
        [Fact]
        public void IsDateTimeUsesLocalCulture()
        {
            using (new CultureReplacer(culture: "en-gb"))
            {
                Assert.True(new DateTime(2010, 12, 21).ToString().IsDateTime());
                Assert.True(new DateTime(2010, 12, 11).ToString().IsDateTime());
                Assert.True("2010/01/01".IsDateTime());
                Assert.True("12/01/2010".IsDateTime());
                Assert.True("12/12/2010".IsDateTime());
                Assert.True("13/12/2010".IsDateTime());
                Assert.True("2010-12-01".IsDateTime());
                Assert.True("2010-12-13".IsDateTime());

                Assert.False("12/13/2010".IsDateTime());
                Assert.False("13/13/2010".IsDateTime());
                Assert.False("2010-13-12".IsDateTime());
            }
        }

        [Fact]
        public void AsDateTimeBasicTests()
        {
            using (new CultureReplacer("en-US"))
            {
                Assert.Equal(DateTime.Parse("1/14/1979"), "1/14/1979".AsDateTime());
                Assert.Equal(DateTime.Parse("Sat, 01 Nov 2008 19:35:00 GMT"), "Sat, 01 Nov 2008 19:35:00 GMT".AsDateTime());
            }
        }

        [Theory]
        [InlineData(new object[] { "en-us" })]
        [InlineData(new object[] { "en-gb" })]
        [InlineData(new object[] { "ug" })]
        [InlineData(new object[] { "lt-LT" })]
        public void AsDateTimeDefaultTests(string culture)
        {
            using (new CultureReplacer(culture))
            {
                // Illegal values default to MinTime
                Assert.Equal(DateTime.MinValue, "1".AsDateTime());

                DateTime defaultV = new DateTime(1979, 01, 05);
                Assert.Equal(defaultV, "adlfkj".AsDateTime(defaultV));
                Assert.Equal(defaultV, "Jn 69".AsDateTime(defaultV));
            }
        }

        [Theory]
        [InlineData(new object[] { "en-us" })]
        [InlineData(new object[] { "en-gb" })]
        [InlineData(new object[] { "lt-LT" })]
        public void IsDateTimeDefaultTests(string culture)
        {
            using (new CultureReplacer(culture))
            {
                var dateTime = new DateTime(2011, 10, 25, 10, 10, 00);
                Assert.True(dateTime.ToShortDateString().IsDateTime());
                Assert.True(dateTime.ToString().IsDateTime());
                Assert.True(dateTime.ToLongDateString().IsDateTime());
            }
        }

        [Fact]
        public void IsBoolTests()
        {
            Assert.True("TRUE".IsBool());
            Assert.True("TRUE   ".IsBool());
            Assert.True("false".IsBool());
            Assert.False("falsey".IsBool());
            Assert.False("gooblygook".IsBool());
            Assert.False("".IsBool());
            Assert.False(((string)null).IsBool());
        }

        [Fact]
        public void AsBoolTests()
        {
            Assert.True("TRuE".AsBool());
            Assert.False("False".AsBool());
            Assert.False("Die".AsBool(false));
            Assert.True("true!".AsBool(true));
            Assert.False("".AsBool());
            Assert.False(((string)null).AsBool());
            Assert.True("".AsBool(true));
            Assert.True(((string)null).AsBool(true));
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Microsoft.TestCommon;

namespace System.Net.Http.Headers
{
    public class CookieHeaderValueTest
    {
        public static TheoryDataSet<CookieHeaderValue, string> CookieHeaderDataSet
        {
            get
            {
                var dataset = new TheoryDataSet<CookieHeaderValue, string>();
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("n1", "v1");
                nvc.Add("n2", "v2");
                nvc.Add("n3", "v3");
                CookieHeaderValue header1 = new CookieHeaderValue("name1", nvc)
                {
                    Domain = "domain1",
                    Expires = new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero),
                    HttpOnly = true,
                    MaxAge = TimeSpan.FromDays(1),
                    Path = "path1",
                    Secure = true
                };
                dataset.Add(header1, "name1=n1=v1&n2=v2&n3=v3; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; httponly");

                CookieHeaderValue header2 = new CookieHeaderValue("name2", "");
                dataset.Add(header2, "name2=");

                CookieHeaderValue header3 = new CookieHeaderValue("name2", "value2");
                dataset.Add(header3, "name2=value2");

                CookieHeaderValue header4 = new CookieHeaderValue("name4", "value4")
                {
                    MaxAge = TimeSpan.FromDays(1),
                };
                dataset.Add(header4, "name4=value4; max-age=86400");

                CookieHeaderValue header5 = new CookieHeaderValue("name5", "value5")
                {
                    Domain = "domain1",
                    Expires = new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero),
                };
                dataset.Add(header5, "name5=value5; expires=Sun, 06 Nov 1994 08:49:37 GMT; domain=domain1");

                return dataset;
            }
        }

        public static TheoryDataSet<string> CookieHeaderDataSet_NoCookie
        {
            get
            {
                var dataSet = new TheoryDataSet<string>();
                foreach (var item in CookieHeaderDataSet)
                {
                    dataSet.Add((string)item[1]);
                }

                return dataSet;
            }
        }

        public static TheoryDataSet<string> InvalidCookieHeaderDataSet
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1",
                    "name=value; expires=Sun, 06 Nov 1994 08:49:37 ZZZ; max-age=86400; domain=domain1",
                    "name=value; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=-86400; domain=domain1",
                };
            }
        }

        [Fact]
        public void CookieHeaderValue_Ctor1_InitializesCorrectly()
        {
            CookieHeaderValue header = new CookieHeaderValue("cookie", "value");
            CookieState cookie = Assert.Single(header.Cookies);
            Assert.Equal("cookie", cookie.Name);
            Assert.Equal("value", cookie.Values.AllKeys[0]);
        }

        [Fact]
        public void CookieHeaderValue_Ctor2_InitializesCorrectly()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("name", "value");
            CookieHeaderValue header = new CookieHeaderValue("cookie", nvc);
            CookieState cookie = Assert.Single(header.Cookies);
            Assert.Equal("cookie", cookie.Name);
            Assert.Equal("name", cookie.Values.AllKeys[0]);
            Assert.Equal("value", cookie.Values["name"]);
        }

        [Fact]
        public void CookieHeaderValue_Clone()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("name", "value");
            CookieHeaderValue expectedValue = new CookieHeaderValue("cookie", nvc);
            expectedValue.Domain = "domain";
            expectedValue.Expires = DateTimeOffset.Now;
            expectedValue.MaxAge = TimeSpan.FromDays(10);
            expectedValue.Path = "path";
            expectedValue.HttpOnly = true;
            expectedValue.Secure = true;

            // Act
            CookieHeaderValue actualValue = expectedValue.Clone() as CookieHeaderValue;

            // Assert
            Assert.Equal(expectedValue.ToString(), actualValue.ToString());
        }

        [Theory]
        [PropertyData("CookieHeaderDataSet")]
        public void CookieHeaderValue_ToString(CookieHeaderValue input, string expectedValue)
        {
            Assert.Equal(expectedValue, input.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CookieHeaderValue_GetItem_ReturnsNullIfNameIsNullOrEmpty(string name)
        {
            CookieHeaderValue header = new CookieHeaderValue("cookie", "value");
            Assert.Null(header[name]);
        }

        [Fact]
        public void CookieHeaderValue_GetItem_ReturnsCookie()
        {
            CookieHeaderValue header = new CookieHeaderValue("cookie", "value");

            Assert.Single(header.Cookies);
            CookieState state = header["cookie"];
            Assert.Equal("value", state.Value);
        }

        [Fact]
        public void CookieHeaderValue_GetItem_CreatesEmptyCookieIfNotAlreadyPresent()
        {
            CookieHeaderValue header = new CookieHeaderValue("cookie", "value");

            CookieState state = header["newstate"];
            Assert.Equal(2, header.Cookies.Count);
            Assert.Equal(String.Empty, state.Value);
        }

        [Theory]
        [PropertyData("CookieHeaderDataSet_NoCookie")]
        public void CookieHeaderValue_TryParse_AcceptsValidValues(string expectedValue)
        {
            CookieHeaderValue header;
            bool result = CookieHeaderValue.TryParse(expectedValue, out header);

            Assert.True(result);
            Assert.Equal(expectedValue, header.ToString());
        }

        [Theory]
        [PropertyData("InvalidCookieHeaderDataSet")]
        public void CookieHeaderValue_TryParse_RejectsInvalidValues(string value)
        {
            CookieHeaderValue header;
            bool result = CookieHeaderValue.TryParse(value, out header);

            Assert.False(result);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpResponseHeadersExtensionsTest
    {
        [Fact]
        public void AddCookies_ThrowsOnNull()
        {
            HttpResponseHeaders headers = CreateHttpResponseHeaders();
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();

            Assert.ThrowsArgumentNull(() => HttpResponseHeadersExtensions.AddCookies(null, cookies), "headers");
            Assert.ThrowsArgumentNull(() => HttpResponseHeadersExtensions.AddCookies(headers, null), "cookies");
        }

        [Fact]
        public void AddCookies_ThrowsOnNullCookie()
        {
            HttpResponseHeaders headers = CreateHttpResponseHeaders();
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();
            cookies.Add(null);

            Assert.ThrowsArgument(() => HttpResponseHeadersExtensions.AddCookies(headers, cookies), "cookies");
        }

        [Theory]
        [InlineData("name1=n1=v1&n2=v2&n3=v3; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; httponly")]
        public void AddCookies_AddsCookies(string expectedCookie)
        {
            // Arrange
            HttpResponseHeaders headers = CreateHttpResponseHeaders();
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();
            CookieHeaderValue cookie;
            bool parsedCorrectly = CookieHeaderValue.TryParse(expectedCookie, out cookie);
            cookies.Add(cookie);

            // Act
            headers.AddCookies(cookies);

            // Assert
            Assert.True(parsedCorrectly);
            IEnumerable<string> actualCookies;
            bool addedCorrectly = headers.TryGetValues("Set-Cookie", out actualCookies);
            Assert.True(addedCorrectly);
            string actualCookie = Assert.Single(actualCookies);
            Assert.Equal(expectedCookie, actualCookie);
        }

        private static HttpResponseHeaders CreateHttpResponseHeaders()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            return response.Headers;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpRequestHeadersExtensionsTest
    {
        public static TheoryDataSet<string[], string, string[]> CookieMatches
        {
            get
            {
                // IEnumerable<string> inputCookies, string matchName, IEnumerable<string> expectedOuput
                return new TheoryDataSet<string[], string, string[]>
                {
                    {
                        new string[] {},
                        "empty",
                        new string[] {}
                    },
                    {
                        new string[]
                        {
                            "RMID=2dab5fc9747d4f8edaf410ff",
                            "adxcs=-",
                            "adxcl=l*2ba62=4fc449bf:1",
                            "adxcs=si=0:1",
                        },
                        "nomatch",
                        new string[] {}
                    },
                    {
                        new string[]
                        {
                            "RMID=2dab5fc9747d4f8edaf410ff",
                            "adxcs=-",
                            "adxcl=l*2ba62=4fc449bf:1",
                            "ADXCS=si=0:1",
                        },
                        "adxcs",
                        new string[]
                        {
                            "adxcs=-",
                            "ADXCS=si=0%3a1"
                        }
                    },
                    {
                        new string[]
                        {
                            "MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!",
                            "MC0=1334766377159; MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!; MUID=20EC57A324256BF3039D54E520256B7D&TUID=1",
                        },
                        "A",
                        new string[]
                        {
                            "MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!",
                            "MC0=1334766377159; MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!; MUID=20EC57A324256BF3039D54E520256B7D&TUID=1",
                        }
                    },
                    {
                        new string[]
                        {
                            "MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!",
                            "MC0=1334766377159; MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!; MUID=20EC57A324256BF3039D54E520256B7D&TUID=1",
                        },
                        "MC0",
                        new string[]
                        {
                            "MC0=1334766377159; MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!; MUID=20EC57A324256BF3039D54E520256B7D&TUID=1",
                        }
                    },
                    {
                        new string[]
                        {
                            "MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!",
                            "MC0=1334766377159; MC1=GUID=e87574286c55d547b5a0b19fb27d57a4&HASH=2874&LV=20124&V=3&LU=1334766376863; MS0=7bbaad2a8316483c89bbd2ca4e96fcea; A=I&I=AxUFAAAAAACSCAAAHFNnP3xE7Uth5BCZZSiqZQ!!; MUID=20EC57A324256BF3039D54E520256B7D&TUID=1",
                        },
                        "MC",
                        new string[] { }
                    },
                };
            }
        }

        [Fact]
        public void GetCookies_ThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestHeadersExtensions.GetCookies(null), "headers");
        }

        [Fact]
        public void GetCookies_GetsCookiesReturnsEmptyCollection()
        {
            // Arrange
            HttpRequestHeaders headers = CreateHttpRequestHeaders();

            // Act
            IEnumerable<CookieHeaderValue> cookies = headers.GetCookies();

            // Assert
            Assert.Empty(cookies);
        }

        [Theory]
        [InlineData("name1=n1=v1&n2=v2&n3=v3; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; httponly")]
        public void GetCookies_GetsCookies(string expectedCookie)
        {
            // Arrange
            HttpRequestHeaders headers = CreateHttpRequestHeaders();
            headers.TryAddWithoutValidation("Cookie", expectedCookie);

            // Act
            IEnumerable<CookieHeaderValue> cookies = headers.GetCookies();

            // Assert
            CookieHeaderValue cookie = Assert.Single(cookies);
            string actualCookie = cookie.ToString();
            Assert.Equal(expectedCookie, actualCookie);
        }

        [Fact]
        public void GetCookiesByName_ThrowsOnNullHeaders()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestHeadersExtensions.GetCookies(null, "empty"), "headers");
        }

        [Fact]
        public void GetCookiesByName_ThrowsOnNullName()
        {
            HttpRequestHeaders headers = CreateHttpRequestHeaders();
            Assert.ThrowsArgumentNull(() => HttpRequestHeadersExtensions.GetCookies(headers, null), "name");
        }

        [Theory]
        [PropertyData("CookieMatches")]
        public void GetCookiesByName_GetsCookies(IEnumerable<string> cookies, string name, IEnumerable<string> expectedCookies)
        {
            // Arrange
            HttpRequestHeaders headers = CreateHttpRequestHeaders();
            foreach (string cookie in cookies)
            {
                headers.TryAddWithoutValidation("Cookie", cookie);
            }

            // Act
            IEnumerable<CookieHeaderValue> actualCookieHeaderValues = headers.GetCookies(name);

            // Assert
            IEnumerable<string> actualCookies = actualCookieHeaderValues.Select(c => c.ToString());
            Assert.True(actualCookies.SequenceEqual(expectedCookies));
        }

        private static HttpRequestHeaders CreateHttpRequestHeaders()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            return request.Headers;
        }
    }
}

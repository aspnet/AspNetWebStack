// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpContentMessageExtensionsTests
    {
        public static TheoryDataSet<IEnumerable<string>> ClientRoundTripData
        {
            get
            {
                return new TheoryDataSet<IEnumerable<string>>
                {
                    new string[]
                    {
                        @"GET / HTTP/1.1",
                        @"Accept: text/html, application/xhtml+xml, */*",
                        @"Accept-Language: en-US",
                        @"User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
                        @"Accept-Encoding: gzip, deflate",
                        @"Proxy-Connection: Keep-Alive",
                        @"Host: msdn.microsoft.com",
                        @"Cookie: omniID=1297715979621_9f45_1519_3f8a_f22f85346ac6; WT_FPC=id=65.55.227.138-2323234032.30136233:lv=1309374389020:ss=1309374389020; A=I&I=AxUFAAAAAACNCAAADYEZ7CFPss7Swnujy4PXZA!!&M=1&CS=126mAa0002ZB51a02gZB51a; MC1=GUID=568428660ad44d4ab8f46133f4b03738&HASH=6628&LV=20113&V=3; WT_NVR_RU=0=msdn:1=:2=; MUID=A44DE185EA1B4E8088CCF7B348C5D65F; MSID=Microsoft.CreationDate=03/04/2011 23:38:15&Microsoft.LastVisitDate=06/20/2011 04:15:08&Microsoft.VisitStartDate=06/20/2011 04:15:08&Microsoft.CookieId=f658f3f2-e6d6-42ab-b86b-96791b942b6f&Microsoft.TokenId=ffffffff-ffff-ffff-ffff-ffffffffffff&Microsoft.NumberOfVisits=106&Microsoft.CookieFirstVisit=1&Microsoft.IdentityToken=AA==&Microsoft.MicrosoftId=0441-6141-1523-9969; msresearch=%7B%22version%22%3A%224.6%22%2C%22state%22%3A%7B%22name%22%3A%22IDLE%22%2C%22url%22%3Aundefined%2C%22timestamp%22%3A1299281911415%7D%2C%22lastinvited%22%3A1299281911415%2C%22userid%22%3A%2212992819114151265672533023080%22%2C%22vendorid%22%3A1%2C%22surveys%22%3A%5Bundefined%5D%7D; CodeSnippetContainerLang=C#; msdn=L=1033; ADS=SN=175A21EF; s_cc=true; s_sq=%5B%5BB%5D%5D; TocHashCookie=ms310241(n)/aa187916(n)/aa187917(n)/dd273952(n)/dd295083(n)/ff472634(n)/ee667046(n)/ee667070(n)/gg259047(n)/gg618436(n)/; WT_NVR=0=/:1=query|library|en-us:2=en-us/vcsharp|en-us/library",
                        @"Cookie: omniID=1297715979621_9f45_1519_3f8a_f22f85346ac6; WT_FPC=id=65.55.227.138-2323234032.30136233:lv=1309374389020:ss=1309374389020; A=I&I=AxUFAAAAAACNCAAADYEZ7CFPss7Swnujy4PXZA!!&M=1&CS=126mAa0002ZB51a02gZB51a; MC1=GUID=568428660ad44d4ab8f46133f4b03738&HASH=6628&LV=20113&V=3; WT_NVR_RU=0=msdn:1=:2=; MUID=A44DE185EA1B4E8088CCF7B348C5D65F; MSID=Microsoft.CreationDate=03/04/2011 23:38:15&Microsoft.LastVisitDate=06/20/2011 04:15:08&Microsoft.VisitStartDate=06/20/2011 04:15:08&Microsoft.CookieId=f658f3f2-e6d6-42ab-b86b-96791b942b6f&Microsoft.TokenId=ffffffff-ffff-ffff-ffff-ffffffffffff&Microsoft.NumberOfVisits=106&Microsoft.CookieFirstVisit=1&Microsoft.IdentityToken=AA==&Microsoft.MicrosoftId=0441-6141-1523-9969; msresearch=%7B%22version%22%3A%224.6%22%2C%22state%22%3A%7B%22name%22%3A%22IDLE%22%2C%22url%22%3Aundefined%2C%22timestamp%22%3A1299281911415%7D%2C%22lastinvited%22%3A1299281911415%2C%22userid%22%3A%2212992819114151265672533023080%22%2C%22vendorid%22%3A1%2C%22surveys%22%3A%5Bundefined%5D%7D; CodeSnippetContainerLang=C#; msdn=L=1033; ADS=SN=175A21EF; s_cc=true; s_sq=%5B%5BB%5D%5D; TocHashCookie=ms310241(n)/aa187916(n)/aa187917(n)/dd273952(n)/dd295083(n)/ff472634(n)/ee667046(n)/ee667070(n)/gg259047(n)/gg618436(n)/; WT_NVR=0=/:1=query|library|en-us:2=en-us/vcsharp|en-us/library",
                    },
                    new string[]
                    {
                        @"GET / HTTP/1.1",
                        @"Host: msdn.microsoft.com",
                        @"Proxy-Connection: keep-alive",
                        @"User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:5.0) Gecko/20100101 Firefox/5.0",
                        @"Accept: text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8",
                        @"Accept-Language: en-us, en; q=0.5",
                        @"Accept-Encoding: gzip, deflate",
                        @"Accept-Charset: ISO-8859-1, utf-8; q=0.7, *; q=0.7",
                    },
                    new string[]
                    {
                        @"GET / HTTP/1.1",
                        @"Host: msdn.microsoft.com",
                        @"Proxy-Connection: keep-alive",
                        @"User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.30 (KHTML, like Gecko) Chrome/12.0.742.100 Safari/534.30",
                        @"Accept: text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8",
                        @"Accept-Encoding: gzip, deflate, sdch",
                        @"Accept-Language: en-US, en; q=0.8",
                        @"Accept-Charset: ISO-8859-1, utf-8; q=0.7, *; q=0.3",
                    },
                    new string[]
                    {
                        @"GET / HTTP/1.1",
                        @"Host: msdn.microsoft.com",
                        @"User-Agent: Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.21.1 (KHTML, like Gecko) Version/5.0.5 Safari/533.21.1",
                        @"Accept: application/xml, application/xhtml+xml, text/html; q=0.9, text/plain; q=0.8, image/png, */*; q=0.5",
                        @"Accept-Language: en-US",
                        @"Accept-Encoding: gzip, deflate",
                        @"Connection: keep-alive",
                        @"Proxy-Connection: keep-alive",
                    },
                    new string[]
                    {
                        @"GET / HTTP/1.0",
                        @"User-Agent: Opera/9.80 (Windows NT 6.1; U; en) Presto/2.8.131 Version/11.11",
                        @"Host: msdn.microsoft.com",
                        @"Accept: text/html, application/xml; q=0.9, application/xhtml+xml, image/png, image/webp, image/jpeg, image/gif, image/x-xbitmap, */*; q=0.1",
                        @"Accept-Language: en-US, en; q=0.9",
                        @"Accept-Encoding: gzip, deflate",
                        @"Proxy-Connection: Keep-Alive",
                    },
                };
            }
        }

        public static TheoryDataSet<IEnumerable<string>> ServerRoundTripData
        {
            get
            {
                return new TheoryDataSet<IEnumerable<string>>
                {
                    new string[]
                    {
                        @"HTTP/1.1 200 OK",
                        @"Server: nginx",
                        @"Date: Mon, 26 Dec 2011 16:33:07 GMT",
                        @"Connection: keep-alive",
                        @"Set-Cookie: CG=US:WA:Bellevue; path=/",
                        @"Vary: Accept-Encoding, User-Agent",
                        @"Cache-Control: max-age=60, private",
                        @"Content-Length: 124",
                        @"Content-Type: text/html; charset=UTF-8",
                    },
                    new string[]
                    {
                        @"HTTP/1.1 302 Found",
                        @"Proxy-Connection: Keep-Alive",
                        @"Connection: Keep-Alive",
                        @"Via: 1.1 RED-PRXY-23",
                        @"Date: Thu, 30 Jun 2011 00:16:35 GMT",
                        @"Location: /en-us/",
                        @"Server: Microsoft-IIS/7.5",
                        @"Cache-Control: private",
                        @"P3P: CP=""ALL IND DSP COR ADM CONo CUR CUSo IVAo IVDo PSA PSD TAI TELo OUR SAMo CNT COM INT NAV ONL PHY PRE PUR UNI"", CP=""ALL IND DSP COR ADM CONo CUR CUSo IVAo IVDo PSA PSD TAI TELo OUR SAMo CNT COM INT NAV ONL PHY PRE PUR UNI""",
                        @"Set-Cookie: A=I&I=AxUFAAAAAAD7BwAA8Jx0njhGoW3MGASDmzeaGw!!&M=1; domain=.microsoft.com; expires=Sun, 30-Jun-2041 00:16:35 GMT; path=/",
                        @"Set-Cookie: ADS=SN=175A21EF; domain=.microsoft.com; path=/",
                        @"Set-Cookie: Sto.UserLocale=en-us; path=/",
                        @"Set-Cookie: A=I&I=AxUFAAAAAAD7BwAA8Jx0njhGoW3MGASDmzeaGw!!&M=1; domain=.microsoft.com; expires=Sun, 30-Jun-2041 00:16:35 GMT; path=/; path=/",
                        @"Set-Cookie: ADS=SN=175A21EF; domain=.microsoft.com; path=/; path=/",
                        @"X-AspNetMvc-Version: 3.0",
                        @"X-AspNet-Version: 4.0.30319",
                        @"X-Powered-By: ASP.NET",
                        @"X-Powered-By: ASP.NET",
                        @"Content-Length: 124",
                        @"Content-Type: text/html; charset=utf-8",
                    },
                    new string[]
                    {
                        @"HTTP/1.1 200 OK",
                        @"Proxy-Connection: Keep-Alive",
                        @"Connection: Keep-Alive",
                        @"Transfer-Encoding: chunked",
                        @"Via: 1.1 RED-PRXY-07",
                        @"Date: Mon, 26 Dec 2011 19:11:47 GMT",
                        @"Server: gws",
                        @"Cache-Control: max-age=0, private",
                        @"Set-Cookie: PREF=ID=e91cfd77b562e989:FF=0:TM=1324926707:LM=1324926707:S=4w8_eSySJPXCCjhT; expires=Wed, 25-Dec-2013 19:11:47 GMT; path=/; domain=.google.com",
                        @"Set-Cookie: NID=54=bSMpxl0q0MVlvG-eZYSBtQuYTF1clqrA-TSIZT8wZcbhrrsdkP9G5zPiXGSBmiNu656QR3xfTXKUPkP-HqY_nSnsjj1fb-ipoZ3DUcyXb9oS9_8tjz3NZ3A44GPCmRPx; expires=Tue, 26-Jun-2012 19:11:47 GMT; path=/; domain=.google.com; HttpOnly",
                        @"P3P: CP=""This is not a P3P policy! See http://www.google.com/support/accounts/bin/answer.py?hl=en&answer=151657 for more info.""",
                        @"X-XSS-Protection: 1; mode=block",
                        @"X-Frame-Options: SAMEORIGIN",
                        @"Expires: -1",
                        @"Content-Type: text/html; charset=ISO-8859-1",
                    }
                };
            }
        }

        public static TheoryDataSet<HttpContent> NotHttpMessageContent
        {
            get
            {
                return new TheoryDataSet<HttpContent>
                {
                    new ByteArrayContent(new byte[] { }),
                    new StringContent(String.Empty),
                    new StringContent(String.Empty, Encoding.UTF8, "application/http"),
                };
            }
        }

        private static HttpContent CreateContent(bool isRequest, bool hasEntity)
        {
            string message;
            if (isRequest)
            {
                message = hasEntity ? ParserData.HttpRequestWithEntity : ParserData.HttpRequest;
            }
            else
            {
                message = hasEntity ? ParserData.HttpResponseWithEntity : ParserData.HttpResponse;
            }

            StringContent content = new StringContent(message);
            content.Headers.ContentType = isRequest ? ParserData.HttpRequestMediaType : ParserData.HttpResponseMediaType;
            return content;
        }

        private static HttpContent CreateContent(bool isRequest, IEnumerable<string> header, string body)
        {
            StringBuilder message = new StringBuilder();
            foreach (string h in header)
            {
                message.Append(h);
                message.Append("\r\n");
            }

            message.Append("\r\n");
            if (body != null)
            {
                message.Append(body);
            }

            StringContent content = new StringContent(message.ToString());
            content.Headers.ContentType = isRequest ? ParserData.HttpRequestMediaType : ParserData.HttpResponseMediaType;
            return content;
        }

        private static async Task ValidateEntityAsync(HttpContent content)
        {
            Assert.NotNull(content);
            Assert.Equal(ParserData.TextContentType, content.Headers.ContentType.ToString());
            string entity = await content.ReadAsStringAsync();
            Assert.Equal(ParserData.HttpMessageEntity, entity);
        }

        private static async Task ValidateRequestMessageAsync(HttpRequestMessage request, bool hasEntity)
        {
            Assert.NotNull(request);
            Assert.Equal(Version.Parse("1.2"), request.Version);
            Assert.Equal(ParserData.HttpMethod, request.Method.ToString());
            Assert.Equal(ParserData.HttpRequestUri, request.RequestUri);
            Assert.Equal(ParserData.HttpHostName, request.Headers.Host);
            Assert.True(request.Headers.Contains("N1"), "request did not contain expected N1 header.");
            Assert.True(request.Headers.Contains("N2"), "request did not contain expected N2 header.");

            if (hasEntity)
            {
                await ValidateEntityAsync(request.Content);
            }
        }

        private static async Task ValidateResponseMessageAsync(HttpResponseMessage response, bool hasEntity)
        {
            Assert.NotNull(response);
            Assert.Equal(new Version("1.2"), response.Version);
            Assert.Equal(ParserData.HttpStatus, response.StatusCode);
            Assert.Equal(ParserData.HttpReasonPhrase, response.ReasonPhrase);
            Assert.True(response.Headers.Contains("N1"), "Response did not contain expected N1 header.");
            Assert.True(response.Headers.Contains("N2"), "Response did not contain expected N2 header.");

            if (hasEntity)
            {
                await ValidateEntityAsync(response.Content);
            }
        }

        [Fact]
        public void ReadAsHttpRequestMessageAsync_VerifyArguments()
        {
            Assert.ThrowsArgumentNull(() => HttpContentMessageExtensions.ReadAsHttpRequestMessageAsync(null), "content");
            Assert.ThrowsArgument(() => new ByteArrayContent(new byte[] { }).ReadAsHttpRequestMessageAsync(), "content");
            Assert.ThrowsArgument(() => new StringContent(String.Empty).ReadAsHttpRequestMessageAsync(), "content");
            Assert.ThrowsArgument(() => new StringContent(String.Empty, Encoding.UTF8, "application/http").ReadAsHttpRequestMessageAsync(), "content");

            Assert.ThrowsArgument(() =>
            {
                HttpContent content = new StringContent(String.Empty);
                content.Headers.ContentType = ParserData.HttpResponseMediaType;
                content.ReadAsHttpRequestMessageAsync();
            }, "content");

            Assert.ThrowsArgumentNull(() =>
            {
                HttpContent content = new StringContent(String.Empty);
                content.Headers.ContentType = ParserData.HttpRequestMediaType;
                content.ReadAsHttpRequestMessageAsync(null);
            }, "uriScheme");

            Assert.ThrowsArgument(() =>
            {
                HttpContent content = new StringContent(String.Empty);
                content.Headers.ContentType = ParserData.HttpRequestMediaType;
                content.ReadAsHttpRequestMessageAsync("i n v a l i d");
            }, "uriScheme");

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() =>
            {
                HttpContent content = new StringContent(String.Empty);
                content.Headers.ContentType = ParserData.HttpRequestMediaType;
                content.ReadAsHttpRequestMessageAsync(Uri.UriSchemeHttp, ParserData.MinBufferSize - 1);
            }, "bufferSize", ParserData.MinBufferSize.ToString(), ParserData.MinBufferSize - 1);
        }

        [Fact]
        public void ReadAsHttpResponseMessageAsync_VerifyArguments()
        {
            Assert.ThrowsArgumentNull(() => HttpContentMessageExtensions.ReadAsHttpResponseMessageAsync(null), "content");
            Assert.ThrowsArgument(() => new ByteArrayContent(new byte[] { }).ReadAsHttpResponseMessageAsync(), "content");
            Assert.ThrowsArgument(() => new StringContent(String.Empty).ReadAsHttpResponseMessageAsync(), "content");
            Assert.ThrowsArgument(() => new StringContent(String.Empty, Encoding.UTF8, "application/http").ReadAsHttpResponseMessageAsync(), "content");

            Assert.ThrowsArgument(() =>
            {
                HttpContent content = new StringContent(String.Empty);
                content.Headers.ContentType = ParserData.HttpRequestMediaType;
                content.ReadAsHttpResponseMessageAsync();
            }, "content");

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() =>
            {
                HttpContent content = new StringContent(String.Empty);
                content.Headers.ContentType = ParserData.HttpResponseMediaType;
                content.ReadAsHttpResponseMessageAsync(ParserData.MinBufferSize - 1);
            }, "bufferSize", ParserData.MinBufferSize.ToString(), ParserData.MinBufferSize - 1);
        }

        [Fact]
        public void IsHttpRequestMessageContentVerifyArguments()
        {
            Assert.ThrowsArgumentNull(() => HttpContentMessageExtensions.IsHttpRequestMessageContent(null), "content");
        }

        [Fact]
        public void IsHttpResponseMessageContentVerifyArguments()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                HttpContent content = null;
                HttpContentMessageExtensions.IsHttpResponseMessageContent(content);
            }, "content");
        }

        [Theory]
        [PropertyData("NotHttpMessageContent")]
        public void IsHttpRequestMessageContentRespondsFalse(HttpContent notHttpMessageContent)
        {
            Assert.False(notHttpMessageContent.IsHttpRequestMessageContent());
        }

        [Fact]
        public void IsHttpRequestMessageContentRespondsTrue()
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = ParserData.HttpRequestMediaType;
            Assert.True(content.IsHttpRequestMessageContent(), "Content should be HTTP request.");
        }

        [Theory]
        [PropertyData("NotHttpMessageContent")]
        public void IsHttpResponseMessageContent(HttpContent notHttpMessageContent)
        {
            Assert.False(notHttpMessageContent.IsHttpResponseMessageContent());

        }

        [Fact]
        public void IsHttpResponseMessageContentRespondsTrue()
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = ParserData.HttpResponseMediaType;
            Assert.True(content.IsHttpResponseMessageContent(), "Content should be HTTP response.");
        }

        [Fact]
        public async Task ReadAsHttpRequestMessageAsync_RequestWithoutEntity_ShouldReturnHttpRequestMessage()
        {
            HttpContent content = CreateContent(isRequest: true, hasEntity: false);
            HttpRequestMessage httpRequest = await content.ReadAsHttpRequestMessageAsync();
            await ValidateRequestMessageAsync(httpRequest, hasEntity: false);
        }

        [Fact]
        public async Task ReadAsHttpRequestMessageAsync_RequestWithEntity_ShouldReturnHttpRequestMessage()
        {
            HttpContent content = CreateContent(isRequest: true, hasEntity: true);
            HttpRequestMessage httpRequest = await content.ReadAsHttpRequestMessageAsync();
            await ValidateRequestMessageAsync(httpRequest, hasEntity: true);
        }

        [Fact]
        public async Task ReadAsHttpRequestMessageAsync_WithHttpsUriScheme_ReturnsUriWithHttps()
        {
            HttpContent content = CreateContent(isRequest: true, hasEntity: true);
            HttpRequestMessage httpRequest = await content.ReadAsHttpRequestMessageAsync(Uri.UriSchemeHttps);
            Assert.Equal(ParserData.HttpsRequestUri, httpRequest.RequestUri);
        }

        [Fact]
        public async Task ReadAsHttpResponseMessageAsync_ResponseWithoutEntity_ShouldReturnHttpResponseMessage()
        {
            HttpContent content = CreateContent(isRequest: false, hasEntity: false);
            HttpResponseMessage httpResponse = await content.ReadAsHttpResponseMessageAsync();
            await ValidateResponseMessageAsync(httpResponse, hasEntity: false);
        }

        [Fact]
        public async Task ReadAsHttpResponseMessageAsync_ResponseWithEntity_ShouldReturnHttpResponseMessage()
        {
            HttpContent content = CreateContent(isRequest: false, hasEntity: true);
            HttpResponseMessage httpResponse = await content.ReadAsHttpResponseMessageAsync();
            await ValidateResponseMessageAsync(httpResponse, hasEntity: true);
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_NoHostHeader_Throws()
        {
            string[] request = new[] {
                @"GET / HTTP/1.1",
            };

            HttpContent content = CreateContent(true, request, null);
            return Assert.ThrowsAsync<InvalidOperationException>(() => content.ReadAsHttpRequestMessageAsync());
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_TwoHostHeaders_Throws()
        {
            string[] request = new[] {
                @"GET / HTTP/1.1",
                @"Host: somehost.com",
                @"Host: otherhost.com",
            };

            HttpContent content = CreateContent(true, request, null);
            return Assert.ThrowsAsync<InvalidOperationException>(() => content.ReadAsHttpRequestMessageAsync());
        }

        [Fact]
        public async Task ReadAsHttpRequestMessageAsync_SortHeaders()
        {
            string[] request = new[] {
                @"GET / HTTP/1.1",
                @"Host: somehost.com",
                @"Content-Language: xx",
                @"Request-Header: zz",
            };

            HttpContent content = CreateContent(true, request, "sample body");
            HttpRequestMessage httpRequest = await content.ReadAsHttpRequestMessageAsync();
            Assert.Equal("xx", httpRequest.Content.Headers.ContentLanguage.ToString());

            IEnumerable<string> requestHeaderValues;
            Assert.True(httpRequest.Headers.TryGetValues("request-header", out requestHeaderValues));
            Assert.Equal("zz", requestHeaderValues.First());
        }

        [Fact]
        public async Task ReadAsHttpResponseMessageAsync_SortHeaders()
        {
            string[] response = new[] {
                @"HTTP/1.1 200 OK",
                @"Content-Language: xx",
                @"Response-Header: zz",
            };

            HttpContent content = CreateContent(false, response, "sample body");
            HttpResponseMessage httpResponse = await content.ReadAsHttpResponseMessageAsync();
            Assert.Equal("xx", httpResponse.Content.Headers.ContentLanguage.ToString());

            IEnumerable<string> ResponseHeaderValues;
            Assert.True(httpResponse.Headers.TryGetValues("Response-header", out ResponseHeaderValues));
            Assert.Equal("zz", ResponseHeaderValues.First());
        }

        [Fact]
        public Task ReadAsHttpResponseMessageAsync_Throws_TheHeaderSizeExceededTheDefaultLimit()
        {
            string[] response = new[] {
                @"HTTP/1.1 200 OK",
                String.Format("Set-Cookie: {0}={1}", new String('a', 16 * 1024), new String('b', 16 * 1024))
            };

            return Assert.ThrowsAsync<InvalidOperationException>(() =>
                {
                    HttpContent content = CreateContent(false, response, "sample body");
                    return content.ReadAsHttpResponseMessageAsync();
                });
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_Throws_TheHeaderSizeExceededTheDefaultLimit()
        {
            string[] request = new[] {
                @"GET / HTTP/1.1",
                @"Host: msdn.microsoft.com",
                String.Format("Cookie: {0}={1}", new String('a', 16 * 1024), new String('b', 16 * 1024))
            };

            return Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                HttpContent content = CreateContent(true, request, "sample body");
                return content.ReadAsHttpRequestMessageAsync();
            });
        }

        [Fact]
        public Task ReadAsHttpResponseMessageAsync_LargeHeaderSize()
        {
            string[] response = new[] {
                @"HTTP/1.1 200 OK",
                String.Format("Set-Cookie: {0}={1}", new String('a', 16 * 1024), new String('b', 16 * 1024))
            };

            HttpContent content = CreateContent(false, response, "sample body");
            return content.ReadAsHttpResponseMessageAsync(64 * 1024, 64 * 1024);
        }

        [Fact]
        public async Task ReadAsHttpRequestMessageAsync_LargeHeaderSize()
        {
            string cookieValue = string.Format("{0}={1}", new String('a', 16 * 1024), new String('b', 16 * 1024));
            string[] request = new[] {
                @"GET / HTTP/1.1",
                @"Host: msdn.microsoft.com",
                string.Format("Cookie: {0}", cookieValue),
            };

            HttpContent content = CreateContent(true, request, "sample body");
            var httpRequestMessage = await content.ReadAsHttpRequestMessageAsync(Uri.UriSchemeHttp, 64 * 1024, 64 * 1024);

            Assert.Equal(HttpMethod.Get, httpRequestMessage.Method);
            Assert.Equal("/", httpRequestMessage.RequestUri.PathAndQuery);
            Assert.Equal("msdn.microsoft.com", httpRequestMessage.Headers.Host);
            IEnumerable<string> actualCookieValue;
            Assert.True(httpRequestMessage.Headers.TryGetValues("Cookie", out actualCookieValue));
            Assert.Equal(cookieValue, Assert.Single(actualCookieValue));
        }

        [Fact]
        public async Task ReadAsHttpRequestMessageAsync_LargeHttpRequestLine()
        {
            string requestPath = string.Format("/myurl?{0}={1}", new string('a', 4 * 1024), new string('b', 4 * 1024));
            string cookieValue = string.Format("{0}={1}", new String('a', 4 * 1024), new String('b', 4 * 1024));
            string[] request = new[]
            {
                string.Format("GET {0} HTTP/1.1", requestPath),
                @"Host: msdn.microsoft.com",
                string.Format("Cookie: {0}", cookieValue),
            };

            HttpContent content = CreateContent(true, request, "sample body");
            var httpRequestMessage = await content.ReadAsHttpRequestMessageAsync(
                Uri.UriSchemeHttp,
                bufferSize: 64 * 1024,
                maxHeaderSize: 64 * 1024);

            Assert.Equal(HttpMethod.Get, httpRequestMessage.Method);
            Assert.Equal(requestPath, httpRequestMessage.RequestUri.PathAndQuery);
            Assert.Equal("msdn.microsoft.com", httpRequestMessage.Headers.Host);
            IEnumerable<string> actualCookieValue;
            Assert.True(httpRequestMessage.Headers.TryGetValues("Cookie", out actualCookieValue));
            Assert.Equal(cookieValue, Assert.Single(actualCookieValue));
        }

        [Theory]
        [InlineData("")]
        [InlineData("HTTP/1.1")]
        [InlineData("HTTP/1.1 200 OK")]
        [InlineData("HTTP/1.1 200 OK\r\n")]
        [InlineData("HTTP/1.1 200 OK\r\nServer:")]
        [InlineData("HTTP/1.1 200 OK\r\nServer: server")]
        [InlineData("HTTP/1.1 200 OK\r\nServer: server\r\n")]
        [InlineData("HTTP/1.1 200 OK\r\nServer: server\r\n\r")]
        public Task ReadAsHttpResponseMessageAsync_IncompleteResponse(string incompleteResponse)
        {
            StringContent content = new StringContent(incompleteResponse);
            content.Headers.ContentType = ParserData.HttpResponseMediaType;
            return Assert.ThrowsAsync<IOException>(() => content.ReadAsHttpResponseMessageAsync());
        }

        [Theory]
        [InlineData("")]
        [InlineData("GET")]
        [InlineData("GET / HTTP/1.0")]
        [InlineData("GET / HTTP/1.0\r\n")]
        [InlineData("GET / HTTP/1.0\r\nHost:")]
        [InlineData("GET / HTTP/1.0\r\nHost: localhost")]
        [InlineData("GET / HTTP/1.0\r\nHost: localhost\r\n")]
        [InlineData("GET / HTTP/1.0\r\nHost: localhost\r\n\r")]
        public Task ReadAsHttpRequestMessageAsync_IncompleteRequest(string incompleteRequest)
        {
            StringContent content = new StringContent(incompleteRequest);
            content.Headers.ContentType = ParserData.HttpRequestMediaType;
            return Assert.ThrowsAsync<IOException>(() => content.ReadAsHttpRequestMessageAsync());
        }

        [Theory]
        [PropertyData("ClientRoundTripData")]
        public async Task RoundtripClientRequest(IEnumerable<string> message)
        {
            HttpContent content = CreateContent(true, message, null);
            HttpRequestMessage httpRequest = await content.ReadAsHttpRequestMessageAsync();
            HttpMessageContent httpMessageContent = new HttpMessageContent(httpRequest);

            MemoryStream destination = new MemoryStream();
            await httpMessageContent.CopyToAsync(destination);
            destination.Seek(0, SeekOrigin.Begin);
            string destinationMessage = new StreamReader(destination).ReadToEnd();
            string sourceMessage = await content.ReadAsStringAsync();
            Assert.Equal(sourceMessage, destinationMessage);
        }

        [Theory]
        [PropertyData("ServerRoundTripData")]
        public async Task RoundtripServerResponse(IEnumerable<string> message)
        {
            HttpContent content = CreateContent(false, message, @"<html><head><title>Object moved</title></head><body><h2>Object moved to <a href=""/en-us/"">here</a>.</h2></body></html>");
            HttpResponseMessage httpResponse = await content.ReadAsHttpResponseMessageAsync();
            HttpMessageContent httpMessageContent = new HttpMessageContent(httpResponse);

            MemoryStream destination = new MemoryStream();
            await httpMessageContent.CopyToAsync(destination);
            destination.Seek(0, SeekOrigin.Begin);
            string destinationMessage = new StreamReader(destination).ReadToEnd();
            string sourceMessage = await content.ReadAsStringAsync();
            Assert.Equal(sourceMessage, destinationMessage);
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_cancellationToken_PassesCancellationToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = CreateContent(isRequest: true, hasEntity: false);

            return Assert.ThrowsAsync<OperationCanceledException>(() => content.ReadAsHttpRequestMessageAsync(cts.Token));
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_uriScheme_cancellationToken_PassesCancellationToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = CreateContent(isRequest: true, hasEntity: false);

            return Assert.ThrowsAsync<OperationCanceledException>(
                () => content.ReadAsHttpRequestMessageAsync("http", cts.Token));
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_uriScheme_bufferSize_cancellationToken_PassesCancellationToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = CreateContent(isRequest: true, hasEntity: false);

            return Assert.ThrowsAsync<OperationCanceledException>(
                () => content.ReadAsHttpRequestMessageAsync("http", 1024, cts.Token));
        }

        [Fact]
        public Task ReadAsHttpRequestMessageAsync_uriScheme_bufferSize_maxHeaderSize_cancellationToken_PassesCancellationToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = CreateContent(isRequest: true, hasEntity: false);

            return Assert.ThrowsAsync<OperationCanceledException>(
                () => content.ReadAsHttpRequestMessageAsync("http", 1024, 1024, cts.Token));
        }
    }
}

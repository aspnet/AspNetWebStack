// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Cors;
using Microsoft.TestCommon;

namespace System.Web.Http.Cors.Test
{
    public class CorsHttpRequestMessageExtensionsTest
    {
        [Fact]
        public void GetCorsRequestContext_NullRequestParam_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => CorsHttpRequestMessageExtensions.GetCorsRequestContext(null),
                "request");
        }

        [Fact]
        public void GetCorsRequestContext_NotOrigin_ReturnsNull()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            CorsRequestContext result = request.GetCorsRequestContext();
            Assert.Null(result);
        }

        [Fact]
        public void GetCorsRequestContext_CachesTheContext()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");
            request.Headers.Add("Host", "example.com");
            request.Headers.Add("Access-Control-Request-Method", "bar");

            CorsRequestContext result = request.GetCorsRequestContext();
            CorsRequestContext result2 = request.GetCorsRequestContext();

            Assert.Same(result, result2);
        }

        [Fact]
        public void GetCorsRequestContext_ReturnsHost()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");
            request.Headers.Add("Host", "example.com");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Equal("example.com", result.Host);
        }

        [Fact]
        public void GetCorsRequestContext_ReturnsHttpMethod()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://example.com/test");
            request.Headers.Add("Origin", "foo");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Equal("OPTIONS", result.HttpMethod);
        }

        [Fact]
        public void GetCorsRequestContext_ReturnsOrigin()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Equal("foo", result.Origin);
        }

        [Fact]
        public void GetCorsRequestContext_ReturnsRequestMethod()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");
            request.Headers.Add("Access-Control-Request-Method", "bar");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Equal("bar", result.AccessControlRequestMethod);
        }

        [Fact]
        public void GetCorsRequestContext_RetunsEmptyRequestHeaders()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Empty(result.AccessControlRequestHeaders);
        }

        [Fact]
        public void GetCorsRequestContext_RetunsRequestHeaders()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");
            request.Headers.Add("Access-Control-Request-Headers", "foo, bar");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Equal(2, result.AccessControlRequestHeaders.Count);
            Assert.Contains("foo", result.AccessControlRequestHeaders);
            Assert.Contains("bar", result.AccessControlRequestHeaders);
        }

        [Fact]
        public void GetCorsRequestContext_RetunsRequestHeadersFromMultipleAccessControlRequestHeaders()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/test");
            request.Headers.Add("Origin", "foo");
            request.Headers.Add("Access-Control-Request-Headers", "foo, bar");
            request.Headers.Add("Access-Control-Request-Headers", "extra,baz");

            CorsRequestContext result = request.GetCorsRequestContext();

            Assert.Equal(4, result.AccessControlRequestHeaders.Count);
            Assert.Contains("foo", result.AccessControlRequestHeaders);
            Assert.Contains("bar", result.AccessControlRequestHeaders);
            Assert.Contains("extra", result.AccessControlRequestHeaders);
            Assert.Contains("baz", result.AccessControlRequestHeaders);
        }

        [Fact]
        public void GetCorsRequestContext_ReturnsHttpRequestInThePropertiesCollection()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://example.com/test");
            request.Headers.Add("Origin", "foo");

            // Act 
            CorsRequestContext result = request.GetCorsRequestContext();

            // Assert
            object actualRequest;
            result.Properties.TryGetValue(typeof(HttpRequestMessage).FullName, out actualRequest);
            Assert.Equal(request, actualRequest);
        }
    }
}
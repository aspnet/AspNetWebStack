// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Cors.Test
{
    public class CorsRequestContextTest
    {
        [Fact]
        public void Default_Constructor()
        {
            CorsRequestContext requestContext = new CorsRequestContext();

            Assert.Null(requestContext.AccessControlRequestMethod);
            Assert.Null(requestContext.Host);
            Assert.Null(requestContext.HttpMethod);
            Assert.Null(requestContext.Origin);
            Assert.Null(requestContext.RequestUri);
            Assert.NotNull(requestContext.AccessControlRequestHeaders);
            Assert.False(requestContext.IsPreflight);
        }

        [Theory]
        [InlineData("OPTIONS", "POST", "foo")]
        [InlineData("options", "POST", "foo")]
        [InlineData("OPTIONS", "GET", "foo")]
        [InlineData("OPTIONS", "OPTIONS", "")]
        public void IsPreflight_ReturnsTrue(string httpMethod, string requestedMethod, string origin)
        {
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = httpMethod,
                AccessControlRequestMethod = requestedMethod,
                Origin = origin
            };

            Assert.True(requestContext.IsPreflight);
        }

        [Theory]
        [InlineData("OPTIONS", "POST", null)]
        [InlineData("options", "POST", null)]
        [InlineData("OPTIONS", null, "foo")]
        [InlineData(null, "POST", "foo")]
        [InlineData("POST", "GET", "bar")]
        public void IsPreflight_ReturnsFalse(string httpMethod, string requestedMethod, string origin)
        {
            CorsRequestContext requestContext = new CorsRequestContext()
            {
                HttpMethod = httpMethod,
                AccessControlRequestMethod = requestedMethod,
                Origin = origin
            };

            Assert.False(requestContext.IsPreflight);
        }

        [Fact]
        public void ToString_ReturnsThePropertyValues()
        {
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Host = "http://example.com",
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "DELETE",
                Origin = "http://localhost",
                RequestUri = new Uri("http://example.com")
            };
            requestContext.AccessControlRequestHeaders.Add("foo");
            requestContext.AccessControlRequestHeaders.Add("bar");

            Assert.Equal(@"Origin: http://localhost, HttpMethod: OPTIONS, IsPreflight: True, Host: http://example.com, AccessControlRequestMethod: DELETE, RequestUri: http://example.com/, AccessControlRequestHeaders: {foo,bar}", requestContext.ToString());
        }
    }
}
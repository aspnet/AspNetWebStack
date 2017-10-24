// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Cors;
using Microsoft.TestCommon;

namespace System.Web.Cors.Test.WebAPI
{
    public class CorsHttpResponseMessageExtensionsTest
    {
        [Fact]
        public void WriteCorsHeaders_NullResponse_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
                System.Web.Http.Cors.CorsHttpResponseMessageExtensions.WriteCorsHeaders(null, new CorsResult()),
                "response");
        }

        [Fact]
        public void WriteCorsHeaders_NullCorsResult_Throws()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            Assert.ThrowsArgumentNull(() =>
                response.WriteCorsHeaders(null),
                "corsResult");
        }

        [Fact]
        public void WriteCorsHeaders_EmptyCorsResult_EmptyHeaders()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.WriteCorsHeaders(new CorsResult());
            Assert.Empty(response.Headers);
        }

        [Fact]
        public void WriteCorsHeaders_WritesAllowMethods()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            CorsResult corsResult = new CorsResult();
            corsResult.AllowedMethods.Add("DELETE");
            corsResult.AllowedMethods.Add("PUT");

            response.WriteCorsHeaders(corsResult);
            HttpResponseHeaders headers = response.Headers;

            Assert.Single(headers);
            string[] allowMethods = headers.GetValues("Access-Control-Allow-Methods").FirstOrDefault().Split(',');
            Assert.Contains("DELETE", allowMethods);
            Assert.Contains("PUT", allowMethods);
        }

        [Fact]
        public void WriteCorsHeaders_WritesAllowExposedHeaders()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            CorsResult corsResult = new CorsResult();
            corsResult.AllowedExposedHeaders.Add("baz");

            response.WriteCorsHeaders(corsResult);
            HttpResponseHeaders headers = response.Headers;

            Assert.Single(headers);
            string[] exposedHeaders = headers.GetValues("Access-Control-Expose-Headers").FirstOrDefault().Split(',');
            Assert.Contains("baz", exposedHeaders);
        }

        [Fact]
        public void WriteCorsHeaders_WritesAllowHeaders()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            CorsResult corsResult = new CorsResult();
            corsResult.AllowedHeaders.Add("foo");
            corsResult.AllowedHeaders.Add("bar");

            response.WriteCorsHeaders(corsResult);
            HttpResponseHeaders headers = response.Headers;

            Assert.Single(headers);
            string[] allowHeaders = headers.GetValues("Access-Control-Allow-Headers").FirstOrDefault().Split(',');
            Assert.Contains("foo", allowHeaders);
            Assert.Contains("bar", allowHeaders);
        }

        [Fact]
        public void WriteCorsHeaders_WritesAllowCredentials()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            CorsResult corsResult = new CorsResult
            {
                SupportsCredentials = true
            };

            response.WriteCorsHeaders(corsResult);
            HttpResponseHeaders headers = response.Headers;

            Assert.Single(headers);
            Assert.Equal("true", headers.GetValues("Access-Control-Allow-Credentials").FirstOrDefault());
        }

        [Fact]
        public void WriteCorsHeaders_WritesAllowOrigin()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            CorsResult corsResult = new CorsResult
            {
                AllowedOrigin = "*"
            };

            response.WriteCorsHeaders(corsResult);
            HttpResponseHeaders headers = response.Headers;

            Assert.Single(headers);
            Assert.Equal("*", headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault());
        }

        [Fact]
        public void WriteCorsHeaders_WritesPreflightMaxAge()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            CorsResult corsResult = new CorsResult
            {
                PreflightMaxAge = 10
            };

            response.WriteCorsHeaders(corsResult);
            HttpResponseHeaders headers = response.Headers;

            Assert.Single(headers);
            Assert.Equal("10", headers.GetValues("Access-Control-Max-Age").FirstOrDefault());
        }
    }
}
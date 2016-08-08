// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Cors.Test
{
    public class CorsResultTest
    {
        [Fact]
        public void Default_Constructor()
        {
            CorsResult result = new CorsResult();

            Assert.Empty(result.AllowedHeaders);
            Assert.Empty(result.AllowedExposedHeaders);
            Assert.Empty(result.AllowedMethods);
            Assert.Empty(result.ErrorMessages);
            Assert.False(result.SupportsCredentials);
            Assert.Null(result.AllowedOrigin);
            Assert.Null(result.PreflightMaxAge);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void SettingNegativePreflightMaxAge_Throws()
        {
            CorsResult result = new CorsResult();
            Assert.ThrowsArgumentOutOfRange(() =>
            {
                result.PreflightMaxAge = -2;
            },
            "value",
            "PreflightMaxAge must be greater than or equal to 0.");
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenThereIsError()
        {
            CorsResult result = new CorsResult();
            result.ErrorMessages.Add("error");

            Assert.False(result.IsValid);
        }

        [Fact]
        public void ToResponseHeaders_ReturnsNoHeaders_ByDefault()
        {
            CorsResult result = new CorsResult();

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Empty(headers);
        }

        [Fact]
        public void ToResponseHeaders_AllowOrigin_AllowOriginHeaderAdded()
        {
            CorsResult result = new CorsResult
            {
                AllowedOrigin = "http://example.com"
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Equal("http://example.com", headers["Access-Control-Allow-Origin"]);
        }

        [Fact]
        public void ToResponseHeaders_NoAllowOrigin_AllowOriginHeaderNotAdded()
        {
            CorsResult result = new CorsResult
            {
                AllowedOrigin = null
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Allow-Origin", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_AllowCredentials_AllowCredentialsHeaderAdded()
        {
            CorsResult result = new CorsResult
            {
                SupportsCredentials = true
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Equal("true", headers["Access-Control-Allow-Credentials"]);
        }

        [Fact]
        public void ToResponseHeaders_NoAllowCredentials_AllowCredentialsHeaderNotAdded()
        {
            CorsResult result = new CorsResult
            {
                SupportsCredentials = false
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Allow-Credentials", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_NoAllowMethods_AllowMethodsHeaderNotAdded()
        {
            CorsResult result = new CorsResult
            {
                // AllowMethods is empty by default
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Allow-Methods", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_OneAllowMethods_AllowMethodsHeaderAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedMethods.Add("PUT");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Equal("PUT", headers["Access-Control-Allow-Methods"]);
        }

        [Fact]
        public void ToResponseHeaders_SomeSimpleAllowMethods_AllowMethodsHeaderAddedForNonSimpleMethods()
        {
            CorsResult result = new CorsResult();
            result.AllowedMethods.Add("PUT");
            result.AllowedMethods.Add("get");
            result.AllowedMethods.Add("DELETE");
            result.AllowedMethods.Add("POST");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Contains("Access-Control-Allow-Methods", headers.Keys);
            string[] methods = headers["Access-Control-Allow-Methods"].Split(',');
            Assert.Equal(2, methods.Length);
            Assert.Contains("PUT", methods);
            Assert.Contains("DELETE", methods);
        }

        [Fact]
        public void ToResponseHeaders_SimpleAllowMethods_AllowMethodsHeaderNotAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedMethods.Add("GET");
            result.AllowedMethods.Add("HEAD");
            result.AllowedMethods.Add("POST");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Allow-Methods", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_NoAllowHeaders_AllowHeadersHeaderNotAdded()
        {
            CorsResult result = new CorsResult
            {
                // AllowHeaders is empty by default
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Allow-Headers", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_OneAllowHeaders_AllowHeadersHeaderAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedHeaders.Add("foo");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Equal("foo", headers["Access-Control-Allow-Headers"]);
        }

        [Fact]
        public void ToResponseHeaders_ManyAllowHeaders_AllowHeadersHeaderAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedHeaders.Add("foo");
            result.AllowedHeaders.Add("bar");
            result.AllowedHeaders.Add("baz");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Contains("Access-Control-Allow-Headers", headers.Keys);
            string[] headerValues = headers["Access-Control-Allow-Headers"].Split(',');
            Assert.Equal(3, headerValues.Length);
            Assert.Contains("foo", headerValues);
            Assert.Contains("bar", headerValues);
            Assert.Contains("baz", headerValues);
        }

        [Fact]
        public void ToResponseHeaders_SomeSimpleAllowHeaders_AllowHeadersHeaderAddedForNonSimpleHeaders()
        {
            CorsResult result = new CorsResult();
            result.AllowedHeaders.Add("Content-Language");
            result.AllowedHeaders.Add("foo");
            result.AllowedHeaders.Add("bar");
            result.AllowedHeaders.Add("Accept");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Contains("Access-Control-Allow-Headers", headers.Keys);
            string[] headerValues = headers["Access-Control-Allow-Headers"].Split(',');
            Assert.Equal(2, headerValues.Length);
            Assert.Contains("foo", headerValues);
            Assert.Contains("bar", headerValues);
        }

        [Fact]
        public void ToResponseHeaders_SimpleAllowHeaders_AllowHeadersHeaderNotAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedHeaders.Add("Accept");
            result.AllowedHeaders.Add("Accept-Language");
            result.AllowedHeaders.Add("Content-Language");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Allow-Headers", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_NoAllowExposedHeaders_ExposedHeadersHeaderNotAdded()
        {
            CorsResult result = new CorsResult
            {
                // AllowExposedHeaders is empty by default
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Expose-Headers", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_OneAllowExposedHeaders_ExposedHeadersHeaderAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedExposedHeaders.Add("foo");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Equal("foo", headers["Access-Control-Expose-Headers"]);
        }

        [Fact]
        public void ToResponseHeaders_ManyAllowExposedHeaders_ExposedHeadersHeaderAdded()
        {
            CorsResult result = new CorsResult();
            result.AllowedExposedHeaders.Add("foo");
            result.AllowedExposedHeaders.Add("bar");
            result.AllowedExposedHeaders.Add("baz");

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Contains("Access-Control-Expose-Headers", headers.Keys);
            string[] exposedHeaderValues = headers["Access-Control-Expose-Headers"].Split(',');
            Assert.Equal(3, exposedHeaderValues.Length);
            Assert.Contains("foo", exposedHeaderValues);
            Assert.Contains("bar", exposedHeaderValues);
            Assert.Contains("baz", exposedHeaderValues);
        }

        [Fact]
        public void ToResponseHeaders_NoPreflightMaxAge_MaxAgeHeaderNotAdded()
        {
            CorsResult result = new CorsResult
            {
                PreflightMaxAge = null
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.DoesNotContain("Access-Control-Max-Age", headers.Keys);
        }

        [Fact]
        public void ToResponseHeaders_PreflightMaxAge_MaxAgeHeaderAdded()
        {
            CorsResult result = new CorsResult
            {
                PreflightMaxAge = 30
            };

            IDictionary<string, string> headers = result.ToResponseHeaders();

            Assert.Equal("30", headers["Access-Control-Max-Age"]);
        }

        [Fact]
        public void ToString_ReturnsThePropertyValues()
        {
            CorsResult corsResult = new CorsResult
            {
                SupportsCredentials = true,
                PreflightMaxAge = 20,
                AllowedOrigin = "*"
            };
            corsResult.AllowedExposedHeaders.Add("foo");
            corsResult.AllowedHeaders.Add("bar");
            corsResult.AllowedHeaders.Add("baz");
            corsResult.AllowedMethods.Add("GET");
            corsResult.ErrorMessages.Add("error1");
            corsResult.ErrorMessages.Add("error2");

            Assert.Equal(@"IsValid: False, AllowCredentials: True, PreflightMaxAge: 20, AllowOrigin: *, AllowExposedHeaders: {foo}, AllowHeaders: {bar,baz}, AllowMethods: {GET}, ErrorMessages: {error1,error2}", corsResult.ToString());
        }
    }
}
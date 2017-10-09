// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Cors.Test
{
    public class CorsEngineTest
    {
        [Fact]
        public void EvaluatePolicy_NullRequest_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.EvaluatePolicy(null, new CorsPolicy()),
                "requestContext");
        }

        [Fact]
        public void EvaluatePolicy_NullPolicy_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.EvaluatePolicy(new CorsRequestContext(), null),
                "policy");
        }

        [Fact]
        public void EvaluatePolicy_NoOrigin_ReturnsInvalidResult()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = null,
                HttpMethod = "GET"
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, new CorsPolicy());

            Assert.False(result.IsValid);
            Assert.Contains("The request does not contain the Origin header.", result.ErrorMessages);
        }

        [Fact]
        public void EvaluatePolicy_NoMatchingOrigin_ReturnsInvalidResult()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy();
            policy.Origins.Add("bar");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.False(result.IsValid);
            Assert.Contains("The origin 'foo' is not allowed.", result.ErrorMessages);
        }

        [Fact]
        public void EvaluatePolicy_EmptyOriginsPolicy_ReturnsInvalidResult()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy();

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.False(result.IsValid);
            Assert.Contains("The origin 'foo' is not allowed.", result.ErrorMessages);
        }

        [Fact]
        public void EvaluatePolicy_AllowAnyOrigin_DoesNotSupportCredentials_EmitsWildcardForOrigin()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                SupportsCredentials = false
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.Equal("*", result.AllowedOrigin);
        }

        [Fact]
        public void EvaluatePolicy_AllowAnyOrigin_SupportsCredentials_AddsSpecificOrigin()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                SupportsCredentials = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.Equal("foo", result.AllowedOrigin);
        }

        [Fact]
        public void EvaluatePolicy_DoesNotSupportCredentials_AllowCredentialsReturnsFalse()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                SupportsCredentials = false
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.False(result.SupportsCredentials);
        }

        [Fact]
        public void EvaluatePolicy_SupportsCredentials_AllowCredentialsReturnsTrue()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                SupportsCredentials = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(result.SupportsCredentials);
        }

        [Fact]
        public void EvaluatePolicy_NoExposedHeaders_NoAllowExposedHeaders()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.Empty(result.AllowedExposedHeaders);
        }

        [Fact]
        public void EvaluatePolicy_OneExposedHeaders_HeadersAllowed()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true
            };
            policy.ExposedHeaders.Add("foo");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.Equal(1, result.AllowedExposedHeaders.Count);
            Assert.Contains("foo", result.AllowedExposedHeaders);
        }

        [Fact]
        public void EvaluatePolicy_ManyExposedHeaders_HeadersAllowed()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true
            };
            policy.ExposedHeaders.Add("foo");
            policy.ExposedHeaders.Add("bar");
            policy.ExposedHeaders.Add("baz");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.Equal(3, result.AllowedExposedHeaders.Count());
            Assert.Contains("foo", result.AllowedExposedHeaders);
            Assert.Contains("bar", result.AllowedExposedHeaders);
            Assert.Contains("baz", result.AllowedExposedHeaders);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_MethodNotAllowed_ReturnsInvalidResult()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true
            };
            policy.Methods.Add("GET");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.False(result.IsValid);
            Assert.Contains("The method 'PUT' is not allowed.", result.ErrorMessages);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_MethodAllowed_ReturnsAllowMethods()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true
            };
            policy.Methods.Add("PUT");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.NotNull(result);
            Assert.Contains("PUT", result.AllowedMethods);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_OriginAllowed_ReturnsOrigin()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyMethod = true
            };
            policy.Origins.Add("foo");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Equal("foo", result.AllowedOrigin);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_SupportsCredentials_AllowCredentialsReturnsTrue()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo",
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true,
                SupportsCredentials = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.True(result.SupportsCredentials);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_NoPreflightMaxAge_NoPreflightMaxAgeSet()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo",
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true,
                PreflightMaxAge = null
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Null(result.PreflightMaxAge);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_PreflightMaxAge_PreflightMaxAgeSet()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo",
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true,
                PreflightMaxAge = 10
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Equal(10, result.PreflightMaxAge);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_AnyMethod_ReturnsRequestMethod()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "GET",
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Equal(1, result.AllowedMethods.Count);
            Assert.Contains("GET", result.AllowedMethods);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_ListedMethod_ReturnsSubsetOfListedMethods()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true
            };
            policy.Methods.Add("PUT");
            policy.Methods.Add("DELETE");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Single(result.AllowedMethods);
            Assert.Contains("PUT", result.AllowedMethods);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_NoHeadersRequested_AllowedAllHeaders_ReturnsEmptyHeaders()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo",
            };
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true,
                AllowAnyHeader = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Empty(result.AllowedHeaders);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_HeadersRequested_AllowAllHeaders_ReturnsRequestedHeaders()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            requestContext.AccessControlRequestHeaders.Add("foo");
            requestContext.AccessControlRequestHeaders.Add("bar");
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyHeader = true,
                AllowAnyMethod = true
            };

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Equal(2, result.AllowedHeaders.Count());
            Assert.Contains("foo", result.AllowedHeaders);
            Assert.Contains("bar", result.AllowedHeaders);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_HeadersRequested_AllowSomeHeaders_ReturnsSubsetOfListedHeaders()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            requestContext.AccessControlRequestHeaders.Add("Content-Type");
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true
            };
            policy.Headers.Add("foo");
            policy.Headers.Add("bar");
            policy.Headers.Add("Content-Type");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.Equal(1, result.AllowedHeaders.Count);
            Assert.Contains("Content-Type", result.AllowedHeaders);
        }

        [Fact]
        public void EvaluatePolicy_PreflightRequest_HeadersRequested_NotAllHeaderMatches_ReturnsInvalidResult()
        {
            CorsEngine corsEngine = new CorsEngine();
            CorsRequestContext requestContext = new CorsRequestContext
            {
                HttpMethod = "OPTIONS",
                AccessControlRequestMethod = "PUT",
                Origin = "foo"
            };
            requestContext.AccessControlRequestHeaders.Add("match");
            requestContext.AccessControlRequestHeaders.Add("noMatch");
            CorsPolicy policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyMethod = true
            };
            policy.Headers.Add("match");
            policy.Headers.Add("foo");

            CorsResult result = corsEngine.EvaluatePolicy(requestContext, policy);

            Assert.True(requestContext.IsPreflight);
            Assert.False(result.IsValid);
            Assert.Contains("The collection of headers 'match,noMatch' is not allowed.", result.ErrorMessages);
        }

        [Fact]
        public void TryValidateMethod_NullPolicy_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateMethod(new CorsRequestContext(), null, new CorsResult()),
                "policy");
        }

        [Fact]
        public void TryValidateMethod_NullRequestContext_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateMethod(null, new CorsPolicy(), new CorsResult()),
                "requestContext");
        }

        [Fact]
        public void TryValidateMethod_NullResult_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateMethod(new CorsRequestContext(), new CorsPolicy(), null),
                "result");
        }

        [Fact]
        public void TryValidateMethod_DoesCaseSensitiveComparison()
        {
            CorsEngine corsEngine = new CorsEngine();

            CorsPolicy policy = new CorsPolicy();
            policy.Methods.Add("POST");
            CorsResult result = new CorsResult();

            bool isValid = corsEngine.TryValidateMethod(new CorsRequestContext { AccessControlRequestMethod = "post" }, policy, result);
            Assert.False(isValid);
            Assert.Equal(1, result.ErrorMessages.Count);
            Assert.Equal("The method 'post' is not allowed.", result.ErrorMessages[0]);
        }

        [Fact]
        public void TryValidateHeaders_NullPolicy_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateHeaders(new CorsRequestContext(), null, new CorsResult()),
                "policy");
        }

        [Fact]
        public void TryValidateHeaders_NullRequestContext_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateHeaders(null, new CorsPolicy(), new CorsResult()),
                "requestContext");
        }

        [Fact]
        public void TryValidateHeaders_NullResult_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateHeaders(new CorsRequestContext(), new CorsPolicy(), null),
                "result");
        }

        [Fact]
        public void TryValidateOrigin_NullPolicy_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateOrigin(new CorsRequestContext(), null, new CorsResult()),
                "policy");
        }

        [Fact]
        public void TryValidateOrigin_NullRequestContext_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateOrigin(null, new CorsPolicy(), new CorsResult()),
                "requestContext");
        }

        [Fact]
        public void TryValidateOrigin_NullResult_Throws()
        {
            CorsEngine corsEngine = new CorsEngine();

            Assert.ThrowsArgumentNull(() =>
                corsEngine.TryValidateOrigin(new CorsRequestContext(), new CorsPolicy(), null),
                "result");
        }

        [Fact]
        public void TryValidateOrigin_DoesCaseSensitiveComparison()
        {
            CorsEngine corsEngine = new CorsEngine();

            CorsPolicy policy = new CorsPolicy();
            policy.Origins.Add("http://Example.com");
            CorsResult result = new CorsResult();

            bool isValid = corsEngine.TryValidateOrigin(new CorsRequestContext { Origin = "http://example.com" }, policy, result);
            Assert.False(isValid);
            Assert.Equal(1, result.ErrorMessages.Count);
            Assert.Equal("The origin 'http://example.com' is not allowed.", result.ErrorMessages[0]);
        }
    }
}
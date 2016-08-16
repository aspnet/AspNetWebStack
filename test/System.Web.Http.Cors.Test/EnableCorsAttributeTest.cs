// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.TestCommon;

namespace System.Web.Http.Cors.Test
{
    public class EnableCorsAttributeTest
    {
        [Fact]
        public void Default_Constructor()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");

            Assert.False(enableCors.SupportsCredentials);
            Assert.Empty(enableCors.ExposedHeaders);
            Assert.Empty(enableCors.Headers);
            Assert.Empty(enableCors.Methods);
            Assert.Empty(enableCors.Origins);
            Assert.Equal(-1, enableCors.PreflightMaxAge);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SettingNullOrEmptyOrigins_Throws(string origins)
        {
            Assert.ThrowsArgument(() =>
            {
                new EnableCorsAttribute(origins: origins, headers: "*", methods: "*");
            },
            "origins",
            "Value cannot be null or an empty string.");
        }

        [Fact]
        public async Task GetCorsPolicyAsync_DefaultPolicyValues()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.True(corsPolicy.AllowAnyHeader);
            Assert.True(corsPolicy.AllowAnyMethod);
            Assert.True(corsPolicy.AllowAnyOrigin);
            Assert.False(corsPolicy.SupportsCredentials);
            Assert.Empty(corsPolicy.ExposedHeaders);
            Assert.Empty(corsPolicy.Headers);
            Assert.Empty(corsPolicy.Methods);
            Assert.Empty(corsPolicy.Origins);
            Assert.Null(corsPolicy.PreflightMaxAge);
        }

        [Fact]
        public async Task GetCorsPolicyAsync_RetunsExpectedSupportsCredentials()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*")
            {
                SupportsCredentials = true
            };

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.True(corsPolicy.SupportsCredentials);
        }

        [Fact]
        public async Task GetCorsPolicyAsync_RetunsExpectedPreflightMaxAge()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*")
            {
                PreflightMaxAge = 20
            };

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(20, corsPolicy.PreflightMaxAge);
        }

        [Theory]
        [InlineData("foo", new[] { "foo" })]
        [InlineData("foo ", new[] { "foo" })]
        [InlineData("foo,", new[] { "foo" })]
        [InlineData("foo,bar", new[] { "foo", "bar" })]
        [InlineData("foo, bar", new[] { "foo", "bar" })]
        [InlineData("foo,bar,", new[] { "foo", "bar" })]
        public async Task GetCorsPolicyAsync_RetunsExpectedExposeHeaders(string exposedHeaders, string[] expectedResults)
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*", exposedHeaders: exposedHeaders);

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(new List<string>(expectedResults), corsPolicy.ExposedHeaders);
        }

        [Theory]
        [InlineData("Accept", new[] { "Accept" })]
        [InlineData("Accept ", new[] { "Accept" })]
        [InlineData("Accept,", new[] { "Accept" })]
        [InlineData("Accept,Content-Type", new[] { "Accept", "Content-Type" })]
        [InlineData("Accept, Content-Type", new[] { "Accept", "Content-Type" })]
        [InlineData("Accept,Content-Type,", new[] { "Accept", "Content-Type" })]
        public async Task GetCorsPolicyAsync_RetunsExpectedHeaders(string headers, string[] expectedResults)
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: headers, methods: "*");

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(new List<string>(expectedResults), corsPolicy.Headers);
        }

        [Theory]
        [InlineData("Get", new[] { "Get" })]
        [InlineData("Get ", new[] { "Get" })]
        [InlineData("Get,", new[] { "Get" })]
        [InlineData("Get,Delete", new[] { "Get", "Delete" })]
        [InlineData("Get, Delete", new[] { "Get", "Delete" })]
        [InlineData("Get,Delete,", new[] { "Get", "Delete" })]
        public async Task GetCorsPolicyAsync_RetunsExpectedMethods(string methods, string[] expectedResults)
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: methods);

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(new List<string>(expectedResults), corsPolicy.Methods);
        }

        [Theory]
        [InlineData("http://example.com", new[] { "http://example.com" })]
        [InlineData("http://example.com ", new[] { "http://example.com" })]
        [InlineData("http://example.com,", new[] { "http://example.com" })]
        [InlineData("http://example.com,http://localhost:8080", new[] { "http://example.com", "http://localhost:8080" })]
        [InlineData("http://example.com, http://localhost:8080", new[] { "http://example.com", "http://localhost:8080" })]
        [InlineData("http://example.com,http://localhost:8080,", new[] { "http://example.com", "http://localhost:8080" })]
        public async Task GetCorsPolicyAsync_RetunsExpectedOrigins(string origins, string[] expectedResults)
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: origins, headers: "*", methods: "*");

            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(new List<string>(expectedResults), corsPolicy.Origins);
        }

        [Theory]
        [InlineData("foo", "The specified policy origin 'foo' is invalid. It must be correctly formed with the scheme, the host, and optionally, the port.")]
        [InlineData("://example.com", "The specified policy origin '://example.com' is invalid. It must be correctly formed with the scheme, the host, and optionally, the port.")]
        [InlineData("http://example.com/", "The specified policy origin 'http://example.com/' is invalid. It cannot end with a forward slash.")]
        [InlineData("http://example.com#fragment", "The specified policy origin 'http://example.com#fragment' is invalid. It must not contain a path, query, or fragment.")]
        [InlineData("http://example.com/path", "The specified policy origin 'http://example.com/path' is invalid. It must not contain a path, query, or fragment.")]
        [InlineData("http://example.com?query=foo", "The specified policy origin 'http://example.com?query=foo' is invalid. It must not contain a path, query, or fragment.")]
        public Task GetCorsPolicyAsync_InvalidOrigin_Throws(string origin, string expectedErrorMessage)
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: origin, headers: "*", methods: "*");

            return Assert.ThrowsAsync<InvalidOperationException>(
                () => enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None),
                expectedErrorMessage);
        }

        [Theory]
        [InlineData("", "The specified policy origin cannot be null or empty.")]
        [InlineData(null, "The specified policy origin cannot be null or empty.")]
        public Task GetCorsPolicyAsync_NullEmptyOrigin_Throws(string origin, string expectedErrorMessage)
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "http://localhost", headers: "*", methods: "*");
            enableCors.Origins.Add(origin);

            return Assert.ThrowsAsync<InvalidOperationException>(
                () => enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None),
                expectedErrorMessage);
        }

        [Fact]
        public async Task AllowAnyHeader_IsFalse_WhenHeadersPropertyIsSet()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "foo", methods: "*");
            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.False(corsPolicy.AllowAnyHeader);
        }

        [Fact]
        public async Task AllowAnyOrigin_IsFalse_WhenOriginsPropertyIsSet()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "http://example.com", headers: "*", methods: "*");
            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.False(corsPolicy.AllowAnyOrigin);
        }

        [Fact]
        public async Task AllowAnyMethod_IsFalse_WhenMethodsPropertyIsSet()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "GET");
            CorsPolicy corsPolicy = await enableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.False(corsPolicy.AllowAnyMethod);
        }

        [Fact]
        public void SettingNegativePreflightMaxAge_Throws()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");
            Assert.ThrowsArgumentOutOfRange(() =>
            {
                enableCors.PreflightMaxAge = -2;
            },
            "value",
            "PreflightMaxAge must be greater than or equal to 0.");
        }
    }
}
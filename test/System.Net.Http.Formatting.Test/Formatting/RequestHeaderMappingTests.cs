// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class RequestHeaderMappingTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(RequestHeaderMapping),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(MediaTypeMapping));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(HttpTestData), "LegalMediaTypeHeaderValues")]
        public void Constructor(string headerName, string headerValue, MediaTypeHeaderValue mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, true, mediaType);
            Assert.Equal(headerName, mapping.HeaderName);
            Assert.Equal(headerValue, mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.True(mapping.IsValueSubstring);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalMediaTypeHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void ConstructorThrowsWithEmptyHeaderName(MediaTypeHeaderValue mediaType, string headerName)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, "value", StringComparison.CurrentCulture, false, mediaType), "headerName");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalMediaTypeHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void ConstructorThrowsWithEmptyHeaderValue(MediaTypeHeaderValue mediaType, string headerValue)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping("name", headerValue, StringComparison.CurrentCulture, false, mediaType), "headerValue");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues")]
        public void ConstructorThrowsWithNullMediaTypeHeaderValue(string headerName, string headerValue)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, false, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(HttpTestData), "LegalMediaTypeHeaderValues")]
        public void ConstructorThrowsWithInvalidStringComparison(string headerName, string headerValue, MediaTypeHeaderValue mediaType)
        {
            int invalidValue = 999;
            Assert.ThrowsInvalidEnumArgument(() => new RequestHeaderMapping(headerName, headerValue, (StringComparison)invalidValue, false, mediaType),
                "valueComparison", invalidValue, typeof(StringComparison));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void Constructor1(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, true, mediaType);
            Assert.Equal(headerName, mapping.HeaderName);
            Assert.Equal(headerValue, mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.True(mapping.IsValueSubstring);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyHeaderName(string mediaType, string headerName)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, "value", StringComparison.CurrentCulture, false, mediaType), "headerName");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyHeaderValue(string mediaType, string headerValue)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping("name", headerValue, StringComparison.CurrentCulture, false, mediaType), "headerValue");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyMediaType(string headerName, string headerValue, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, false, mediaType), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void Constructor1ThrowsWithInvalidStringComparison(string headerName, string headerValue, string mediaType)
        {
            int invalidValue = 999;
            Assert.ThrowsInvalidEnumArgument(
                () => new RequestHeaderMapping(headerName, headerValue, (StringComparison)invalidValue, false, mediaType),
                "valueComparison", invalidValue, typeof(StringComparison));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(HttpTestData), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "Bools")]
        public void TryMatchMediaTypeReturnsTrueWithNameAndValueInRequest(string headerName, string headerValue, string mediaType, bool subset)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, subset, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(headerName, headerValue);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalHttpHeaderNames",
            typeof(HttpTestData), "LegalHttpHeaderValues",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeReturnsTrueWithNameAndValueSubsetInRequest(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, true, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, headerValue + "postfix");
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue + "postfix");
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
           typeof(HttpTestData), "LegalHttpHeaderNames",
           typeof(HttpTestData), "LegalHttpHeaderValues",
           typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeReturnsFalseWithNameNotInRequest(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, false, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add("prefix" + headerName, headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName + "postfix", headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add("prefix" + headerName + "postfix", headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
           typeof(HttpTestData), "LegalHttpHeaderNames",
           typeof(HttpTestData), "LegalHttpHeaderValues",
           typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeReturnsFalseWithValueNotInRequest(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, false, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, headerValue + "postfix");
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue + "postfix");
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
           typeof(HttpTestData), "LegalHttpHeaderNames",
           typeof(HttpTestData), "LegalHttpHeaderValues",
           typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeThrowsWithNullHttpRequestMessage(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, true, mediaType);
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(request: null), "request");
        }
    }
}

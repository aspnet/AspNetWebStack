// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Formatting
{
    public class UriPathExtensionMappingTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(UriPathExtensionMapping),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(MediaTypeMapping));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void ConstructorMediaType_Initialises_MediaTypeAndUriPathExtension(string uriPathExtension, string mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Assert.Equal(uriPathExtension, mapping.UriPathExtension);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "Failed to set MediaType.");
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "EmptyStrings",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void ConstructorMediaType_ThrowsWithEmptyUriPathExtension(string uriPathExtension, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, mediaType), "uriPathExtension");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void ConstructorMediaType_ThrowsWithEmptyMediaType(string uriPathExtension, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, mediaType), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(HttpTestData), "LegalMediaTypeHeaderValues")]
        public void ConstructorMediaTypeHeaderValue_Initialises_MediaTypeAndUriPathExtension(string uriPathExtension, MediaTypeHeaderValue mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Assert.Equal(uriPathExtension, mapping.UriPathExtension);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "Failed to set MediaType.");
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "EmptyStrings",
            typeof(HttpTestData), "LegalMediaTypeHeaderValues")]
        public void ConstructorMediaTypeHeaderValue_ThrowsWithEmptyUriPathExtension(string uriPathExtension, MediaTypeHeaderValue mediaType)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, mediaType), "uriPathExtension");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions")]
        public void ConstructorMediaTypeHeaderValue_ThrowsWithEmptyMediaType(string uriPathExtension)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(HttpTestData), "LegalMediaTypeStrings",
            typeof(HttpTestData), "UriTestDataStrings")]
        public void TryMatchMediaType_Returns_MatchWithExtensionInRouteData(string uriPathExtension, string mediaType, string baseUriString)
        {
            GC.KeepAlive(baseUriString); // Mark parameter as used. See xUnit1026, [Theory] method doesn't use all parameters.
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            HttpRequestMessage request = GetRequestWithExtInRouteData(uriPathExtension);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(HttpTestData), "LegalMediaTypeStrings",
            typeof(HttpTestData), "UriTestDataStrings")]
        public void TryMatchMediaType_Returns_MatchWithExtensionInRouteData_DifferCase(string uriPathExtension, string mediaType, string baseUriString)
        {
            GC.KeepAlive(baseUriString); // Mark parameter as used. See xUnit1026, [Theory] method doesn't use all parameters.
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension.ToUpperInvariant(), mediaType);
            HttpRequestMessage request = GetRequestWithExtInRouteData(uriPathExtension.ToLowerInvariant());
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(HttpTestData), "LegalMediaTypeStrings",
            typeof(HttpTestData), "UriTestDataStrings")]
        public void TryMatchMediaType_Returns_ZeroWithExtensionNotInRouteData(string uriPathExtension, string mediaType, string baseUriString)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Uri uri = new Uri(baseUriString);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpTestData), "LegalUriPathExtensions",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void TryMatchMediaType_Throws_WithNullHttpRequestMessage(string uriPathExtension, string mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(request: null), "request");
        }

        private static HttpRequestMessage GetRequestWithExtInRouteData(string extensionValue)
        {
            IHttpRoute route = new Mock<IHttpRoute>().Object;
            IHttpRouteData routeData = new HttpRouteData(route);
            routeData.Values[UriPathExtensionMapping.UriPathExtensionKey] = extensionValue;

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRouteData(routeData);

            return request;
        }
    }
}

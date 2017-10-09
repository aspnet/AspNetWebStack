// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterExtensionsTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeFormatterExtensions), TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        public void AddQueryStringMappingThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddQueryStringMapping("name", "value", new MediaTypeHeaderValue("application/xml")), "formatter");
        }

        [Fact]
        public void AddQueryStringMapping1ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddQueryStringMapping("name", "value", "application/xml"), "formatter");
        }

        [Fact]
        public void AddRequestHeaderMappingThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, new MediaTypeHeaderValue("application/xml")), "formatter");
        }

        [Fact]
        public void AddRequestHeaderMappingAddsSuccessfully()
        {
            MediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Assert.Empty(formatter.MediaTypeMappings);
            formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, new MediaTypeHeaderValue("application/xml"));
            IEnumerable<RequestHeaderMapping> mappings = formatter.MediaTypeMappings.OfType<RequestHeaderMapping>();
            RequestHeaderMapping mapping = Assert.Single(mappings);
            Assert.Equal("name", mapping.HeaderName);
            Assert.Equal("value", mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.True(mapping.IsValueSubstring);
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), mapping.MediaType);
        }

        [Fact]
        public void AddRequestHeaderMapping1ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, "application/xml"), "formatter");
        }

        [Fact]
        public void AddRequestHeaderMapping1AddsSuccessfully()
        {
            MediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Assert.Empty(formatter.MediaTypeMappings);
            formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, "application/xml");
            IEnumerable<RequestHeaderMapping> mappings = formatter.MediaTypeMappings.OfType<RequestHeaderMapping>();
            RequestHeaderMapping mapping = Assert.Single(mappings);
            Assert.Equal("name", mapping.HeaderName);
            Assert.Equal("value", mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.True(mapping.IsValueSubstring);
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), mapping.MediaType);
        }
    }
}

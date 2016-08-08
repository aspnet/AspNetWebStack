// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterExtensionsTests
    {
        [Fact]
        public void AddUriPathExtensionMapping_MediaTypeHeaderValue_ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddUriPathExtensionMapping("xml", new MediaTypeHeaderValue("application/xml")), "formatter");
        }

        [Fact]
        public void AddUriPathExtensionMapping_MediaTypeHeaderValue_UpdatesMediaTypeMappingsCollection()
        {
            MediaTypeFormatter mockFormatter = new Mock<MediaTypeFormatter> { CallBase = true }.Object;

            mockFormatter.AddUriPathExtensionMapping("ext", new MediaTypeHeaderValue("application/test"));

            Assert.Equal(1, mockFormatter.MediaTypeMappings.Count);
            Assert.IsType(typeof(UriPathExtensionMapping), mockFormatter.MediaTypeMappings[0]);
            Assert.Equal("ext", (mockFormatter.MediaTypeMappings[0] as UriPathExtensionMapping).UriPathExtension);
            Assert.Equal("application/test", (mockFormatter.MediaTypeMappings[0] as UriPathExtensionMapping).MediaType.MediaType);
        }

        [Fact]
        public void AddUriPathExtensionMapping_MediaType_ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddUriPathExtensionMapping("xml", "application/xml"), "formatter");
        }

        [Fact]
        public void AddUriPathExtensionMapping_MediaType_UpdatesMediaTypeMappingsCollection()
        {
            MediaTypeFormatter mockFormatter = new Mock<MediaTypeFormatter> { CallBase = true }.Object;

            mockFormatter.AddUriPathExtensionMapping("ext", "application/test");

            Assert.Equal(1, mockFormatter.MediaTypeMappings.Count);
            Assert.IsType(typeof(UriPathExtensionMapping), mockFormatter.MediaTypeMappings[0]);
            Assert.Equal("ext", (mockFormatter.MediaTypeMappings[0] as UriPathExtensionMapping).UriPathExtension);
            Assert.Equal("application/test", (mockFormatter.MediaTypeMappings[0] as UriPathExtensionMapping).MediaType.MediaType);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class MediaTypeMappingTests
    {
        [Fact]
        public void Constructor_ThrowsOnNullMediaTypeHeaderValue()
        {
            Assert.ThrowsArgumentNull(() => new MockMediaTypeMapping((MediaTypeHeaderValue)null), "mediaType");
        }

        [Fact]
        public void Constructor_ThrowsOnNullMediaType()
        {
            Assert.ThrowsArgumentNull(() => new MockMediaTypeMapping((string)null), "mediaType");
            Assert.ThrowsArgumentNull(() => new MockMediaTypeMapping(String.Empty), "mediaType");
        }

        public class MockMediaTypeMapping : MediaTypeMapping
        {
            public MockMediaTypeMapping(MediaTypeHeaderValue mediaType)
                : base(mediaType)
            {
            }

            public MockMediaTypeMapping(string mediaType)
                : base(mediaType)
            {
            }

            public override double TryMatchMediaType(HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }
        }
    }
}

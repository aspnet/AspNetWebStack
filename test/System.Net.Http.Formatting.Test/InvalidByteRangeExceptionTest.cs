// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class InvalidByteRangeExceptionTest
    {
        [Fact]
        public void Ctor_ThrowsOnNullRange()
        {
            Assert.ThrowsArgumentNull(() => new InvalidByteRangeException(contentRange: null), "contentRange");
        }

        [Fact]
        public void Ctor_SetsContentRange()
        {
            ContentRangeHeaderValue contentRange = new ContentRangeHeaderValue(0, 20, 100);
            InvalidByteRangeException invalidByteRangeException = new InvalidByteRangeException(contentRange);
            Assert.Same(contentRange, invalidByteRangeException.ContentRange);
        }
    }
}

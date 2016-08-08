// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpUnsortedRequestTest
    {
        [Fact]
        public void Constructor_InitializesHeaders()
        {
            HttpUnsortedRequest request = new HttpUnsortedRequest();
            Assert.IsType<HttpUnsortedHeaders>(request.HttpHeaders);
        }
    }
}

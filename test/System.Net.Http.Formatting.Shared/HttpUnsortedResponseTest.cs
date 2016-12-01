// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpUnsortedResponseTest
    {
        [Fact]
        public void Constructor_InitializesHeaders()
        {
            HttpUnsortedResponse response = new HttpUnsortedResponse();
            Assert.IsType<HttpUnsortedHeaders>(response.HttpHeaders);
        }
    }
}

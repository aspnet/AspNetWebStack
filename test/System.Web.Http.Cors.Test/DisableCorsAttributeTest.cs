// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.TestCommon;

namespace System.Web.Http.Cors.Test
{
    public class DisableCorsAttributeTest
    {
        [Fact]
        public async Task GetCorsPolicyAsync_ReturnsNull()
        {
            DisableCorsAttribute disableCors = new DisableCorsAttribute();
            CorsPolicy corsPolicy = await disableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Null(corsPolicy);
        }
    }
}
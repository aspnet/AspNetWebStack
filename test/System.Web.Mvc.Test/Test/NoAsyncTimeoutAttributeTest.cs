// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class NoAsyncTimeoutAttributeTest
    {
        [Fact]
        public void DurationPropertyIsZero()
        {
            // Act
            AsyncTimeoutAttribute attr = new NoAsyncTimeoutAttribute();

            // Assert
            Assert.Equal(Timeout.Infinite, attr.Duration);
        }
    }
}

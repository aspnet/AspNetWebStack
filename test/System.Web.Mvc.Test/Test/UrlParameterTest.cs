// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class UrlParameterTest
    {
        [Fact]
        public void UrlParameterOptionalToStringReturnsEmptyString()
        {
            // Act & Assert
            Assert.Empty(UrlParameter.Optional.ToString());
        }
    }
}

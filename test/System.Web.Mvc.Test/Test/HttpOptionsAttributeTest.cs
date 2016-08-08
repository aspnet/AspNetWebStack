// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class HttpOptionsAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotOptions()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpOptionsAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsOptions()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpOptionsAttribute>("OPTIONS");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpOptionsAttribute>();
        }
    }
}
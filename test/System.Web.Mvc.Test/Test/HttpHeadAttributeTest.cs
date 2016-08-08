// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class HttpHeadAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotHead()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpHeadAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsHead()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpHeadAttribute>("HEAD");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpHeadAttribute>();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class HttpPutAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotPost()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpPutAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsPost()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpPutAttribute>("PUT");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpPutAttribute>();
        }
    }
}

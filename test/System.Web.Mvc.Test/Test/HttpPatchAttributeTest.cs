// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test {
    public class HttpPatchAttributeTest 
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotPatch() 
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpPatchAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsPatch() 
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpPatchAttribute>("PATCH");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpPatchAttribute>();
        }
    }
}
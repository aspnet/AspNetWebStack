// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class LabelHelperTest
    {
        [Fact]
        public void LabelWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.Label("foo", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.Label("foo", "bar", attributes));
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class AntiForgeryTest
    {
        [Fact]
        public void GetHtml_ThrowsWhenNotCalledInWebContext()
        {
            Assert.Throws<ArgumentException>(() => AntiForgery.GetHtml(),
                                                    "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }

        [Fact]
        public void GetTokens_ThrowsWhenNotCalledInWebContext()
        {
            Assert.Throws<ArgumentException>(() => { string dummy1, dummy2; AntiForgery.GetTokens("dummy", out dummy1, out dummy2); },
                                                    "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }

        [Fact]
        public void Validate_ThrowsWhenNotCalledInWebContext()
        {
            Assert.Throws<ArgumentException>(() => AntiForgery.Validate(),
                                                    "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");

            Assert.Throws<ArgumentException>(() => AntiForgery.Validate("cookie-token", "form-token"),
                                                    "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }
    }
}

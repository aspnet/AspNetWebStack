// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    /// <summary>
    /// RequestBrowserOverrideStore simply returns the user agent of the current request.
    /// </summary>
    internal sealed class RequestBrowserOverrideStore : BrowserOverrideStore
    {
        public override string GetOverriddenUserAgent(HttpContextBase httpContext)
        {
            return httpContext.Request.UserAgent;
        }

        public override void SetOverriddenUserAgent(HttpContextBase httpContext, string userAgent)
        {
            return;
        }
    }
}

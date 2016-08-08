// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    /// <summary>
    /// The current BrowserOverrideStore is used to get and set the user agent of a request.
    /// For an example see CookieBasedBrowserOverrideStore.
    /// </summary>
    public abstract class BrowserOverrideStore
    {
        public abstract string GetOverriddenUserAgent(HttpContextBase httpContext);
        public abstract void SetOverriddenUserAgent(HttpContextBase httpContext, string userAgent);
    }
}

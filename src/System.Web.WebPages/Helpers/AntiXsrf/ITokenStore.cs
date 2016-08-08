// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf
{
    // Provides an abstraction around how tokens are persisted and retrieved for a request
    internal interface ITokenStore
    {
        AntiForgeryToken GetCookieToken(HttpContextBase httpContext);
        AntiForgeryToken GetFormToken(HttpContextBase httpContext);
        void SaveCookieToken(HttpContextBase httpContext, AntiForgeryToken token);
    }
}

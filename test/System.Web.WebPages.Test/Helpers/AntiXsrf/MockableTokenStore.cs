// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace System.Web.Helpers.AntiXsrf.Test
{
    // An ITokenStore that can be passed to MoQ
    public abstract class MockableTokenStore : ITokenStore
    {
        public abstract object GetCookieToken(HttpContextBase httpContext);
        public abstract object GetFormToken(HttpContextBase httpContext);
        public abstract void SaveCookieToken(HttpContextBase httpContext, object token);

        AntiForgeryToken ITokenStore.GetCookieToken(HttpContextBase httpContext)
        {
            return (AntiForgeryToken)GetCookieToken(httpContext);
        }

        AntiForgeryToken ITokenStore.GetFormToken(HttpContextBase httpContext)
        {
            return (AntiForgeryToken)GetFormToken(httpContext);
        }

        void ITokenStore.SaveCookieToken(HttpContextBase httpContext, AntiForgeryToken token)
        {
            SaveCookieToken(httpContext, (AntiForgeryToken)token);
        }
    }
}

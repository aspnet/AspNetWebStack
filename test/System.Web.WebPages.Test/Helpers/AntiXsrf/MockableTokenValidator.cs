// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Helpers.AntiXsrf.Test
{
    // An ITokenValidator that can be passed to MoQ
    public abstract class MockableTokenValidator : ITokenValidator
    {
        public abstract object GenerateCookieToken();
        public abstract object GenerateFormToken(HttpContextBase httpContext, IIdentity identity, object cookieToken);
        public abstract bool IsCookieTokenValid(object cookieToken);
        public abstract void ValidateTokens(HttpContextBase httpContext, IIdentity identity, object cookieToken, object formToken);

        AntiForgeryToken ITokenValidator.GenerateCookieToken()
        {
            return (AntiForgeryToken)GenerateCookieToken();
        }

        AntiForgeryToken ITokenValidator.GenerateFormToken(HttpContextBase httpContext, IIdentity identity, AntiForgeryToken cookieToken)
        {
            return (AntiForgeryToken)GenerateFormToken(httpContext, identity, (AntiForgeryToken)cookieToken);
        }

        bool ITokenValidator.IsCookieTokenValid(AntiForgeryToken cookieToken)
        {
            return IsCookieTokenValid((AntiForgeryToken)cookieToken);
        }

        void ITokenValidator.ValidateTokens(HttpContextBase httpContext, IIdentity identity, AntiForgeryToken cookieToken, AntiForgeryToken formToken)
        {
            ValidateTokens(httpContext, identity, (AntiForgeryToken)cookieToken, (AntiForgeryToken)formToken);
        }
    }
}

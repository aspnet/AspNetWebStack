// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.ServiceModel;

namespace System.Web.Http.SelfHost.ServiceModel
{
    internal static class HttpProxyCredentialTypeHelper
    {
        internal static AuthenticationSchemes MapToAuthenticationScheme(HttpProxyCredentialType proxyCredentialType)
        {
            switch (proxyCredentialType)
            {
                case HttpProxyCredentialType.None:
                    return AuthenticationSchemes.Anonymous;

                case HttpProxyCredentialType.Basic:
                    return AuthenticationSchemes.Basic;

                case HttpProxyCredentialType.Digest:
                    return AuthenticationSchemes.Digest;

                case HttpProxyCredentialType.Ntlm:
                    return AuthenticationSchemes.Ntlm;

                case HttpProxyCredentialType.Windows:
                    return AuthenticationSchemes.Negotiate;
            }

            Contract.Assert(false, "Invalid proxyCredentialType " + proxyCredentialType);
            return AuthenticationSchemes.Anonymous;
        }
    }
}

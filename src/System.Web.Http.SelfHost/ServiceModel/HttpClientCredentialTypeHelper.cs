// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.ServiceModel;

namespace System.Web.Http.SelfHost.ServiceModel
{
    internal static class HttpClientCredentialTypeHelper
    {
        internal static AuthenticationSchemes MapToAuthenticationScheme(HttpClientCredentialType clientCredentialType)
        {
            switch (clientCredentialType)
            {
                case HttpClientCredentialType.None:
                case HttpClientCredentialType.Certificate:
                    return AuthenticationSchemes.Anonymous;

                case HttpClientCredentialType.Basic:
                    return AuthenticationSchemes.Basic;

                case HttpClientCredentialType.Digest:
                    return AuthenticationSchemes.Digest;

                case HttpClientCredentialType.Ntlm:
                    return AuthenticationSchemes.Ntlm;

                case HttpClientCredentialType.Windows:
                    return AuthenticationSchemes.Negotiate;
            }

            Contract.Assert(false, "Invalid clientCredentialType " + clientCredentialType);
            return AuthenticationSchemes.Anonymous;
        }
    }
}

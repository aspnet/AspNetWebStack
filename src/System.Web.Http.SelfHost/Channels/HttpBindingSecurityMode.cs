// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.SelfHost.Channels
{
    /// <summary>
    /// Defines the modes of security that can be used to configure a service endpoint that uses the
    /// <see cref="HttpBinding"/>.
    /// </summary>
    public enum HttpBindingSecurityMode
    {
        /// <summary>
        /// Indicates no security is used with HTTP requests.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that transport-level security is used with HTTP requests.
        /// </summary>
        Transport,

        /// <summary>
        /// Indicates that only HTTP-based client authentication is provided.
        /// </summary>
        TransportCredentialOnly,
    }
}

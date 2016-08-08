// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Owin
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces.
    /// </summary>
    internal static class OwinConstants
    {
        // Request keys
        public const string ClientCertifiateKey = "ssl.ClientCertificate";
        public const string IsLocalKey = "server.IsLocal";
    }
}
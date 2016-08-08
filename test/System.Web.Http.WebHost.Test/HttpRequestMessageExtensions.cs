// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;

namespace System.Web.Http
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetIsLocal(this HttpRequestMessage request, Lazy<bool> isLocal)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Properties[HttpPropertyKeys.IsLocalKey] = isLocal;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel.Security;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace System.Net.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Gets the current <see cref="T:System.ServiceModel.Security.SecurityMessageProperty"/> 
        /// stored in <see cref="M:HttpRequestMessage.Properties"/> for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="SecurityMessageProperty"/>.</returns>
        public static SecurityMessageProperty GetSecurityMessageProperty(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetProperty<SecurityMessageProperty>(HttpSelfHostServer.SecurityKey);
        }

        private static T GetProperty<T>(this HttpRequestMessage request, string key)
        {
            T value;
            request.Properties.TryGetValue(key, out value);
            return value;
        }
    }
}

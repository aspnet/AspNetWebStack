// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// CORS-related extension methods for <see cref="HttpRequestMessage"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CorsHttpRequestMessageExtensions
    {
        private const string CorsRequestContextKey = "MS_CorsRequestContextKey";

        /// <summary>
        /// Gets the <see cref="CorsRequestContext"/> for a given request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <returns>The <see cref="CorsRequestContext"/>.</returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public static CorsRequestContext GetCorsRequestContext(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            object corsRequestContext;
            if (!request.Properties.TryGetValue(CorsRequestContextKey, out corsRequestContext))
            {
                if (!request.Headers.Contains(CorsConstants.Origin))
                {
                    return null;
                }

                CorsRequestContext requestContext = new CorsRequestContext
                {
                    RequestUri = request.RequestUri,
                    HttpMethod = request.Method.Method,
                    Host = request.Headers.Host,
                    Origin = request.GetHeader(CorsConstants.Origin),
                    AccessControlRequestMethod = request.GetHeader(CorsConstants.AccessControlRequestMethod)
                };
                requestContext.Properties.Add(typeof(HttpRequestMessage).FullName, request);

                IEnumerable<string> accessControlRequestHeaders = request.GetHeaders(CorsConstants.AccessControlRequestHeaders);
                foreach (string accessControlRequestHeader in accessControlRequestHeaders)
                {
                    if (accessControlRequestHeader != null)
                    {
                        IEnumerable<string> headerValues = accessControlRequestHeader.Split(',').Select(x => x.Trim());
                        foreach (string header in headerValues)
                        {
                            requestContext.AccessControlRequestHeaders.Add(header);
                        }
                    }
                }

                request.Properties.Add(CorsRequestContextKey, requestContext);
                corsRequestContext = requestContext;
            }

            return (CorsRequestContext)corsRequestContext;
        }

        private static string GetHeader(this HttpRequestMessage request, string name)
        {
            return request.GetHeaders(name).FirstOrDefault();
        }

        private static IEnumerable<string> GetHeaders(this HttpRequestMessage request, string name)
        {
            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues(name, out headerValues))
            {
                if (headerValues != null)
                {
                    return headerValues;
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
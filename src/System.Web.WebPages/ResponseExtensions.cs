// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace System.Web.WebPages
{
    public static class ResponseExtensions
    {
        public static void SetStatus(this HttpResponseBase response, HttpStatusCode httpStatusCode)
        {
            SetStatus(response, (int)httpStatusCode);
        }

        public static void SetStatus(this HttpResponseBase response, int httpStatusCode)
        {
            response.StatusCode = httpStatusCode;
            response.End();
        }

        public static void WriteBinary(this HttpResponseBase response, byte[] data, string mimeType)
        {
            response.ContentType = mimeType;
            WriteBinary(response, data);
        }

        public static void WriteBinary(this HttpResponseBase response, byte[] data)
        {
            response.OutputStream.Write(data, 0, data.Length);
        }

        // REVIEW: See what this is actually calling that's needed
        // Configure output caching for the request
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "We are not removing optional parameters from helpers")]
        public static void OutputCache(this HttpResponseBase response,
                                       int numberOfSeconds,
                                       bool sliding = false,
                                       IEnumerable<string> varyByParams = null,
                                       IEnumerable<string> varyByHeaders = null,
                                       IEnumerable<string> varyByContentEncodings = null,
                                       HttpCacheability cacheability = HttpCacheability.Public)
        {
            OutputCache(new HttpContextWrapper(HttpContext.Current), response.Cache, numberOfSeconds, sliding, varyByParams, varyByHeaders, varyByContentEncodings,
                        cacheability);
        }

        internal static void OutputCache(HttpContextBase httpContext,
                                         HttpCachePolicyBase cache,
                                         int numberOfSeconds,
                                         bool sliding,
                                         IEnumerable<string> varyByParams,
                                         IEnumerable<string> varyByHeaders,
                                         IEnumerable<string> varyByContentEncodings,
                                         HttpCacheability cacheability)
        {
            cache.SetCacheability(cacheability);
            cache.SetExpires(httpContext.Timestamp.AddSeconds(numberOfSeconds));
            cache.SetMaxAge(new TimeSpan(0, 0, numberOfSeconds));
            cache.SetValidUntilExpires(true);
            cache.SetLastModified(httpContext.Timestamp);
            cache.SetSlidingExpiration(sliding);

            if (varyByParams != null)
            {
                foreach (var p in varyByParams)
                {
                    cache.VaryByParams[p] = true;
                }
            }

            if (varyByHeaders != null)
            {
                foreach (var headerName in varyByHeaders)
                {
                    cache.VaryByHeaders[headerName] = true;
                }
            }

            if (varyByContentEncodings != null)
            {
                foreach (var contentEncoding in varyByContentEncodings)
                {
                    cache.VaryByContentEncodings[contentEncoding] = true;
                }
            }
        }
    }
}

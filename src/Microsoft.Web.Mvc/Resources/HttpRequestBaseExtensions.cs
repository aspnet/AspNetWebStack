// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// HttpRequestBase extension methods that call directly into the DefaultFormatHelper
    /// </summary>
    public static class HttpRequestBaseExtensions
    {
        public static ContentType GetRequestFormat(this HttpRequestBase request)
        {
            return DefaultFormatHelper.GetRequestFormat(request, true);
        }

        public static IEnumerable<ContentType> GetResponseFormats(this HttpRequestBase request)
        {
            return DefaultFormatHelper.GetResponseFormats(request);
        }

        internal static bool HasBody(this HttpRequestBase request)
        {
            return request.ContentLength > 0 || String.Compare("chunked", request.Headers["Transfer-Encoding"], StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsBrowserRequest(this HttpRequestBase request)
        {
            return DefaultFormatHelper.IsBrowserRequest(request);
        }

        public static bool IsHttpMethod(this HttpRequestBase request, HttpVerbs httpMethod)
        {
            return request.IsHttpMethod(httpMethod, false);
        }

        public static bool IsHttpMethod(this HttpRequestBase request, string httpMethod)
        {
            return request.IsHttpMethod(httpMethod, false);
        }

        // CODEREVIEW: this implementation kind of misses the point of HttpVerbs
        // by falling back to string comparison, consider something better
        // also, how do we keep this switch in sync?
        public static bool IsHttpMethod(this HttpRequestBase request, HttpVerbs httpMethod, bool allowOverride)
        {
            switch (httpMethod)
            {
                case HttpVerbs.Get:
                    return request.IsHttpMethod("GET", allowOverride);
                case HttpVerbs.Post:
                    return request.IsHttpMethod("POST", allowOverride);
                case HttpVerbs.Put:
                    return request.IsHttpMethod("PUT", allowOverride);
                case HttpVerbs.Delete:
                    return request.IsHttpMethod("DELETE", allowOverride);
                case HttpVerbs.Head:
                    return request.IsHttpMethod("HEAD", allowOverride);
                case HttpVerbs.Patch:
                    return request.IsHttpMethod("PATCH", allowOverride);
                case HttpVerbs.Options:
                    return request.IsHttpMethod("OPTIONS", allowOverride);
                default:
                    // CODEREVIEW: does this look reasonable?
                    return request.IsHttpMethod(httpMethod.ToString().ToUpperInvariant(), allowOverride);
            }
        }

        public static bool IsHttpMethod(this HttpRequestBase request, string httpMethod, bool allowOverride)
        {
            string requestHttpMethod = allowOverride ? request.GetHttpMethodOverride() : request.HttpMethod;
            return String.Equals(requestHttpMethod, httpMethod, StringComparison.OrdinalIgnoreCase);
        }
    }
}

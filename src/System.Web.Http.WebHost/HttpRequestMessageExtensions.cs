// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.WebHost
{
    internal static class HttpRequestMessageExtensions
    {
        private const string HttpContextBaseKey = "MS_HttpContext";
        private const string HttpBatchContextKey = "MS_HttpBatchContext";

        public static HttpContextBase GetHttpContext(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            HttpContextBase context;

            if (request.IsBatchRequest())
            {
                if (!request.Properties.TryGetValue(HttpBatchContextKey, out context))
                {
                    if (request.Properties.TryGetValue(HttpContextBaseKey, out context))
                    {
                        context = new HttpBatchContextWrapper(context, request);
                        request.Properties[HttpBatchContextKey] = context;
                    }
                    else
                    {
                        context = null;
                    }
                }
            }
            else if (!request.Properties.TryGetValue(HttpContextBaseKey, out context))
            {
                context = null;
            }

            return context;
        }

        public static void SetHttpContext(this HttpRequestMessage request, HttpContextBase context)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            request.Properties[HttpContextBaseKey] = context;
        }
    }
}
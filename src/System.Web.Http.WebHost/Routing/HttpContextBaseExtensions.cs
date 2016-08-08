// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.WebHost.Routing
{
    internal static class HttpContextBaseExtensions
    {
        internal static readonly string HttpRequestMessageKey = "MS_HttpRequestMessage";

        public static HttpRequestMessage GetHttpRequestMessage(this HttpContextBase context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (context.Items == null || !context.Items.Contains(HttpRequestMessageKey))
            {
                return null;
            }

            return context.Items[HttpRequestMessageKey] as HttpRequestMessage;
        }

        public static void SetHttpRequestMessage(this HttpContextBase context, HttpRequestMessage request)
        {
            if (context.Items != null)
            {
                context.Items.Add(HttpRequestMessageKey, request);
            }
        }

        public static HttpRequestMessage GetOrCreateHttpRequestMessage(this HttpContextBase context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HttpRequestMessage request = context.GetHttpRequestMessage();
            if (request == null)
            {
                request = HttpControllerHandler.ConvertRequest(context);
                context.SetHttpRequestMessage(request);
            }

            return request;
        }
    }
}

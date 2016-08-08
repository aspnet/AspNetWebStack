// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Owin;

namespace System.Web.Http.Owin
{
    internal static class OwinRequestExtensions
    {
        private const string ContentLengthHeaderName = "Content-Length";
        private const string DisableRequestBufferingKey = "server.DisableRequestBuffering";

        public static void DisableBuffering(this IOwinRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IDictionary<string, object> environment = request.Environment;

            if (environment == null)
            {
                return;
            }

            Action action;

            if (!environment.TryGetValue(DisableRequestBufferingKey, out action))
            {
                return;
            }

            Contract.Assert(action != null);
            action.Invoke();
        }

        public static int? GetContentLength(this IOwinRequest request)
        {
            Contract.Assert(request != null);

            IHeaderDictionary headers = request.Headers;

            if (headers == null)
            {
                return null;
            }

            string[] values;

            if (!headers.TryGetValue(ContentLengthHeaderName, out values))
            {
                return null;
            }

            if (values == null || values.Length != 1)
            {
                return null;
            }

            string value = values[0];

            if (value == null)
            {
                return null;
            }

            int parsed;

            if (!Int32.TryParse(value, out parsed))
            {
                return null;
            }

            if (parsed < 0)
            {
                return null;
            }

            return parsed;
        }
    }
}

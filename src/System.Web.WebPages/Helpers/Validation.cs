// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Helpers
{
    [Obsolete("Use System.Web.HttpRequest.Unvalidated instead.")]
    public static class Validation
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request",
            Justification = "Parameter is only meant for making this show up as 'Request.Unvalidated()', which closely resembles FX45 syntax.")]
#pragma warning disable 0618 // Obsolete System.Web.Helpers.UnvalidatedRequestValues
        public static UnvalidatedRequestValues Unvalidated(this HttpRequestBase request)
#pragma warning restore
        {
            return Unvalidated((HttpRequest)null);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request",
            Justification = "Parameter is only meant for making this show up as 'Request.Unvalidated()', which closely resembles FX45 syntax.")]
#pragma warning disable 0618 // Obsolete System.Web.Helpers.UnvalidatedRequestValues
        public static UnvalidatedRequestValues Unvalidated(this HttpRequest request)
#pragma warning restore
        {
            // We don't actually need the request object; we'll get HttpContext.Current directly.
            HttpContext context = HttpContext.Current;
#pragma warning disable 0618 // Obsolete System.Web.Helpers.UnvalidatedRequestValues
            return new UnvalidatedRequestValues(new HttpRequestWrapper(context.Request));
#pragma warning restore
        }

        public static string Unvalidated(this HttpRequestBase request, string key)
        {
            return Unvalidated(request)[key];
        }

        public static string Unvalidated(this HttpRequest request, string key)
        {
            return Unvalidated(request)[key];
        }
    }
}
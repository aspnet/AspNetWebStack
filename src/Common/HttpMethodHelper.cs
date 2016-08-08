// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http
{
    /// <summary>
    /// Various helper methods for the static members of <see cref="HttpMethod"/>. 
    /// </summary>
    internal static class HttpMethodHelper
    {
        /// <summary>
        /// Gets the static <see cref="HttpMethod"/> instance for any given HTTP method name.
        /// </summary>
        /// <param name="method">The HTTP request method.</param>
        /// <returns>An existing static <see cref="HttpMethod"/> or a new instance if the method was not found.</returns>
        internal static HttpMethod GetHttpMethod(string method)
        {
            if (String.IsNullOrEmpty(method))
            {
                return null;
            }

            if (String.Equals("GET", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Get;
            }

            if (String.Equals("POST", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Post;
            }

            if (String.Equals("PUT", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Put;
            }

            if (String.Equals("DELETE", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Delete;
            }

            if (String.Equals("HEAD", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Head;
            }

            if (String.Equals("OPTIONS", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Options;
            }

            if (String.Equals("TRACE", method, StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Trace;
            }

            return new HttpMethod(method);
        }
    }
}

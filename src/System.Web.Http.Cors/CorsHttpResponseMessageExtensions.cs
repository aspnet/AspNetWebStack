// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Cors;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// CORS-related extension methods for <see cref="HttpResponseMessage"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CorsHttpResponseMessageExtensions
    {
        /// <summary>
        /// Writes the CORS headers on the response.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="corsResult">The <see cref="CorsResult"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// response
        /// or
        /// corsResult
        /// </exception>
        public static void WriteCorsHeaders(this HttpResponseMessage response, CorsResult corsResult)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (corsResult == null)
            {
                throw new ArgumentNullException("corsResult");
            }

            IDictionary<string, string> corsHeaders = corsResult.ToResponseHeaders();
            if (corsHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in corsHeaders)
                {
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
    }
}
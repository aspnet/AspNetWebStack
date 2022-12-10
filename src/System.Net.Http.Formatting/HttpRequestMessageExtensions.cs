// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> wired up to the associated <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller will dispose")]
        public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode statusCode)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return new HttpResponseMessage
            {
                Content = new StreamContent(Stream.Null),
                StatusCode = statusCode,
                RequestMessage = request
            };
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> wired up to the associated <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller will dispose")]
        public static HttpResponseMessage CreateResponse(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return new HttpResponseMessage
            {
                Content = new StreamContent(Stream.Null),
                RequestMessage = request
            };
        }
    }
}

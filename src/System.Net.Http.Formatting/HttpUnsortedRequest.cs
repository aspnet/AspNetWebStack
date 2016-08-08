// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Represents the HTTP Request Line and header parameters parsed by <see cref="HttpRequestLineParser"/>
    /// and <see cref="HttpRequestHeaderParser"/>.
    /// </summary>
    internal class HttpUnsortedRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUnsortedRequest"/> class.
        /// </summary>
        public HttpUnsortedRequest()
        {
            // Collection of unsorted headers. Later we will sort it into the appropriate
            // HttpContentHeaders, HttpRequestHeaders, and HttpResponseHeaders.
            HttpHeaders = new HttpUnsortedHeaders();
        }

        /// <summary>
        /// Gets or sets the HTTP method.
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request URI portion that is carried in the RequestLine (i.e the URI path + query).
        /// </summary>
        /// <value>
        /// The request URI.
        /// </value>
        public string RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        /// <value>
        /// The HTTP version.
        /// </value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets the unsorted HTTP request headers.
        /// </summary>
        public HttpHeaders HttpHeaders { get; private set; }
    }
}

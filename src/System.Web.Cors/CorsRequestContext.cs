// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;

namespace System.Web.Cors
{
    /// <summary>
    /// Provides access to CORS-specific information on the request.
    /// </summary>
    public class CorsRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorsRequestContext"/> class.
        /// </summary>
        public CorsRequestContext()
        {
            AccessControlRequestHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the request URI.
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the request method.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the Origin header value.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the Host header value.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Request-Method header value.
        /// </summary>
        public string AccessControlRequestMethod { get; set; }

        /// <summary>
        /// Gets the Access-Control-Request-Headers header value.
        /// </summary>
        public ISet<string> AccessControlRequestHeaders { get; private set; }

        /// <summary>
        /// Gets a set of properties for the <see cref="CorsRequestContext"/>.
        /// </summary>
        public IDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a preflight request.
        /// </summary>
        public bool IsPreflight
        {
            get
            {
                return Origin != null &&
                    AccessControlRequestMethod != null &&
                    String.Equals(HttpMethod, CorsConstants.PreflightHttpMethod, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Origin: ");
            builder.Append(Origin ?? "null");
            builder.Append(", HttpMethod: ");
            builder.Append(HttpMethod ?? "null");
            builder.Append(", IsPreflight: ");
            builder.Append(IsPreflight);
            builder.Append(", Host: ");
            builder.Append(Host);
            builder.Append(", AccessControlRequestMethod: ");
            builder.Append(AccessControlRequestMethod ?? "null");
            builder.Append(", RequestUri: ");
            builder.Append(RequestUri);
            builder.Append(", AccessControlRequestHeaders: {");
            builder.Append(String.Join(",", AccessControlRequestHeaders));
            builder.Append("}");
            return builder.ToString();
        }
    }
}
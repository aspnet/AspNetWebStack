// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Cors.Properties;

namespace System.Web.Cors
{
    /// <summary>
    /// Results returned by <see cref="CorsEngine"/>.
    /// </summary>
    public class CorsResult
    {
        private long? _preflightMaxAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorsResult"/> class.
        /// </summary>
        public CorsResult()
        {
            AllowedMethods = new List<string>();
            AllowedHeaders = new List<string>();
            AllowedExposedHeaders = new List<string>();
            ErrorMessages = new List<string>();
        }

        /// <summary>
        /// Gets a value indicating whether the result is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return ErrorMessages.Count == 0;
            }
        }

        /// <summary>
        /// Gets the error messages.
        /// </summary>
        public IList<string> ErrorMessages { get; private set; }

        /// <summary>
        /// Gets or sets the allowed origin.
        /// </summary>
        public string AllowedOrigin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource supports user credentials.
        /// </summary>
        public bool SupportsCredentials { get; set; }

        /// <summary>
        /// Gets the allowed methods.
        /// </summary>
        public IList<string> AllowedMethods { get; private set; }

        /// <summary>
        /// Gets the allowed headers.
        /// </summary>
        public IList<string> AllowedHeaders { get; private set; }

        /// <summary>
        /// Gets the allowed headers that can be exposed on the response.
        /// </summary>
        public IList<string> AllowedExposedHeaders { get; private set; }

        /// <summary>
        /// Gets or sets the number of seconds the results of a preflight request can be cached.
        /// </summary>
        public long? PreflightMaxAge
        {
            get
            {
                return _preflightMaxAge;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", SRResources.PreflightMaxAgeOutOfRange);
                }
                _preflightMaxAge = value;
            }
        }

        /// <summary>
        /// Returns CORS-specific headers that should be added to the response.
        /// </summary>
        /// <returns>The response headers.</returns>
        public virtual IDictionary<string, string> ToResponseHeaders()
        {
            IDictionary<string, string> headers = new Dictionary<string, string>();

            if (AllowedOrigin != null)
            {
                headers.Add(CorsConstants.AccessControlAllowOrigin, AllowedOrigin);
            }

            if (SupportsCredentials)
            {
                headers.Add(CorsConstants.AccessControlAllowCredentials, "true");
            }

            if (AllowedMethods.Count > 0)
            {
                // Filter out simple methods
                IEnumerable<string> nonSimpleAllowMethods = AllowedMethods.Where(m =>
                    !CorsConstants.SimpleMethods.Contains(m, StringComparer.OrdinalIgnoreCase));
                AddHeader(headers, CorsConstants.AccessControlAllowMethods, nonSimpleAllowMethods);
            }

            if (AllowedHeaders.Count > 0)
            {
                // Filter out simple request headers
                IEnumerable<string> nonSimpleAllowRequestHeaders = AllowedHeaders.Where(header =>
                    !CorsConstants.SimpleRequestHeaders.Contains(header, StringComparer.OrdinalIgnoreCase));
                AddHeader(headers, CorsConstants.AccessControlAllowHeaders, nonSimpleAllowRequestHeaders);
            }

            if (AllowedExposedHeaders.Count > 0)
            {
                // Filter out simple response headers
                IEnumerable<string> nonSimpleAllowResponseHeaders = AllowedExposedHeaders.Where(header =>
                    !CorsConstants.SimpleResponseHeaders.Contains(header, StringComparer.OrdinalIgnoreCase));
                AddHeader(headers, CorsConstants.AccessControlExposeHeaders, nonSimpleAllowResponseHeaders);
            }

            if (PreflightMaxAge.HasValue)
            {
                headers.Add(CorsConstants.AccessControlMaxAge, PreflightMaxAge.ToString());
            }

            return headers;
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
            builder.Append("IsValid: ");
            builder.Append(IsValid);
            builder.Append(", AllowCredentials: ");
            builder.Append(SupportsCredentials);
            builder.Append(", PreflightMaxAge: ");
            builder.Append(PreflightMaxAge.HasValue ? PreflightMaxAge.Value.ToString(CultureInfo.InvariantCulture) : "null");
            builder.Append(", AllowOrigin: ");
            builder.Append(AllowedOrigin);
            builder.Append(", AllowExposedHeaders: {");
            builder.Append(String.Join(",", AllowedExposedHeaders));
            builder.Append("}");
            builder.Append(", AllowHeaders: {");
            builder.Append(String.Join(",", AllowedHeaders));
            builder.Append("}");
            builder.Append(", AllowMethods: {");
            builder.Append(String.Join(",", AllowedMethods));
            builder.Append("}");
            builder.Append(", ErrorMessages: {");
            builder.Append(String.Join(",", ErrorMessages));
            builder.Append("}");
            return builder.ToString();
        }

        private static void AddHeader(IDictionary<string, string> headers, string headerName, IEnumerable<string> headerValues)
        {
            string methods = String.Join(",", headerValues);
            if (!String.IsNullOrEmpty(methods))
            {
                headers.Add(headerName, methods);
            }
        }
    }
}
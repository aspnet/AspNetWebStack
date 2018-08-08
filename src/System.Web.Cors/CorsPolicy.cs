// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Cors.Properties;

namespace System.Web.Cors
{
    /// <summary>
    /// Defines the policy for Cross-Origin requests based on the CORS specifications.
    /// </summary>
    public class CorsPolicy
    {
        private long? _preflightMaxAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorsPolicy"/> class.
        /// </summary>
        public CorsPolicy()
        {
            ExposedHeaders = new List<string>();
            Headers = new List<string>();
            Methods = new List<string>();
            Origins = new List<string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow all headers.
        /// </summary>
        public bool AllowAnyHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow all methods.
        /// </summary>
        public bool AllowAnyMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow all origins.
        /// </summary>
        public bool AllowAnyOrigin { get; set; }

        /// <summary>
        /// Gets the headers that the resource might use and can be exposed.
        /// </summary>
        public IList<string> ExposedHeaders { get; private set; }

        /// <summary>
        /// Gets the headers that are supported by the resource.
        /// </summary>
        public IList<string> Headers { get; private set; }

        /// <summary>
        /// Gets the methods that are supported by the resource.
        /// </summary>
        public IList<string> Methods { get; private set; }

        /// <summary>
        /// Gets the origins that are allowed to access the resource.
        /// </summary>
        public IList<string> Origins { get; private set; }

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
        /// Gets or sets a value indicating whether the resource supports user credentials in the request.
        /// </summary>
        public bool SupportsCredentials { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("AllowAnyHeader: ");
            builder.Append(AllowAnyHeader);
            builder.Append(", AllowAnyMethod: ");
            builder.Append(AllowAnyMethod);
            builder.Append(", AllowAnyOrigin: ");
            builder.Append(AllowAnyOrigin);
            builder.Append(", PreflightMaxAge: ");
            builder.Append(PreflightMaxAge.HasValue ? PreflightMaxAge.Value.ToString(CultureInfo.InvariantCulture) : "null");
            builder.Append(", SupportsCredentials: ");
            builder.Append(SupportsCredentials);
            builder.Append(", Origins: {");
            builder.Append(String.Join(",", Origins));
            builder.Append("}");
            builder.Append(", Methods: {");
            builder.Append(String.Join(",", Methods));
            builder.Append("}");
            builder.Append(", Headers: {");
            builder.Append(String.Join(",", Headers));
            builder.Append("}");
            builder.Append(", ExposedHeaders: {");
            builder.Append(String.Join(",", ExposedHeaders));
            builder.Append("}");
            return builder.ToString();
        }
    }
}
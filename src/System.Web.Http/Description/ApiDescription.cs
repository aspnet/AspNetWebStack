// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Describes an API defined by relative URI path and HTTP method.
    /// </summary>
    public class ApiDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescription"/> class.
        /// </summary>
        public ApiDescription()
        {
            SupportedRequestBodyFormatters = new Collection<MediaTypeFormatter>();
            SupportedResponseFormatters = new Collection<MediaTypeFormatter>();
            ParameterDescriptions = new Collection<ApiParameterDescription>();
            ResponseDescription = new ResponseDescription();
        }

        /// <summary>
        /// Gets or sets the HTTP method.
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the relative path.
        /// </summary>
        /// <value>
        /// The relative path.
        /// </value>
        public string RelativePath { get; set; }

        /// <summary>
        /// Gets or sets the action descriptor that will handle the API.
        /// </summary>
        /// <value>
        /// The action descriptor.
        /// </value>
        public HttpActionDescriptor ActionDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the registered route for the API.
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        public IHttpRoute Route { get; set; }

        /// <summary>
        /// Gets or sets the documentation of the API.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets the supported response formatters.
        /// </summary>
        public Collection<MediaTypeFormatter> SupportedResponseFormatters { get; internal set; }

        /// <summary>
        /// Gets the supported request body formatters.
        /// </summary>
        public Collection<MediaTypeFormatter> SupportedRequestBodyFormatters { get; internal set; }

        /// <summary>
        /// Gets the parameter descriptions.
        /// </summary>
        public Collection<ApiParameterDescription> ParameterDescriptions { get; internal set; }

        /// <summary>
        /// Gets the response description.
        /// </summary>
        public ResponseDescription ResponseDescription { get; internal set; }

        /// <summary>
        /// Gets the ID. The ID is unique within <see cref="HttpServer"/>.
        /// </summary>
        public string ID
        {
            get
            {
                return (HttpMethod != null ? HttpMethod.Method : String.Empty) +
                    (RelativePath ?? String.Empty);
            }
        }
    }
}
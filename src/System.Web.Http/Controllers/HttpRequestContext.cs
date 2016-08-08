// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    /// <summary>Represents the context associated with a request.</summary>
    public class HttpRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestContext"/> class.
        /// </summary>
        public HttpRequestContext()
        {
            // This is constructor is available to allow placing breakpoints on construction.
        }

        /// <summary>Gets or sets the client certificate.</summary>
        public virtual X509Certificate2 ClientCertificate { get; set; }

        /// <summary>Gets or sets the configuration.</summary>
        public virtual HttpConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether error details, such as exception messages and stack traces,
        /// should be included in the response for this request.
        /// </summary>
        public virtual bool IncludeErrorDetail { get; set; }

        /// <summary>Gets or sets a value indicating whether the request originates from a local address.</summary>
        public virtual bool IsLocal { get; set; }

        /// <summary>Gets or sets the principal.</summary>
        public virtual IPrincipal Principal { get; set; }

        /// <summary>Gets or sets the route data.</summary>
        public virtual IHttpRouteData RouteData { get; set; }

        /// <summary>Gets or sets the factory used to generate URLs to other APIs.</summary>
        public virtual UrlHelper Url { get; set; }

        /// <summary>Gets or sets the virtual path root.</summary>
        public virtual string VirtualPathRoot { get; set; }
    }
}

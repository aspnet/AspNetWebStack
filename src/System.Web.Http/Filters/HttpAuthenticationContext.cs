// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    /// <summary>Represents an authentication context containing information for performing authentication.</summary>
    public class HttpAuthenticationContext
    {
        /// <summary>Initializes a new instance of the <see cref="HttpAuthenticationContext"/> class.</summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="principal">The current principal.</param>
        public HttpAuthenticationContext(HttpActionContext actionContext, IPrincipal principal)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            ActionContext = actionContext;
            Principal = principal;
        }

        /// <summary>Gets the action context.</summary>
        public HttpActionContext ActionContext { get; private set; }

        /// <summary>Gets or sets the authenticated principal.</summary>
        public IPrincipal Principal { get; set; }

        /// <summary>
        /// Gets or sets an action result that will produce an error response (if authentication failed; otherwise,
        /// <see langword="null"/>).
        /// </summary>
        public IHttpActionResult ErrorResult { get; set; }

        /// <summary>Gets the request message.</summary>
        public HttpRequestMessage Request
        {
            get
            {
                Contract.Assert(ActionContext != null);
                return ActionContext.Request;
            }
        }
    }
}

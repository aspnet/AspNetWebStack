// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result for a <see cref="HttpStatusCode.Redirect"/>.
    /// </summary>
    public class RedirectResult : IHttpActionResult
    {
        private readonly Uri _location;
        private readonly StatusCodeResult.IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectResult"/> class with the values provided.
        /// </summary>
        /// <param name="location">The location to which to redirect.</param>
        /// <param name="request">The request message which led to this result.</param>
        public RedirectResult(Uri location, HttpRequestMessage request)
            : this(location, new StatusCodeResult.DirectDependencyProvider(request))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectResult"/> class with the values provided.
        /// </summary>
        /// <param name="location">The location to which to redirect.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public RedirectResult(Uri location, ApiController controller)
            : this(location, new StatusCodeResult.ApiControllerDependencyProvider(controller))
        {
        }

        private RedirectResult(Uri location, StatusCodeResult.IDependencyProvider dependencies)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            Contract.Assert(dependencies != null);

            _location = location;
            _dependencies = dependencies;
        }

        /// <summary>Gets the location at which the content has been created.</summary>
        public Uri Location
        {
            get { return _location; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Redirect);

            try
            {
                response.Headers.Location = _location;
                response.RequestMessage = _dependencies.Request;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }
    }
}

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
    /// Represents an action result that returns an empty <see cref="HttpStatusCode.Conflict"/> response.
    /// </summary>
    public class ConflictResult : IHttpActionResult
    {
        private readonly StatusCodeResult.IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="ConflictResult"/> class.</summary>
        /// <param name="request">The request message which led to this result.</param>
        public ConflictResult(HttpRequestMessage request)
            : this(new StatusCodeResult.DirectDependencyProvider(request))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConflictResult"/> class.</summary>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public ConflictResult(ApiController controller)
            : this(new StatusCodeResult.ApiControllerDependencyProvider(controller))
        {
        }

        private ConflictResult(StatusCodeResult.IDependencyProvider dependencies)
        {
            Contract.Assert(dependencies != null);

            _dependencies = dependencies;
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(StatusCodeResult.Execute(HttpStatusCode.Conflict, _dependencies.Request));
        }
    }
}

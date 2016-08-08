// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Results
{
    /// <summary>Represents an action result that returns a specified HTTP status code.</summary>
    public class StatusCodeResult : IHttpActionResult
    {
        private readonly HttpStatusCode _statusCode;
        private readonly IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="StatusCodeResult"/> class.</summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="request">The request message which led to this result.</param>
        public StatusCodeResult(HttpStatusCode statusCode, HttpRequestMessage request)
            : this(statusCode, new DirectDependencyProvider(request))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="StatusCodeResult"/> class.</summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public StatusCodeResult(HttpStatusCode statusCode, ApiController controller)
            : this(statusCode, new ApiControllerDependencyProvider(controller))
        {
        }

        private StatusCodeResult(HttpStatusCode statusCode, IDependencyProvider dependencies)
        {
            Contract.Assert(dependencies != null);

            _statusCode = statusCode;
            _dependencies = dependencies;
        }

        /// <summary>Gets the HTTP status code for the response message.</summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
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
            return Execute(_statusCode, _dependencies.Request);
        }

        internal static HttpResponseMessage Execute(HttpStatusCode statusCode, HttpRequestMessage request)
        {
            HttpResponseMessage response = new HttpResponseMessage(statusCode);

            try
            {
                response.RequestMessage = request;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }

        /// <summary>Defines a provider for dependencies that are not always directly available.</summary>
        /// <remarks>
        /// This abstraction supports the unit testing scenario of creating the result without creating a request
        /// message. (The ApiController provider implementation does lazy evaluation to make that scenario work.)
        /// </remarks>
        internal interface IDependencyProvider
        {
            HttpRequestMessage Request { get; }
        }

        internal sealed class DirectDependencyProvider : IDependencyProvider
        {
            private readonly HttpRequestMessage _request;

            public DirectDependencyProvider(HttpRequestMessage request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                _request = request;
            }

            public HttpRequestMessage Request
            {
                get { return _request; }
            }
        }

        internal sealed class ApiControllerDependencyProvider : IDependencyProvider
        {
            private readonly ApiController _controller;

            private HttpRequestMessage _request;

            public ApiControllerDependencyProvider(ApiController controller)
            {
                if (controller == null)
                {
                    throw new ArgumentNullException("controller");
                }

                _controller = controller;
            }

            public HttpRequestMessage Request
            {
                get
                {
                    EnsureResolved();
                    return _request;
                }
            }

            private void EnsureResolved()
            {
                if (_request == null)
                {
                    HttpRequestMessage request = _controller.Request;

                    if (request == null)
                    {
                        throw new InvalidOperationException(SRResources.ApiController_RequestMustNotBeNull);
                    }

                    _request = request;
                }
            }
        }
    }
}

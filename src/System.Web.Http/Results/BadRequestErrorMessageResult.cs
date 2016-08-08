// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns a <see cref="HttpStatusCode.BadRequest"/> response and performs
    /// content negotiation on an <see cref="HttpError"/> with a <see cref="HttpError.Message"/>.
    /// </summary>
    public class BadRequestErrorMessageResult : IHttpActionResult
    {
        private readonly string _message;
        private readonly NegotiatedContentResult<HttpError>.IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="BadRequestErrorMessageResult"/> class.</summary>
        /// <param name="message">The user-visible error message.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public BadRequestErrorMessageResult(string message, IContentNegotiator contentNegotiator,
            HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : this(message, new NegotiatedContentResult<HttpError>.DirectDependencyProvider(contentNegotiator, request,
                formatters))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BadRequestErrorMessageResult"/> class.</summary>
        /// <param name="message">The user-visible error message.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public BadRequestErrorMessageResult(string message, ApiController controller)
            : this(message, new NegotiatedContentResult<HttpError>.ApiControllerDependencyProvider(controller))
        {
        }

        private BadRequestErrorMessageResult(string message,
            NegotiatedContentResult<HttpError>.IDependencyProvider dependencies)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Contract.Assert(dependencies != null);

            _message = message;
            _dependencies = dependencies;
        }

        /// <summary>Gets the user-visible error message.</summary>
        public string Message
        {
            get { return _message; }
        }

        /// <summary>Gets the content negotiator to handle content negotiation.</summary>
        public IContentNegotiator ContentNegotiator
        {
            get { return _dependencies.ContentNegotiator; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <summary>Gets the formatters to use to negotiate and format the content.</summary>
        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get { return _dependencies.Formatters; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpError error = new HttpError(_message);
            return NegotiatedContentResult<HttpError>.Execute(HttpStatusCode.BadRequest, error,
                _dependencies.ContentNegotiator, _dependencies.Request, _dependencies.Formatters);
        }
    }
}

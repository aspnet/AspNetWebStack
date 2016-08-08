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
    /// Represents an action result that performs content negotiation and returns an <see cref="HttpStatusCode.OK"/>
    /// response when it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class OkNegotiatedContentResult<T> : IHttpActionResult
    {
        private readonly T _content;
        private readonly NegotiatedContentResult<T>.IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="OkNegotiatedContentResult{T}"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public OkNegotiatedContentResult(T content, IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
            : this(content, new NegotiatedContentResult<T>.DirectDependencyProvider(contentNegotiator, request,
            formatters))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OkNegotiatedContentResult{T}"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public OkNegotiatedContentResult(T content, ApiController controller)
            : this(content, new NegotiatedContentResult<T>.ApiControllerDependencyProvider(controller))
        {
        }

        private OkNegotiatedContentResult(T content, NegotiatedContentResult<T>.IDependencyProvider dependencies)
        {
            Contract.Assert(dependencies != null);

            _content = content;
            _dependencies = dependencies;
        }

        /// <summary>Gets the content value to negotiate and format in the entity body.</summary>
        public T Content
        {
            get { return _content; }
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
            return Task.FromResult(NegotiatedContentResult<T>.Execute(HttpStatusCode.OK, _content,
                _dependencies.ContentNegotiator, _dependencies.Request, _dependencies.Formatters));
        }
    }
}

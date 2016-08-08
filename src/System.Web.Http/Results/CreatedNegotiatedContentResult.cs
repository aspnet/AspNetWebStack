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
    /// Represents an action result that performs content negotiation and returns a
    /// <see cref="HttpStatusCode.Created"/> response when it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class CreatedNegotiatedContentResult<T> : IHttpActionResult
    {
        private readonly Uri _location;
        private readonly T _content;
        private readonly NegotiatedContentResult<T>.IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedNegotiatedContentResult{T}"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="location">The location at which the content has been created.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public CreatedNegotiatedContentResult(Uri location, T content, IContentNegotiator contentNegotiator,
            HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : this(location, content, new NegotiatedContentResult<T>.DirectDependencyProvider(contentNegotiator,
                request, formatters))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedNegotiatedContentResult{T}"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="location">The location at which the content has been created.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public CreatedNegotiatedContentResult(Uri location, T content, ApiController controller)
            : this(location, content, new NegotiatedContentResult<T>.ApiControllerDependencyProvider(controller))
        {
        }

        private CreatedNegotiatedContentResult(Uri location, T content,
            NegotiatedContentResult<T>.IDependencyProvider dependencies)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            Contract.Assert(dependencies != null);

            _location = location;
            _content = content;
            _dependencies = dependencies;
        }

        /// <summary>Gets the location at which the content has been created.</summary>
        public Uri Location
        {
            get { return _location; }
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
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            // Run content negotiation.
            ContentNegotiationResult result = _dependencies.ContentNegotiator.Negotiate(typeof(T),
                _dependencies.Request, _dependencies.Formatters);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                if (result == null)
                {
                    // A null result from content negotiation indicates that the response should be a 406.
                    response.StatusCode = HttpStatusCode.NotAcceptable;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.Created;
                    response.Headers.Location = _location;
                    Contract.Assert(result.Formatter != null);
                    // At this point mediaType should be a cloned value. (The content negotiator is responsible for
                    // returning a new copy.)
                    response.Content = new ObjectContent<T>(_content, result.Formatter, result.MediaType);
                }

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

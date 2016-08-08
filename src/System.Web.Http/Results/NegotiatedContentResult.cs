// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Results
{
    /// <summary>Represents an action result that performs content negotiation.</summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class NegotiatedContentResult<T> : IHttpActionResult
    {
        private readonly HttpStatusCode _statusCode;
        private readonly T _content;
        private readonly IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="NegotiatedContentResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public NegotiatedContentResult(HttpStatusCode statusCode, T content, IContentNegotiator contentNegotiator,
            HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : this(statusCode, content, new DirectDependencyProvider(contentNegotiator, request, formatters))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NegotiatedContentResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public NegotiatedContentResult(HttpStatusCode statusCode, T content, ApiController controller)
            : this(statusCode, content, new ApiControllerDependencyProvider(controller))
        {
        }

        private NegotiatedContentResult(HttpStatusCode statusCode, T content, IDependencyProvider dependencies)
        {
            Contract.Assert(dependencies != null);

            _statusCode = statusCode;
            _content = content;
            _dependencies = dependencies;
        }

        /// <summary>Gets the HTTP status code for the response message.</summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
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
            return Execute(_statusCode, _content, _dependencies.ContentNegotiator, _dependencies.Request,
                _dependencies.Formatters);
        }

        internal static HttpResponseMessage Execute(HttpStatusCode statusCode, T content,
            IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
        {
            Contract.Assert(contentNegotiator != null);
            Contract.Assert(request != null);
            Contract.Assert(formatters != null);

            // Run content negotiation.
            ContentNegotiationResult result = contentNegotiator.Negotiate(typeof(T), request, formatters);

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
                    response.StatusCode = statusCode;
                    Contract.Assert(result.Formatter != null);
                    // At this point mediaType should be a cloned value. (The content negotiator is responsible for
                    // returning a new copy.)
                    response.Content = new ObjectContent<T>(content, result.Formatter, result.MediaType);
                }

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
        /// This abstraction supports the unit testing scenario of creating the result without creating a content
        /// negotiator, request message, or formatters. (The ApiController provider implementation does lazy evaluation
        /// to make that scenario work.)
        /// </remarks>
        internal interface IDependencyProvider
        {
            IContentNegotiator ContentNegotiator { get; }

            HttpRequestMessage Request { get; }

            IEnumerable<MediaTypeFormatter> Formatters { get; }
        }

        internal sealed class DirectDependencyProvider : IDependencyProvider
        {
            private readonly IContentNegotiator _contentNegotiator;
            private readonly HttpRequestMessage _request;
            private readonly IEnumerable<MediaTypeFormatter> _formatters;

            public DirectDependencyProvider(IContentNegotiator contentNegotiator, HttpRequestMessage request,
                IEnumerable<MediaTypeFormatter> formatters)
            {
                if (contentNegotiator == null)
                {
                    throw new ArgumentNullException("contentNegotiator");
                }

                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (formatters == null)
                {
                    throw new ArgumentNullException("formatters");
                }

                _contentNegotiator = contentNegotiator;
                _request = request;
                _formatters = formatters;
            }

            public IContentNegotiator ContentNegotiator
            {
                get { return _contentNegotiator; }
            }

            public HttpRequestMessage Request
            {
                get { return _request; }
            }

            public IEnumerable<MediaTypeFormatter> Formatters
            {
                get { return _formatters; }
            }
        }

        internal sealed class ApiControllerDependencyProvider : IDependencyProvider
        {
            private readonly ApiController _controller;

            private IDependencyProvider _resolvedDependencies;

            public ApiControllerDependencyProvider(ApiController controller)
            {
                if (controller == null)
                {
                    throw new ArgumentNullException("controller");
                }

                _controller = controller;
            }

            public IContentNegotiator ContentNegotiator
            {
                get
                {
                    EnsureResolved();
                    return _resolvedDependencies.ContentNegotiator;
                }
            }

            public HttpRequestMessage Request
            {
                get
                {
                    EnsureResolved();
                    return _resolvedDependencies.Request;
                }
            }

            public IEnumerable<MediaTypeFormatter> Formatters
            {
                get
                {
                    EnsureResolved();
                    return _resolvedDependencies.Formatters;
                }
            }

            private void EnsureResolved()
            {
                if (_resolvedDependencies == null)
                {
                    HttpConfiguration configuration = _controller.Configuration;

                    if (configuration == null)
                    {
                        throw new InvalidOperationException(
                            SRResources.HttpControllerContext_ConfigurationMustNotBeNull);
                    }

                    ServicesContainer services = configuration.Services;
                    Contract.Assert(services != null);
                    IContentNegotiator contentNegotiator = services.GetContentNegotiator();

                    if (contentNegotiator == null)
                    {
                        throw new InvalidOperationException(Error.Format(
                            SRResources.HttpRequestMessageExtensions_NoContentNegotiator, typeof(IContentNegotiator)));
                    }

                    HttpRequestMessage request = _controller.Request;

                    if (request == null)
                    {
                        throw new InvalidOperationException(SRResources.ApiController_RequestMustNotBeNull);
                    }

                    IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;
                    Contract.Assert(formatters != null);

                    _resolvedDependencies = new DirectDependencyProvider(contentNegotiator, request, formatters);
                }
            }
        }
    }
}

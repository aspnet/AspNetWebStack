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
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns a <see cref="HttpStatusCode.InternalServerError"/> response and
    /// performs content negotiation on an <see cref="HttpError"/> based on an <see cref="Exception"/>.
    /// </summary>
    public class ExceptionResult : IHttpActionResult
    {
        private readonly Exception _exception;
        private readonly IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="ExceptionResult"/> class.</summary>
        /// <param name="exception">The exception to include in the error.</param>
        /// <param name="includeErrorDetail">
        /// <see langword="true"/> if the error should include exception messages; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public ExceptionResult(Exception exception, bool includeErrorDetail, IContentNegotiator contentNegotiator,
            HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : this(exception, new DirectDependencyProvider(includeErrorDetail, contentNegotiator, request,
                formatters))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExceptionResult"/> class.</summary>
        /// <param name="exception">The exception to include in the error.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public ExceptionResult(Exception exception, ApiController controller)
            : this(exception, new ApiControllerDependencyProvider(controller))
        {
        }

        private ExceptionResult(Exception exception, IDependencyProvider dependencies)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Contract.Assert(dependencies != null);

            _exception = exception;
            _dependencies = dependencies;
        }

        /// <summary>Gets the exception to include in the error.</summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>Gets a value indicating whether the error should include exception messages.</summary>
        public bool IncludeErrorDetail
        {
            get { return _dependencies.IncludeErrorDetail; }
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
            HttpError error = new HttpError(_exception, _dependencies.IncludeErrorDetail);
            return NegotiatedContentResult<HttpError>.Execute(HttpStatusCode.InternalServerError, error,
                _dependencies.ContentNegotiator, _dependencies.Request, _dependencies.Formatters);
        }

        /// <summary>Defines a provider for dependencies that are not always directly available.</summary>
        /// <remarks>
        /// This abstraction supports the unit testing scenario of creating the result without creating a content
        /// negotiator, request message, or formatters. (The ApiController provider implementation does lazy evaluation
        /// to make that scenario work.)
        /// </remarks>
        internal interface IDependencyProvider
        {
            bool IncludeErrorDetail { get; }

            IContentNegotiator ContentNegotiator { get; }

            HttpRequestMessage Request { get; }

            IEnumerable<MediaTypeFormatter> Formatters { get; }
        }

        internal sealed class DirectDependencyProvider : IDependencyProvider
        {
            private readonly bool _includeErrorDetail;
            private readonly IContentNegotiator _contentNegotiator;
            private readonly HttpRequestMessage _request;
            private readonly IEnumerable<MediaTypeFormatter> _formatters;

            public DirectDependencyProvider(bool includeErrorDetail, IContentNegotiator contentNegotiator,
                HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
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

                _includeErrorDetail = includeErrorDetail;
                _contentNegotiator = contentNegotiator;
                _request = request;
                _formatters = formatters;
            }

            public bool IncludeErrorDetail
            {
                get { return _includeErrorDetail; }
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

            public bool IncludeErrorDetail
            {
                get
                {
                    EnsureResolved();
                    return _resolvedDependencies.IncludeErrorDetail;
                }
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
                    HttpRequestContext requestContext = _controller.RequestContext;
                    Contract.Assert(requestContext != null);
                    bool includeErrorDetail = requestContext.IncludeErrorDetail;

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

                    _resolvedDependencies = new DirectDependencyProvider(includeErrorDetail, contentNegotiator, request,
                        formatters);
                }
            }
        }
    }
}

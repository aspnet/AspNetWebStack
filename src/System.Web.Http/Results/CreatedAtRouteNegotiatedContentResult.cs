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
using System.Web.Http.Routing;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that performs route generation and content negotiation and returns a
    /// <see cref="HttpStatusCode.Created"/> response when content negotiation succeeds.
    /// </summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class CreatedAtRouteNegotiatedContentResult<T> : IHttpActionResult
    {
        private readonly string _routeName;
        private readonly IDictionary<string, object> _routeValues;
        private readonly T _content;
        private readonly IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> class with the
        /// values provided.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="urlFactory">The factory to use to generate the route URL.</param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public CreatedAtRouteNegotiatedContentResult(string routeName, IDictionary<string, object> routeValues,
            T content, UrlHelper urlFactory, IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
            : this(routeName, routeValues, content, new DirectDependencyProvider(urlFactory, contentNegotiator,
                request, formatters))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> class with the
        /// values provided.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public CreatedAtRouteNegotiatedContentResult(string routeName, IDictionary<string, object> routeValues,
            T content, ApiController controller)
            : this(routeName, routeValues, content, new ApiControllerDependencyProvider(controller))
        {
        }

        private CreatedAtRouteNegotiatedContentResult(string routeName, IDictionary<string, object> routeValues,
            T content, IDependencyProvider dependencies)
        {
            if (routeName == null)
            {
                throw new ArgumentNullException("routeName");
            }

            Contract.Assert(dependencies != null);

            _routeName = routeName;
            _routeValues = routeValues;
            _content = content;
            _dependencies = dependencies;
        }

        /// <summary>Gets the name of the route to use for generating the URL.</summary>
        public string RouteName
        {
            get { return _routeName; }
        }

        /// <summary>Gets the route data to use for generating the URL.</summary>
        public IDictionary<string, object> RouteValues
        {
            get { return _routeValues; }
        }

        /// <summary>Gets the content value to negotiate and format in the entity body.</summary>
        public T Content
        {
            get { return _content; }
        }

        /// <summary>Gets the factory to use to generate the route URL.</summary>
        public UrlHelper UrlFactory
        {
            get { return _dependencies.UrlFactory; }
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

                    string link = _dependencies.UrlFactory.Link(_routeName, _routeValues);

                    if (link == null)
                    {
                        throw new InvalidOperationException(SRResources.UrlHelper_LinkMustNotReturnNull);
                    }

                    response.Headers.Location = new Uri(link);
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

        /// <summary>Defines a provider for dependencies that are not always directly available.</summary>
        /// <remarks>
        /// This abstraction supports the unit testing scenario of creating the result without creating a content
        /// negotiator, request message, or formatters. (The ApiController provider implementation does lazy evaluation
        /// to make that scenario work.)
        /// </remarks>
        private interface IDependencyProvider
        {
            UrlHelper UrlFactory { get; }

            IContentNegotiator ContentNegotiator { get; }

            HttpRequestMessage Request { get; }

            IEnumerable<MediaTypeFormatter> Formatters { get; }
        }

        private sealed class DirectDependencyProvider : IDependencyProvider
        {
            private readonly UrlHelper _urlFactory;
            private readonly IContentNegotiator _contentNegotiator;
            private readonly HttpRequestMessage _request;
            private readonly IEnumerable<MediaTypeFormatter> _formatters;

            public DirectDependencyProvider(UrlHelper urlFactory, IContentNegotiator contentNegotiator,
                HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            {
                if (urlFactory == null)
                {
                    throw new ArgumentNullException("urlFactory");
                }

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

                _urlFactory = urlFactory;
                _contentNegotiator = contentNegotiator;
                _request = request;
                _formatters = formatters;
            }

            public UrlHelper UrlFactory
            {
                get { return _urlFactory; }
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

        private sealed class ApiControllerDependencyProvider : IDependencyProvider
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

            public UrlHelper UrlFactory
            {
                get
                {
                    EnsureResolved();
                    return _resolvedDependencies.UrlFactory;
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
                    HttpRequestMessage request = _controller.Request;

                    if (request == null)
                    {
                        throw new InvalidOperationException(SRResources.ApiController_RequestMustNotBeNull);
                    }

                    UrlHelper urlFactory = _controller.Url ?? new UrlHelper(request);

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

                    IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;
                    Contract.Assert(formatters != null);

                    _resolvedDependencies = new DirectDependencyProvider(urlFactory, contentNegotiator, request,
                        formatters);
                }
            }
        }
    }
}

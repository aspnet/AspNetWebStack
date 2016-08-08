// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that performs route generation and returns a <see cref="HttpStatusCode.Redirect"/>
    /// response.
    /// </summary>
    public class RedirectToRouteResult : IHttpActionResult
    {
        private readonly string _routeName;
        private readonly IDictionary<string, object> _routeValues;
        private readonly IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> class with the values provided.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="urlFactory">The factory to use to generate the route URL.</param>
        /// <param name="request">The request message which led to this result.</param>
        public RedirectToRouteResult(string routeName, IDictionary<string, object> routeValues, UrlHelper urlFactory,
            HttpRequestMessage request)
            : this(routeName, routeValues, new DirectDependencyProvider(urlFactory, request))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> class with the values provided.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public RedirectToRouteResult(string routeName, IDictionary<string, object> routeValues,
            ApiController controller)
            : this(routeName, routeValues, new ApiControllerDependencyProvider(controller))
        {
        }

        private RedirectToRouteResult(string routeName, IDictionary<string, object> routeValues,
            IDependencyProvider dependencies)
        {
            if (routeName == null)
            {
                throw new ArgumentNullException("routeName");
            }

            Contract.Assert(dependencies != null);

            _routeName = routeName;
            _routeValues = routeValues;
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

        /// <summary>Gets the factory to use to generate the route URL.</summary>
        public UrlHelper UrlFactory
        {
            get { return _dependencies.UrlFactory; }
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
                string link = _dependencies.UrlFactory.Link(_routeName, _routeValues);

                if (link == null)
                {
                    throw new InvalidOperationException(SRResources.UrlHelper_LinkMustNotReturnNull);
                }

                response.Headers.Location = new Uri(link);
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

            HttpRequestMessage Request { get; }
        }

        private sealed class DirectDependencyProvider : IDependencyProvider
        {
            private readonly UrlHelper _urlFactory;
            private readonly HttpRequestMessage _request;

            public DirectDependencyProvider(UrlHelper urlFactory, HttpRequestMessage request)
            {
                if (urlFactory == null)
                {
                    throw new ArgumentNullException("urlFactory");
                }

                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                _urlFactory = urlFactory;
                _request = request;
            }

            public UrlHelper UrlFactory
            {
                get { return _urlFactory; }
            }

            public HttpRequestMessage Request
            {
                get { return _request; }
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

            public HttpRequestMessage Request
            {
                get
                {
                    EnsureResolved();
                    return _resolvedDependencies.Request;
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

                    _resolvedDependencies = new DirectDependencyProvider(urlFactory, request);
                }
            }
        }
    }
}

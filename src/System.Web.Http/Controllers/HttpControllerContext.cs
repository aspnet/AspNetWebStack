// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Contains information for a single HTTP operation.
    /// </summary>
    public class HttpControllerContext
    {
        private HttpRequestContext _requestContext;
        private HttpRequestMessage _request;
        private HttpControllerDescriptor _controllerDescriptor;
        private IHttpController _controller;

        /// <summary>Initializes a new instance of the <see cref="HttpControllerContext"/> class.</summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="request">The HTTP request.</param>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="controller">The controller.</param>
        public HttpControllerContext(HttpRequestContext requestContext, HttpRequestMessage request,
            HttpControllerDescriptor controllerDescriptor, IHttpController controller)
        {
            if (requestContext == null)
            {
                throw Error.ArgumentNull("requestContext");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            if (controller == null)
            {
                throw Error.ArgumentNull("controller");
            }

            _requestContext = requestContext;
            _request = request;
            _controllerDescriptor = controllerDescriptor;
            _controller = controller;
        }

        public HttpControllerContext(HttpConfiguration configuration, IHttpRouteData routeData,
            HttpRequestMessage request)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            _requestContext = new HttpRequestContext
            {
                Configuration = configuration,
                RouteData = routeData
            };
            _request = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerContext"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpControllerContext()
        {
            // Note: This constructor is also used by a FormDataCollectionExtensions where we attempt to create a 
            // controller context with incomplete request context.
            _requestContext = new HttpRequestContext();
        }

        public HttpConfiguration Configuration
        {
            get
            {
                return _requestContext.Configuration;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _requestContext.Configuration = value;
            }
        }

        /// <summary>
        /// Gets or sets the controller descriptor.
        /// </summary>
        /// <value>
        /// The controller descriptor.
        /// </value>
        public HttpControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _controllerDescriptor = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP controller.
        /// </summary>
        /// <value>
        /// The HTTP controller.
        /// </value>
        public IHttpController Controller
        {
            get { return _controller; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _controller = value;
            }
        }

        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _request = value;
            }
        }

        /// <summary>Gets or sets the request context.</summary>
        public HttpRequestContext RequestContext
        {
            get { return _requestContext; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _requestContext = value;
            }
        }

        public IHttpRouteData RouteData
        {
            get { return _requestContext.RouteData; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _requestContext.RouteData = value;
            }
        }
    }
}

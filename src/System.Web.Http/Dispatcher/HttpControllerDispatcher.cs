// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Dispatches an incoming <see cref="HttpRequestMessage"/> to an <see cref="IHttpController"/> implementation for processing.
    /// </summary>
    public class HttpControllerDispatcher : HttpMessageHandler
    {
        private readonly HttpConfiguration _configuration;

        private IExceptionLogger _exceptionLogger;
        private IExceptionHandler _exceptionHandler;
        private IHttpControllerSelector _controllerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerDispatcher"/> class.
        /// </summary>
        public HttpControllerDispatcher(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Gets the <see cref="HttpConfiguration"/>.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <remarks>This property is internal and settable only for unit testing purposes.</remarks>
        internal IExceptionLogger ExceptionLogger
        {
            get
            {
                if (_exceptionLogger == null)
                {
                    _exceptionLogger = ExceptionServices.GetLogger(_configuration);
                }

                return _exceptionLogger;
            }
            set
            {
                _exceptionLogger = value;
            }
        }

        /// <remarks>This property is internal and settable only for unit testing purposes.</remarks>
        internal IExceptionHandler ExceptionHandler
        {
            get
            {
                if (_exceptionHandler == null)
                {
                    _exceptionHandler = ExceptionServices.GetHandler(_configuration);
                }

                return _exceptionHandler;
            }
            set
            {
                _exceptionHandler = value;
            }
        }

        private IHttpControllerSelector ControllerSelector
        {
            get
            {
                if (_controllerSelector == null)
                {
                    _controllerSelector = _configuration.Services.GetHttpControllerSelector();
                }

                return _controllerSelector;
            }
        }

        /// <summary>
        /// Dispatches an incoming <see cref="HttpRequestMessage"/> to an <see cref="IHttpController"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the ongoing operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We report the error in the HTTP response.")]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ExceptionDispatchInfo exceptionInfo;
            HttpControllerContext controllerContext = null;

            try
            {
                HttpControllerDescriptor controllerDescriptor = ControllerSelector.SelectController(request);
                if (controllerDescriptor == null)
                {
                    return request.CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                        SRResources.NoControllerSelected);
                }

                IHttpController controller = controllerDescriptor.CreateController(request);
                if (controller == null)
                {
                    return request.CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                        SRResources.NoControllerCreated);
                }

                controllerContext = CreateControllerContext(request, controllerDescriptor, controller);
                return await controller.ExecuteAsync(controllerContext, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Propogate the canceled task without calling exception loggers or handlers.
                throw;
            }
            catch (HttpResponseException httpResponseException)
            {
                return httpResponseException.Response;
            }
            catch (Exception exception)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            Debug.Assert(exceptionInfo.SourceException != null);

            ExceptionContext exceptionContext = new ExceptionContext(
                exceptionInfo.SourceException,
                ExceptionCatchBlocks.HttpControllerDispatcher,
                request)
                {
                    ControllerContext = controllerContext,
                };

            await ExceptionLogger.LogAsync(exceptionContext, cancellationToken);
            HttpResponseMessage response = await ExceptionHandler.HandleAsync(exceptionContext, cancellationToken);

            if (response == null)
            {
                exceptionInfo.Throw();
            }

            return response;
        }

        private static HttpControllerContext CreateControllerContext(
            HttpRequestMessage request, 
            HttpControllerDescriptor controllerDescriptor,
            IHttpController controller)
        {
            Contract.Assert(request != null);
            Contract.Assert(controllerDescriptor != null);
            Contract.Assert(controller != null);

            HttpConfiguration controllerConfiguration = controllerDescriptor.Configuration;

            // Set the controller configuration on the request properties
            HttpConfiguration requestConfig = request.GetConfiguration();
            if (requestConfig == null)
            {
                request.SetConfiguration(controllerConfiguration);
            }
            else
            {
                if (requestConfig != controllerConfiguration)
                {
                    request.SetConfiguration(controllerConfiguration);
                }
            }

            HttpRequestContext requestContext = request.GetRequestContext();

            // if the host doesn't create the context we will fallback to creating it.
            if (requestContext == null)
            {
                requestContext = new RequestBackedHttpRequestContext(request)
                {
                    // we are caching controller configuration to support per controller configuration.
                    Configuration = controllerConfiguration,
                };

                // if the host did not set a request context we will also set it back to the request.
                request.SetRequestContext(requestContext);
            }

            return new HttpControllerContext(requestContext, request, controllerDescriptor, controller);
        }

        private static HttpConfiguration EnsureNonNull(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            return configuration;
        }
    }
}

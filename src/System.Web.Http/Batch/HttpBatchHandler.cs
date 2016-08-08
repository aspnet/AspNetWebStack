// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;

namespace System.Web.Http.Batch
{
    /// <summary>
    /// Defines the abstraction for handling HTTP batch requests.
    /// </summary>
    // Class Hierarchy
    // - HttpBatchHandler
    //   - DefaultHttpBatchHandler
    //   - ODataBatchHandler
    //     - DefaultODataBatchHandler
    //     - UnbufferedODataBatchHandler
    public abstract class HttpBatchHandler : HttpMessageHandler
    {
        private readonly HttpServer _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        protected HttpBatchHandler(HttpServer httpServer)
        {
            if (httpServer == null)
            {
                throw Error.ArgumentNull("httpServer");
            }

            _server = httpServer;
            Invoker = new HttpMessageInvoker(httpServer);
        }

        /// <summary>
        /// Gets the invoker to send the batch requests to the <see cref="HttpServer"/>.
        /// </summary>
        public HttpMessageInvoker Invoker { get; private set; }

        /// <remarks>This property is internal and settable only for unit testing purposes.</remarks>
        internal IExceptionLogger ExceptionLogger
        {
            get
            {
                return _server.ExceptionLogger;
            }
            set
            {
                _server.ExceptionLogger = value;
            }
        }

        /// <remarks>This property is internal and settable only for unit testing purposes.</remarks>
        internal IExceptionHandler ExceptionHandler
        {
            get
            {
                return _server.ExceptionHandler;
            }
            set
            {
                _server.ExceptionHandler = value;
            }
        }

        /// <inheritdoc/>
        protected sealed override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[HttpPropertyKeys.IsBatchRequest] = true;

            ExceptionDispatchInfo exceptionInfo;

            try
            {
                return await ProcessBatchAsync(request, cancellationToken);
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

            ExceptionContext exceptionContext = new ExceptionContext(exceptionInfo.SourceException,
                ExceptionCatchBlocks.HttpBatchHandler, request);
            await ExceptionLogger.LogAsync(exceptionContext, cancellationToken);
            HttpResponseMessage response = await ExceptionHandler.HandleAsync(exceptionContext, cancellationToken);

            if (response == null)
            {
                exceptionInfo.Throw();
            }

            return response;
        }

        /// <summary>
        /// Processes the incoming batch request as a single <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The batch request.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The batch response.</returns>
        public abstract Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
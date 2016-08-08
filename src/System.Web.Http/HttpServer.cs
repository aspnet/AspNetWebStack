// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Properties;

namespace System.Web.Http
{
    /// <summary>
    /// Defines an implementation of an <see cref="HttpMessageHandler"/> which dispatches an 
    /// incoming <see cref="HttpRequestMessage"/> and creates an <see cref="HttpResponseMessage"/> as a result.
    /// </summary>
    public class HttpServer : DelegatingHandler
    {
        // _anonymousPrincipal needs thread-safe intialization so use a static field initializer
        private static readonly IPrincipal _anonymousPrincipal = new GenericPrincipal(new GenericIdentity(String.Empty), new string[0]);

        private readonly HttpConfiguration _configuration;
        private readonly HttpMessageHandler _dispatcher;

        private IExceptionLogger _exceptionLogger;
        private IExceptionHandler _exceptionHandler;
        private bool _disposed;
        private bool _initialized = false;
        private object _initializationLock = new object();
        private object _initializationTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class with default configuration and dispatcher.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The configuration object is disposed as part of this class.")]
        public HttpServer()
            : this(new HttpConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class with default dispatcher.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure this <see cref="HttpServer"/> instance.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The configuration object is disposed as part of this class.")]
        public HttpServer(HttpConfiguration configuration)
            : this(configuration, new HttpRoutingDispatcher(configuration))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class with a custom dispatcher.
        /// </summary>
        /// <param name="dispatcher">Http dispatcher responsible for handling incoming requests.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The configuration object is disposed as part of this class.")]
        public HttpServer(HttpMessageHandler dispatcher)
            : this(new HttpConfiguration(), dispatcher)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure this <see cref="HttpServer"/> instance.</param>
        /// <param name="dispatcher">Http dispatcher responsible for handling incoming requests.</param>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "principal",
            Justification = "Must access Thread.CurrentPrincipal to work around problem in .NET 4.5")]
        public HttpServer(HttpConfiguration configuration, HttpMessageHandler dispatcher)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (dispatcher == null)
            {
                throw Error.ArgumentNull("dispatcher");
            }

            // Read the thread principal to work around a problem up to .NET 4.5.1 that CurrentPrincipal creates a new instance each time it is read in async 
            // code if it has not been queried from the spawning thread.
            IPrincipal principal = Thread.CurrentPrincipal;

            _dispatcher = dispatcher;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        public HttpMessageHandler Dispatcher
        {
            get { return _dispatcher; }
        }

        /// <summary>
        /// Gets the <see cref="HttpConfiguration"/>.
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <remarks>This property is settable only for unit testing purposes.</remarks>
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

        /// <remarks>This property is settable only for unit testing purposes.</remarks>
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged SRResources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _configuration.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Dispatches an incoming <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the ongoing operation.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "we are converting exceptions to error responses.")]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (_disposed)
            {
                return request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, SRResources.HttpServerDisposed);
            }

            // The first request initializes the server
            EnsureInitialized();

            // Capture current synchronization context and add it as a parameter to the request
            SynchronizationContext context = SynchronizationContext.Current;
            if (context != null)
            {
                request.SetSynchronizationContext(context);
            }

            // Add HttpConfiguration object as a parameter to the request 
            request.SetConfiguration(_configuration);

            // Ensure we have a principal, even if the host didn't give us one
            IPrincipal originalPrincipal = Thread.CurrentPrincipal;
            if (originalPrincipal == null)
            {
                Thread.CurrentPrincipal = _anonymousPrincipal;
            }

            // Ensure we have a principal on the request context (if there is a request context).
            HttpRequestContext requestContext = request.GetRequestContext();

            if (requestContext == null)
            {
                requestContext = new RequestBackedHttpRequestContext(request);

                // if the host did not set a request context we will also set it back to the request.
                request.SetRequestContext(requestContext);
            }

            try
            {
                ExceptionDispatchInfo exceptionInfo;

                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Propogate the canceled task without calling exception loggers or handlers.
                    throw;
                }
                catch (HttpResponseException exception)
                {
                    return exception.Response;
                }
                catch (Exception exception)
                {
                    exceptionInfo = ExceptionDispatchInfo.Capture(exception);
                }

                Debug.Assert(exceptionInfo.SourceException != null);

                ExceptionContext exceptionContext = new ExceptionContext(exceptionInfo.SourceException,
                    ExceptionCatchBlocks.HttpServer, request);
                await ExceptionLogger.LogAsync(exceptionContext, cancellationToken);
                HttpResponseMessage response = await ExceptionHandler.HandleAsync(exceptionContext,
                    cancellationToken);

                if (response == null)
                {
                    exceptionInfo.Throw();
                }

                return response;
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }
        }

        private void EnsureInitialized()
        {
            LazyInitializer.EnsureInitialized(ref _initializationTarget, ref _initialized, ref _initializationLock, () =>
            {
                Initialize();
                return null;
            });
        }

        /// <summary>
        /// Prepares the server for operation.
        /// </summary>
        /// <remarks>
        /// This method must be called after all configuration is complete
        /// but before the first request is processed.
        /// </remarks>
        protected virtual void Initialize()
        {
            // Do final initialization of the configuration.
            // It is considered immutable from this point forward.
            _configuration.EnsureInitialized();

            // Create pipeline
            InnerHandler = HttpClientFactory.CreatePipeline(_dispatcher, _configuration.MessageHandlers);
        }

        private static HttpConfiguration EnsureNonNull(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return configuration;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Owin.ExceptionHandling;
using System.Web.Http.Owin.Properties;
using Microsoft.Owin;

namespace System.Web.Http.Owin
{
    /// <summary>
    /// Represents an OWIN component that submits requests to an <see cref="HttpMessageHandler"/> when invoked.
    /// </summary>
    public class HttpMessageHandlerAdapter : OwinMiddleware, IDisposable
    {
        private readonly HttpMessageHandler _messageHandler;
        private readonly HttpMessageInvoker _messageInvoker;
        private readonly IHostBufferPolicySelector _bufferPolicySelector;
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly CancellationToken _appDisposing;

        private bool _disposed;

        /// <summary>Initializes a new instance of the <see cref="HttpMessageHandlerAdapter"/> class.</summary>
        /// <param name="next">The next component in the pipeline.</param>
        /// <param name="options">The options to configure this adapter.</param>
        public HttpMessageHandlerAdapter(OwinMiddleware next, HttpMessageHandlerOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _messageHandler = options.MessageHandler;

            if (_messageHandler == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(HttpMessageHandlerOptions).Name, "MessageHandler"), "options");
            }

            _messageInvoker = new HttpMessageInvoker(_messageHandler);
            _bufferPolicySelector = options.BufferPolicySelector;

            if (_bufferPolicySelector == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(HttpMessageHandlerOptions).Name, "BufferPolicySelector"), "options");
            }

            _exceptionLogger = options.ExceptionLogger;

            if (_exceptionLogger == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(HttpMessageHandlerOptions).Name, "ExceptionLogger"), "options");
            }

            _exceptionHandler = options.ExceptionHandler;

            if (_exceptionHandler == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(HttpMessageHandlerOptions).Name, "ExceptionHandler"), "options");
            }

            _appDisposing = options.AppDisposing;

            if (_appDisposing.CanBeCanceled)
            {
                _appDisposing.Register(OnAppDisposing);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="HttpMessageHandlerAdapter"/> class.</summary>
        /// <param name="next">The next component in the pipeline.</param>
        /// <param name="messageHandler">The <see cref="HttpMessageHandler"/> to submit requests to.</param>
        /// <param name="bufferPolicySelector">
        /// The <see cref="IHostBufferPolicySelector"/> that determines whether or not to buffer requests and
        /// responses.
        /// </param>
        /// <remarks>
        /// This constructor is retained for backwards compatibility. The constructor taking
        /// <see cref="HttpMessageHandlerOptions"/> should be used instead.
        /// </remarks>
        [Obsolete("Use the HttpMessageHandlerAdapter(OwinMiddleware, HttpMessageHandlerOptions) constructor instead.")]
        public HttpMessageHandlerAdapter(OwinMiddleware next, HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
            : this(next, CreateOptions(messageHandler, bufferPolicySelector))
        {
        }

        /// <summary>Gets the <see cref="HttpMessageHandler"/> to submit requests to.</summary>
        public HttpMessageHandler MessageHandler
        {
            get { return _messageHandler; }
        }

        /// <summary>
        /// Gets the <see cref="IHostBufferPolicySelector"/> that determines whether or not to buffer requests and
        /// responses.
        /// </summary>
        public IHostBufferPolicySelector BufferPolicySelector
        {
            get { return _bufferPolicySelector; }
        }

        /// <summary>Gets the <see cref="IExceptionLogger"/> to use to log unhandled exceptions.</summary>
        public IExceptionLogger ExceptionLogger
        {
            get { return _exceptionLogger; }
        }

        /// <summary>Gets the <see cref="IExceptionHandler"/> to use to process unhandled exceptions.</summary>
        public IExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler; }
        }

        /// <summary>Gets the <see cref="CancellationToken"/> that triggers cleanup of this component.</summary>
        public CancellationToken AppDisposing
        {
            get { return _appDisposing; }
        }

        /// <inheritdoc />
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinRequest owinRequest = context.Request;
            IOwinResponse owinResponse = context.Response;

            if (owinRequest == null)
            {
                throw Error.InvalidOperation(OwinResources.OwinContext_NullRequest);
            }
            if (owinResponse == null)
            {
                throw Error.InvalidOperation(OwinResources.OwinContext_NullResponse);
            }

            return InvokeCore(context, owinRequest, owinResponse);
        }

        private async Task InvokeCore(IOwinContext context, IOwinRequest owinRequest, IOwinResponse owinResponse)
        {
            CancellationToken cancellationToken = owinRequest.CallCancelled;
            HttpContent requestContent;

            bool bufferInput = _bufferPolicySelector.UseBufferedInputStream(hostContext: context);

            if (!bufferInput)
            {
                owinRequest.DisableBuffering();
            }

            if (!owinRequest.Body.CanSeek && bufferInput)
            {
                requestContent = await CreateBufferedRequestContentAsync(owinRequest, cancellationToken);
            }
            else
            {
                requestContent = CreateStreamedRequestContent(owinRequest);
            }

            HttpRequestMessage request = CreateRequestMessage(owinRequest, requestContent);
            MapRequestProperties(request, context);

            SetPrincipal(owinRequest.User);

            HttpResponseMessage response = null;
            bool callNext;

            try
            {
                response = await _messageInvoker.SendAsync(request, cancellationToken);

                // Handle null responses
                if (response == null)
                {
                    throw Error.InvalidOperation(OwinResources.SendAsync_ReturnedNull);
                }

                // Handle soft 404s where no route matched - call the next component
                if (IsSoftNotFound(request, response))
                {
                    callNext = true;
                }
                else
                {
                    callNext = false;

                    // Compute Content-Length before calling UseBufferedOutputStream because the default implementation
                    // accesses that header and we want to catch any exceptions calling TryComputeLength here.

                    if (response.Content == null
                        || await ComputeContentLengthAsync(request, response, owinResponse, cancellationToken))
                    {
                        bool bufferOutput = _bufferPolicySelector.UseBufferedOutputStream(response);

                        if (!bufferOutput)
                        {
                            owinResponse.DisableBuffering();
                        }
                        else if (response.Content != null)
                        {
                            response = await BufferResponseContentAsync(request, response, cancellationToken);
                        }

                        if (await PrepareHeadersAsync(request, response, owinResponse, cancellationToken))
                        {
                            await SendResponseMessageAsync(request, response, owinResponse, cancellationToken);
                        }
                    }
                }
            }
            finally
            {
                request.DisposeRequestResources();
                request.Dispose();
                if (response != null)
                {
                    response.Dispose();
                }
            }

            // Call the next component if no route matched
            if (callNext && Next != null)
            {
                await Next.Invoke(context);
            }
        }

        private static HttpContent CreateStreamedRequestContent(IOwinRequest owinRequest)
        {
            // Note that we must NOT dispose owinRequest.Body in this case. Disposing it would close the input
            // stream and prevent cascaded components from accessing it. The server MUST handle any necessary
            // cleanup upon request completion. NonOwnedStream prevents StreamContent (or its callers including
            // HttpRequestMessage) from calling Close or Dispose on owinRequest.Body.
            return new StreamContent(new NonOwnedStream(owinRequest.Body));
        }

        private static async Task<HttpContent> CreateBufferedRequestContentAsync(IOwinRequest owinRequest,
            CancellationToken cancellationToken)
        {
            // We need to replace the request body with a buffered stream so that other components can read the stream.
            // For this stream to be useful, it must NOT be diposed along with the request. Streams created by
            // StreamContent do get disposed along with the request, so use MemoryStream to buffer separately.
            MemoryStream buffer;
            int? contentLength = owinRequest.GetContentLength();

            if (!contentLength.HasValue)
            {
                buffer = new MemoryStream();
            }
            else
            {
                buffer = new MemoryStream(Math.Min(4 * 1024, contentLength.Value));
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (StreamContent copier = new StreamContent(owinRequest.Body))
            {
                await copier.CopyToAsync(buffer);
            }

            // Provide the non-disposing, buffered stream to later OWIN components (set to the stream's beginning).
            buffer.Position = 0;
            owinRequest.Body = buffer;

            // For MemoryStream, Length is guaranteed to be an int.
            return new ByteArrayContent(buffer.GetBuffer(), 0, (int)buffer.Length);
        }

        private static HttpRequestMessage CreateRequestMessage(IOwinRequest owinRequest, HttpContent requestContent)
        {
            // Create the request
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(owinRequest.Method), owinRequest.Uri);

            try
            {
                // Set the body
                request.Content = requestContent;

                // Copy the headers
                foreach (KeyValuePair<string, string[]> header in owinRequest.Headers)
                {
                    if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        bool success = requestContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        Contract.Assert(success,
                            "Every header can be added either to the request headers or to the content headers");
                    }
                }
            }
            catch
            {
                request.Dispose();
                throw;
            }

            return request;
        }

        private static void MapRequestProperties(HttpRequestMessage request, IOwinContext context)
        {
            // Set the OWIN context on the request
            request.SetOwinContext(context);

            // Set a request context on the request that lazily populates each property.
            HttpRequestContext requestContext = new OwinHttpRequestContext(context, request);
            request.SetRequestContext(requestContext);
        }

        private static void SetPrincipal(IPrincipal user)
        {
            if (user != null)
            {
                Thread.CurrentPrincipal = user;
            }
        }

        private static bool IsSoftNotFound(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                bool routingFailure;
                if (request.Properties.TryGetValue<bool>(HttpPropertyKeys.NoRouteMatched, out routingFailure)
                    && routingFailure)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<HttpResponseMessage> BufferResponseContentAsync(HttpRequestMessage request,
            HttpResponseMessage response, CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo exceptionInfo;

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await response.Content.LoadIntoBufferAsync();
                return response;
            }
            catch (OperationCanceledException)
            {
                // Propogate the canceled task without calling exception loggers or handlers.
                throw;
            }
            catch (Exception exception)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            // If the content can't be buffered, create a buffered error response for the exception
            // This code will commonly run when a formatter throws during the process of serialization

            Debug.Assert(exceptionInfo.SourceException != null);

            ExceptionContext exceptionContext = new ExceptionContext(exceptionInfo.SourceException,
                OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent, request, response);

            await _exceptionLogger.LogAsync(exceptionContext, cancellationToken);
            HttpResponseMessage errorResponse = await _exceptionHandler.HandleAsync(exceptionContext,
                cancellationToken);

            response.Dispose();

            if (errorResponse == null)
            {
                exceptionInfo.Throw();
                return null;
            }

            // We have an error response to try to buffer and send back.

            response = errorResponse;
            cancellationToken.ThrowIfCancellationRequested();

            Exception errorException;

            try
            {
                // Try to buffer the error response and send it back.
                await response.Content.LoadIntoBufferAsync();
                return response;
            }
            catch (OperationCanceledException)
            {
                // Propogate the canceled task without calling exception loggers.
                throw;
            }
            catch (Exception exception)
            {
                errorException = exception;
            }

            // We tried to send back an error response with content, but we couldn't. It's an edge case; the best we
            // can do is to log that exception and send back an empty 500.

            ExceptionContext errorExceptionContext = new ExceptionContext(errorException,
                OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferError, request, response);
            await _exceptionLogger.LogAsync(errorExceptionContext, cancellationToken);

            response.Dispose();
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        // Prepares Content-Length and Transfer-Encoding headers.
        private Task<bool> PrepareHeadersAsync(HttpRequestMessage request, HttpResponseMessage response,
            IOwinResponse owinResponse, CancellationToken cancellationToken)
        {
            Contract.Assert(response != null);
            HttpResponseHeaders responseHeaders = response.Headers;
            Contract.Assert(responseHeaders != null);
            HttpContent content = response.Content;
            bool isTransferEncodingChunked = responseHeaders.TransferEncodingChunked == true;
            HttpHeaderValueCollection<TransferCodingHeaderValue> transferEncoding = responseHeaders.TransferEncoding;

            if (content != null)
            {
                HttpContentHeaders contentHeaders = content.Headers;
                Contract.Assert(contentHeaders != null);

                if (isTransferEncodingChunked)
                {
                    // According to section 4.4 of the HTTP 1.1 spec, HTTP responses that use chunked transfer
                    // encoding must not have a content length set. Chunked should take precedence over content
                    // length in this case because chunked is always set explicitly by users while the Content-Length
                    // header can be added implicitly by System.Net.Http.
                    contentHeaders.ContentLength = null;
                }
                else
                {
                    // Copy the response content headers only after ensuring they are complete.
                    // We ask for Content-Length first because HttpContent lazily computes this header and only
                    // afterwards writes the value into the content headers.
                    return ComputeContentLengthAsync(request, response, owinResponse, cancellationToken);
                }
            }

            // Ignore the Transfer-Encoding header if it is just "chunked"; the host will likely provide it when no
            // Content-Length is present (and if the host does not, there's not much better this code could do to
            // transmit the current response, since HttpContent is assumed to be unframed; in that case, silently drop
            // the Transfer-Encoding: chunked header).
            // HttpClient sets this header when it receives chunked content, but HttpContent does not include the
            // frames. The OWIN contract is to set this header only when writing chunked frames to the stream.
            // A Web API caller who desires custom framing would need to do a different Transfer-Encoding (such as
            // "identity, chunked").
            if (isTransferEncodingChunked && transferEncoding.Count == 1)
            {
                transferEncoding.Clear();
            }

            return Task.FromResult(true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "unused",
            Justification = "unused variable necessary to call getter")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is turned into an error response.")]
        private Task<bool> ComputeContentLengthAsync(HttpRequestMessage request, HttpResponseMessage response,
            IOwinResponse owinResponse, CancellationToken cancellationToken)
        {
            Contract.Assert(response != null);
            HttpResponseHeaders responseHeaders = response.Headers;
            Contract.Assert(responseHeaders != null);
            HttpContent content = response.Content;
            Contract.Assert(content != null);
            HttpContentHeaders contentHeaders = content.Headers;
            Contract.Assert(contentHeaders != null);

            Exception exception;

            try
            {
                var unused = contentHeaders.ContentLength;

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return HandleTryComputeLengthExceptionAsync(exception, request, response, owinResponse, cancellationToken);
        }

        private async Task<bool> HandleTryComputeLengthExceptionAsync(Exception exception, HttpRequestMessage request,
            HttpResponseMessage response, IOwinResponse owinResponse, CancellationToken cancellationToken)
        {
            Contract.Assert(owinResponse != null);

            ExceptionContext exceptionContext = new ExceptionContext(exception,
                OwinExceptionCatchBlocks.HttpMessageHandlerAdapterComputeContentLength, request, response);
            await _exceptionLogger.LogAsync(exceptionContext, cancellationToken);

            // Send back an empty error response if TryComputeLength throws.
            owinResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
            SetHeadersForEmptyResponse(owinResponse.Headers);
            return false;
        }

        private Task SendResponseMessageAsync(HttpRequestMessage request, HttpResponseMessage response,
            IOwinResponse owinResponse, CancellationToken cancellationToken)
        {
            owinResponse.StatusCode = (int)response.StatusCode;
            owinResponse.ReasonPhrase = response.ReasonPhrase;

            // Copy non-content headers
            IDictionary<string, string[]> responseHeaders = owinResponse.Headers;
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
            {
                responseHeaders[header.Key] = header.Value.AsArray();
            }

            HttpContent responseContent = response.Content;
            if (responseContent == null)
            {
                SetHeadersForEmptyResponse(responseHeaders);
                return TaskHelpers.Completed();
            }
            else
            {
                // Copy content headers
                foreach (KeyValuePair<string, IEnumerable<string>> contentHeader in responseContent.Headers)
                {
                    responseHeaders[contentHeader.Key] = contentHeader.Value.AsArray();
                }

                // Copy body
                return SendResponseContentAsync(request, response, owinResponse, cancellationToken);
            }
        }

        private static void SetHeadersForEmptyResponse(IDictionary<string, string[]> headers)
        {
            // Set the content-length to 0 to prevent the server from sending back the response chunked
            headers["Content-Length"] = new string[] { "0" };
        }

        private async Task SendResponseContentAsync(HttpRequestMessage request, HttpResponseMessage response,
            IOwinResponse owinResponse, CancellationToken cancellationToken)
        {
            Contract.Assert(response != null);
            Contract.Assert(response.Content != null);

            Exception exception;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await response.Content.CopyToAsync(owinResponse.Body);
                return;
            }
            catch (OperationCanceledException)
            {
                // Propogate the canceled task without calling exception loggers;
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // We're streaming content, so we can only call loggers, not handlers, as we've already (possibly) send the
            // status code and headers across the wire. Log the exception, but then just abort.
            ExceptionContext exceptionContext = new ExceptionContext(exception,
                OwinExceptionCatchBlocks.HttpMessageHandlerAdapterStreamContent, request, response);
            await _exceptionLogger.LogAsync(exceptionContext, cancellationToken);
            await AbortResponseAsync();
        }

        private static Task AbortResponseAsync()
        {
            // OWIN doesn't yet support an explicit Abort event. Returning a canceled task is the best contract at the
            // moment.
            return TaskHelpers.Canceled();
        }

        // Provides HttpMessageHandlerOptions for callers using the old constructor.
        private static HttpMessageHandlerOptions CreateOptions(HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }

            if (bufferPolicySelector == null)
            {
                throw new ArgumentNullException("bufferPolicySelector");
            }

            // Callers using the old constructor get the default exception handler, no exception logging support, and no
            // app cleanup support.

            return new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = bufferPolicySelector,
                ExceptionLogger = new EmptyExceptionLogger(),
                ExceptionHandler = new DefaultExceptionHandler(),
                AppDisposing = CancellationToken.None
            };
        }

        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.
        /// </param>
        /// <remarks>
        /// This class implements <see cref="IDisposable"/> for legacy reasons. New callers should instead provide a
        /// cancellation token via <see cref="AppDisposing"/> using the constructor that takes
        /// <see cref="HttpMessageHandlerOptions"/>.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnAppDisposing();
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This class implements <see cref="IDisposable"/> for legacy reasons. New callers should instead provide a
        /// cancellation token via <see cref="AppDisposing"/> using the constructor that takes
        /// <see cref="HttpMessageHandlerOptions"/>.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnAppDisposing()
        {
            if (!_disposed)
            {
                _messageInvoker.Dispose();
                _disposed = true;
            }
        }
    }
}

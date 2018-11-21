// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Http.WebHost.Properties;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// An <see cref="HttpTaskAsyncHandler"/> that uses an <see cref="HttpServer"/> to process ASP.NET requests asynchronously.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This class is a coordinator, so this coupling is expected.")]
    [SuppressMessage("Microsoft.Design", "CA1001:Implement IDisposable", Justification = "HttpMessageInvoker doesn’t have any resources of its own to dispose.")]
    public class HttpControllerHandler : HttpTaskAsyncHandler
    {
        // See Microsoft.Owin.Host.SystemWeb.
        internal static readonly string OwinEnvironmentHttpContextKey = "owin.Environment";

        internal static readonly string OwinEnvironmentKey = "MS_OwinEnvironment";

        private static readonly Lazy<Action<HttpContextBase>> _suppressRedirectAction =
            new Lazy<Action<HttpContextBase>>(
                () =>
                {
                    // If the behavior is explicitly disabled, do nothing
                    if (!SuppressFormsAuthRedirectHelper.GetEnabled(WebConfigurationManager.AppSettings))
                    {
                        return httpContext => { };
                    }

                    return httpContext => httpContext.Response.SuppressFormsAuthenticationRedirect = true;
                });

        private static readonly Lazy<IHostBufferPolicySelector> _bufferPolicySelector =
            new Lazy<IHostBufferPolicySelector>(() => GlobalConfiguration.Configuration.Services.GetHostBufferPolicySelector());

        private static readonly Lazy<IExceptionHandler> _exceptionHandler = new Lazy<IExceptionHandler>(() =>
            ExceptionServices.GetHandler(GlobalConfiguration.Configuration));
        private static readonly Lazy<IExceptionLogger> _exceptionLogger = new Lazy<IExceptionLogger>(() =>
            ExceptionServices.GetLogger(GlobalConfiguration.Configuration));

        private static readonly Func<HttpRequestMessage, X509Certificate2> _retrieveClientCertificate = new Func<HttpRequestMessage, X509Certificate2>(RetrieveClientCertificate);

        private readonly IHttpRouteData _routeData;
        private readonly HttpMessageInvoker _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerHandler"/> class.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        public HttpControllerHandler(RouteData routeData)
            : this(routeData, GlobalConfiguration.DefaultServer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerHandler"/> class.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        /// <param name="handler">The message handler to dispatch requests to.</param>
        public HttpControllerHandler(RouteData routeData, HttpMessageHandler handler)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            _routeData = new HostedHttpRouteData(routeData);
            _server = new HttpMessageInvoker(handler);
        }

        public override Task ProcessRequestAsync(HttpContext context)
        {
            return ProcessRequestAsyncCore(new HttpContextWrapper(context));
        }

        internal async Task ProcessRequestAsyncCore(HttpContextBase contextBase)
        {
            HttpRequestMessage request = contextBase.GetHttpRequestMessage() ?? ConvertRequest(contextBase);

            // Add route data
            request.SetRouteData(_routeData);
            CancellationToken cancellationToken = contextBase.Response.GetClientDisconnectedTokenWhenFixed();
            HttpResponseMessage response = null;

            try
            {
                response = await _server.SendAsync(request, cancellationToken);
                await CopyResponseAsync(contextBase, request, response, _exceptionLogger.Value, _exceptionHandler.Value,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // HttpTaskAsyncHandler treats a canceled task as an unhandled exception (logged to Application event
                // log). Instead of returning a canceled task, abort the request and return a completed task.
                contextBase.Request.Abort();
            }
            finally
            {
                // The other HttpTaskAsyncHandler is HttpRouteExceptionHandler; it has similar cleanup logic.
                request.DisposeRequestResources();
                request.Dispose();

                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        private static void CopyHeaders(HttpHeaders from, HttpContextBase to)
        {
            Contract.Assert(from != null);
            Contract.Assert(to != null);

            foreach (var header in from)
            {
                string name = header.Key;
                foreach (var value in header.Value)
                {
                    to.Response.AppendHeader(name, value);
                }
            }
        }

        private static void AddHeaderToHttpRequestMessage(HttpRequestMessage httpRequestMessage, string headerName, string[] headerValues)
        {
            Contract.Assert(httpRequestMessage != null);
            Contract.Assert(headerName != null);
            Contract.Assert(headerValues != null);

            if (!httpRequestMessage.Headers.TryAddWithoutValidation(headerName, headerValues))
            {
                httpRequestMessage.Content.Headers.TryAddWithoutValidation(headerName, headerValues);
            }
        }

        internal static async Task CopyResponseAsync(HttpContextBase httpContextBase, HttpRequestMessage request,
            HttpResponseMessage response, IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler,
            CancellationToken cancellationToken)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(request != null);

            // A null response creates a 500 with no content
            if (response == null)
            {
                SetEmptyErrorResponse(httpContextBase.Response);
                return;
            }

            if (!await CopyResponseStatusAndHeadersAsync(httpContextBase, request, response, exceptionLogger,
                cancellationToken))
            {
                return;
            }

            // TODO 335085: Consider this when coming up with our caching story
            if (response.Headers.CacheControl == null)
            {
                // DevDiv2 #332323. ASP.NET by default always emits a cache-control: private header.
                // However, we don't want requests to be cached by default.
                // If nobody set an explicit CacheControl then explicitly set to no-cache to override the
                // default behavior. This will cause the following response headers to be emitted:
                //     Cache-Control: no-cache
                //     Pragma: no-cache
                //     Expires: -1
                httpContextBase.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }

            // Asynchronously write the response body.  If there is no body, we use
            // a completed task to share the Finally() below.
            // The response-writing task will not fault -- it handles errors internally.
            if (response.Content != null)
            {
                await WriteResponseContentAsync(httpContextBase, request, response, exceptionLogger, exceptionHandler, cancellationToken);
            }
        }

        internal static HttpRequestMessage ConvertRequest(HttpContextBase httpContextBase)
        {
            return ConvertRequest(httpContextBase, _bufferPolicySelector.Value);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner")]
        internal static HttpRequestMessage ConvertRequest(HttpContextBase httpContextBase, IHostBufferPolicySelector policySelector)
        {
            Contract.Assert(httpContextBase != null);

            HttpRequestBase requestBase = httpContextBase.Request;
            HttpMethod method = HttpMethodHelper.GetHttpMethod(requestBase.HttpMethod);
            Uri uri = requestBase.Url;
            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            // Choose a buffered or bufferless input stream based on user's policy
            bool bufferInput = policySelector == null ? true : policySelector.UseBufferedInputStream(httpContextBase);
            request.Content = GetStreamContent(requestBase, bufferInput);

            foreach (string headerName in requestBase.Headers)
            {
                string[] values = requestBase.Headers.GetValues(headerName);
                AddHeaderToHttpRequestMessage(request, headerName, values);
            }

            // Add context to enable route lookup later on
            request.SetHttpContext(httpContextBase);

            HttpRequestContext requestContext = new WebHostHttpRequestContext(httpContextBase, requestBase, request);
            request.SetRequestContext(requestContext);

            IDictionary httpContextItems = httpContextBase.Items;

            // Add the OWIN environment, when available (such as when using the OWIN integrated pipeline HTTP module).
            if (httpContextItems != null && httpContextItems.Contains(OwinEnvironmentHttpContextKey))
            {
                request.Properties.Add(OwinEnvironmentKey, httpContextItems[OwinEnvironmentHttpContextKey]);
            }

            // The following three properties are set for backwards compatibility only. The request context controls
            // the behavior for all cases except when accessing the property directly by key.

            // Add the retrieve client certificate delegate to the property bag to enable lookup later on
            request.Properties.Add(HttpPropertyKeys.RetrieveClientCertificateDelegateKey, _retrieveClientCertificate);

            // Add information about whether the request is local or not
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => requestBase.IsLocal));

            // Add information about whether custom errors are enabled for this request or not
            request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => !httpContextBase.IsCustomErrorEnabled));

            return request;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner")]
        private static HttpContent GetStreamContent(HttpRequestBase requestBase, bool bufferInput)
        {
            if (bufferInput)
            {
                return new LazyStreamContent(() =>
                {
                    if (requestBase.ReadEntityBodyMode == ReadEntityBodyMode.None)
                    {
                        return new SeekableBufferedRequestStream(requestBase);
                    }
                    else if (requestBase.ReadEntityBodyMode == ReadEntityBodyMode.Classic)
                    {
                        requestBase.InputStream.Position = 0;
                        return requestBase.InputStream;
                    }
                    else if (requestBase.ReadEntityBodyMode == ReadEntityBodyMode.Buffered)
                    {
                        if (requestBase.GetBufferedInputStream().Position > 0)
                        {
                            // If GetBufferedInputStream() was completely read, we can continue accessing it via Request.InputStream.
                            // If it was partially read, accessing InputStream will throw, but at that point we have no
                            // way of recovering.
                            requestBase.InputStream.Position = 0;
                            return requestBase.InputStream;
                        }
                        return new SeekableBufferedRequestStream(requestBase);
                    }
                    else
                    {
                        Contract.Assert(requestBase.ReadEntityBodyMode == ReadEntityBodyMode.Bufferless);
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                                          SRResources.RequestBodyAlreadyReadInMode,
                                                                          ReadEntityBodyMode.Bufferless));
                    }
                });
            }
            else
            {
                return new LazyStreamContent(() =>
                {
                    if (requestBase.ReadEntityBodyMode == ReadEntityBodyMode.None)
                    {
                        return requestBase.GetBufferlessInputStream();
                    }
                    else if (requestBase.ReadEntityBodyMode == ReadEntityBodyMode.Classic)
                    {
                        // The user intended that the request be read in a bufferless manner, but we are starting with a buffered stream.
                        // To maintain compatibility with legacy behavior, we'll throw in this case.
                        throw new InvalidOperationException(SRResources.RequestStreamCannotBeReadBufferless);
                    }
                    else if (requestBase.ReadEntityBodyMode == ReadEntityBodyMode.Bufferless)
                    {
                        Stream bufferlessInputStream = requestBase.GetBufferlessInputStream();
                        if (bufferlessInputStream.Position > 0)
                        {
                            throw new InvalidOperationException(SRResources.RequestBodyAlreadyRead);
                        }
                        return bufferlessInputStream;
                    }
                    else
                    {
                        Contract.Assert(requestBase.ReadEntityBodyMode == ReadEntityBodyMode.Buffered);
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, SRResources.RequestBodyAlreadyReadInMode, ReadEntityBodyMode.Buffered));
                    }
                });
            }
        }

        /// <summary>
        /// Prevents the <see cref="T:System.Web.Security.FormsAuthenticationModule"/> from altering a 401 response to 302 by
        /// setting <see cref="P:System.Web.HttpResponseBase.SuppressFormsAuthenticationRedirect" /> to <c>true</c> if available.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context base.</param>
        internal static void EnsureSuppressFormsAuthenticationRedirect(HttpContextBase httpContextBase)
        {
            Contract.Assert(httpContextBase != null);

            // Only if the response is status code is 401
            if (httpContextBase.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                _suppressRedirectAction.Value(httpContextBase);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "unused", Justification = "unused variable necessary to call getter")]
        private static Task WriteResponseContentAsync(HttpContextBase httpContextBase, HttpRequestMessage request,
            HttpResponseMessage response, IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler,
            CancellationToken cancellationToken)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(response != null);
            Contract.Assert(request != null);
            Contract.Assert(response.Content != null);

            HttpResponseBase httpResponseBase = httpContextBase.Response;
            HttpContent responseContent = response.Content;

            CopyHeaders(responseContent.Headers, httpContextBase);

            // PrepareHeadersAsync already evaluated the buffer policy.
            bool isBuffered = httpResponseBase.BufferOutput;

            return isBuffered
                    ? WriteBufferedResponseContentAsync(httpContextBase, request, response, exceptionLogger, exceptionHandler, cancellationToken)
                    : WriteStreamedResponseContentAsync(httpContextBase, request, response, exceptionLogger, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions caught here become error responses")]
        internal static async Task WriteStreamedResponseContentAsync(HttpContextBase httpContextBase,
            HttpRequestMessage request, HttpResponseMessage response, IExceptionLogger exceptionLogger,
            CancellationToken cancellationToken)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(httpContextBase.Response != null);
            Contract.Assert(request != null);
            Contract.Assert(response != null);
            Contract.Assert(response.Content != null);

            Exception exception = null;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Copy the HttpContent into the output stream asynchronously.
                await response.Content.CopyToAsync(httpContextBase.Response.OutputStream);
                return;
            }
            catch (OperationCanceledException)
            {
                // Propogate the canceled task without calling exception loggers.
                throw;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Contract.Assert(exception != null);

            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpControllerHandlerStreamContent;
            ExceptionContext exceptionContext = new ExceptionContext(exception, catchBlock, request, response);
            await exceptionLogger.LogAsync(exceptionContext, cancellationToken);

            // Streamed content may have been written and cannot be recalled.
            // Our only choice is to abort the connection.
            httpContextBase.Request.Abort();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "continuation task owned by caller")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions caught here become error responses")]
        internal static async Task WriteBufferedResponseContentAsync(HttpContextBase httpContextBase,
            HttpRequestMessage request, HttpResponseMessage response, IExceptionLogger exceptionLogger,
            IExceptionHandler exceptionHandler, CancellationToken cancellationToken)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(httpContextBase.Response != null);
            Contract.Assert(request != null);
            Contract.Assert(response != null);
            Contract.Assert(response.Content != null);

            HttpResponseBase httpResponseBase = httpContextBase.Response;

            // Return a task that writes the response body asynchronously.
            // We guarantee we will handle all error responses internally
            // and always return a non-faulted task, except for custom error handlers that choose to propagate these
            // exceptions.
            ExceptionDispatchInfo exceptionInfo;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Copy the HttpContent into the output stream asynchronously.
                await response.Content.CopyToAsync(httpResponseBase.OutputStream);
                return;
            }
            catch (OperationCanceledException)
            {
                // Propogate the canceled task without calling exception loggers or handlers.
                throw;
            }
            catch (Exception exception)
            {
                // Can't use await inside a catch block
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            Debug.Assert(exceptionInfo.SourceException != null);

            // If we were using a buffered stream, we can still set the headers and status code, and we can create an
            // error response with the exception.
            // We create a continuation task to write an error response that will run after returning from this Catch()
            // but before other continuations the caller appends to this task.
            // The error response writing task handles errors internally and will not show as faulted, except for
            // custom error handlers that choose to propagate these exceptions.
            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent;

            if (!await CopyErrorResponseAsync(catchBlock, httpContextBase, request, response,
                exceptionInfo.SourceException, exceptionLogger, exceptionHandler, cancellationToken))
            {
                exceptionInfo.Throw();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions caught here become error responses")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "errorResponse gets disposed in the async continuation")]
        internal static async Task<bool> CopyErrorResponseAsync(ExceptionContextCatchBlock catchBlock,
            HttpContextBase httpContextBase, HttpRequestMessage request, HttpResponseMessage response,
            Exception exception, IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler,
            CancellationToken cancellationToken)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(httpContextBase.Response != null);
            Contract.Assert(request != null);
            Contract.Assert(exception != null);
            Contract.Assert(catchBlock != null);
            Contract.Assert(catchBlock.CallsHandler);

            HttpResponseBase httpResponseBase = httpContextBase.Response;
            HttpResponseMessage errorResponse = null;
            HttpResponseException responseException = exception as HttpResponseException;

            // Ensure all headers and content are cleared to eliminate any partial results.
            ClearContentAndHeaders(httpResponseBase);

            // If the exception we are handling is HttpResponseException,
            // that becomes the error response.
            if (responseException != null)
            {
                errorResponse = responseException.Response;
            }
            else
            {
                ExceptionContext exceptionContext = new ExceptionContext(exception, catchBlock, request)
                {
                    Response = response
                };
                await exceptionLogger.LogAsync(exceptionContext, cancellationToken);
                errorResponse = await exceptionHandler.HandleAsync(exceptionContext, cancellationToken);

                if (errorResponse == null)
                {
                    return false;
                }
            }

            Contract.Assert(errorResponse != null);
            if (!await CopyResponseStatusAndHeadersAsync(httpContextBase, request, errorResponse, exceptionLogger,
                cancellationToken))
            {
                // Don't rethrow the original exception unless explicitly requested to do so. In this case, the
                // exception handler indicated it wanted to handle the exception; it simply failed create a stable
                // response to send.
                return true;
            }

            // The error response may return a null content if content negotiation
            // fails to find a formatter, or this may be an HttpResponseException without
            // content.  In either case, cleanup and return a completed task.

            if (errorResponse.Content == null)
            {
                errorResponse.Dispose();
                return true;
            }

            CopyHeaders(errorResponse.Content.Headers, httpContextBase);

            await WriteErrorResponseContentAsync(httpResponseBase, request, errorResponse, cancellationToken,
                exceptionLogger);
            return true;
        }

        private static async Task WriteErrorResponseContentAsync(HttpResponseBase httpResponseBase,
            HttpRequestMessage request, HttpResponseMessage errorResponse, CancellationToken cancellationToken,
            IExceptionLogger exceptionLogger)
        {
            HttpRequestMessage ignoreUnused = request;

            try
            {
                Exception exception = null;
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Asynchronously write the content of the new error HttpResponseMessage
                    await errorResponse.Content.CopyToAsync(httpResponseBase.OutputStream);
                    return;
                }
                catch (OperationCanceledException)
                {
                    // Propogate the canceled task without calling exception loggers.
                    throw;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Contract.Assert(exception != null);

                ExceptionContext exceptionContext = new ExceptionContext(exception,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferError, request, errorResponse);
                await exceptionLogger.LogAsync(exceptionContext, cancellationToken);

                // Failure writing the error response.  Likely cause is a formatter
                // serialization exception.  Create empty error response and
                // return a non-faulted task.
                SetEmptyErrorResponse(httpResponseBase);
            }
            finally
            {
                // Dispose the temporary HttpResponseMessage carrying the error response
                errorResponse.Dispose();
            }
        }

        private static async Task<bool> CopyResponseStatusAndHeadersAsync(HttpContextBase httpContextBase,
            HttpRequestMessage request, HttpResponseMessage response, IExceptionLogger exceptionLogger,
            CancellationToken cancellationToken)
        {
            Contract.Assert(httpContextBase != null);
            HttpResponseBase httpResponseBase = httpContextBase.Response;
            httpResponseBase.StatusCode = (int)response.StatusCode;
            httpResponseBase.StatusDescription = response.ReasonPhrase;
            httpResponseBase.TrySkipIisCustomErrors = true;
            EnsureSuppressFormsAuthenticationRedirect(httpContextBase);

            if (!await PrepareHeadersAsync(httpResponseBase, request, response, exceptionLogger, cancellationToken))
            {
                return false;
            }

            CopyHeaders(response.Headers, httpContextBase);
            return true;
        }

        // Prepares Content-Length and Transfer-Encoding headers.
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "unused", Justification = "unused variable necessary to call getter")]
        internal static async Task<bool> PrepareHeadersAsync(HttpResponseBase responseBase, HttpRequestMessage request,
            HttpResponseMessage response, IExceptionLogger exceptionLogger, CancellationToken cancellationToken)
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
                    Exception exception = null;

                    // Copy the response content headers only after ensuring they are complete.
                    // We ask for Content-Length first because HttpContent lazily computes this
                    // and only afterwards writes the value into the content headers.
                    try
                    {
                        var unused = contentHeaders.ContentLength;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    if (exception != null)
                    {
                        ExceptionContext exceptionContext = new ExceptionContext(exception,
                            WebHostExceptionCatchBlocks.HttpControllerHandlerComputeContentLength, request, response);
                        await exceptionLogger.LogAsync(exceptionContext, cancellationToken);

                        SetEmptyErrorResponse(responseBase);
                        return false;
                    }
                }

                // Select output buffering based on the user-controlled buffering policy
                bool isBuffered = _bufferPolicySelector.Value != null ?
                    _bufferPolicySelector.Value.UseBufferedOutputStream(response) : true;
                responseBase.BufferOutput = isBuffered;
            }

            // Ignore the Transfer-Encoding header if it is just "chunked"; the host will provide it when no
            // Content-Length is present and BufferOutput is disabled (and this method guarantees those conditions).
            // HttpClient sets this header when it receives chunked content, but HttpContent does not include the
            // frames. The ASP.NET contract is to set this header only when writing chunked frames to the stream.
            // A Web API caller who desires custom framing would need to do a different Transfer-Encoding (such as
            // "identity, chunked").
            if (isTransferEncodingChunked && transferEncoding.Count == 1)
            {
                transferEncoding.Clear();

                // In the case of a conflict between a Transfer-Encoding: chunked header and the output buffering
                // policy, honor the Transnfer-Encoding: chunked header and ignore the buffer policy.
                // If output buffering is not disabled, ASP.NET will not write the TransferEncoding: chunked header.
                responseBase.BufferOutput = false;
            }

            return true;
        }

        private static void ClearContentAndHeaders(HttpResponseBase httpResponseBase)
        {
            httpResponseBase.Clear();

            // Despite what the documentation indicates, calling Clear on its own doesn't fully clear the headers.
            httpResponseBase.ClearHeaders();
        }

        private static void SetEmptyErrorResponse(HttpResponseBase httpResponseBase)
        {
            ClearContentAndHeaders(httpResponseBase);
            httpResponseBase.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpResponseBase.SuppressContent = true;
        }

        private static X509Certificate2 RetrieveClientCertificate(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            X509Certificate2 result = null;

            HttpContextBase httpContextBase = request.GetHttpContext();
            if (httpContextBase != null)
            {
                if (httpContextBase.Request.ClientCertificate.Certificate != null && httpContextBase.Request.ClientCertificate.Certificate.Length > 0)
                {
                    result = new X509Certificate2(httpContextBase.Request.ClientCertificate.Certificate);
                }
            }

            return result;
        }

        private class DelegatingStreamContent : StreamContent
        {
            public DelegatingStreamContent(Stream stream)
                : base(stream)
            {
            }

            public Task WriteToStreamAsync(Stream stream, TransportContext context)
            {
                return SerializeToStreamAsync(stream, context);
            }

            public Task<Stream> GetContentReadStreamAsync()
            {
                return CreateContentReadStreamAsync();
            }

            protected override bool TryComputeLength(out long length)
            {
                // Do not attempt to calculate length because SeekableBufferedRequestStream (for example)
                // may report 0 until the underlying Stream has been read to end.
                length = 0L;
                return false;
            }
        }

        private class LazyStreamContent : HttpContent
        {
            private readonly Func<Stream> _getStream;
            private DelegatingStreamContent _streamContent;

            public LazyStreamContent(Func<Stream> getStream)
            {
                _getStream = getStream;
            }

            private DelegatingStreamContent StreamContent
            {
                get
                {
                    if (_streamContent == null)
                    {
                        _streamContent = new DelegatingStreamContent(_getStream());
                    }
                    return _streamContent;
                }
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return StreamContent.WriteToStreamAsync(stream, context);
            }

            protected override Task<Stream> CreateContentReadStreamAsync()
            {
                return StreamContent.GetContentReadStreamAsync();
            }

            protected override bool TryComputeLength(out long length)
            {
                // Do not attempt to calculate length because SeekableBufferedRequestStream (for example)
                // may report 0 until the underlying Stream has been read to end.
                length = 0L;
                return false;
            }
        }
    }
}

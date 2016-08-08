// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using System.Web.Http.WebHost.Properties;

namespace System.Web.Http.WebHost
{
    /// <summary>Provides the default implementation for handling exceptions within Web API web host.</summary>
    /// <remarks>
    /// This class preserves the legacy behavior of catch blocks and is the the default registered IExceptionHandler
    /// for web host.
    /// </remarks>
    internal class WebHostExceptionHandler : IExceptionHandler
    {
        private readonly IExceptionHandler _innerHandler;

        public WebHostExceptionHandler(IExceptionHandler innerHandler)
        {
            Contract.Assert(innerHandler != null);
            _innerHandler = innerHandler;
        }

        public IExceptionHandler InnerHandler
        {
            get { return _innerHandler; }
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            if (exceptionContext.CatchBlock ==
                WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent)
            {
                HandleWebHostBufferedContentException(context);
                return TaskHelpers.Completed();
            }

            return _innerHandler.HandleAsync(context, cancellationToken);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "We already shipped this code; avoiding even minor breaking changes in error handling.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "All exceptions caught here become error responses")]
        private static void HandleWebHostBufferedContentException(ExceptionHandlerContext context)
        {
            Contract.Assert(context != null);

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            Exception exception = exceptionContext.Exception;
            Contract.Assert(exception != null);

            HttpRequestMessage request = exceptionContext.Request;

            if (request == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Request"), "context");
            }

            HttpResponseMessage response = exceptionContext.Response;

            if (response == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Response"), "context");
            }

            HttpContent responseContent = response.Content;

            if (responseContent == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(HttpResponseMessage).Name, "Content"), "context");
            }

            HttpResponseMessage errorResponse;

            // Create a 500 response with content containing an explanatory message and
            // stack trace, subject to content negotiation and policy for error details.
            try
            {
                MediaTypeHeaderValue mediaType = responseContent.Headers.ContentType;
                string messageDetails = (mediaType != null)
                                            ? Error.Format(
                                                SRResources.Serialize_Response_Failed_MediaType,
                                                responseContent.GetType().Name,
                                                mediaType)
                                            : Error.Format(
                                                SRResources.Serialize_Response_Failed,
                                                responseContent.GetType().Name);

                errorResponse = request.CreateErrorResponse(
                                            HttpStatusCode.InternalServerError,
                                            new InvalidOperationException(messageDetails, exception));

                // CreateErrorResponse will choose 406 if it cannot find a formatter,
                // but we want our default error response to be 500 always
                errorResponse.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch
            {
                // Failed creating an HttpResponseMessage for the error response.
                // This can happen for missing config, missing conneg service, etc.
                // Create an empty error response and return a non-faulted task.
                errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            context.Result = new ResponseMessageResult(errorResponse);
        }
    }
}

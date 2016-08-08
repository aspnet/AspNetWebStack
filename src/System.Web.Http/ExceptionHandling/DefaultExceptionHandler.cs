// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;
using System.Web.Http.Results;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides the default implementation for handling exceptions within Web API.</summary>
    /// <remarks>
    /// This class preserves the legacy behavior of catch blocks and is the the default registered IExceptionHandler.
    /// This default service allows adding the IExceptionHandler service extensibility point without making any
    /// breaking changes in the default implementation.
    /// </remarks>
    internal class DefaultExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return TaskHelpers.Completed();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "We already shipped this code; avoiding even minor breaking changes in error handling.")]
        private static void Handle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);
            Exception exception = exceptionContext.Exception;

            HttpRequestMessage request = exceptionContext.Request;

            if (request == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Request"), "context");
            }

            if (exceptionContext.CatchBlock == ExceptionCatchBlocks.IExceptionFilter)
            {
                // The exception filter stage propagates unhandled exceptions by default (when no filter handles the
                // exception).
                return;
            }

            context.Result = new ResponseMessageResult(request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                exception));
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides extension methods for <see cref="IExceptionHandler"/>.</summary>
    public static class ExceptionHandlerExtensions
    {
        /// <summary>Calls an exception handler and determines the response handling it, if any.</summary>
        /// <param name="handler">The unhandled exception handler.</param>
        /// <param name="context">The exception context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that, when completed, contains the response message to return when the exception is handled, or
        /// <see langword="null"/> when the exception remains unhandled.
        /// </returns>
        public static Task<HttpResponseMessage> HandleAsync(this IExceptionHandler handler,
            ExceptionContext context, CancellationToken cancellationToken)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionHandlerContext handlerContext = new ExceptionHandlerContext(context);
            return HandleAsyncCore(handler, handlerContext, cancellationToken);
        }

        private static async Task<HttpResponseMessage> HandleAsyncCore(IExceptionHandler handler,
            ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Contract.Assert(handler != null);
            Contract.Assert(context != null);

            await handler.HandleAsync(context, cancellationToken);

            IHttpActionResult result = context.Result;

            if (result == null)
            {
                return null;
            }

            HttpResponseMessage response = await result.ExecuteAsync(cancellationToken);

            if (response == null)
            {
                throw new InvalidOperationException(Error.Format(SRResources.TypeMethodMustNotReturnNull,
                    typeof(IHttpActionResult).Name, "ExecuteAsync"));
            }

            return response;
        }
    }
}

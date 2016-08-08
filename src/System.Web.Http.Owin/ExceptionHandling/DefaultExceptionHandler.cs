// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Owin.Properties;
using System.Web.Http.Results;

namespace System.Web.Http.Owin.ExceptionHandling
{
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

            HttpRequestMessage request = exceptionContext.Request;

            if (request == null)
            {
                throw new ArgumentException(Error.Format(OwinResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Request"), "context");
            }

            context.Result = new ResponseMessageResult(request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                exceptionContext.Exception));
        }
    }
}

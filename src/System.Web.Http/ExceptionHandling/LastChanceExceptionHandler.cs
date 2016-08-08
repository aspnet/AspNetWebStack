// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Results;

namespace System.Web.Http.ExceptionHandling
{
    internal class LastChanceExceptionHandler : IExceptionHandler
    {
        private readonly IExceptionHandler _innerHandler;

        public LastChanceExceptionHandler(IExceptionHandler innerHandler)
        {
            if (innerHandler == null)
            {
                throw new ArgumentNullException("innerHandler");
            }

            _innerHandler = innerHandler;
        }

        public IExceptionHandler InnerHandler
        {
            get { return _innerHandler; }
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context != null)
            {
                ExceptionContext exceptionContext = context.ExceptionContext;
                Contract.Assert(exceptionContext != null);

                ExceptionContextCatchBlock catchBlock = exceptionContext.CatchBlock;

                if (catchBlock != null && catchBlock.IsTopLevel)
                {
                    context.Result = CreateDefaultLastChanceResult(exceptionContext);
                }
            }

            return _innerHandler.HandleAsync(context, cancellationToken);
        }

        private static IHttpActionResult CreateDefaultLastChanceResult(ExceptionContext context)
        {
            Contract.Assert(context != null);

            Exception exception = context.Exception;

            if (exception == null)
            {
                return null;
            }

            HttpRequestMessage request = context.Request;

            if (request == null)
            {
                return null;
            }

            HttpRequestContext requestContext = context.RequestContext;

            if (requestContext == null)
            {
                return null;
            }

            HttpConfiguration configuration = requestContext.Configuration;

            if (configuration == null)
            {
                return null;
            }

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);
            IContentNegotiator contentNegotiator = services.GetContentNegotiator();

            if (contentNegotiator == null)
            {
                return null;
            }

            IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;
            Contract.Assert(formatters != null);

            return new ExceptionResult(exception, requestContext.IncludeErrorDetail, contentNegotiator, request,
                formatters);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;

namespace System.Web.Http.Controllers
{
    internal class ExceptionFilterResult : IHttpActionResult
    {
        private readonly HttpActionContext _context;
        private readonly IExceptionFilter[] _filters;
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IExceptionHandler _exceptionHandler;

        private readonly IHttpActionResult _innerResult;

        public ExceptionFilterResult(HttpActionContext context, IExceptionFilter[] filters,
            IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler, IHttpActionResult innerResult)
        {
            Contract.Assert(context != null);
            Contract.Assert(filters != null);
            Contract.Assert(exceptionLogger != null);
            Contract.Assert(exceptionHandler != null);
            Contract.Assert(innerResult != null);

            _context = context;
            _filters = filters;
            _exceptionLogger = exceptionLogger;
            _exceptionHandler = exceptionHandler;
            _innerResult = innerResult;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo exceptionInfo;

            try
            {
                return await _innerResult.ExecuteAsync(cancellationToken);
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            // This code path only runs if the task is faulted with an exception
            Exception exception = exceptionInfo.SourceException;
            Debug.Assert(exception != null);

            bool isCancellationException = exception is OperationCanceledException;

            ExceptionContext exceptionContext = new ExceptionContext(
                exception,
                ExceptionCatchBlocks.IExceptionFilter,
                _context);

            if (!isCancellationException)
            {
                // We don't log cancellation exceptions because it doesn't represent an error.
                await _exceptionLogger.LogAsync(exceptionContext, cancellationToken);
            }

            HttpActionExecutedContext executedContext = new HttpActionExecutedContext(_context, exception);

            // Note: exception filters need to be scheduled in the reverse order so that
            // the more specific filter (e.g. Action) executes before the less specific ones (e.g. Global)
            for (int i = _filters.Length - 1; i >= 0; i--)
            {
                IExceptionFilter exceptionFilter = _filters[i];
                await exceptionFilter.ExecuteExceptionFilterAsync(executedContext, cancellationToken);
            }

            if (executedContext.Response == null && !isCancellationException)
            {
                // We don't log cancellation exceptions because it doesn't represent an error.
                executedContext.Response = await _exceptionHandler.HandleAsync(exceptionContext, cancellationToken);
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }
            else
            {
                // Preserve the original stack trace when the exception is not changed by any filter.
                if (exception == executedContext.Exception)
                {
                    exceptionInfo.Throw();
                }

                // If the exception is changed by a filter, throw the new exception instead.
                throw executedContext.Exception;
            }
        }
    }
}

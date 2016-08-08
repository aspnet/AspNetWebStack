// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IExceptionFilter"/>.
    /// </summary>
    internal class ExceptionFilterTracer : FilterTracer, IExceptionFilter, IDecorator<IExceptionFilter>
    {
        private const string ExecuteExceptionFilterAsyncMethodName = "ExecuteExceptionFilterAsync";

        public ExceptionFilterTracer(IExceptionFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        public new IExceptionFilter Inner
        {
            get { return InnerExceptionFilter; }
        }

        public IExceptionFilter InnerExceptionFilter
        {
            get { return InnerFilter as IExceptionFilter; }
        }

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext,
                                                CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                actionExecutedContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                InnerExceptionFilter.GetType().Name,
                ExecuteExceptionFilterAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Exception = actionExecutedContext.Exception;
                },

                execute: () => InnerExceptionFilter.ExecuteExceptionFilterAsync(actionExecutedContext, cancellationToken),

                endTrace: (tr) =>
                {
                    tr.Exception = actionExecutedContext.Exception;
                },

                errorTrace: null);
        }
    }
}

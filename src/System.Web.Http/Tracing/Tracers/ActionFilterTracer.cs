// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IActionFilter"/>.
    /// </summary>
    internal class ActionFilterTracer : FilterTracer, IActionFilter, IDecorator<IActionFilter>
    {
        private const string ExecuteActionFilterAsyncMethodName = "ExecuteActionFilterAsync";

        public ActionFilterTracer(IActionFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        public new IActionFilter Inner
        {
            get { return InnerActionFilter; }
        }

        private IActionFilter InnerActionFilter
        {
            get { return InnerFilter as IActionFilter; }
        }

        Task<HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(HttpActionContext actionContext,
                                                                         CancellationToken cancellationToken,
                                                                         Func<Task<HttpResponseMessage>> continuation)
        {
            return TraceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                actionContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                InnerActionFilter.GetType().Name,
                ExecuteActionFilterAsyncMethodName,
                beginTrace: null,
                execute: () => InnerActionFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, continuation),
                endTrace: (tr, response) =>
                {
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: null);
        }
    }
}

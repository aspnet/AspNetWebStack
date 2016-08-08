// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="ActionFilterAttribute"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "internal type needs to override, tracer are not sealed")]
    internal class ActionFilterAttributeTracer : ActionFilterAttribute, IDecorator<ActionFilterAttribute>
    {
        private readonly ActionFilterAttribute _innerFilter;
        private readonly ITraceWriter _traceWriter;

        public ActionFilterAttributeTracer(ActionFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            Contract.Assert(innerFilter != null);
            Contract.Assert(traceWriter != null);

            _innerFilter = innerFilter;
            _traceWriter = traceWriter;
        }

        public ActionFilterAttribute Inner
        {
            get { return _innerFilter; }
        }

        public override bool AllowMultiple
        {
            get
            {
                return _innerFilter.AllowMultiple;
            }
        }

        public override object TypeId
        {
            get
            {
                return _innerFilter.TypeId;
            }
        }

        public override bool Equals(object obj)
        {
            return _innerFilter.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _innerFilter.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return _innerFilter.IsDefaultAttribute();
        }

        public override bool Match(object obj)
        {
            return _innerFilter.Match(obj);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // This will never get called, all the traces are going through OnActionExecutingAsync, which calls directly the inner method.
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return OnActionExecutedAsyncCore(actionExecutedContext, cancellationToken);
        }

        private Task OnActionExecutedAsyncCore(HttpActionExecutedContext actionExecutedContext,
                                               CancellationToken cancellationToken,
                                               [CallerMemberName] string methodName = null)
        {
            return _traceWriter.TraceBeginEndAsync(
                actionExecutedContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                _innerFilter.GetType().Name,
                methodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                        SRResources.TraceActionFilterMessage,
                                        FormattingUtilities.ActionDescriptorToString(
                                            actionExecutedContext.ActionContext.ActionDescriptor));
                    tr.Exception = actionExecutedContext.Exception;
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                execute: async () =>
                {
                    await _innerFilter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
                },
                endTrace: (tr) =>
                {
                    tr.Exception = actionExecutedContext.Exception;
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: (tr) =>
                {
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                });
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // This will never get called, all the traces are going through OnActionExecutingAsync, which calls directly the inner method.
        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return OnActionExecutingAsyncCore(actionContext, cancellationToken);
        }

        private Task OnActionExecutingAsyncCore(HttpActionContext actionContext,
                                                CancellationToken cancellationToken,
                                                [CallerMemberName] string methodName = null)
        {
            return _traceWriter.TraceBeginEndAsync(
                actionContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                _innerFilter.GetType().Name,
                methodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                    SRResources.TraceActionFilterMessage,
                                    FormattingUtilities.ActionDescriptorToString(
                                        actionContext.ActionDescriptor));

                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                execute: async () =>
                {
                    await _innerFilter.OnActionExecutingAsync(actionContext, cancellationToken);
                },
                endTrace: (tr) =>
                {
                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: (tr) =>
                {
                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                });
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpActionInvoker"/>.
    /// </summary>
    internal class HttpActionInvokerTracer : IHttpActionInvoker, IDecorator<IHttpActionInvoker>
    {
        private const string InvokeActionAsyncMethodName = "InvokeActionAsync";

        private readonly IHttpActionInvoker _innerInvoker;
        private readonly ITraceWriter _traceWriter;

        public HttpActionInvokerTracer(IHttpActionInvoker innerInvoker, ITraceWriter traceWriter)
        {
            Contract.Assert(innerInvoker != null);
            Contract.Assert(traceWriter != null);

            _innerInvoker = innerInvoker;
            _traceWriter = traceWriter;
        }

        public IHttpActionInvoker Inner
        {
            get { return _innerInvoker; }
        }

        Task<HttpResponseMessage> IHttpActionInvoker.InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                actionContext.ControllerContext.Request,
                TraceCategories.ActionCategory,
                TraceLevel.Info,
                _innerInvoker.GetType().Name,
                InvokeActionAsyncMethodName,

                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceActionInvokeMessage,
                        FormattingUtilities.ActionInvokeToString(actionContext));
                },

                execute: () => (Task<HttpResponseMessage>)_innerInvoker.InvokeActionAsync(actionContext, cancellationToken),

                endTrace: (tr, result) =>
                {
                    HttpResponseMessage response = result;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },

                errorTrace: null);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpController"/>.
    /// </summary>
    internal class HttpControllerTracer : IHttpController, IDisposable, IDecorator<IHttpController>
    {
        private const string DisposeMethodName = "Dispose";
        private const string ExecuteAsyncMethodName = "ExecuteAsync";

        private readonly IHttpController _innerController;
        private readonly HttpRequestMessage _request;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerTracer(HttpRequestMessage request, IHttpController innerController, ITraceWriter traceWriter)
        {
            Contract.Assert(innerController != null);
            Contract.Assert(traceWriter != null);

            _innerController = innerController;
            _request = request;
            _traceWriter = traceWriter;
        }

        public IHttpController Inner
        {
            get { return _innerController; }
        }

        void IDisposable.Dispose()
        {
            IDisposable disposable = _innerController as IDisposable;
            if (disposable != null)
            {
                _traceWriter.TraceBeginEnd(
                    _request,
                    TraceCategories.ControllersCategory,
                    TraceLevel.Info,
                    _innerController.GetType().Name,
                    DisposeMethodName,
                    beginTrace: null,
                    execute: disposable.Dispose,
                    endTrace: null,
                    errorTrace: null);
            }
        }

        Task<HttpResponseMessage> IHttpController.ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                controllerContext.Request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerController.GetType().Name,
                ExecuteAsyncMethodName,
                beginTrace: null,
                execute: () =>
                {
                    // Critical to allow wrapped controller to have itself in ControllerContext
                    controllerContext.Controller = ActualController(controllerContext.Controller);
                    return ExecuteAsyncCore(controllerContext, cancellationToken);
                },
                endTrace: (tr, response) =>
                {
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: null);
        }

        private async Task<HttpResponseMessage> ExecuteAsyncCore(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            try
            {
                return await _innerController.ExecuteAsync(controllerContext, cancellationToken);
            }
            finally
            {
                IDisposable disposable = _innerController as IDisposable;

                if (disposable != null)
                {
                    // Need to remove the original controller from the disposables list, if it's
                    // there, and put ourselves in there instead, so we can trace the dispose.
                    // This currently knows a little too much about how RegisterForDispose works,
                    // but that's unavoidable unless we want to offer UnregisterForDispose.
                    IList<IDisposable> disposables;
                    if (_request.Properties.TryGetValue(HttpPropertyKeys.DisposableRequestResourcesKey, out disposables))
                    {
                        disposables.Remove(disposable);
                        disposables.Add(this);
                    }
                }
            }
        }

        public static IHttpController ActualController(IHttpController controller)
        {
            HttpControllerTracer tracer = controller as HttpControllerTracer;
            return tracer == null ? controller : tracer._innerController;
        }

        public static Type ActualControllerType(IHttpController controller)
        {
            return ActualController(controller).GetType();
        }
    }
}

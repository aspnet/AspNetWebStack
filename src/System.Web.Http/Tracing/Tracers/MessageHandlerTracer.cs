// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to wrap a <see cref="DelegatingHandler"/>.
    /// </summary>
    internal class MessageHandlerTracer : DelegatingHandler, IDecorator<DelegatingHandler>
    {
        private const string SendAsyncMethodName = "SendAsync";

        private readonly DelegatingHandler _innerHandler;
        private readonly ITraceWriter _traceWriter;

        public MessageHandlerTracer(DelegatingHandler innerHandler, ITraceWriter traceWriter)
        {
            Contract.Assert(innerHandler != null);
            Contract.Assert(traceWriter != null);

            _innerHandler = innerHandler;
            _traceWriter = traceWriter;
        }

        public DelegatingHandler Inner
        {
            get { return _innerHandler; }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                request,
                TraceCategories.MessageHandlersCategory,
                TraceLevel.Info,
                _innerHandler.GetType().Name,
                SendAsyncMethodName,
                beginTrace: null,
                execute: () => base.SendAsync(request, cancellationToken),
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

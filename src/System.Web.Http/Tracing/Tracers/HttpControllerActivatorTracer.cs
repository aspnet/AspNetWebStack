// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpControllerActivator"/>.
    /// </summary>
    internal class HttpControllerActivatorTracer : IHttpControllerActivator, IDecorator<IHttpControllerActivator>
    {
        private const string CreateMethodName = "Create";

        private readonly IHttpControllerActivator _innerActivator;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerActivatorTracer(IHttpControllerActivator innerActivator, ITraceWriter traceWriter)
        {
            Contract.Assert(innerActivator != null);
            Contract.Assert(traceWriter != null);

            _innerActivator = innerActivator;
            _traceWriter = traceWriter;
        }

        public IHttpControllerActivator Inner
        {
            get { return _innerActivator; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposable controller is later released in ReleaseController")]
        IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            IHttpController controller = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerActivator.GetType().Name,
                CreateMethodName,
                beginTrace: null,
                execute: () =>
                {
                    controller = _innerActivator.Create(request, controllerDescriptor, controllerType);
                },
                endTrace: (tr) =>
                {
                    tr.Message = controller == null ? SRResources.TraceNoneObjectMessage : controller.GetType().FullName;
                },
                errorTrace: null);

            if (controller != null && !(controller is HttpControllerTracer))
            {
                controller = new HttpControllerTracer(request, controller, _traceWriter);
            }

            return controller;
        }
    }
}

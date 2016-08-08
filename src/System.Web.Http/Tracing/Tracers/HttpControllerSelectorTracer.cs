// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpControllerSelector"/>.
    /// </summary>
    internal class HttpControllerSelectorTracer : IHttpControllerSelector, IDecorator<IHttpControllerSelector>
    {
        private const string SelectControllerMethodName = "SelectController";

        private readonly IHttpControllerSelector _innerSelector;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerSelectorTracer(IHttpControllerSelector innerSelector, ITraceWriter traceWriter)
        {
            Contract.Assert(innerSelector != null);
            Contract.Assert(traceWriter != null);

            _innerSelector = innerSelector;
            _traceWriter = traceWriter;
        }

        public IHttpControllerSelector Inner
        {
            get { return _innerSelector; }
        }

        HttpControllerDescriptor IHttpControllerSelector.SelectController(HttpRequestMessage request)
        {
            HttpControllerDescriptor controllerDescriptor = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerSelector.GetType().Name,
                SelectControllerMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                    SRResources.TraceRouteMessage,
                                    FormattingUtilities.RouteToString(request.GetRouteData()));
                },
                execute: () =>
                {
                    controllerDescriptor = _innerSelector.SelectController(request);
                },
                endTrace: (tr) =>
                {
                    tr.Message = controllerDescriptor == null
                                        ? SRResources.TraceNoneObjectMessage
                                        : controllerDescriptor.ControllerName;
                },
                errorTrace: null);

            if (controllerDescriptor != null && !(controllerDescriptor is HttpControllerDescriptorTracer))
            {
                return new HttpControllerDescriptorTracer(controllerDescriptor, _traceWriter);
            }

            return controllerDescriptor;
        }

        IDictionary<string, HttpControllerDescriptor> IHttpControllerSelector.GetControllerMapping()
        {
            return _innerSelector.GetControllerMapping();
        }
    }
}

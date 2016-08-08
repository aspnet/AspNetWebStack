// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref=" HttpControllerDescriptor"/>
    /// </summary>
    internal class HttpControllerDescriptorTracer : HttpControllerDescriptor, IDecorator<HttpControllerDescriptor>
    {
        private const string CreateControllerMethodName = "CreateController";

        private readonly HttpControllerDescriptor _innerDescriptor;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerDescriptorTracer(HttpControllerDescriptor innerDescriptor, ITraceWriter traceWriter)
        {
            Contract.Assert(innerDescriptor != null);
            Contract.Assert(traceWriter != null);

            Configuration = innerDescriptor.Configuration;
            ControllerName = innerDescriptor.ControllerName;
            ControllerType = innerDescriptor.ControllerType;

            _innerDescriptor = innerDescriptor;
            _traceWriter = traceWriter;
        }

        public HttpControllerDescriptor Inner
        {
            get { return _innerDescriptor; }
        }

        public override ConcurrentDictionary<object, object> Properties
        {
            get
            {
                return _innerDescriptor.Properties;
            }
        }

        public override Collection<T> GetCustomAttributes<T>()
        {
            return _innerDescriptor.GetCustomAttributes<T>();
        }

        public override Collection<T> GetCustomAttributes<T>(bool inherit)
        {
            return _innerDescriptor.GetCustomAttributes<T>(inherit);
        }

        public override Collection<Filters.IFilter> GetFilters()
        {
            return _innerDescriptor.GetFilters();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This object is returned back to the caller")]
        public override IHttpController CreateController(HttpRequestMessage request)
        {
            IHttpController controller = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerDescriptor.GetType().Name,
                CreateControllerMethodName,
                beginTrace: null,
                execute: () =>
                {
                    controller = _innerDescriptor.CreateController(request);
                },
                endTrace: (tr) =>
                {
                    tr.Message = controller == null
                                        ? SRResources.TraceNoneObjectMessage
                                        : HttpControllerTracer.ActualControllerType(controller).FullName;
                },
                errorTrace: null);

            if (controller != null && !(controller is HttpControllerTracer))
            {
                return new HttpControllerTracer(request, controller, _traceWriter);
            }

            return controller;
        }
    }
}

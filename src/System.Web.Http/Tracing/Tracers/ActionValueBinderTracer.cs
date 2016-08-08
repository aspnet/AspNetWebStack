// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IActionValueBinder"/>
    /// </summary>
    internal class ActionValueBinderTracer : IActionValueBinder, IDecorator<IActionValueBinder>
    {
        private readonly IActionValueBinder _innerBinder;
        private readonly ITraceWriter _traceWriter;

        public ActionValueBinderTracer(IActionValueBinder innerBinder, ITraceWriter traceWriter)
        {
            Contract.Assert(innerBinder != null);
            Contract.Assert(traceWriter != null);

            _innerBinder = innerBinder;
            _traceWriter = traceWriter;
        }

        public IActionValueBinder Inner
        {
            get { return _innerBinder; }
        }

        // Creates wrapping tracers for all HttpParameterBindings
        HttpActionBinding IActionValueBinder.GetBinding(HttpActionDescriptor actionDescriptor)
        {
            HttpActionBinding actionBinding = _innerBinder.GetBinding(actionDescriptor);

            if (actionBinding == null)
            {
                return null;
            }

            HttpParameterBinding[] parameterBindings = actionBinding.ParameterBindings;
            HttpParameterBinding[] newParameterBindings = new HttpParameterBinding[parameterBindings.Length];
            for (int i = 0; i < newParameterBindings.Length; i++)
            {
                HttpParameterBinding parameterBinding = parameterBindings[i];

                // Intercept FormatterParameterBinding to replace its formatters
                FormatterParameterBinding formatterParameterBinding = parameterBinding as FormatterParameterBinding;
                newParameterBindings[i] = formatterParameterBinding != null
                                            ? (HttpParameterBinding)new FormatterParameterBindingTracer(formatterParameterBinding, _traceWriter)
                                            : (HttpParameterBinding)new HttpParameterBindingTracer(parameterBinding, _traceWriter);
            }

            // Replace the inner HttpActionBinding's parameter bindings with our tracing versions.
            // This allows each individual parameter binding to trace.
            actionBinding.ParameterBindings = newParameterBindings;

            // Then create an HttpActionBindingTracer to wrap the actual HttpActionBinding
            if (!(actionBinding is HttpActionBindingTracer))
            {
                return new HttpActionBindingTracer(actionBinding, _traceWriter);
            }

            return actionBinding;
        }
    }
}
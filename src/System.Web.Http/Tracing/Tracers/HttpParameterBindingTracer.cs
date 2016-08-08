// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Services;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to wrap an <see cref="HttpParameterBinding"/>.
    /// Its primary purpose is to monitor <see cref="ExecuteBindingAsync"/>.
    /// </summary>
    internal class HttpParameterBindingTracer : HttpParameterBinding, IValueProviderParameterBinding, IDecorator<HttpParameterBinding>
    {
        private const string ExecuteBindingAsyncMethodName = "ExecuteBindingAsync";

        public HttpParameterBindingTracer(HttpParameterBinding innerBinding, ITraceWriter traceWriter) : base(innerBinding.Descriptor)
        {
            Contract.Assert(innerBinding != null);
            Contract.Assert(traceWriter != null);

            InnerBinding = innerBinding;
            TraceWriter = traceWriter;
        }

        public HttpParameterBinding Inner
        {
            get { return InnerBinding; }
        }

        protected HttpParameterBinding InnerBinding { get; private set; }

        protected ITraceWriter TraceWriter { get; private set; }

        public override string ErrorMessage
        {
            get
            {
                return InnerBinding.ErrorMessage;
            }
        }

        public override bool WillReadBody
        {
            get
            {
                return InnerBinding.WillReadBody;
            }
        }

        public IEnumerable<ValueProviderFactory> ValueProviderFactories
        {
            get
            {
                IValueProviderParameterBinding valueProviderParameterBinding = InnerBinding as IValueProviderParameterBinding;
                return valueProviderParameterBinding != null ? valueProviderParameterBinding.ValueProviderFactories : Enumerable.Empty<ValueProviderFactory>();
            }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                actionContext.Request,
                TraceCategories.ModelBindingCategory,
                TraceLevel.Info,
                InnerBinding.GetType().Name,
                ExecuteBindingAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(SRResources.TraceBeginParameterBind,
                                              InnerBinding.Descriptor.ParameterName);
                },

                execute: () => InnerBinding.ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken),

                endTrace: (tr) =>
                {
                    string parameterName = InnerBinding.Descriptor.ParameterName;

                    // Model binding error for this parameter shows the error
                    if (!actionContext.ModelState.IsValid && actionContext.ModelState.ContainsKey(parameterName))
                    {
                        tr.Message = Error.Format(SRResources.TraceModelStateInvalidMessage,
                                                  FormattingUtilities.ModelStateToString(
                                                       actionContext.ModelState));
                    }
                    else
                    {
                        tr.Message = actionContext.ActionArguments.ContainsKey(parameterName)
                                         ? Error.Format(SRResources.TraceEndParameterBind, parameterName,
                                                        FormattingUtilities.ValueToString(
                                                             actionContext.ActionArguments[parameterName],
                                                             CultureInfo.CurrentCulture))
                                         : Error.Format(SRResources.TraceEndParameterBindNoBind,
                                                        parameterName);
                    }
                },
                errorTrace: null);
        }
    }
}

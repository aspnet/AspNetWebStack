// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to wrap a <see cref="FormatterParameterBinding"/>.
    /// Its primary purpose is to intercept binding requests so that it can create tracers for the formatters.
    /// </summary>
    internal class FormatterParameterBindingTracer : FormatterParameterBinding, IDecorator<FormatterParameterBinding>
    {
        private const string ExecuteBindingAsyncMethodName = "ExecuteBindingAsync";

        private readonly FormatterParameterBinding _innerBinding;
        private readonly ITraceWriter _traceWriter;

        public FormatterParameterBindingTracer(FormatterParameterBinding innerBinding, ITraceWriter traceWriter)
            : base(innerBinding.Descriptor, innerBinding.Formatters, innerBinding.BodyModelValidator)
        {
            Contract.Assert(innerBinding != null);
            Contract.Assert(traceWriter != null);

            _innerBinding = innerBinding;
            _traceWriter = traceWriter;
        }

        public FormatterParameterBinding Inner
        {
            get { return _innerBinding; }
        }

        public override string ErrorMessage
        {
            get { return _innerBinding.ErrorMessage; }
        }

        public override bool WillReadBody
        {
            get { return _innerBinding.WillReadBody; }
        }

        public override Task<object> ReadContentAsync(HttpRequestMessage request, Type type,
            IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            // Intercept this method solely to wrap formatters with request-aware formatter tracers
            // There is no other interception point where a request and a formatter are paired.
            return _innerBinding.ReadContentAsync(request, type, CreateFormatterTracers(request, formatters), formatterLogger);
        }

        public override Task<object> ReadContentAsync(HttpRequestMessage request, Type type,
            IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            return _innerBinding.ReadContentAsync(request, type, formatters, formatterLogger, cancellationToken);
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync(
                actionContext.Request,
                TraceCategories.ModelBindingCategory,
                TraceLevel.Info,
                _innerBinding.GetType().Name,
                ExecuteBindingAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(SRResources.TraceBeginParameterBind,
                                              _innerBinding.Descriptor.ParameterName);
                },

                execute: () => _innerBinding.ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken),

                endTrace: (tr) =>
                {
                    string parameterName = _innerBinding.Descriptor.ParameterName;
                    tr.Message = actionContext.ActionArguments.ContainsKey(parameterName)
                                    ? Error.Format(SRResources.TraceEndParameterBind, parameterName,
                                                    FormattingUtilities.ValueToString(actionContext.ActionArguments[parameterName], CultureInfo.CurrentCulture))
                                    : Error.Format(SRResources.TraceEndParameterBindNoBind,
                                                    parameterName);
                },
                errorTrace: null);
        }

        private IEnumerable<MediaTypeFormatter> CreateFormatterTracers(HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            List<MediaTypeFormatter> formatterTracers = new List<MediaTypeFormatter>();
            foreach (MediaTypeFormatter formatter in formatters)
            {
                formatterTracers.Add(MediaTypeFormatterTracer.CreateTracer(formatter, _traceWriter, request));
            }

            return formatterTracers;
        }
    }
}

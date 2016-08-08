// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IContentNegotiator"/>.
    /// </summary>
    internal class ContentNegotiatorTracer : IContentNegotiator, IDecorator<IContentNegotiator>
    {
        private const string NegotiateMethodName = "Negotiate";

        private readonly IContentNegotiator _innerNegotiator;
        private readonly ITraceWriter _traceWriter;

        public ContentNegotiatorTracer(IContentNegotiator innerNegotiator, ITraceWriter traceWriter)
        {
            Contract.Assert(innerNegotiator != null);
            Contract.Assert(traceWriter != null);

            _innerNegotiator = innerNegotiator;
            _traceWriter = traceWriter;
        }

        public IContentNegotiator Inner
        {
            get { return _innerNegotiator; }
        }

        public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            ContentNegotiationResult result = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                _innerNegotiator.GetType().Name,
                NegotiateMethodName,

                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceNegotiateFormatter,
                        type.Name,
                        FormattingUtilities.FormattersToString(formatters));
                },

                execute: () =>
                {
                    result = _innerNegotiator.Negotiate(type, request, formatters);
                },

                endTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceSelectedFormatter,
                        result == null
                            ? SRResources.TraceNoneObjectMessage
                            : MediaTypeFormatterTracer.ActualMediaTypeFormatter(result.Formatter).GetType().Name,
                        result == null || result.MediaType == null
                            ? SRResources.TraceNoneObjectMessage
                            : result.MediaType.ToString());
                },

                errorTrace: null);

            if (result != null)
            {
                result.Formatter = MediaTypeFormatterTracer.CreateTracer(result.Formatter, _traceWriter, request);
            }

            return result;
        }
    }
}

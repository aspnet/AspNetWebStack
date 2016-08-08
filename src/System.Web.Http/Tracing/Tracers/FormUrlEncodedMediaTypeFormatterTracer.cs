// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="FormUrlEncodedMediaTypeFormatter"/>.  
    /// It is required because users can select formatters by this type.
    /// </summary>
    internal class FormUrlEncodedMediaTypeFormatterTracer : FormUrlEncodedMediaTypeFormatter, IFormatterTracer, IDecorator<FormUrlEncodedMediaTypeFormatter>
    {
        private readonly FormUrlEncodedMediaTypeFormatter _inner;
        private MediaTypeFormatterTracer _innerTracer;

        public FormUrlEncodedMediaTypeFormatterTracer(FormUrlEncodedMediaTypeFormatter innerFormatter, ITraceWriter traceWriter, HttpRequestMessage request)
            : base(innerFormatter)
        {
            _inner = innerFormatter;
            _innerTracer = new MediaTypeFormatterTracer(innerFormatter, traceWriter, request);
        }

        HttpRequestMessage IFormatterTracer.Request
        {
            get { return _innerTracer.Request; }
        }

        public FormUrlEncodedMediaTypeFormatter Inner
        {
            get { return _inner; }
        }

        public MediaTypeFormatter InnerFormatter
        {
            get { return _innerTracer.InnerFormatter; }
        }

        public override IRequiredMemberSelector RequiredMemberSelector
        {
            get
            {
                return _innerTracer.RequiredMemberSelector;
            }
            set
            {
                _innerTracer.RequiredMemberSelector = value;
            }
        }

        public override bool CanReadType(Type type)
        {
            return _innerTracer.CanReadType(type);
        }

        public override bool CanWriteType(Type type)
        {
            return _innerTracer.CanWriteType(type);
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            return _innerTracer.GetPerRequestFormatterInstance(type, request, mediaType);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
            IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            return _innerTracer.ReadFromStreamAsync(type, readStream, content, formatterLogger, cancellationToken);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return _innerTracer.ReadFromStreamAsync(type, readStream, content, formatterLogger);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext, CancellationToken cancellationToken)
        {
            return _innerTracer.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return _innerTracer.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            _innerTracer.SetDefaultContentHeaders(type, headers, mediaType);
        }
    }
}

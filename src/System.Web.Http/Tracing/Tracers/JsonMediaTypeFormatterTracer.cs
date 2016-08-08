// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Services;
using Newtonsoft.Json;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="JsonMediaTypeFormatter"/>.  
    /// It is required because users can select formatters by this type.
    /// </summary>
    internal class JsonMediaTypeFormatterTracer : JsonMediaTypeFormatter, IFormatterTracer, IDecorator<JsonMediaTypeFormatter>
    {
        private readonly JsonMediaTypeFormatter _inner;
        private MediaTypeFormatterTracer _innerTracer;

        public JsonMediaTypeFormatterTracer(JsonMediaTypeFormatter innerFormatter, ITraceWriter traceWriter, HttpRequestMessage request)
            : base(innerFormatter)
        {
            _inner = innerFormatter;
            _innerTracer = new MediaTypeFormatterTracer(innerFormatter, traceWriter, request);
        }

        HttpRequestMessage IFormatterTracer.Request
        {
            get { return _innerTracer.Request; }
        }

        public JsonMediaTypeFormatter Inner
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
            IFormatterLogger formatterLogger)
        {
            return _innerTracer.ReadFromStreamAsync(type, readStream, content, formatterLogger);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
            IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            return _innerTracer.ReadFromStreamAsync(type, readStream, content, formatterLogger, cancellationToken);
        }

        // Callback from ReadFromStreamAsync is not expected to be called; _innerTracer.ReadFromStreamAsync uses
        // _inner.ReadFromStreamAsync
        public override object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
        {
            return _inner.ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
        }

        // Callback from ReadFromStreamAsync is not expected to be called; _innerTracer.ReadFromStreamAsync uses
        // _inner.ReadFromStreamAsync
        public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            return _inner.CreateJsonReader(type, readStream, effectiveEncoding);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            return _innerTracer.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext, CancellationToken cancellationToken)
        {
            return _innerTracer.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
        }

        // Callback from WriteToStreamAsync is not expected to be called; _innerTracer.WriteToStreamAsync uses
        // _inner.WriteToStreamAsync
        public override void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
        {
            _inner.WriteToStream(type, value, writeStream, effectiveEncoding);
        }

        // Callback from WriteToStreamAsync is not expected to be called; _innerTracer.WriteToStreamAsync uses
        // _inner.WriteToStreamAsync
        public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
        {
            return _inner.CreateJsonWriter(type, writeStream, effectiveEncoding);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            _innerTracer.SetDefaultContentHeaders(type, headers, mediaType);
        }

        // Callback is not expected to be called; _innerTracer methods won't use our base methods
        public override JsonSerializer CreateJsonSerializer()
        {
            return _inner.CreateJsonSerializer();
        }

        // Callback is not expected to be called; _innerTracer methods won't use our base methods
        public override DataContractJsonSerializer CreateDataContractSerializer(Type type)
        {
            return _inner.CreateDataContractSerializer(type);
        }
    }
}

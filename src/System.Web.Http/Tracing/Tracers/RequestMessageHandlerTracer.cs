// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Internal <see cref="DelegatingHandler"/> that executes before and after all of the installed message handlers.
    /// The begin trace of this handler is the first trace for the request.
    /// </summary>
    internal class RequestMessageHandlerTracer : DelegatingHandler
    {
        private readonly ITraceWriter _traceWriter;

        public RequestMessageHandlerTracer(ITraceWriter traceWriter)
        {
            Contract.Assert(traceWriter != null);

            _traceWriter = traceWriter;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                request,
                TraceCategories.RequestCategory,
                TraceLevel.Info,
                String.Empty,
                String.Empty,
                beginTrace: (tr) =>
                {
                    tr.Message = request.RequestUri == null ? SRResources.TraceNoneObjectMessage : request.RequestUri.ToString();
                },

                execute: () => base.SendAsync(request, cancellationToken),

                endTrace: (tr, response) =>
                {
                    MediaTypeHeaderValue contentType = response == null
                                                            ? null
                                                            : response.Content == null
                                                                    ? null
                                                                    : response.Content.Headers.ContentType;

                    long? contentLength = response == null
                                                ? null
                                                : response.Content == null
                                                    ? null
                                                    : response.Content.Headers.ContentLength;

                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }

                    tr.Message =
                        Error.Format(SRResources.TraceRequestCompleteMessage,
                                     contentType == null
                                        ? SRResources.TraceNoneObjectMessage
                                        : contentType.ToString(),
                                     contentLength.HasValue
                                        ? contentLength.Value.ToString(CultureInfo.CurrentCulture)
                                        : SRResources.TraceUnknownMessage);
                },
                errorTrace: null);
        }
    }
}

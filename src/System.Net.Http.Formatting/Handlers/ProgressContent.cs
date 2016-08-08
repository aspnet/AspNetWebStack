// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace System.Net.Http.Handlers
{
    /// <summary>
    /// Wraps an inner <see cref="HttpContent"/> in order to insert a <see cref="ProgressStream"/> on writing data.
    /// </summary>
    internal class ProgressContent : HttpContent
    {
        private readonly HttpContent _innerContent;
        private readonly ProgressMessageHandler _handler;
        private readonly HttpRequestMessage _request;

        public ProgressContent(HttpContent innerContent, ProgressMessageHandler handler, HttpRequestMessage request)
        {
            Contract.Assert(innerContent != null);
            Contract.Assert(handler != null);
            Contract.Assert(request != null);

            _innerContent = innerContent;
            _handler = handler;
            _request = request;

            innerContent.Headers.CopyTo(Headers);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            ProgressStream progressStream = new ProgressStream(stream, _handler, _request, response: null);
            return _innerContent.CopyToAsync(progressStream);
        }

        protected override bool TryComputeLength(out long length)
        {
            long? contentLength = _innerContent.Headers.ContentLength;
            if (contentLength.HasValue)
            {
                length = contentLength.Value;
                return true;
            }

            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _innerContent.Dispose();
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Net.Http.Formatting.Mocks
{
    public delegate bool TryComputeLengthDelegate(out long length);

    public class MockHttpContent : HttpContent
    {
        public MockHttpContent()
        {
        }

        public MockHttpContent(HttpContent innerContent)
        {
            InnerContent = innerContent;
            Headers.ContentType = innerContent.Headers.ContentType;
        }

        public MockHttpContent(MediaTypeHeaderValue contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            Headers.ContentType = contentType;
        }

        public MockHttpContent(string contentType)
        {
            if (String.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentNullException("contentType");
            }
            Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        public HttpContent InnerContent { get; set; }

        public Action<bool> DisposeCallback { get; set; }
        public TryComputeLengthDelegate TryComputeLengthCallback { get; set; }
        public Action<Stream, TransportContext> SerializeToStreamCallback { get; set; }
        public Func<Stream, TransportContext, Task> SerializeToStreamAsyncCallback { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (DisposeCallback != null)
            {
                DisposeCallback(disposing);
            }

            base.Dispose(disposing);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            if (SerializeToStreamAsyncCallback != null)
            {
                return SerializeToStreamAsyncCallback(stream, context);
            }
            else if (InnerContent != null)
            {
                return InnerContent.CopyToAsync(stream, context);
            }
            else
            {
                throw new InvalidOperationException("Construct with inner HttpContent or set SerializeToStreamCallback first.");
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            if (TryComputeLengthCallback != null)
            {
                return TryComputeLengthCallback(out length);
            }

            if (InnerContent != null)
            {
                long? len = InnerContent.Headers.ContentLength;
                length = len.HasValue ? len.Value : 0L;
                return len.HasValue;
            }

            length = 0L;
            return false;
        }
    }
}

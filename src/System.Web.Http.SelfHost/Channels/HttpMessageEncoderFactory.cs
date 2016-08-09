// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.SelfHost.Properties;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.Channels
{
    internal class HttpMessageEncoderFactory : MessageEncoderFactory
    {
        private HttpMessageEncoder _encoder;

        public HttpMessageEncoderFactory()
        {
            _encoder = new HttpMessageEncoder();
        }

        public override MessageEncoder Encoder
        {
            get { return _encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return MessageVersion.None; }
        }

        public override MessageEncoder CreateSessionEncoder()
        {
            throw Error.NotSupported(SRResources.HttpMessageEncoderFactoryDoesNotSupportSessionEncoder, typeof(HttpMessageEncoderFactory).Name);
        }

        private class HttpMessageEncoder : MessageEncoder
        {
            private const string ContentTypeHeaderName = "Content-Type";
            private const string MaxSentMessageSizeExceededResourceStringName = "MaxSentMessageSizeExceeded";
            private static readonly string _httpBindingClassName = typeof(HttpBinding).FullName;
            private static readonly string _httpResponseMessageClassName = typeof(HttpResponseMessage).FullName;

            public override string ContentType
            {
                get { return String.Empty; }
            }

            public override string MediaType
            {
                get { return String.Empty; }
            }

            public override MessageVersion MessageVersion
            {
                get { return MessageVersion.None; }
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType == null)
                {
                    throw Error.ArgumentNull("contentType");
                }

                return true;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                if (bufferManager == null)
                {
                    throw Error.ArgumentNull("bufferManager");
                }

                HttpRequestMessage request = new HttpRequestMessage();
                request.Content = new ByteArrayBufferManagerContent(bufferManager, buffer.Array, buffer.Offset, buffer.Count);
                if (!String.IsNullOrEmpty(contentType))
                {
                    request.Content.Headers.TryAddWithoutValidation(ContentTypeHeaderName, contentType);
                }

                Message message = request.ToMessage();
                message.Properties.Encoder = this;

                return message;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                {
                    throw Error.ArgumentNull("stream");
                }

                HttpRequestMessage request = new HttpRequestMessage();
                request.Content = new StreamContent(stream);
                if (!String.IsNullOrEmpty(contentType))
                {
                    request.Content.Headers.TryAddWithoutValidation(ContentTypeHeaderName, contentType);
                }

                Message message = request.ToMessage();
                message.Properties.Encoder = this;

                return message;
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                if (message == null)
                {
                    throw Error.ArgumentNull("message");
                }

                if (bufferManager == null)
                {
                    throw Error.ArgumentNull("bufferManager");
                }

                if (maxMessageSize < 0)
                {
                    throw Error.ArgumentOutOfRange("maxMessageSize", maxMessageSize, SRResources.NonnegativeNumberRequired);
                }

                if (messageOffset < 0)
                {
                    throw Error.ArgumentOutOfRange("messageOffset", messageOffset, SRResources.NonnegativeNumberRequired);
                }

                if (messageOffset > maxMessageSize)
                {
                    throw Error.Argument(String.Empty, SRResources.ParameterMustBeLessThanOrEqualSecondParameter, "messageOffset", "maxMessageSize");
                }

                // TODO: DevDiv2 bug #378887 -- find out how to eliminate this middle buffer
                using (BufferManagerOutputStream stream = new BufferManagerOutputStream(MaxSentMessageSizeExceededResourceStringName, 0, maxMessageSize, bufferManager))
                {
                    int num;
                    stream.Skip(messageOffset);
                    WriteMessage(message, stream);

                    byte[] buffer = stream.ToArray(out num);
                    ArraySegment<byte> messageData = new ArraySegment<byte>(buffer, 0, num - messageOffset);

                    // ToArray transfers full ownership of buffer to us, meaning we are responsible for returning it to BufferManager.
                    // But we must delay that release until WCF has finished with the buffer we are returning from this method.
                    HttpMessageEncodingRequestContext requestContext = HttpMessageEncodingRequestContext.GetContextFromMessage(message);
                    Contract.Assert(requestContext != null);
                    requestContext.BufferManager = bufferManager;
                    requestContext.BufferToReturn = buffer;

                    return messageData;
                }
            }

            [SuppressMessage("Microsoft.Web.FxCop", "MW1201:DoNotCallProblematicMethodsOnTask", Justification = "The WriteMessage() API is synchronous, and Wait() won't deadlock in self-host.")]
            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                {
                    throw Error.ArgumentNull("message");
                }

                if (stream == null)
                {
                    throw Error.ArgumentNull("stream");
                }

                ThrowIfMismatchedMessageVersion(message);

                message.Properties.Encoder = this;

                HttpResponseMessage response = GetHttpResponseMessageOrThrow(message);

                if (response.Content != null)
                {
                    HttpMessageEncodingRequestContext requestContext =
                        HttpMessageEncodingRequestContext.GetContextFromMessage(message);

                    WriteMessageCore(response, stream, requestContext).Wait();
                }
            }

            private static async Task WriteMessageCore(
                HttpResponseMessage response,
                Stream stream,
                HttpMessageEncodingRequestContext requestContext)
            {
                try
                {
                    await response.Content.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    if (requestContext != null)
                    {
                        requestContext.Exception = ex;
                    }

                    throw;
                }
            }

            internal void ThrowIfMismatchedMessageVersion(Message message)
            {
                if (message.Version != MessageVersion)
                {
                    throw new ProtocolException(Error.Format(SRResources.EncoderMessageVersionMismatch, message.Version, MessageVersion));
                }
            }

            private static HttpResponseMessage GetHttpResponseMessageOrThrow(Message message)
            {
                HttpResponseMessage response = message.ToHttpResponseMessage();
                if (response == null)
                {
                    throw Error.InvalidOperation(
                        SRResources.MessageInvalidForHttpMessageEncoder,
                        _httpBindingClassName,
                        HttpMessageExtensions.ToMessageMethodName,
                        _httpResponseMessageClassName);
                }

                return response;
            }

            private class ByteArrayBufferManagerContent : ByteArrayContent
            {
                private BufferManager _bufferManager;
                private byte[] _content;

                public ByteArrayBufferManagerContent(BufferManager bufferManager, byte[] content, int offset, int count)
                    : base(content, offset, count)
                {
                    Contract.Assert(bufferManager != null, "The 'bufferManager' parameter should never be null.");

                    _bufferManager = bufferManager;
                    _content = content;
                }

                protected override void Dispose(bool disposing)
                {
                    try
                    {
                        if (disposing)
                        {
                            BufferManager oldBufferManager = Interlocked.Exchange(ref _bufferManager, null);
                            if (oldBufferManager != null)
                            {
                                oldBufferManager.ReturnBuffer(_content);
                                _content = null;
                            }
                        }
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
            }
        }
    }
}

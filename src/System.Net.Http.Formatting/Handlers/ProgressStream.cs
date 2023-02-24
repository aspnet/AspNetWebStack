// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Handlers
{
    /// <summary>
    /// This implementation of <see cref="DelegatingStream"/> registers how much data has been
    /// read (received) versus written (sent) for a particular HTTP operation. The implementation
    /// is client side in that the total bytes to send is taken from the request and the total
    /// bytes to read is taken from the response. In a server side scenario, it would be the
    /// other way around (reading the request and writing the response).
    /// </summary>
    internal class ProgressStream : DelegatingStream
    {
        private readonly ProgressMessageHandler _handler;
        private readonly HttpRequestMessage _request;

        private long _bytesReceived;
        private long? _totalBytesToReceive;

        private long _bytesSent;
        private long? _totalBytesToSend;

        public ProgressStream(Stream innerStream, ProgressMessageHandler handler, HttpRequestMessage request, HttpResponseMessage response)
            : base(innerStream)
        {
            Contract.Assert(handler != null);
            Contract.Assert(request != null);

            if (request.Content != null)
            {
                _totalBytesToSend = request.Content.Headers.ContentLength;
            }

            if (response != null && response.Content != null)
            {
                _totalBytesToReceive = response.Content.Headers.ContentLength;
            }

            _handler = handler;
            _request = request;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = InnerStream.Read(buffer, offset, count);
            ReportBytesReceived(bytesRead, userState: null);
            return bytesRead;
        }

        public override int ReadByte()
        {
            int byteRead = InnerStream.ReadByte();
            ReportBytesReceived(byteRead == -1 ? 0 : 1, userState: null);
            return byteRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int readCount = await InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
            ReportBytesReceived(readCount, userState: null);
            return readCount;
        }

#if !NETSTANDARD1_3 // BeginX and EndX are not supported on Streams in netstandard1.3
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int bytesRead = InnerStream.EndRead(asyncResult);
            ReportBytesReceived(bytesRead, asyncResult.AsyncState);
            return bytesRead;
        }
#endif

        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
            ReportBytesSent(count, userState: null);
        }

        public override void WriteByte(byte value)
        {
            InnerStream.WriteByte(value);
            ReportBytesSent(1, userState: null);
        }

       public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
            ReportBytesSent(count, userState: null);
        }

#if !NETSTANDARD1_3 // BeginX and EndX are not supported on Streams in netstandard1.3
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new ProgressWriteAsyncResult(InnerStream, this, buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ProgressWriteAsyncResult.End(asyncResult);
        }
#endif

        internal void ReportBytesSent(int bytesSent, object userState)
        {
            if (bytesSent > 0)
            {
                _bytesSent += bytesSent;
                int percentage = 0;
                if (_totalBytesToSend.HasValue && _totalBytesToSend != 0)
                {
                    percentage = (int)((100L * _bytesSent) / _totalBytesToSend);
                }

                // We only pass the request as it is guaranteed to be non-null (the response may be null)
                _handler.OnHttpRequestProgress(_request, new HttpProgressEventArgs(percentage, userState, _bytesSent, _totalBytesToSend));
            }
        }

        private void ReportBytesReceived(int bytesReceived, object userState)
        {
            if (bytesReceived > 0)
            {
                _bytesReceived += bytesReceived;
                int percentage = 0;
                if (_totalBytesToReceive.HasValue && _totalBytesToReceive != 0)
                {
                    percentage = (int)((100L * _bytesReceived) / _totalBytesToReceive);
                }

                // We only pass the request as it is guaranteed to be non-null (the response may be null)
                _handler.OnHttpResponseProgress(_request, new HttpProgressEventArgs(percentage, userState, _bytesReceived, _totalBytesToReceive));
            }
        }
    }
}

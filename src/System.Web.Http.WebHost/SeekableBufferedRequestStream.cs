// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.WebHost
{
    internal class SeekableBufferedRequestStream : NonOwnedStream
    {
        private const int ReadBufferSize = 1024;

        private readonly HttpRequestBase _request;

        private bool _isReadToEndComplete;

        public SeekableBufferedRequestStream(HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            _request = request;
            InnerStream = request.GetBufferedInputStream();
        }

        public override bool CanSeek
        {
            get
            {
                return !IsDisposed;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return InnerStream.Position;
            }
            set
            {
                ThrowIfDisposed();
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();

            int bytesRead = InnerStream.EndRead(asyncResult);
            if (bytesRead == 0 && !_isReadToEndComplete)
            {
                SwapToSeekableStream();
            }

            return bytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();

            int bytesRead = InnerStream.Read(buffer, offset, count);
            if (bytesRead == 0 && !_isReadToEndComplete)
            {
                SwapToSeekableStream();
            }

            return bytesRead;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            int bytesRead = await InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
            if (bytesRead == 0 && !_isReadToEndComplete)
            {
                SwapToSeekableStream();
            }

            return bytesRead;
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();

            int result = InnerStream.ReadByte();
            if (result == -1 && !_isReadToEndComplete)
            {
                SwapToSeekableStream();
            }

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();

            long currentPosition = InnerStream.Position;
            long? newPosition = null;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = currentPosition + offset;
                    break;
                case SeekOrigin.End:
                    // We have to check Length here because we might not know the length in some scenarios. 
                    // If we don't know, then we just do the safe thing and force a read to end.
                    if (Length >= 0)
                    {
                        newPosition = Length + offset;
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException("origin", (int)origin, typeof(SeekOrigin));
            }

            if (newPosition == currentPosition)
            {
                // This is a no-op, we want to short circuit because we do significant work on a seek.
                return currentPosition;
            }

            if (!_isReadToEndComplete)
            {
                // The current stream is the one returned from GetBufferedInputStream(), and it's not
                // seekable in the web host case.
                //
                // We need to read the non-seekable stream to the end, which will populate the seekable stream
                // that's provided by .InputStream. This is only done for the side-effect, and we just ignore the
                // data, it's already being buffered for us.
                //
                // This is done synchronously, because we need to block the calling thread so that the result of
                // Seek can be returned.
                byte[] buffer = new byte[ReadBufferSize];
                while (InnerStream.Read(buffer, 0, buffer.Length) > 0)
                {
                }

                SwapToSeekableStream();
            }

            return InnerStream.Seek(offset, origin);
        }

        private void SwapToSeekableStream()
        {
            // At this point we've actually read the non-seekable stream to the end, and we're about to swap streams
            // and toggle the value of _isReadToEndComplete. Reading the non-seekable stream to the end will populate
            // InnerStream with the buffered data, so we can use it for all future operations.
            Debug.Assert(!_isReadToEndComplete);

            Stream seekableStream = _request.InputStream;
            seekableStream.Position = InnerStream.Position;
            InnerStream = seekableStream;

            _isReadToEndComplete = true;
        }
    }
}

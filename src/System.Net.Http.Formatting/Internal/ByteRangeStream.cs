// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http.Internal
{
    /// <summary>
    /// Stream which only exposes a read-only only range view of an
    /// inner stream.
    /// </summary>
    internal class ByteRangeStream : DelegatingStream
    {
        // The offset stream position at which the range starts.
        private readonly long _lowerbounds;

        // The total number of bytes within the range.
        private readonly long _totalCount;

        // The current number of bytes read into the range
        private long _currentCount;

        public ByteRangeStream(Stream innerStream, RangeItemHeaderValue range)
            : base(innerStream)
        {
            if (range == null)
            {
                throw Error.ArgumentNull("range");
            }
            if (!innerStream.CanSeek)
            {
                throw Error.Argument("innerStream", Properties.Resources.ByteRangeStreamNotSeekable, typeof(ByteRangeStream).Name);
            }
            if (innerStream.Length < 1)
            {
                throw Error.ArgumentOutOfRange("innerStream", innerStream.Length, Properties.Resources.ByteRangeStreamEmpty, typeof(ByteRangeStream).Name);
            }
            if (range.From.HasValue && range.From.Value > innerStream.Length)
            {
                throw Error.ArgumentOutOfRange("range", range.From, Properties.Resources.ByteRangeStreamInvalidFrom, innerStream.Length);
            }

            // Ranges are inclusive so 0-9 means the first 10 bytes
            long maxLength = innerStream.Length - 1;
            long upperbounds;
            if (range.To.HasValue)
            {
                if (range.From.HasValue)
                {
                    // e.g bytes=0-499 (the first 500 bytes offsets 0-499)
                    upperbounds = Math.Min(range.To.Value, maxLength);
                    _lowerbounds = range.From.Value;
                }
                else
                {
                    // e.g bytes=-500 (the final 500 bytes)
                    upperbounds = maxLength;
                    _lowerbounds = Math.Max(innerStream.Length - range.To.Value, 0);
                }
            }
            else
            {
                if (range.From.HasValue)
                {
                    // e.g bytes=500- (from byte offset 500 and up)
                    upperbounds = maxLength;
                    _lowerbounds = range.From.Value;
                }
                else
                {
                    // e.g. bytes=- (invalid so will never get here)
                    upperbounds = maxLength;
                    _lowerbounds = 0;
                }
            }

            _totalCount = upperbounds - _lowerbounds + 1;
            ContentRange = new ContentRangeHeaderValue(_lowerbounds, upperbounds, innerStream.Length);
        }

        public ContentRangeHeaderValue ContentRange { get; private set; }

        public override long Length
        {
            get { return _totalCount; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Position
        {
            get
            {
                return _currentCount;
            }
            set
            {
                if (value < 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 0L);
                }

                _currentCount = value;
            }
        }

#if !NETSTANDARD1_3 // BeginX and EndX are not supported on Streams in netstandard1.3
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return base.BeginRead(buffer, offset, PrepareStreamForRangeRead(count), callback, state);
        }
#endif

        public override int Read(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, PrepareStreamForRangeRead(count));
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.ReadAsync(buffer, offset, PrepareStreamForRangeRead(count), cancellationToken);
        }

        public override int ReadByte()
        {
            int effectiveCount = PrepareStreamForRangeRead(1);
            if (effectiveCount <= 0)
            {
                return -1;
            }

            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _currentCount = offset;
                    break;
                case SeekOrigin.Current:
                    _currentCount = _currentCount + offset;
                    break;
                case SeekOrigin.End:
                    _currentCount = _totalCount + offset;
                    break;
                default:
                    throw Error.InvalidEnumArgument("origin", (int)origin, typeof(SeekOrigin));
            }

            if (_currentCount < 0L)
            {
                throw new IOException(Properties.Resources.ByteRangeStreamInvalidOffset);
            }

            return _currentCount;
        }

        public override void SetLength(long value)
        {
            throw Error.NotSupported(Properties.Resources.ByteRangeStreamReadOnly);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw Error.NotSupported(Properties.Resources.ByteRangeStreamReadOnly);
        }

#if !NETSTANDARD1_3 // BeginX and EndX are not supported on Streams in netstandard1.3
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw Error.NotSupported(Properties.Resources.ByteRangeStreamReadOnly);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw Error.NotSupported(Properties.Resources.ByteRangeStreamReadOnly);
        }
#endif

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw Error.NotSupported(Properties.Resources.ByteRangeStreamReadOnly);
        }

        public override void WriteByte(byte value)
        {
            throw Error.NotSupported(Properties.Resources.ByteRangeStreamReadOnly);
        }

        /// <summary>
        /// Gets the correct count for the next read operation.
        /// </summary>
        /// <param name="count">The count requested to be read by the caller.</param>
        /// <returns>The remaining bytes to read within the range defined for this stream.</returns>
        private int PrepareStreamForRangeRead(int count)
        {
            // A negative count causes base.Raad* methods to throw an ArgumentOutOfRangeException.
            if (count <= 0)
            {
                return count;
            }

            // Reading past the end simply does nothing.
            if (_currentCount >= _totalCount)
            {
                return 0;
            }

            long effectiveCount = Math.Min(count, _totalCount - _currentCount);

            // Check if we should update the inner stream's position.
            var newPosition = _lowerbounds + _currentCount;
            var position = InnerStream.Position;
            if (newPosition != position)
            {
                InnerStream.Position = newPosition;
            }

            // Update current number of bytes read.
            _currentCount += effectiveCount;

            // Effective count can never be bigger than int.
            return (int)effectiveCount;
        }
    }
}

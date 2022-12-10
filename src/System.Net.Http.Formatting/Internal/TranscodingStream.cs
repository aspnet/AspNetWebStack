// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// From https://github.com/dotnet/runtime/blob/88868b7a781f4e5b9037b8721f30440207a7aa42/src/libraries/System.Private.CoreLib/src/System/Text/TranscodingStream.cs

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Properties = System.Net.Http.Properties;

#nullable enable

namespace System.Text
{
    internal sealed class TranscodingStream : Stream
    {
        private const int DefaultReadByteBufferSize = 4 * 1024; // lifted from StreamReader.cs (FileStream)

        // We optimistically assume 1 byte ~ 1 char during transcoding. This is a good rule of thumb
        // but isn't always appropriate: transcoding between single-byte and multi-byte encodings
        // will violate this, as will any invalid data fixups performed by the transcoder itself.
        // To account for these unknowns we have a minimum scratch buffer size we use during the
        // transcoding process. This should be generous enough to account for even the largest
        // fallback mechanism we're likely to see in the real world.

        private const int MinWriteRentedArraySize = 4 * 1024;
        private const int MaxWriteRentedArraySize = 1024 * 1024;

        private static readonly byte[] EmptyByteBuffer = new byte[0];
        private static readonly char[] EmptyCharBuffer = new char[0];

        private readonly Encoding _innerEncoding;
        private readonly Encoding _thisEncoding;
        private Stream _innerStream; // null if the wrapper has been disposed
        private readonly bool _leaveOpen;
        private readonly byte[] _singleByteBuffer = new byte[1];

        /*
         * Fields used for writing bytes [this] -> chars -> bytes [inner]
         * Lazily initialized the first time we need to write
         */

        private Encoder? _innerEncoder;
        private Decoder? _thisDecoder;

        /*
         * Fields used for reading bytes [inner] -> chars -> bytes [this]
         * Lazily initialized the first time we need to read
         */

        private Encoder? _thisEncoder;
        private Decoder? _innerDecoder;
        private int _readCharBufferMaxSize; // the maximum number of characters _innerDecoder.ReadChars can return
        private byte[]? _readBuffer; // contains the data that Read() should return
        private int _readBufferOffset;
        private int _readBufferCount;

        internal TranscodingStream(Stream innerStream, Encoding innerEncoding, Encoding thisEncoding, bool leaveOpen = false)
        {
            _innerStream = innerStream ?? throw Error.ArgumentNull(nameof(innerStream));
            _leaveOpen = leaveOpen;

            _innerEncoding = innerEncoding ?? throw Error.ArgumentNull(nameof(innerEncoding));
            _thisEncoding = thisEncoding ?? throw Error.ArgumentNull(nameof(thisEncoding));
        }

        /*
         * Most CanXyz methods delegate to the inner stream, returning false
         * if this instance has been disposed. CanSeek is always false.
         */

        public override bool CanRead => _innerStream?.CanRead ?? false;

        public override bool CanSeek => false;

        public override bool CanWrite => _innerStream?.CanWrite ?? false;

        public override long Length => throw Error.NotSupported(Properties.Resources.NotSupported_UnseekableStream);

        public override long Position
        {
            get => throw Error.NotSupported(Properties.Resources.NotSupported_UnseekableStream);
            set => throw Error.NotSupported(Properties.Resources.NotSupported_UnseekableStream);
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Assert(disposing, "This type isn't finalizable.");
            base.Dispose(disposing);

            if (_innerStream is null)
            {
                return; // dispose called multiple times, ignore
            }

            // First, flush any pending data to the inner stream.

            ArraySegment<byte> pendingData = FinalFlushWriteBuffers();
            if (pendingData.Count != 0)
            {
                _innerStream.Write(pendingData.Array, pendingData.Offset, pendingData.Count);
            }

            // Mark our object as disposed

            Stream innerStream = _innerStream;
            _innerStream = null!;

            // And dispose the inner stream if needed

            if (!_leaveOpen)
            {
                innerStream.Dispose();
            }
        }

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1
        public override ValueTask DisposeAsync()
        {
            if (_innerStream is null)
            {
                return default; // dispose called multiple times, ignore
            }

            // First, get any pending data destined for the inner stream.

            ArraySegment<byte> pendingData = FinalFlushWriteBuffers();

            if (pendingData.Count == 0)
            {
                // Fast path: just dispose of the object graph.
                // No need to write anything to the stream first.

                Stream innerStream = _innerStream;
                _innerStream = null!;

                return (_leaveOpen)
                    ? default /* no work to do */
                    : innerStream.DisposeAsync();
            }

            // Slower path; need to perform an async write followed by an async dispose.

            return DisposeAsyncCore(pendingData);
            async ValueTask DisposeAsyncCore(ArraySegment<byte> pendingData)
            {
                Debug.Assert(pendingData.Count != 0);

                Stream innerStream = _innerStream;
                _innerStream = null!;

                await innerStream.WriteAsync(pendingData.AsMemory()).ConfigureAwait(false);

                if (!_leaveOpen)
                {
                    await innerStream.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
#endif

#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
#pragma warning disable CS8774 // Member must have a non-null value when exiting.

        // Sets up the data structures that are necessary before any read operation takes place,
        // throwing if the object is in a state where reads are not possible.
        [MemberNotNull(nameof(_innerDecoder), nameof(_thisEncoder), nameof(_readBuffer))]
        private void EnsurePreReadConditions()
        {
            ThrowIfDisposed();
            if (_innerDecoder is null)
            {
                InitializeReadDataStructures();
            }

            void InitializeReadDataStructures()
            {
                if (!CanRead)
                {
                    throw Error.NotSupported(Properties.Resources.NotSupported_UnreadableStream);
                }

                _innerDecoder = _innerEncoding.GetDecoder();
                _thisEncoder = _thisEncoding.GetEncoder();
                _readCharBufferMaxSize = _innerEncoding.GetMaxCharCount(DefaultReadByteBufferSize);

                // Can't use ArrayPool for the below array since it's an instance field of this object.
                // But since we never expose the raw array contents to our callers we can get away
                // with skipping the array zero-init during allocation. The segment points to the
                // data which we haven't yet read; however, we own the entire backing array and can
                // re-create the segment as needed once the array is repopulated.

#if NET5_0_OR_GREATER
                _readBuffer = GC.AllocateUninitializedArray<byte>(_thisEncoding.GetMaxByteCount(_readCharBufferMaxSize));
#else
                _readBuffer = new byte[_thisEncoding.GetMaxByteCount(_readCharBufferMaxSize)];
#endif
            }
        }

        // Sets up the data structures that are necessary before any write operation takes place,
        // throwing if the object is in a state where writes are not possible.
        [MemberNotNull(nameof(_thisDecoder), nameof(_innerEncoder))]
        private void EnsurePreWriteConditions()
        {
            ThrowIfDisposed();
            if (_innerEncoder is null)
            {
                InitializeReadDataStructures();
            }

            void InitializeReadDataStructures()
            {
                if (!CanWrite)
                {
                    throw Error.NotSupported(Properties.Resources.NotSupported_UnwritableStream);
                }

                _innerEncoder = _innerEncoding.GetEncoder();
                _thisDecoder = _thisEncoding.GetDecoder();
            }
        }

#pragma warning restore CS8774 // Member must have a non-null value when exiting.
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant

        // returns any pending data that needs to be flushed to the inner stream before disposal
        private ArraySegment<byte> FinalFlushWriteBuffers()
        {
            // If this stream was never used for writing, no-op.

            if (_thisDecoder is null || _innerEncoder is null)
            {
                return default;
            }

            // convert bytes [this] -> chars
            // Having leftover data in our buffers should be very rare since it should only
            // occur if the end of the stream contains an incomplete multi-byte sequence.
            // Let's not bother complicating this logic with array pool rentals or allocation-
            // avoiding loops.

            char[] chars = EmptyCharBuffer;
            int charCount = _thisDecoder.GetCharCount(EmptyByteBuffer, 0, 0, flush: true);
            if (charCount > 0)
            {
                chars = new char[charCount];
                charCount = _thisDecoder.GetChars(EmptyByteBuffer, 0, 0, chars, 0, flush: true);
            }

            // convert chars -> bytes [inner]
            // It's possible that _innerEncoder might need to perform some end-of-text fixup
            // (due to flush: true), even if _thisDecoder didn't need to do so.

            byte[] bytes = EmptyByteBuffer;
            int byteCount = _innerEncoder.GetByteCount(chars, 0, charCount, flush: true);
            if (byteCount > 0)
            {
                bytes = new byte[byteCount];
                byteCount = _innerEncoder.GetBytes(chars, 0, charCount, bytes, 0, flush: true);
            }

            return new ArraySegment<byte>(bytes, 0, byteCount);
        }

        public override void Flush()
        {
            // Don't pass flush: true to our inner decoder + encoder here, since it could cause data
            // corruption if a flush occurs mid-stream. Wait until the stream is being closed.

            ThrowIfDisposed();
            _innerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            // Don't pass flush: true to our inner decoder + encoder here, since it could cause data
            // corruption if a flush occurs mid-stream. Wait until the stream is being closed.

            ThrowIfDisposed();
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);

            return Read(new Span<byte>(buffer, offset, count));
        }

#if NETCOREAPP || NETSTANDARD2_1
        public override
#else
        private
#endif
        int Read(Span<byte> buffer)
        {
            EnsurePreReadConditions();

            // If there's no data in our pending read buffer, we'll need to populate it from
            // the inner stream. We read the inner stream's bytes, decode that to chars using
            // the 'inner' encoding, then re-encode those chars under the 'this' encoding.
            // We've already calculated the worst-case expansions for the intermediate buffers,
            // so we use GetChars / GetBytes instead of Convert to simplify the below code
            // and to ensure an exception is thrown if the Encoding reported an incorrect
            // worst-case expansion.

            if (_readBufferCount == 0)
            {
                byte[] rentedBytes = ArrayPool<byte>.Shared.Rent(DefaultReadByteBufferSize);
                char[] rentedChars = ArrayPool<char>.Shared.Rent(_readCharBufferMaxSize);

                try
                {
                    int pendingReadDataPopulatedJustNow;
                    bool isEofReached;

                    do
                    {
                        // Beware: Use our constant value instead of 'rentedBytes.Length' for the count
                        // parameter below. The reason for this is that the array pool could've returned
                        // a larger-than-expected array, but our worst-case expansion calculations
                        // performed earlier didn't take that into account.

                        int innerBytesReadJustNow = _innerStream.Read(rentedBytes, 0, DefaultReadByteBufferSize);
                        isEofReached = (innerBytesReadJustNow == 0);

                        // Convert bytes [inner] -> chars, then convert chars -> bytes [this].
                        // We can't return 0 to our caller until inner stream EOF has been reached. But if the
                        // inner stream returns a non-empty but incomplete buffer, GetBytes may return 0 anyway
                        // since it can't yet make forward progress on the input data. If this happens, we'll
                        // loop so that we don't return 0 to our caller until we truly see inner stream EOF.

                        int charsDecodedJustNow = _innerDecoder.GetChars(rentedBytes, 0, innerBytesReadJustNow, rentedChars, 0, flush: isEofReached);
                        pendingReadDataPopulatedJustNow = _thisEncoder.GetBytes(rentedChars, 0, charsDecodedJustNow, _readBuffer, 0, flush: isEofReached);
                    } while (!isEofReached && pendingReadDataPopulatedJustNow == 0);

                    _readBufferOffset = 0;
                    _readBufferCount = pendingReadDataPopulatedJustNow;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rentedBytes);
                    ArrayPool<char>.Shared.Return(rentedChars);
                }
            }

            // At this point: (a) we've populated our pending read buffer and there's
            // useful data to return to our caller; or (b) the pending read buffer is
            // empty because the inner stream has reached EOF and all pending read data
            // has already been flushed, and we should return 0.

            int bytesToReturn = Math.Min(_readBufferCount, buffer.Length);
            _readBuffer.AsSpan(_readBufferOffset, bytesToReturn).CopyTo(buffer);
            _readBufferOffset += bytesToReturn;
            _readBufferCount -= bytesToReturn;
            return bytesToReturn;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);

            return ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

#if NETCOREAPP || NETSTANDARD2_1
        public override
#else
        private
#endif
        ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsurePreReadConditions();

            if (cancellationToken.IsCancellationRequested)
            {
#if NETCOREAPP || NETSTANDARD
                return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
#else
                // Lose track of the CancellationToken in this case.
                return new ValueTask<int>(TaskHelpers.Canceled<int>());
#endif
            }

            return ReadAsyncCore(buffer, cancellationToken);
            async ValueTask<int> ReadAsyncCore(Memory<byte> buffer, CancellationToken cancellationToken)
            {
                // If there's no data in our pending read buffer, we'll need to populate it from
                // the inner stream. We read the inner stream's bytes, decode that to chars using
                // the 'inner' encoding, then re-encode those chars under the 'this' encoding.
                // We've already calculated the worst-case expansions for the intermediate buffers,
                // so we use GetChars / GetBytes instead of Convert to simplify the below code
                // and to ensure an exception is thrown if the Encoding reported an incorrect
                // worst-case expansion.

                if (_readBufferCount == 0)
                {
                    byte[] rentedBytes = ArrayPool<byte>.Shared.Rent(DefaultReadByteBufferSize);
                    char[] rentedChars = ArrayPool<char>.Shared.Rent(_readCharBufferMaxSize);

                    try
                    {
                        int pendingReadDataPopulatedJustNow;
                        bool isEofReached;

                        do
                        {
                            // Beware: Use our constant value instead of 'rentedBytes.Length' when creating
                            // the Mem<byte> struct. The reason for this is that the array pool could've returned
                            // a larger-than-expected array, but our worst-case expansion calculations
                            // performed earlier didn't take that into account.

                            int innerBytesReadJustNow = await _innerStream.ReadAsync(rentedBytes, 0, DefaultReadByteBufferSize, cancellationToken).ConfigureAwait(false);
                            isEofReached = (innerBytesReadJustNow == 0);

                            // Convert bytes [inner] -> chars, then convert chars -> bytes [this].
                            // We can't return 0 to our caller until inner stream EOF has been reached. But if the
                            // inner stream returns a non-empty but incomplete buffer, GetBytes may return 0 anyway
                            // since it can't yet make forward progress on the input data. If this happens, we'll
                            // loop so that we don't return 0 to our caller until we truly see inner stream EOF.

                            int charsDecodedJustNow = _innerDecoder.GetChars(rentedBytes, 0, innerBytesReadJustNow, rentedChars, 0, flush: isEofReached);
                            pendingReadDataPopulatedJustNow = _thisEncoder.GetBytes(rentedChars, 0, charsDecodedJustNow, _readBuffer, 0, flush: isEofReached);
                        } while (!isEofReached && pendingReadDataPopulatedJustNow == 0);

                        _readBufferOffset = 0;
                        _readBufferCount = pendingReadDataPopulatedJustNow;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rentedBytes);
                        ArrayPool<char>.Shared.Return(rentedChars);
                    }
                }

                // At this point: (a) we've populated our pending read buffer and there's
                // useful data to return to our caller; or (b) the pending read buffer is
                // empty because the inner stream has reached EOF and all pending read data
                // has already been flushed, and we should return 0.

                int bytesToReturn = Math.Min(_readBufferCount, buffer.Length);
                _readBuffer.AsSpan(_readBufferOffset, bytesToReturn).CopyTo(buffer.Span);
                _readBufferOffset += bytesToReturn;
                _readBufferCount -= bytesToReturn;
                return bytesToReturn;
            }
        }

        public override int ReadByte()
        {
            return Read(_singleByteBuffer, offset: 0, count: 1) != 0 ? _singleByteBuffer[0] : -1;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw Error.NotSupported(Properties.Resources.NotSupported_UnseekableStream);

        public override void SetLength(long value)
            => throw Error.NotSupported(Properties.Resources.NotSupported_UnseekableStream);

#if NET6_0_OR_GREATER
        [StackTraceHidden]
#endif
        private void ThrowIfDisposed()
        {
            if (_innerStream is null)
            {
                ThrowObjectDisposedException();
            }
        }

        [DoesNotReturn]
#if NET6_0_OR_GREATER
        [StackTraceHidden]
#endif
        private void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(GetType().Name, Properties.Resources.ObjectDisposed_StreamClosed);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);

#if NETCOREAPP || NETSTANDARD2_1
            Write(new ReadOnlySpan<byte>(buffer, offset, count));
#else
            WriteCore(buffer, offset, count);
#endif
        }

#if NETCOREAPP || NETSTANDARD2_1
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsurePreWriteConditions();

            if (buffer.IsEmpty)
            {
                return;
            }

            int rentalLength = buffer.Length < MinWriteRentedArraySize ? MinWriteRentedArraySize :
                buffer.Length > MaxWriteRentedArraySize ? MaxWriteRentedArraySize :
                buffer.Length;

            char[] scratchChars = ArrayPool<char>.Shared.Rent(rentalLength);
            byte[] scratchBytes = ArrayPool<byte>.Shared.Rent(rentalLength);

            try
            {
                bool decoderFinished, encoderFinished;
                do
                {
                    // convert bytes [this] -> chars

                    _thisDecoder.Convert(
                        bytes: buffer,
                        chars: scratchChars,
                        flush: false,
                        out int bytesConsumed,
                        out int charsWritten,
                        out decoderFinished);

                    buffer = buffer.Slice(bytesConsumed);

                    // convert chars -> bytes [inner]

                    Span<char> decodedChars = scratchChars.AsSpan(0, charsWritten);

                    do
                    {
                        _innerEncoder.Convert(
                            chars: decodedChars,
                            bytes: scratchBytes,
                            flush: false,
                            out int charsConsumed,
                            out int bytesWritten,
                            out encoderFinished);

                        decodedChars = decodedChars.Slice(charsConsumed);

                        // It's more likely that the inner stream provides an optimized implementation of
                        // Write(byte[], ...) over Write(ROS<byte>), so we'll prefer the byte[]-based overloads.

                        _innerStream.Write(scratchBytes, 0, bytesWritten);
                    } while (!encoderFinished);
                } while (!decoderFinished);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(scratchChars);
                ArrayPool<byte>.Shared.Return(scratchBytes);
            }
        }
#else
        private void WriteCore(byte[] buffer, int offset, int count)
        {
            EnsurePreWriteConditions();

            if (count == 0)
            {
                return;
            }

            int rentalLength = buffer.Length < MinWriteRentedArraySize ? MinWriteRentedArraySize :
                buffer.Length > MaxWriteRentedArraySize ? MaxWriteRentedArraySize :
                buffer.Length;

            char[] scratchChars = ArrayPool<char>.Shared.Rent(rentalLength);
            byte[] scratchBytes = ArrayPool<byte>.Shared.Rent(rentalLength);

            try
            {
                bool decoderFinished, encoderFinished;
                do
                {
                    // convert bytes [this] -> chars

                    _thisDecoder.Convert(
                        bytes: buffer,
                        byteIndex: offset,
                        byteCount: count,
                        chars: scratchChars,
                        charIndex: 0,
                        charCount: rentalLength,
                        flush: false,
                        out int bytesConsumed,
                        out int charsWritten,
                        out decoderFinished);

                    offset += bytesConsumed;
                    count -= bytesConsumed;

                    // convert chars -> bytes [inner]

                    int scratchOffset = 0;
                    do
                    {
                        _innerEncoder.Convert(
                            chars: scratchChars,
                            charIndex: scratchOffset,
                            charCount: charsWritten,
                            bytes: scratchBytes,
                            byteIndex: 0,
                            byteCount: rentalLength,
                            flush: false,
                            out int charsConsumed,
                            out int bytesWritten,
                            out encoderFinished);

                        scratchOffset += charsConsumed;
                        charsWritten -= charsConsumed;

                        // It's more likely that the inner stream provides an optimized implementation of
                        // Write(byte[], ...) over Write(ROS<byte>), so we'll prefer the byte[]-based overloads.

                        _innerStream.Write(scratchBytes, 0, bytesWritten);
                    } while (!encoderFinished);
                } while (!decoderFinished);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(scratchChars);
                ArrayPool<byte>.Shared.Return(scratchBytes);
            }
        }
#endif

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);

#if NETCOREAPP || NETSTANDARD2_1
            return WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();
#else
            return WriteAsyncCore(buffer, offset, count, cancellationToken).AsTask();
#endif
        }

#if NETCOREAPP || NETSTANDARD2_1
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsurePreWriteConditions();

            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask(Task.FromCanceled<int>(cancellationToken));
            }

            if (buffer.IsEmpty)
            {
                // ValueTask.CompletedTask
                return default;
            }

            return WriteAsyncCore(buffer, cancellationToken);
            async ValueTask WriteAsyncCore(ReadOnlyMemory<byte> remainingOuterEncodedBytes, CancellationToken cancellationToken)
            {
                int rentalLength = remainingOuterEncodedBytes.Length < MinWriteRentedArraySize ? MinWriteRentedArraySize :
                    remainingOuterEncodedBytes.Length > MaxWriteRentedArraySize ? MaxWriteRentedArraySize:
                    remainingOuterEncodedBytes.Length;

                char[] scratchChars = ArrayPool<char>.Shared.Rent(rentalLength);
                byte[] scratchBytes = ArrayPool<byte>.Shared.Rent(rentalLength);

                try
                {
                    bool decoderFinished, encoderFinished;
                    do
                    {
                        // convert bytes [this] -> chars

                        _thisDecoder.Convert(
                            bytes: buffer,
                            chars: scratchChars,
                            flush: false,
                            out int bytesConsumed,
                            out int charsWritten,
                            out decoderFinished);

                        buffer = buffer.Slice(bytesConsumed);

                        // convert chars -> bytes [inner]

                        Span<char> decodedChars = scratchChars.AsSpan(0, charsWritten);

                        do
                        {
                            _innerEncoder.Convert(
                                chars: decodedChars,
                                bytes: scratchBytes,
                                flush: false,
                                out int charsConsumed,
                                out int bytesWritten,
                                out encoderFinished);

                            decodedChars = decodedChars.Slice(charsConsumed);

                            await _innerStream.WriteAsync(scratchBytes, 0, bytesWritten, cancellationToken).ConfigureAwait(false);
                        } while (!encoderFinished);
                    } while (!decoderFinished);
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(scratchChars);
                    ArrayPool<byte>.Shared.Return(scratchBytes);
                }
            }
        }
#else
        private ValueTask WriteAsyncCore(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsurePreWriteConditions();

            if (cancellationToken.IsCancellationRequested)
            {
#if NETSTANDARD
                return new ValueTask(Task.FromCanceled<int>(cancellationToken));
#else
                // Lose track of the CancellationToken in this case.
                return new ValueTask(TaskHelpers.Canceled());
#endif
            }

            if (count == 0)
            {
                // ValueTask.CompletedTask
                return default;
            }

            return WriteAsyncCore(buffer, cancellationToken);
            async ValueTask WriteAsyncCore(ReadOnlyMemory<byte> remainingOuterEncodedBytes, CancellationToken cancellationToken)
            {
                int rentalLength = remainingOuterEncodedBytes.Length < MinWriteRentedArraySize ? MinWriteRentedArraySize :
                    remainingOuterEncodedBytes.Length > MaxWriteRentedArraySize ? MaxWriteRentedArraySize :
                    remainingOuterEncodedBytes.Length;

                char[] scratchChars = ArrayPool<char>.Shared.Rent(rentalLength);
                byte[] scratchBytes = ArrayPool<byte>.Shared.Rent(rentalLength);

                try
                {
                    bool decoderFinished, encoderFinished;
                    do
                    {
                        // convert bytes [this] -> chars

                        _thisDecoder.Convert(
                            bytes: buffer,
                            byteIndex: offset,
                            byteCount: count,
                            chars: scratchChars,
                            charIndex: 0,
                            charCount: rentalLength,
                            flush: false,
                            out int bytesConsumed,
                            out int charsWritten,
                            out decoderFinished);

                        offset += bytesConsumed;
                        count -= bytesConsumed;

                        // convert chars -> bytes [inner]

                        int scratchOffset = 0;
                        do
                        {
                            _innerEncoder.Convert(
                                chars: scratchChars,
                                charIndex: scratchOffset,
                                charCount: charsWritten,
                                bytes: scratchBytes,
                                byteIndex: 0,
                                byteCount: rentalLength,
                                flush: false,
                                out int charsConsumed,
                                out int bytesWritten,
                                out encoderFinished);

                            scratchOffset += charsConsumed;
                            charsWritten -= charsConsumed;

                            await _innerStream.WriteAsync(scratchBytes, 0, bytesWritten, cancellationToken).ConfigureAwait(false);
                        } while (!encoderFinished);
                    } while (!decoderFinished);
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(scratchChars);
                    ArrayPool<byte>.Shared.Return(scratchBytes);
                }
            }
        }
#endif

        public override void WriteByte(byte value)
        {
            _singleByteBuffer[0] = value;
            Write(_singleByteBuffer, offset: 0, count: 1);
        }

        // From https://github.com/dotnet/runtime/blob/88868b7a781f4e5b9037b8721f30440207a7aa42/src/libraries/System.Private.CoreLib/src/System/IO/Stream.cs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
            {
                throw Error.ArgumentNull(nameof(buffer));
            }

            if (offset < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(offset), offset, minValue: 0);
            }

            if ((uint)count > buffer.Length - offset)
            {
                throw Error.ArgumentOutOfRange(nameof(count), count, Properties.Resources.Argument_InvalidOffLen);
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal class BufferedOutputStream : Stream
    {
        private BufferManager _bufferManager;

        private byte[][] _chunks;

        private int _chunkCount;
        private byte[] _currentChunk;
        private int _currentChunkSize;
        private int _maxSize;
        private int _theMaxSizeQuota;
        private int _totalSize;
        private bool _callerReturnsBuffer;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used for internal checking")]
        private bool _bufferReturned;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used for internal checking")]
        private bool _initialized;

        // requires an explicit call to Init() by the caller
        public BufferedOutputStream()
        {
            _chunks = new byte[4][];
        }

        public BufferedOutputStream(int initialSize, int maxSize, BufferManager bufferManager)
            : this()
        {
            Reinitialize(initialSize, maxSize, bufferManager);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return _totalSize; }
        }

        public override long Position
        {
            get { throw Error.NotSupported(SRResources.SeekNotSupported); }

            set { throw Error.NotSupported(SRResources.SeekNotSupported); }
        }

        public void Reinitialize(int initialSize, int maxSizeQuota, BufferManager bufferManager)
        {
            Reinitialize(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
        }

        public void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, BufferManager bufferManager)
        {
            Contract.Assert(!_initialized, "Clear must be called before re-initializing stream");

            if (bufferManager == null)
            {
                throw Error.ArgumentNull("bufferManager");
            }

            _theMaxSizeQuota = maxSizeQuota;
            _maxSize = effectiveMaxSize;
            _bufferManager = bufferManager;
            _currentChunk = bufferManager.TakeBuffer(initialSize);
            _currentChunkSize = 0;
            _totalSize = 0;
            _chunkCount = 1;
            _chunks[0] = _currentChunk;
            _initialized = true;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw Error.NotSupported(SRResources.ReadNotSupported);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw Error.NotSupported(SRResources.ReadNotSupported);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Write(buffer, offset, count);
            return new CompletedAsyncResult(callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            CompletedAsyncResult.End(asyncResult);
        }

        public void Clear()
        {
            if (!_callerReturnsBuffer)
            {
                for (int i = 0; i < _chunkCount; i++)
                {
                    _bufferManager.ReturnBuffer(_chunks[i]);
                    _chunks[i] = null;
                }
            }

            _callerReturnsBuffer = false;
            _initialized = false;
            _bufferReturned = false;
            _chunkCount = 0;
            _currentChunk = null;
        }

        public override void Close()
        {
            // Called directly or via base.Dispose, ensure all buffers are returned to the BufferManager
            Clear();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw Error.NotSupported(SRResources.ReadNotSupported);
        }

        public override int ReadByte()
        {
            throw Error.NotSupported(SRResources.ReadNotSupported);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw Error.NotSupported(SRResources.SeekNotSupported);
        }

        public override void SetLength(long value)
        {
            throw Error.NotSupported(SRResources.SeekNotSupported);
        }

        public MemoryStream ToMemoryStream()
        {
            int bufferSize;
            byte[] buffer = ToArray(out bufferSize);
            return new MemoryStream(buffer, 0, bufferSize);
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter is fine here.")]
        public byte[] ToArray(out int bufferSize)
        {
            Contract.Assert(_initialized, "No data to return from uninitialized stream");
            Contract.Assert(!_bufferReturned, "ToArray cannot be called more than once");

            byte[] buffer;
            if (_chunkCount == 1)
            {
                buffer = _currentChunk;
                bufferSize = _currentChunkSize;
                _callerReturnsBuffer = true;
            }
            else
            {
                buffer = _bufferManager.TakeBuffer(_totalSize);
                int offset = 0;
                int count = _chunkCount - 1;
                for (int i = 0; i < count; i++)
                {
                    byte[] chunk = _chunks[i];
                    Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
                    offset += chunk.Length;
                }

                Buffer.BlockCopy(_currentChunk, 0, buffer, offset, _currentChunkSize);
                bufferSize = _totalSize;
            }

            _bufferReturned = true;
            return buffer;
        }

        public void Skip(int size)
        {
            WriteCore(null, 0, size);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteCore(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            Contract.Assert(_initialized, "Cannot write to uninitialized stream");
            Contract.Assert(!_bufferReturned, "Cannot write to stream once ToArray has been called.");

            if (_totalSize == _maxSize)
            {
                throw CreateQuotaExceededException(_maxSize);
            }

            if (_currentChunkSize == _currentChunk.Length)
            {
                AllocNextChunk(1);
            }

            _currentChunk[_currentChunkSize++] = value;
        }

        protected virtual Exception CreateQuotaExceededException(int maxSizeQuota)
        {
            return new InvalidOperationException(Error.Format(SRResources.BufferedOutputStreamQuotaExceeded, maxSizeQuota));
        }

        private void WriteCore(byte[] buffer, int offset, int size)
        {
            Contract.Assert(_initialized, "Cannot write to uninitialized stream");
            Contract.Assert(!_bufferReturned, "Cannot write to stream once ToArray has been called.");

            if (size < 0)
            {
                throw Error.ArgumentOutOfRange("size", size, SRResources.ValueMustBeNonNegative);
            }

            if ((Int32.MaxValue - size) < _totalSize)
            {
                throw CreateQuotaExceededException(_theMaxSizeQuota);
            }

            int newTotalSize = _totalSize + size;
            if (newTotalSize > _maxSize)
            {
                throw CreateQuotaExceededException(_theMaxSizeQuota);
            }

            int remainingSizeInChunk = _currentChunk.Length - _currentChunkSize;
            if (size > remainingSizeInChunk)
            {
                if (remainingSizeInChunk > 0)
                {
                    if (buffer != null)
                    {
                        Buffer.BlockCopy(buffer, offset, _currentChunk, _currentChunkSize, remainingSizeInChunk);
                    }

                    _currentChunkSize = _currentChunk.Length;
                    offset += remainingSizeInChunk;
                    size -= remainingSizeInChunk;
                }

                AllocNextChunk(size);
            }

            if (buffer != null)
            {
                Buffer.BlockCopy(buffer, offset, _currentChunk, _currentChunkSize, size);
            }

            _totalSize = newTotalSize;
            _currentChunkSize += size;
        }

        private void AllocNextChunk(int minimumChunkSize)
        {
            int newChunkSize;
            if (_currentChunk.Length > (Int32.MaxValue / 2))
            {
                newChunkSize = Int32.MaxValue;
            }
            else
            {
                newChunkSize = _currentChunk.Length * 2;
            }

            if (minimumChunkSize > newChunkSize)
            {
                newChunkSize = minimumChunkSize;
            }

            byte[] newChunk = _bufferManager.TakeBuffer(newChunkSize);
            if (_chunkCount == _chunks.Length)
            {
                byte[][] newChunks = new byte[_chunks.Length * 2][];
                Array.Copy(_chunks, newChunks, _chunks.Length);
                _chunks = newChunks;
            }

            _chunks[_chunkCount++] = newChunk;
            _currentChunk = newChunk;
            _currentChunkSize = 0;
        }
    }
}

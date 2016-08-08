// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http
{
    /// <summary>Represents a stream that replaces another stream to prevent actually closing that stream.</summary>
    /// <remarks>
    /// This class uses the Decorator [GoF] pattern; it forwards all calls except those related to Dispose and Close.
    /// </remarks>
    internal class NonOwnedStream : Stream
    {
        protected NonOwnedStream()
        {
        }

        public NonOwnedStream(Stream innerStream)
        {
            if (innerStream == null)
            {
                throw new ArgumentNullException("innerStream");
            }

            InnerStream = innerStream;
        }

        protected Stream InnerStream
        {
            get;
            set;
        }

        protected bool IsDisposed
        {
            get;
            private set;
        }

        public override bool CanRead
        {
            get
            {
                // Per documentation, CanRead should return false rather than throw when the stream is closed.
                if (IsDisposed)
                {
                    return false;
                }

                return InnerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                // Per documentation, CanSeek should return false rather than throw when the stream is closed.
                if (IsDisposed)
                {
                    return false;
                }

                return InnerStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                // Per documentation, this value apparently is a constant for a particular implementation class.
                // Throwing when disposed appears inappropriate here.
                return InnerStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                // Per documentation, CanWrite should return false rather than throw when the stream is closed.
                if (IsDisposed)
                {
                    return false;
                }

                return InnerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                // Per documentation, this property throws ObjectDisposedException if the stream is closed.
                ThrowIfDisposed();
                return InnerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                // Per documentation, this property throws ObjectDisposedException if the stream is closed.
                ThrowIfDisposed();
                return InnerStream.Position;
            }
            set
            {
                // Per documentation, this property throws ObjectDisposedException if the stream is closed.
                ThrowIfDisposed();
                InnerStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                return InnerStream.ReadTimeout;
            }
            set
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                InnerStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                return InnerStream.WriteTimeout;
            }
            set
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                InnerStream.WriteTimeout = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return InnerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            // base.Close() calls Dispose(true) and GC.SuppressFinalize(this), which is exactly what we want.
            // Note that we do NOT call _innerStream.Close here, as that would actually close the original source
            // stream, which is the one thing this class is designed to prevent.
            base.Close();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        // Not overriding MarshalByRefObj.CreateObjRef.

        // Not overriding Stream.CreateWaitHandle.

        [SuppressMessage(
            "Microsoft.Usage", 
            "CA2215:Dispose methods should call base class dispose",
            Justification = "We're intentionally preventing a double dispose here.")]
        protected override void Dispose(bool disposing)
        {
            // Note that we do NOT call _innerStream.Dispose or Close here, as that would actually close the original
            // source stream, which is the one thing this class is designed to prevent.

            if (!IsDisposed)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            return InnerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            InnerStream.EndWrite(asyncResult);
        }

        // Not overriding Object.Equals.

        public override void Flush()
        {
            ThrowIfDisposed();
            InnerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return InnerStream.FlushAsync(cancellationToken);
        }

        // Not overriding Object.GetHashCode.

        // Not overriding MarshalByRefObj.InitializeLifetimeService.

        // Per documentation, don't override Stream.ObjectInvariant.

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return InnerStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();
            return InnerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            InnerStream.SetLength(value);
        }

        // Not overriding Object.ToString().

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            InnerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            InnerStream.WriteByte(value);
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}

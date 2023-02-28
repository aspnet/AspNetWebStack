// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Mocks;
using System.Threading;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Net.Http.Internal
{
    public class DelegatingStreamTest
    {
        [Fact]
        public void DelegatingStream_CtorThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => new MockDelegatingStream(null), "innerStream");
        }

        [Fact]
        public void DelegatingStream_CanRead()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            bool canRead = mockStream.CanRead;

            // Assert
            mockInnerStream.Verify(s => s.CanRead, Times.Once());
        }

        [Fact]
        public void DelegatingStream_CanSeek()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            bool canSeek = mockStream.CanSeek;

            // Assert
            mockInnerStream.Verify(s => s.CanSeek, Times.Once());
        }

        [Fact]
        public void DelegatingStream_CanWrite()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            bool canWrite = mockStream.CanWrite;

            // Assert
            mockInnerStream.Verify(s => s.CanWrite, Times.Once());
        }

        [Fact]
        public void DelegatingStream_Length()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            long length = mockStream.Length;

            // Assert
            mockInnerStream.Verify(s => s.Length, Times.Once());
        }

        [Fact]
        public void DelegatingStream_Position()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            long position = mockStream.Position;

            // Assert
            mockInnerStream.Verify(s => s.Position, Times.Once());
        }

        [Fact]
        public void DelegatingStream_ReadTimeout()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            int readTimeout = mockStream.ReadTimeout;

            // Assert
            mockInnerStream.Verify(s => s.ReadTimeout, Times.Once());
        }

        [Fact]
        public void DelegatingStream_CanTimeout()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            bool canTimeout = mockStream.CanTimeout;

            // Assert
            mockInnerStream.Verify(s => s.CanTimeout, Times.Once());
        }

        [Fact]
        public void DelegatingStream_WriteTimeout()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            int writeTimeout = mockStream.WriteTimeout;

            // Assert
            mockInnerStream.Verify(s => s.WriteTimeout, Times.Once());
        }

        [Fact]
        public void DelegatingStream_Dispose()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            mockStream.Dispose();

            // Assert
            mockInnerStream.Protected().Verify("Dispose", Times.Once(), exactParameterMatch: true, args: true);
            mockInnerStream.Verify(s => s.Close(), Times.Once());
        }

        [Fact]
        public void DelegatingStream_Close()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            mockStream.Close();

            // Assert
            mockInnerStream.Protected().Verify("Dispose", Times.Once(), exactParameterMatch: true, args: true);
            mockInnerStream.Verify(s => s.Close(), Times.Once());
        }

        [Fact]
        public void DelegatingStream_Seek()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            long offset = 1;
            SeekOrigin origin = SeekOrigin.End;

            // Act
            long seek = mockStream.Seek(offset, origin);

            // Assert
            mockInnerStream.Verify(s => s.Seek(offset, origin), Times.Once());
        }

        [Fact]
        public void DelegatingStream_Read()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte[] buffer = new byte[2];
            int offset = 1;
            int count = 1;

            // Act
            mockStream.Read(buffer, offset, count);

            // Assert
            mockInnerStream.Verify(s => s.Read(buffer, offset, count), Times.Once());
        }

#if !Testing_NetStandard1_3 // BeginX and EndX not supported on Streams in netstandard1.3
        [Fact]
        public void DelegatingStream_BeginRead()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte[] buffer = new byte[2];
            int offset = 1;
            int count = 1;
            AsyncCallback callback = new AsyncCallback((asyncResult) => { });
            object state = new object();

            // Act
            mockStream.BeginRead(buffer, offset, count, callback, state);

            // Assert
            mockInnerStream.Verify(s => s.BeginRead(buffer, offset, count, callback, state), Times.Once());
        }

        [Fact]
        public void DelegatingStream_EndRead()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            Mock<IAsyncResult> mockIAsyncResult = new Mock<IAsyncResult>();

            // Act
            int endRead = mockStream.EndRead(mockIAsyncResult.Object);

            // Assert
            mockInnerStream.Verify(s => s.EndRead(mockIAsyncResult.Object), Times.Once());
        }
#endif

        [Fact]
        public void DelegatingStream_ReadAsyc()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte[] buffer = new byte[2];
            int offset = 1;
            int count = 1;

            // Act
            mockStream.ReadAsync(buffer, offset, count, CancellationToken.None);

            // Assert
            mockInnerStream.Verify(s => s.ReadAsync(buffer, offset, count, CancellationToken.None), Times.Once());
        }

        [Fact]
        public void DelegatingStream_ReadByte()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            int readByte = mockStream.ReadByte();

            // Assert
            mockInnerStream.Verify(s => s.ReadByte(), Times.Once());
        }

        [Fact]
        public void DelegatingStream_Flush()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            mockStream.Flush();

            // Assert
            mockInnerStream.Verify(s => s.Flush(), Times.Once());
        }

        [Fact]
        public void DelegatingStream_SetLength()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);

            // Act
            mockStream.SetLength(10L);

            // Assert
            mockInnerStream.Verify(s => s.SetLength(10L), Times.Once());
        }

        [Fact]
        public void DelegatingStream_Write()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte[] buffer = new byte[2];
            int offset = 1;
            int count = 1;

            // Act
            mockStream.Write(buffer, offset, count);

            // Assert
            mockInnerStream.Verify(s => s.Write(buffer, offset, count), Times.Once());
        }

#if !Testing_NetStandard1_3 // BeginX and EndX not supported on Streams in netstandard1.3
        [Fact]
        public void DelegatingStream_BeginWrite()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte[] buffer = new byte[2];
            int offset = 1;
            int count = 1;
            AsyncCallback callback = new AsyncCallback((asyncResult) => { });
            object state = new object();

            // Act
            mockStream.BeginWrite(buffer, offset, count, callback, state);

            // Assert
            mockInnerStream.Verify(s => s.BeginWrite(buffer, offset, count, callback, state), Times.Once());
        }

        [Fact]
        public void DelegatingStream_EndWrite()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            Mock<IAsyncResult> mockIAsyncResult = new Mock<IAsyncResult>();

            // Act
            mockStream.EndWrite(mockIAsyncResult.Object);

            // Assert
            mockInnerStream.Verify(s => s.EndWrite(mockIAsyncResult.Object), Times.Once());
        }
#endif

        [Fact]
        public void DelegatingStream_WriteAsync()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte[] buffer = new byte[2];
            int offset = 1;
            int count = 1;

            // Act
            mockStream.WriteAsync(buffer, offset, count, CancellationToken.None);

            // Assert
            mockInnerStream.Verify(s => s.WriteAsync(buffer, offset, count, CancellationToken.None), Times.Once());
        }

        [Fact]
        public void DelegatingStream_WriteByte()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            MockDelegatingStream mockStream = new MockDelegatingStream(mockInnerStream.Object);
            byte data = new byte();

            // Act
            mockStream.WriteByte(data);

            // Assert
            mockInnerStream.Verify(s => s.WriteByte(data), Times.Once());
        }
    }
}

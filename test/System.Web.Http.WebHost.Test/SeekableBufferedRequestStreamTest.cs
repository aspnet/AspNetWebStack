// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost
{
    public class SeekableBufferedRequestStreamTest
    {
        private const string Content = "Hello, World!";

        /// <summary>
        /// Chosen to require multiple reads.
        /// </summary>
        private const int BufferSize = 3;

        [Fact]
        public void ReadToEnd_WithRead_SwapsStreams()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            // Act
            byte[] buffer = new byte[BufferSize];
            while (stream.Read(buffer, 0, buffer.Length) > 0)
            {
                // Guard
                Assert.Same(nonSeekable, stream.InnerStream);
            }

            // Assert
            Assert.Same(seekable, stream.InnerStream);
        }

        [Fact]
        public void ReadToEnd_WithBeginRead_SwapsStreams()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            // Act
            byte[] buffer = new byte[BufferSize];
            while (stream.EndRead(stream.BeginRead(buffer, 0, buffer.Length, null, null)) > 0)
            {
                // Guard
                Assert.Same(nonSeekable, stream.InnerStream);
            }

            // Assert
            Assert.Same(seekable, stream.InnerStream);
        }

        [Fact]
        public async Task ReadToEnd_WithReadAsync_SwapsStreams()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            // Act
            byte[] buffer = new byte[BufferSize];
            while (await stream.ReadAsync(buffer, 0, buffer.Length) > 0)
            {
                // Guard
                Assert.Same(nonSeekable, stream.InnerStream);
            }

            // Assert
            Assert.Same(seekable, stream.InnerStream);
        }

        [Fact]
        public void ReadToEnd_WithReadByte_SwapsStreams()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            // Act
            while (stream.ReadByte() > 0)
            {
                // Guard
                Assert.Same(nonSeekable, stream.InnerStream);
            }

            // Assert
            Assert.Same(seekable, stream.InnerStream);
        }

        [Fact]
        public void SwapStream_PreservesPosition()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            stream.ReadByte();

            // Guard
            Assert.Same(nonSeekable, stream.InnerStream);
            Assert.Equal(1L, stream.Position);

            stream.Seek(2L, SeekOrigin.Begin);

            // Assert
            Assert.Equal(2L, stream.Position);
            Assert.Same(seekable, stream.InnerStream);
        }

        [Fact]
        public void Seek_SwapsStreams()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            // Guard
            Assert.Same(nonSeekable, stream.InnerStream);

            // Act
            stream.Seek(1L, SeekOrigin.Begin);

            // Assert
            Assert.Same(seekable, stream.InnerStream);
        }

        [Fact]
        public void Seek_NoOpBegin()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            for (int i = 0; i < 3; i++)
            {
                stream.ReadByte();
            }

            // Act
            stream.Seek(3L, SeekOrigin.Begin);

            // Assert
            Assert.Same(nonSeekable, stream.InnerStream);
        }

        [Fact]
        public void Seek_NoOpCurent()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            for (int i = 0; i < 3; i++)
            {
                stream.ReadByte();
            }

            // Act
            stream.Seek(0L, SeekOrigin.Current);

            // Assert
            Assert.Same(nonSeekable, stream.InnerStream);
        }

        [Fact]
        public void Seek_NoOpEnd()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            for (int i = 0; i < 3; i++)
            {
                stream.ReadByte();
            }

            // Act
            stream.Seek(stream.Position - stream.Length, SeekOrigin.End);

            // Assert
            Assert.Same(nonSeekable, stream.InnerStream);
        }

        [Fact]
        public void Dispose_DoesNotDisposeInnerStreams()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            // Act
            stream.Dispose();

            // Assert
            Assert.False(stream.CanRead);
            Assert.True(nonSeekable.CanRead);
            Assert.True(seekable.CanRead);
        }

        [Fact]
        public void Seek_ThrowsOnInvalidSeekOrigin()
        {
            // Arrange
            var nonSeekable = CreateNonSeekableStream(Content);
            var seekable = CreateSeekableStream(Content);
            var stream = CreateStream(nonSeekable, seekable);

            var origin = (SeekOrigin)5;

            var message = 
                "The value of argument 'origin' (" + (int)origin + ") is invalid for Enum type " +
                "'SeekOrigin'." + Environment.NewLine +
                "Parameter name: origin";

            // Act & Assert
            Assert.Throws<InvalidEnumArgumentException>(() => stream.Seek(0L, origin), message);
        }

        private Stream CreateSeekableStream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        private Stream CreateNonSeekableStream(string content)
        {
            return new NonSeekableStream(Encoding.UTF8.GetBytes(content));
        }

        private AccessibleStreamWrapper CreateStream(Stream stream1, Stream stream2)
        {
            // Guards
            Assert.False(stream1.CanSeek);
            Assert.True(stream2.CanSeek);

            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.GetBufferedInputStream()).Returns(stream1);
            request.SetupGet(r => r.InputStream).Returns(stream2);

            return new AccessibleStreamWrapper(request.Object);
        }

        private class AccessibleStreamWrapper : SeekableBufferedRequestStream
        {
            public AccessibleStreamWrapper(HttpRequestBase request)
                : base(request)
            {
            }

            public new Stream InnerStream
            {
                get
                {
                    return base.InnerStream;
                }
            }
        }

        private class NonSeekableStream : MemoryStream
        {
            public NonSeekableStream()
            {
            }

            public NonSeekableStream(byte[] bytes)
                : base(bytes)
            {
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

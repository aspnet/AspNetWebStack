// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.Owin
{
    public class NonOwnedStreamTests
    {
        [Fact]
        public void Constructor_IfInnerStreamIsNull_Throws()
        {
            // Arrange
            Stream innerStream = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(innerStream), "innerStream");
        }

        [Fact]
        public void Read_DelegatesToInnerStream()
        {
            // Arrange
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            int expectedBytesRead = 123;
            mock.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(expectedBytesRead);
            mock.Setup(s => s.Close());
            mock.As<IDisposable>().Setup(s => s.Dispose());

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                byte[] expectedBuffer = new byte[0];
                int expectedOffset = 456;
                int expectedCount = 789;

                // Act
                int bytesRead = product.Read(expectedBuffer, expectedOffset, expectedCount);

                // Assert
                Assert.Equal(expectedBytesRead, bytesRead);
                mock.Verify(s => s.Read(expectedBuffer, expectedOffset, expectedCount), Times.Once());
            }
        }

        [Fact]
        public void Read_IfDisposed_Throws()
        {
            // Arrange
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.Setup(s => s.Close());
            mock.As<IDisposable>().Setup(s => s.Dispose());

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                byte[] buffer = new byte[0];
                int offset = 0;
                int count = 0;

                product.Dispose();

                // Act & Assert
                Assert.Throws<ObjectDisposedException>(() => product.Read(buffer, offset, count));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanRead_DelegatesToInnerStream(bool expectedCanRead)
        {
            // Arrange
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.SetupGet(s => s.CanRead)
                .Returns(expectedCanRead);
            mock.Setup(s => s.Close());
            mock.As<IDisposable>().Setup(s => s.Dispose());

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                // Act
                bool canRead = product.CanRead;

                // Assert
                Assert.Equal(expectedCanRead, canRead);
            }
        }

        [Fact]
        public void CanRead_IfDisposed_ReturnsFalse()
        {
            // Arrange
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.Setup(s => s.Close());
            mock.As<IDisposable>().Setup(s => s.Dispose());

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                product.Dispose();

                // Act
                bool canRead = product.CanRead;

                // Assert
                Assert.False(canRead);
            }
        }

        [Fact]
        public void Dispose_DoesNotDisposeInnerStream()
        {
            // Arrange
            bool innerStreamDisposed = false;
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.Setup(s => s.Close()).Callback(() => innerStreamDisposed = true);
            mock.As<IDisposable>().Setup(s => s.Dispose());
            mock.Protected().Setup("Dispose", true).Callback(() => innerStreamDisposed = true);

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                product.Dispose();

                // Act & Assert
                Assert.False(innerStreamDisposed);
            }
        }

        [Fact]
        public void Dispose_IfCalledTwice_DoesNotThrow()
        {
            // Arrange
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.Setup(s => s.Close());
            mock.As<IDisposable>().Setup(s => s.Dispose());

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                product.Dispose();

                // Act & Assert
                Assert.DoesNotThrow(() => product.Dispose());
            }
        }

        [Fact]
        public void Close_DoesNotDisposeInnerStream()
        {
            // Arrange
            bool innerStreamDisposed = false;
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.Setup(s => s.Close()).Callback(() => innerStreamDisposed = true);
            mock.As<IDisposable>().Setup(s => s.Dispose());
            mock.Protected().Setup("Dispose", true).Callback(() => innerStreamDisposed = true);

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                product.Close();

                // Act & Assert
                Assert.False(innerStreamDisposed);
            }
        }

        [Fact]
        public void Close_DisposesThisObject()
        {
            // Arrange
            Mock<Stream> mock = new Mock<Stream>(MockBehavior.Strict);
            mock.Setup(s => s.Close());
            mock.As<IDisposable>().Setup(s => s.Dispose());

            using (Stream innerStream = mock.Object)
            using (Stream product = CreateProductUnderTest(innerStream))
            {
                product.Close();

                byte[] buffer = new byte[0];
                int offset = 0;
                int count = 0;

                // Act & Assert
                Assert.Throws<ObjectDisposedException>(() => product.Read(buffer, offset, count));
            }
        }

        private static NonOwnedStream CreateProductUnderTest(Stream innerStream)
        {
            return new NonOwnedStream(innerStream);
        }
    }
}

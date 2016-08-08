// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Mocks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class MimeBodyPartTest
    {
        public static TheoryDataSet<MultipartStreamProvider> BadMultipartStreamProviders
        {
            get
            {
                return new TheoryDataSet<MultipartStreamProvider>
                {
                    new HttpContentMultipartExtensionsTests.NullProvider(),
                    new HttpContentMultipartExtensionsTests.BadStreamProvider(),
                    new HttpContentMultipartExtensionsTests.ReadOnlyStreamProvider(),
                };
            }
        }

        [Theory]
        [TestDataSet(typeof(MimeBodyPartTest), "BadMultipartStreamProviders")]
        public void GetOutputStream_ThrowsOnInvalidStreamProvider(MultipartStreamProvider streamProvider)
        {
            // Arrange
            HttpContent parent = new StringContent("hello");
            MimeBodyPart bodypart = new MimeBodyPart(streamProvider, 1024, parent);
            bodypart.Segments.Add(new ArraySegment<byte>(new byte[] { 1 }));

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => bodypart.WriteSegment(bodypart.Segments[0], CancellationToken.None).Wait());
        }

        [Fact]
        public void Dispose_ClosesOutputStreamOnNonMemoryStream()
        {
            // Arrange
            HttpContent parent = new StringContent("hello");
            Mock<Stream> mockStream = new Mock<Stream>() { CallBase = true };
            mockStream.Setup(s => s.CanWrite).Returns(true);
            Mock<MultipartStreamProvider> mockStreamProvider = new Mock<MultipartStreamProvider>();
            mockStreamProvider.Setup(sp => sp.GetStream(It.IsAny<HttpContent>(), It.IsAny<HttpContentHeaders>())).Returns(mockStream.Object);
            MimeBodyPart bodypart = new MimeBodyPart(mockStreamProvider.Object, 1024, parent);
            bodypart.Segments.Add(new ArraySegment<byte>(new byte[] { 1 }));
            bodypart.WriteSegment(bodypart.Segments[0], CancellationToken.None).Wait();
            bodypart.IsComplete = true;

            // Act
            bodypart.GetCompletedHttpContent();
            bodypart.Dispose();

            // Assert
            mockStream.Verify(s => s.Close(), Times.Once());
        }

        [Fact]
        public void Dispose_SetsPositionToZeroOnMemoryStream()
        {
            // Arrange
            HttpContent parent = new StringContent("hello");
            Mock<MemoryStream> mockStream = new Mock<MemoryStream> { CallBase = true };
            Mock<MultipartStreamProvider> mockStreamProvider = new Mock<MultipartStreamProvider>();
            mockStreamProvider.Setup(sp => sp.GetStream(It.IsAny<HttpContent>(), It.IsAny<HttpContentHeaders>())).Returns(mockStream.Object);
            MimeBodyPart bodypart = new MimeBodyPart(mockStreamProvider.Object, 1024, parent);
            bodypart.Segments.Add(new ArraySegment<byte>(new byte[] { 1 }));
            bodypart.WriteSegment(bodypart.Segments[0], CancellationToken.None).Wait();
            bodypart.IsComplete = true;

            // Act
            bodypart.GetCompletedHttpContent();
            bodypart.Dispose();

            // Assert
            mockStream.VerifySet(s => s.Position = 0);
        }
    }
}

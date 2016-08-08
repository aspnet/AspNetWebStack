// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Formatting.Mocks;
using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Handlers
{
    public class ProgressWriteAsyncResultTest
    {
        static readonly byte[] sampleData = Encoding.UTF8.GetBytes("Hello World! Hello World! Hello World! Hello World! Hello World!");

        [Fact]
        public void Constructor_BeginWriteOnInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            ProgressStream progressStream = ProgressStreamTest.CreateProgressStream();

            // Act
            IAsyncResult result = new ProgressWriteAsyncResult(
                mockInnerStream.Object, progressStream, sampleData, 2, 4, null, null);

            // Assert 
            mockInnerStream.Verify(s => s.BeginWrite(sampleData, 2, 4, It.IsAny<AsyncCallback>(), It.IsAny<object>()),
                Times.Once());
        }

        [Fact]
        public void Constructor_CompletesSynchronouslyIfInnerStreamCompletesSynchronously()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            object userState = new object();
            IAsyncResult mockIAsyncResult = MockCompletedAsyncResult.Create(true, userState);
            mockInnerStream.Setup(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Returns(mockIAsyncResult);
            ProgressStream progressStream = ProgressStreamTest.CreateProgressStream();

            // Act
            IAsyncResult result = new ProgressWriteAsyncResult(
                mockInnerStream.Object, progressStream, sampleData, 2, 4, null, userState);

            // Assert 
            Assert.True(result.IsCompleted);
            Assert.True(result.CompletedSynchronously);
            Assert.Same(userState, result.AsyncState);
        }

        [Fact]
        public void Constructor_CompletesWithExceptionIfInnerStreamThrows()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Throws<ApplicationException>();
            ProgressStream progressStream = ProgressStreamTest.CreateProgressStream();

            // Act
            IAsyncResult result = new ProgressWriteAsyncResult(
                mockInnerStream.Object, progressStream, sampleData, 2, 2, null, null);

            // Assert 
            Assert.True(result.IsCompleted);
            Assert.Throws<ApplicationException>(() => ProgressWriteAsyncResult.End(result));
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(10, 1)]
        public void Constructor_ReportsBytesWritten(int offset, int count)
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            object userState = new object();
            IAsyncResult mockIAsyncResult = MockCompletedAsyncResult.Create(true, userState);
            mockInnerStream.Setup(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Returns(mockIAsyncResult);

            MockProgressEventHandler mockProgressHandler;
            ProgressMessageHandler progressMessageHandler = MockProgressEventHandler.CreateProgressMessageHandler(out mockProgressHandler, sendProgress: true);
            HttpRequestMessage request = new HttpRequestMessage();

            ProgressStream progressStream = ProgressStreamTest.CreateProgressStream(progressMessageHandler: progressMessageHandler, request: request);

            // Act
            IAsyncResult result = new ProgressWriteAsyncResult(
                mockInnerStream.Object, progressStream, sampleData, offset, count, null, userState);

            // Assert 
            Assert.True(mockProgressHandler.WasInvoked);
            Assert.Same(request, mockProgressHandler.Sender);
            Assert.Equal(count, mockProgressHandler.EventArgs.BytesTransferred);
            Assert.Same(userState, mockProgressHandler.EventArgs.UserState);
        }
    }
}

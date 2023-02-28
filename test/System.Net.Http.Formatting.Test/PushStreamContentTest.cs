// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Net.Http
{
    public class PushStreamContentTest
    {
        [Fact]
        public void Constructor_ThrowsOnNullAction()
        {
            Action<Stream, HttpContent, TransportContext> action = null;
            Assert.ThrowsArgumentNull(() => new PushStreamContent(action), "onStreamAvailable");
        }

        [Fact]
        public void Constructor_SetsDefaultMediaType()
        {
            Action<Stream, HttpContent, TransportContext> streamAction = new MockStreamAction().Action;
            PushStreamContent content = new PushStreamContent(streamAction);
            Assert.Equal(MediaTypeConstants.ApplicationOctetStreamMediaType, content.Headers.ContentType);
        }

        [Fact]
        public void Constructor_SetsMediaTypeFromString()
        {
            Action<Stream, HttpContent, TransportContext> streamAction = new MockStreamAction().Action;
            PushStreamContent content = new PushStreamContent(streamAction, "text/xml");
            Assert.Equal(MediaTypeConstants.TextXmlMediaType, content.Headers.ContentType);
        }

        [Fact]
        public void Constructor_SetsMediaType()
        {
            Action<Stream, HttpContent, TransportContext> streamAction = new MockStreamAction().Action;
            PushStreamContent content = new PushStreamContent(streamAction, MediaTypeConstants.TextXmlMediaType);
            Assert.Equal(MediaTypeConstants.TextXmlMediaType, content.Headers.ContentType);
        }

        [Fact]
        public async Task SerializeToStreamAsync_CallsAction()
        {
            // Arrange
            MemoryStream outputStream = new MemoryStream();
            MockStreamAction streamAction = new MockStreamAction(close: true);
            PushStreamContent content = new PushStreamContent((Action<Stream, HttpContent, TransportContext>)streamAction.Action);

            // Act
            await content.CopyToAsync(outputStream);

            // Assert
            Assert.True(streamAction.WasInvoked);
            Assert.Same(content, streamAction.Content);
            Assert.IsType<PushStreamContent.CompleteTaskOnCloseStream>(streamAction.OutputStream);

            // We don't close the underlying stream
            Assert.True(outputStream.CanRead);
        }

        [Fact]
        public async Task SerializeToStreamAsync_CompletesTaskOnActionException()
        {
            // Arrange
            MemoryStream outputStream = new MemoryStream();
            MockStreamAction streamAction = new MockStreamAction(throwException: true);
            PushStreamContent content = new PushStreamContent((Action<Stream, HttpContent, TransportContext>)streamAction.Action);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => content.CopyToAsync(outputStream));
            Assert.True(streamAction.WasInvoked);
            Assert.IsType<PushStreamContent.CompleteTaskOnCloseStream>(streamAction.OutputStream);
            Assert.True(outputStream.CanRead);
        }

#if Testing_NetStandard1_3
        [Fact]
        public async Task CompleteTaskOnCloseStream_Dispose_CompletesTaskButDoNotDisposeInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            MockCompleteTaskOnCloseStream mockStream = new MockCompleteTaskOnCloseStream(mockInnerStream.Object, serializeToStreamTask);

            // Act
            mockStream.Dispose();

            // Assert
            mockInnerStream.Protected().Verify("Dispose", Times.Never(), exactParameterMatch: true, args: true);
            Assert.Equal(TaskStatus.RanToCompletion, serializeToStreamTask.Task.Status);
            Assert.True(await serializeToStreamTask.Task);
        }
#else
        [Fact]
        public async Task CompleteTaskOnCloseStream_Dispose_CompletesTaskButDoNotCloseInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            MockCompleteTaskOnCloseStream mockStream = new MockCompleteTaskOnCloseStream(mockInnerStream.Object, serializeToStreamTask);

            // Act
            mockStream.Dispose();

            // Assert
            mockInnerStream.Protected().Verify("Dispose", Times.Never(), exactParameterMatch: true, args: true);
            mockInnerStream.Verify(s => s.Close(), Times.Never());
            Assert.Equal(TaskStatus.RanToCompletion, serializeToStreamTask.Task.Status);
            Assert.True(await serializeToStreamTask.Task);
        }

        [Fact]
        public async Task NonClosingDelegatingStream_Close_CompletesTaskButDoNotCloseInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            MockCompleteTaskOnCloseStream mockStream = new MockCompleteTaskOnCloseStream(mockInnerStream.Object, serializeToStreamTask);

            // Act
            mockStream.Close();

            // Assert
            mockInnerStream.Protected().Verify("Dispose", Times.Never(), exactParameterMatch: true, args: true);
            mockInnerStream.Verify(s => s.Close(), Times.Never());
            Assert.Equal(TaskStatus.RanToCompletion, serializeToStreamTask.Task.Status);
            Assert.True(await serializeToStreamTask.Task);
        }
#endif

        [Fact]
        public async Task PushStreamContentWithAsyncOnStreamAvailableHandler_ExceptionsInOnStreamAvailable_AreCaught()
        {
            // Arrange
            bool faulted = false;
            Exception exception = new ApplicationException();
            PushStreamContent content = new PushStreamContent(async (s, c, tc) =>
            {
                await Task.FromResult(42);
                throw exception;
            });
            MemoryStream stream = new MemoryStream();

            try
            {
                // Act
                await content.CopyToAsync(stream);
            }
            catch (ApplicationException e)
            {
                Assert.Same(exception, e);
                faulted = true;
            }

            // Assert
            Assert.True(faulted);
        }

        [Fact]
        public async Task PushStream_HttpContentIntegrationTest()
        {
            // Arrange
            var expected = "Hello, world!";

            using (var client = new MockHttpClient())
            {
                // We mock the client, so this doesn't actually hit the web. This client will just echo back
                // the body content we give it.
                using (var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:30000/"))
                {
                    request.Content = new PushStreamContent((stream, content, context) =>
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(expected);
                        }
                    }, "text/plain");

                    // Act
                    using (var response = await client.SendAsync(request, CancellationToken.None))
                    {
                        // Assert
                        response.EnsureSuccessStatusCode();
                        var responseText = await response.Content.ReadAsStringAsync();
                        Assert.Equal(expected, responseText);
                    }
                }
            }
        }

        private class MockStreamAction
        {
            bool _close;
            bool _throwException;

            public MockStreamAction(bool close = false, bool throwException = false)
            {
                _close = close;
                _throwException = throwException;
            }

            public bool WasInvoked { get; private set; }

            public Stream OutputStream { get; private set; }

            public HttpContent Content { get; private set; }

            public TransportContext TransportContext { get; private set; }

            public void Action(Stream stream, HttpContent content, TransportContext context)
            {
                WasInvoked = true;
                OutputStream = stream;
                Content = content;
                TransportContext = context;

                if (_close)
                {
#if Testing_NetStandard1_3
                    stream.Dispose();
#else
                    stream.Close();
#endif
                }

                if (_throwException)
                {
                    throw new ApplicationException("Action threw exception!");
                }
            }
        }

        internal class MockCompleteTaskOnCloseStream : PushStreamContent.CompleteTaskOnCloseStream
        {
            public MockCompleteTaskOnCloseStream(Stream innerStream, TaskCompletionSource<bool> serializeToStreamTask)
                : base(innerStream, serializeToStreamTask)
            {
            }
        }

        private class MockHttpClient : HttpClient
        {
            public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, Threading.CancellationToken cancellationToken)
            {
                var stream = new MemoryStream();
                await request.Content.CopyToAsync(stream);
                stream.Position = 0;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream),
                };
            }
        }
    }
}

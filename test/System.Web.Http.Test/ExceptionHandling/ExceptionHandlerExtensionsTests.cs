// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerExtensionsTests
    {
        [Fact]
        public async Task HandleAsync_CallsInterfaceHandleAsync()
        {
            // Arrange
            Mock<IExceptionHandler> mock = CreateStubHandlerMock();
            IExceptionHandler handler = mock.Object;

            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                ExceptionContext expectedContext = CreateMinimalValidContext();
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                await ExceptionHandlerExtensions.HandleAsync(handler, expectedContext,
                    expectedCancellationToken);

                // Assert
                mock.Verify(h => h.HandleAsync(It.Is<ExceptionHandlerContext>(
                    c => c.ExceptionContext == expectedContext), expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task HandleAsync_IfResultIsNotSet_ReturnsCompletedTaskWithNullResponse()
        {
            // Arrange
            IExceptionHandler handler = CreateStubHandler();
            ExceptionContext context = CreateMinimalValidContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            HttpResponseMessage response = await ExceptionHandlerExtensions.HandleAsync(handler, context, cancellationToken);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task HandleAsync_IfResultIsSet_CallsResultExecuteAsync()
        {
            // Arrange
            using (HttpResponseMessage response = CreateResponse())
            {
                Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
                mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
                IHttpActionResult result = mock.Object;
                IExceptionHandler handler = CreateResultHandler(result);

                using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
                {
                    ExceptionContext context = CreateMinimalValidContext();
                    CancellationToken expectedCancellationToken = tokenSource.Token;

                    // Act
                    await ExceptionHandlerExtensions.HandleAsync(handler, context,
                        expectedCancellationToken);

                    // Assert
                    mock.Verify(h => h.ExecuteAsync(expectedCancellationToken), Times.Once());
                }
            }
        }

        [Fact]
        public async Task HandleAsync_IfResultIsSet_ReturnsCompletedTaskWithResultResponse()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
                mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponse));
                IHttpActionResult result = mock.Object;
                IExceptionHandler handler = CreateResultHandler(result);

                ExceptionContext context = CreateMinimalValidContext();
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                HttpResponseMessage response = await handler.HandleAsync(context, cancellationToken);

                // Assert
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public void HandleAsync_IfHandlerIsNull_Throws()
        {
            // Arrange
            IExceptionHandler handler = null;
            ExceptionContext context = CreateMinimalValidContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
                ExceptionHandlerExtensions.HandleAsync(handler, context, cancellationToken), "handler");
        }

        [Fact]
        public void HandleAsync_IfContextIsNull_Throws()
        {
            // Arrange
            IExceptionHandler handler = CreateDummyHandler();
            ExceptionContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
                ExceptionHandlerExtensions.HandleAsync(handler, context, cancellationToken), "context");
        }

        [Fact]
        public async Task HandleAsync_IfResultIsSetButReturnsNull_ReturnsFaultedTask()
        {
            // Arrange
            Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            mock
                .Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<HttpResponseMessage>(null));
            IHttpActionResult result = mock.Object;
            IExceptionHandler handler = CreateResultHandler(result);
            ExceptionContext context = CreateMinimalValidContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => ExceptionHandlerExtensions.HandleAsync(handler, context, cancellationToken));
            Assert.Equal("IHttpActionResult.ExecuteAsync must not return null.", exception.Message);
        }

        private static CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static ExceptionContext CreateMinimalValidContext()
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer);
        }

        private static IExceptionHandler CreateDummyHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static IExceptionHandler CreateResultHandler(IHttpActionResult result)
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                {
                    c.Result = result;
                    return Task.FromResult(0);
                });
            return mock.Object;
        }

        private static IExceptionHandler CreateStubHandler()
        {
            return CreateStubHandlerMock().Object;
        }

        private static Mock<IExceptionHandler> CreateStubHandlerMock()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }
    }
}

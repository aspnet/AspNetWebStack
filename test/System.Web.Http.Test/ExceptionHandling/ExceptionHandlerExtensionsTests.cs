// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerExtensionsTests
    {
        [Fact]
        public void HandleAsync_CallsInterfaceHandleAsync()
        {
            // Arrange
            Mock<IExceptionHandler> mock = CreateStubHandlerMock();
            IExceptionHandler handler = mock.Object;

            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                ExceptionContext expectedContext = CreateMinimalValidContext();
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                Task task = ExceptionHandlerExtensions.HandleAsync(handler, expectedContext,
                    expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();
                task.ThrowIfFaulted();

                mock.Verify(h => h.HandleAsync(It.Is<ExceptionHandlerContext>(
                    c => c.ExceptionContext == expectedContext), expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public void HandleAsync_IfResultIsNotSet_ReturnsCompletedTaskWithNullResponse()
        {
            // Arrange
            IExceptionHandler handler = CreateStubHandler();
            ExceptionContext context = CreateMinimalValidContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            Task<HttpResponseMessage> task = ExceptionHandlerExtensions.HandleAsync(handler, context,
                cancellationToken);

            // Assert
            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Null(task.Result);
        }

        [Fact]
        public void HandleAsync_IfResultIsSet_CallsResultExecuteAsync()
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
                    Task task = ExceptionHandlerExtensions.HandleAsync(handler, context,
                        expectedCancellationToken);

                    // Assert
                    Assert.NotNull(task);
                    task.WaitUntilCompleted();
                    task.ThrowIfFaulted();

                    mock.Verify(h => h.ExecuteAsync(expectedCancellationToken), Times.Once());
                }
            }
        }

        [Fact]
        public void HandleAsync_IfResultIsSet_ReturnsCompletedTaskWithResultResponse()
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
                Task<HttpResponseMessage> task = handler.HandleAsync(context, cancellationToken);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                Assert.Same(expectedResponse, task.Result);
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
        public void HandleAsync_IfResultIsSetButReturnsNull_ReturnsFaultedTask()
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

            // Act
            Task<HttpResponseMessage> task =
                ExceptionHandlerExtensions.HandleAsync(handler, context, cancellationToken);

            // Assert
            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, task.Status);
            Assert.NotNull(task.Exception);
            Exception exception = task.Exception.GetBaseException();
            Assert.IsType<InvalidOperationException>(exception);
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

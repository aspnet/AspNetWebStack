// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerTests
    {
        [Fact]
        public void HandleAsync_IfContextIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler> { CallBase = true };
            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.HandleAsync(context, cancellationToken), "context");
        }

        [Fact]
        public void HandleAsync_IfShouldHandleReturnsTrue_DelegatesToHandleAsyncCore()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler> { CallBase = true };
            Task expectedTask = CreateCompletedTask();
            mock.Setup(h => h.ShouldHandle(It.IsAny<ExceptionHandlerContext>())).Returns(true);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(expectedTask);

            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext expectedContext = CreateMinimalValidHandlerContext();

            using (CancellationTokenSource tokenSource = CreateTokenSource())
            {
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                Task task = product.HandleAsync(expectedContext, expectedCancellationToken);

                // Assert
                Assert.Same(expectedTask, task);
                mock.Verify(h => h.ShouldHandle(expectedContext), Times.Once());
                mock.Verify(h => h.HandleAsync(expectedContext, expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public void HandleAsync_IfShouldHandleReturnsFalse_ReturnsCompletedTask()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler> { CallBase = true };
            mock.Setup(h => h.ShouldHandle(It.IsAny<ExceptionHandlerContext>())).Returns(false);

            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext expectedContext = CreateMinimalValidHandlerContext();
            CancellationToken expectedCancellationToken = CancellationToken.None;

            // Act
            Task task = product.HandleAsync(expectedContext, expectedCancellationToken);

            // Assert
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            mock.Verify(h => h.ShouldHandle(expectedContext), Times.Once());
            mock.Verify(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [Fact]
        public async Task HandleAsyncCore_DelegatesToHandleCore_AndReturnsCompletedTask()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext expectedContext = CreateMinimalValidHandlerContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            await product.HandleAsync(expectedContext, cancellationToken);

            // Assert
            mock.Verify(h => h.Handle(expectedContext), Times.Once());
        }

        [Fact]
        public void HandleCore_DoesNotThrow()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = CreateMinimalValidHandlerContext();

            // Act & Assert
            Assert.DoesNotThrow(() => product.Handle(context));
        }

        [Fact]
        public void ShouldHandle_IfContextIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.ShouldHandle(context), "context");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldHandle_ReturnsIsTopLevelCatchBlock(bool isTopLevelCatchBlock)
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = CreateContext(new ExceptionContext(new Exception(),
                new ExceptionContextCatchBlock("IgnoreCaughtAt", isTopLevelCatchBlock, callsHandler: false)));

            // Act
            bool shouldHandle = product.ShouldHandle(context);

            // Assert
            Assert.Equal(isTopLevelCatchBlock, shouldHandle);
        }

        private static Task CreateCompletedTask()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            return source.Task;
        }

        private static ExceptionHandlerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }

        private static ExceptionContext CreateMinimalValidExceptionContext()
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer);
        }

        private static CancellationTokenSource CreateTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static ExceptionHandlerContext CreateMinimalValidHandlerContext()
        {
            return new ExceptionHandlerContext(CreateMinimalValidExceptionContext());
        }
    }
}

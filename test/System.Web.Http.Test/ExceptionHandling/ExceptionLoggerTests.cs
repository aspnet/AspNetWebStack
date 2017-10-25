// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionLoggerTests
    {
        [Fact]
        public void LogAsync_IfContextIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger> { CallBase = true };

            IExceptionLogger product = mock.Object;

            ExceptionLoggerContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.LogAsync(context, cancellationToken), "context");
        }

        [Fact]
        public void LogAsync_IfShouldLogReturnsTrue_DelegatesToLogAsyncCore()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger> { CallBase = true };
            Task expectedTask = CreateCompletedTask();
            mock.Setup(h => h.ShouldLog(It.IsAny<ExceptionLoggerContext>())).Returns(true);
            mock
                .Setup(h => h.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(expectedTask);

            IExceptionLogger product = mock.Object;

            ExceptionLoggerContext expectedContext = CreateMinimalValidLoggerContext();

            using (CancellationTokenSource tokenSource = CreateTokenSource())
            {
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                Task task = product.LogAsync(expectedContext, expectedCancellationToken);

                // Assert
                Assert.Same(expectedTask, task);
                mock.Verify(h => h.ShouldLog(expectedContext), Times.Once());
                mock.Verify(h => h.LogAsync(expectedContext, expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public void LogAsync_IfShouldLogReturnsFalse_ReturnsCompletedTask()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger> { CallBase = true };
            mock.Setup(h => h.ShouldLog(It.IsAny<ExceptionLoggerContext>())).Returns(false);

            IExceptionLogger product = mock.Object;

            ExceptionLoggerContext expectedContext = CreateMinimalValidLoggerContext();
            CancellationToken expectedCancellationToken = CancellationToken.None;

            // Act
            Task task = product.LogAsync(expectedContext, expectedCancellationToken);

            // Assert
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            mock.Verify(h => h.ShouldLog(expectedContext), Times.Once());
            mock.Verify(h => h.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [Fact]
        public async Task LogAsyncCore_DelegatesToLogCore_AndReturnsCompletedTask()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            ExceptionLoggerContext expectedContext = CreateMinimalValidLoggerContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            await product.LogAsync(expectedContext, cancellationToken);

            // Assert
            mock.Verify(h => h.Log(expectedContext), Times.Once());
        }

        [Fact]
        public void LogCore_DoesNotThrow()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            ExceptionLoggerContext context = CreateMinimalValidLoggerContext();

            // Act & Assert
            Assert.DoesNotThrow(() => product.Log(context));
        }

        [Fact]
        public void ShouldLog_IfContextIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            ExceptionLoggerContext context = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.ShouldLog(context), "context");
        }

        [Fact]
        public void ShouldLog_IfExceptionDataIsNull_ReturnsTrue()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            Exception exception = CreateException(data: null);
            ExceptionLoggerContext context = CreateMinimalValidLoggerContext(exception);

            // Act
            bool shouldLog = product.ShouldLog(context);

            // Assert
            Assert.True(shouldLog);
        }

        [Fact]
        public void LoggedByKey_IsSpecifiedValue()
        {
            // Act & Assert
            Assert.Equal("MS_LoggedBy", ExceptionLogger.LoggedByKey);
        }

        [Fact]
        public void ShouldLog_IfExceptionDataIsReadOnly_ReturnsTrue()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            Mock<IDictionary> dataMock = new Mock<IDictionary>(MockBehavior.Strict);
            dataMock.Setup(d => d.IsReadOnly).Returns(true);
            IDictionary data = dataMock.Object;
            Exception exception = CreateException(data);
            ExceptionLoggerContext context = CreateMinimalValidLoggerContext(exception);

            // Act
            bool shouldLog = product.ShouldLog(context);

            // Assert
            Assert.True(shouldLog);
        }

        [Fact]
        public void ShouldLog_IfExceptionDataIsEmpty_AddsLoggedByKeyWithLoggerValueAndReturnsTrue()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            IDictionary data = new Dictionary();
            Exception exception = CreateException(data);
            ExceptionLoggerContext context = CreateMinimalValidLoggerContext(exception);

            // Act
            bool shouldLog = product.ShouldLog(context);

            // Assert
            Assert.True(shouldLog);
            Assert.True(data.Contains(ExceptionLogger.LoggedByKey));
            object loggedBy = data[ExceptionLogger.LoggedByKey];
            ICollection<object> loggedByCollection = Assert.IsAssignableFrom<ICollection<object>>(loggedBy);
            Assert.Contains(product, loggedByCollection);
        }

        [Fact]
        public void ShouldLog_IfExceptionDataHasEmptyLoggedBy_AddsLoggerAndReturnsTrue()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            ICollection<object> loggedBy = new List<object>();
            IDictionary data = new Dictionary();
            data.Add(ExceptionLogger.LoggedByKey, loggedBy);
            Exception exception = CreateException(data);
            ExceptionLoggerContext context = CreateMinimalValidLoggerContext(exception);

            // Act
            bool shouldLog = product.ShouldLog(context);

            // Assert
            Assert.True(shouldLog);
            Assert.True(data.Contains(ExceptionLogger.LoggedByKey));
            object updatedLoggedBy = data[ExceptionLogger.LoggedByKey];
            Assert.Same(loggedBy, updatedLoggedBy);
            Assert.Contains(product, loggedBy);
        }

        [Fact]
        public void ShouldLog_IfExceptionDataContainsLoggedByLogger_ReturnsFalse()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            IDictionary data = new Dictionary();
            ICollection<object> loggedBy = new List<object>() { product };
            data.Add(ExceptionLogger.LoggedByKey, loggedBy);
            Exception exception = CreateException(data);
            ExceptionLoggerContext context = CreateMinimalValidLoggerContext(exception);

            // Act
            bool shouldLog = product.ShouldLog(context);

            // Assert
            Assert.False(shouldLog);
            Assert.True(data.Contains(ExceptionLogger.LoggedByKey));
            object updatedLoggedBy = data[ExceptionLogger.LoggedByKey];
            Assert.Same(loggedBy, updatedLoggedBy);
            Assert.Equal(1, loggedBy.Count);
        }

        [Fact]
        public void ShouldLog_IfExceptionDataContainsLoggedByOfIncompatibleType_ReturnsTrue()
        {
            // Arrange
            Mock<ExceptionLogger> mock = new Mock<ExceptionLogger>();
            mock.CallBase = true;
            ExceptionLogger product = mock.Object;

            IDictionary data = new Dictionary();
            data.Add(ExceptionLogger.LoggedByKey, null);
            Exception exception = CreateException(data);
            ExceptionLoggerContext context = CreateMinimalValidLoggerContext(exception);

            // Act
            bool shouldLog = product.ShouldLog(context);

            // Assert
            Assert.True(shouldLog);
            Assert.True(data.Contains(ExceptionLogger.LoggedByKey));
            object updatedLoggedBy = data[ExceptionLogger.LoggedByKey];
            Assert.Null(updatedLoggedBy);
        }

        private static Task CreateCompletedTask()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            return source.Task;
        }

        private static ExceptionLoggerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionLoggerContext(exceptionContext);
        }

        private static Exception CreateException(IDictionary data)
        {
            Mock<Exception> mock = new Mock<Exception>();
            mock.Setup(e => e.Data).Returns(data);
            return mock.Object;
        }

        private static ExceptionContext CreateMinimalValidExceptionContext()
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer);
        }

        private static CancellationTokenSource CreateTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static ExceptionLoggerContext CreateMinimalValidLoggerContext()
        {
            return CreateMinimalValidLoggerContext(new Exception());
        }

        private static ExceptionLoggerContext CreateMinimalValidLoggerContext(Exception exception)
        {
            return CreateContext(new ExceptionContext(exception, ExceptionCatchBlocks.HttpServer));
        }

        private class Dictionary : DictionaryBase
        {
        }
    }
}

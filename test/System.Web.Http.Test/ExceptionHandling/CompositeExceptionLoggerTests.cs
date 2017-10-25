// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class CompositeExceptionLoggerTests
    {
        [Fact]
        public void Constructor_IfLoggersIsNull_Throws()
        {
            // Arrange
            IEnumerable<IExceptionLogger> loggers = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(loggers), "loggers");
        }

        [Fact]
        public void Loggers_ContainsSpecifiedInstances()
        {
            // Arrange
            IExceptionLogger expectedLogger = CreateDummyLogger();
            List<IExceptionLogger> expectedLoggers = new List<IExceptionLogger> { expectedLogger };
            CompositeExceptionLogger product = CreateProductUnderTest(expectedLoggers);

            // Act
            IEnumerable<IExceptionLogger> loggers = product.Loggers;

            // Assert
            Assert.NotNull(loggers);
            IExceptionLogger logger = Assert.Single(loggers);
            Assert.Same(expectedLogger, logger);

        }

        [Fact]
        public void Constructor_CapturesLoggersContents()
        {
            // Arrange
            IExceptionLogger expectedLogger = CreateDummyLogger();
            List<IExceptionLogger> expectedLoggers = new List<IExceptionLogger> { expectedLogger };

            // Act
            CompositeExceptionLogger product = CreateProductUnderTest(expectedLoggers);

            // Assert
            expectedLoggers.Clear();
            IEnumerable<IExceptionLogger> loggers = product.Loggers;
            Assert.NotNull(loggers);
            IExceptionLogger logger = Assert.Single(loggers);
            Assert.Same(expectedLogger, logger);
        }

        [Fact]
        public void ConstructorWithParams_SetsLoggers()
        {
            // Arrange
            IExceptionLogger expectedLogger1 = CreateDummyLogger();
            IExceptionLogger expectedLogger2 = CreateDummyLogger();

            // Act
            CompositeExceptionLogger product = new CompositeExceptionLogger(expectedLogger1, expectedLogger2);

            // Assert
            IEnumerable<IExceptionLogger> loggers = product.Loggers;
            Assert.NotNull(loggers);
            Assert.Equal(2, loggers.Count());
            Assert.Same(expectedLogger1, loggers.ElementAt(0));
            Assert.Same(expectedLogger2, loggers.ElementAt(1));
        }

        [Fact]
        public async Task LogAsync_DelegatesToLoggers()
        {
            // Arrange
            Mock<IExceptionLogger> exceptionLogger1Mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            Mock<IExceptionLogger> exceptionLogger2Mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            exceptionLogger1Mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            exceptionLogger2Mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            IEnumerable<IExceptionLogger> loggers = new IExceptionLogger[]
            {
                exceptionLogger1Mock.Object,
                exceptionLogger2Mock.Object
            };
            IExceptionLogger product = CreateProductUnderTest(loggers);

            ExceptionLoggerContext expectedContext = CreateMinimalValidContext();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            // Act
            await product.LogAsync(expectedContext, expectedCancellationToken);

            // Assert
            exceptionLogger1Mock.Verify(l => l.LogAsync(expectedContext, expectedCancellationToken), Times.Once());
            exceptionLogger2Mock.Verify(l => l.LogAsync(expectedContext, expectedCancellationToken), Times.Once());
        }

        [Fact]
        public void LogAsync_IfLoggerIsNull_Throws()
        {
            // Arrange
            IEnumerable<IExceptionLogger> loggers = new IExceptionLogger[] { null };
            IExceptionLogger product = CreateProductUnderTest(loggers);

            ExceptionLoggerContext context = CreateMinimalValidContext();
            CancellationToken cancellationToken = CreateCancellationToken();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => product.LogAsync(context, cancellationToken),
                "The IExceptionLogger instance must not be null.");

        }

        private static CancellationToken CreateCancellationToken()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return source.Token;
        }

        private static ExceptionLoggerContext CreateMinimalValidContext()
        {
            return new ExceptionLoggerContext(new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer));
        }

        private static IExceptionLogger CreateDummyLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private static CompositeExceptionLogger CreateProductUnderTest(IEnumerable<IExceptionLogger> loggers)
        {
            return new CompositeExceptionLogger(loggers);
        }
    }
}

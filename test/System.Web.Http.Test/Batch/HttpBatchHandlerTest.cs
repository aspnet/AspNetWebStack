// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Batch;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpBatchHandlerTest
    {
        [Fact]
        public void Constructor_Throws_WhenServerIsNull()
        {
            // Arrange
            HttpServer httpServer = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(httpServer), "httpServer");
        }

        [Fact]
        public void ExceptionLoggerGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            IExceptionLogger expectedExceptionLogger = CreateDummyExceptionLogger();
            IExceptionHandler exceptionHandler = CreateDummyExceptionHandler();

            using (HttpServer server = CreateServer())
            using (HttpBatchHandler handler = CreateProductUnderTest(server, expectedExceptionLogger,
                exceptionHandler))
            {
                // Act
                IExceptionLogger exceptionLogger = handler.ExceptionLogger;

                // Assert
                Assert.Same(expectedExceptionLogger, exceptionLogger);
            }
        }

        [Fact]
        public void ExceptionHandlerGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            IExceptionLogger exceptionLogger = CreateDummyExceptionLogger();
            IExceptionHandler expectedExceptionHandler = CreateDummyExceptionHandler();

            using (HttpServer server = CreateServer())
            using (HttpBatchHandler handler = CreateProductUnderTest(server, exceptionLogger,
                expectedExceptionHandler))
            {
                // Act
                IExceptionHandler exceptionHandler = handler.ExceptionHandler;

                // Assert
                Assert.Same(expectedExceptionHandler, exceptionHandler);
            }
        }

        [Fact]
        public void ExceptionLoggerGet_IfUnset_ReturnsExceptionLoggerFromConfiguration()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                IExceptionLogger expectedExceptionLogger = CreateDummyExceptionLogger();
                configuration.Services.Add(typeof(IExceptionLogger), expectedExceptionLogger);

                using (HttpServer server = new HttpServer(configuration))
                using (HttpBatchHandler product = CreateProductUnderTest(server))
                {
                    // Act
                    IExceptionLogger exceptionLogger = product.ExceptionLogger;

                    // Assert
                    CompositeExceptionLogger compositeLogger = Assert.IsType<CompositeExceptionLogger>(exceptionLogger);
                    IEnumerable<IExceptionLogger> loggers = compositeLogger.Loggers;
                    Assert.NotNull(loggers);
                    IExceptionLogger logger = Assert.Single(loggers);
                    Assert.Same(expectedExceptionLogger, logger);
                }
            }
        }

        [Fact]
        public void ExceptionHandlerGet_IfUnset_ReturnsExceptionHandlerFromConfiguration()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                IExceptionHandler expectedExceptionHandler = CreateDummyExceptionHandler();
                configuration.Services.Replace(typeof(IExceptionHandler), expectedExceptionHandler);

                using (HttpServer server = new HttpServer(configuration))
                using (HttpBatchHandler product = CreateProductUnderTest(server))
                {
                    // Act
                    IExceptionHandler exceptionHandler = product.ExceptionHandler;

                    // Assert
                    LastChanceExceptionHandler lastChanceHandler = Assert.IsType<LastChanceExceptionHandler>(exceptionHandler);
                    Assert.Same(expectedExceptionHandler, lastChanceHandler.InnerHandler);
                }
            }
        }

        [Fact]
        public Task SendAsync_Throws_WhenRequestIsNull()
        {
            // Arrange
            MockHttpBatchHandler mockHandler = new MockHttpBatchHandler(new HttpServer());

            // Act & Assert
            return Assert.ThrowsArgumentNullAsync(() => mockHandler.SendAsync(null), "request");
        }

        [Fact]
        public async Task SendAsync_CallsProcessBatchAsync()
        {
            Mock<HttpBatchHandler> handler = new Mock<HttpBatchHandler>(new HttpServer());
            handler.Setup(h => h.ProcessBatchAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Redirect)
                {
                    Content = new StringContent("ProcessBatchAsync called.")
                }));
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler.Object);

            var response = await invoker.SendAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("ProcessBatchAsync called.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SendAsync_ReturnsHttpResponseException()
        {
            Mock<HttpBatchHandler> handler = new Mock<HttpBatchHandler>(new HttpServer());
            handler.Setup(h => h.ProcessBatchAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("HttpResponseException Error.")
                    });
                });
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler.Object);

            var response = await invoker.SendAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("HttpResponseException Error.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SendAsync_IfProcessBatchAsyncTaskIsFaulted_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();

            Mock<IExceptionLogger> exceptionLoggerMock = CreateStubExceptionLoggerMock();
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = CreateStubExceptionHandlerMock();
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer server = CreateServer(configuration))
            using (HttpBatchHandler product = new LambdaHttpBatchHandler(server, exceptionLogger, exceptionHandler,
                (i1, i2) => CreateFaultedTask<HttpResponseMessage>(expectedException)))
            {
                CancellationToken cancellationToken = CreateCancellationToken();

                // Act
                await Assert.ThrowsAsync<Exception>(() => product.SendAsync(expectedRequest, cancellationToken));

                // Assert
                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == ExceptionCatchBlocks.HttpBatchHandler
                    && c.Request == expectedRequest;

                exceptionLoggerMock.Verify(l => l.LogAsync(
                    It.Is<ExceptionLoggerContext>(c => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());

                exceptionHandlerMock.Verify(h => h.HandleAsync(
                    It.Is<ExceptionHandlerContext>((c) => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task SendAsync_IfProcessBatchAsyncTaskIsCanceled_DoesNotCallExceptionServices()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            Mock<IExceptionLogger> exceptionLoggerMock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer server = CreateServer(configuration))
            using (HttpBatchHandler product = new LambdaHttpBatchHandler(server, exceptionLogger, exceptionHandler,
                (i1, i2) => CreateFaultedTask<HttpResponseMessage>(expectedException)))
            {
                CancellationToken cancellationToken = CreateCancellationToken();

                // Act & Assert
                await Assert.ThrowsAsync<OperationCanceledException>(() => product.SendAsync(expectedRequest, cancellationToken));
            }
        }

        [Fact]
        public async Task SendAsync_IfExceptionHandlerSetsNullResult_PropogatesFaultedTaskException()
        {
            // Arrange
            Exception expectedException = CreateExceptionWithCallStack();
            string expectedStackTrace = expectedException.StackTrace;

            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();

            Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            exceptionHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Callback<ExceptionHandlerContext, CancellationToken>((c, i) => c.Result = null)
                .Returns(Task.FromResult(0));
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer server = CreateServer(configuration))
            using (HttpBatchHandler product = new LambdaHttpBatchHandler(server, exceptionLogger, exceptionHandler,
                (i1, i2) => CreateFaultedTask<HttpResponseMessage>(expectedException)))
            {
                CancellationToken cancellationToken = CreateCancellationToken();

                // Act
                var exception = await Assert.ThrowsAsync<Exception>(() => product.SendAsync(request, cancellationToken));

                // Assert
                Assert.Same(expectedException, exception);
                Assert.NotNull(exception.StackTrace);
                Assert.StartsWith(expectedStackTrace, exception.StackTrace);
            }
        }

        [Fact]
        public async Task SendAsync_IfExceptionHandlerHandlesException_ReturnsResponse()
        {
            // Arrange
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();

            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
                exceptionHandlerMock
                    .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                    .Callback<ExceptionHandlerContext, CancellationToken>((c, i) =>
                        c.Result = new ResponseMessageResult(expectedResponse))
                    .Returns(Task.FromResult(0));
                IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

                using (HttpRequestMessage request = CreateRequest())
                using (HttpConfiguration configuration = new HttpConfiguration())
                using (HttpServer server = CreateServer(configuration))
                using (HttpBatchHandler product = new LambdaHttpBatchHandler(server, exceptionLogger, exceptionHandler,
                    (i1, i2) => CreateFaultedTask<HttpResponseMessage>(CreateException())))
                {
                    CancellationToken cancellationToken = CreateCancellationToken();

                    // Act
                    HttpResponseMessage response = await product.SendAsync(request, cancellationToken);

                    // Assert
                    Assert.Same(expectedResponse, response);
                }
            }
        }

        [Fact]
        public async Task SendAsync_WithDefaultExceptionHandler_IfProcessBatchAsyncTaskIsFaulted_ReturnsInternalServerError()
        {
            Mock<HttpBatchHandler> handler = new Mock<HttpBatchHandler>(new HttpServer());
            handler.Setup(h => h.ProcessBatchAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    throw new InvalidOperationException();
                });
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler.Object);

            var response = await invoker.SendAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private static CancellationToken CreateCancellationToken()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return source.Token;
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static IExceptionHandler CreateDummyExceptionHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IExceptionLogger CreateDummyExceptionLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private static Exception CreateException()
        {
            return new Exception();
        }

        private static Exception CreateExceptionWithCallStack()
        {
            try
            {
                throw CreateException();
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Task<TResult> CreateFaultedTask<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>();
            source.SetException(exception);
            return source.Task;
        }

        private static HttpBatchHandler CreateProductUnderTest(HttpServer httpServer)
        {
            return new MockHttpBatchHandler(httpServer);
        }

        private static HttpBatchHandler CreateProductUnderTest(HttpServer httpServer, IExceptionLogger exceptionLogger,
                IExceptionHandler exceptionHanlder)
        {
            return new MockHttpBatchHandler(httpServer, exceptionLogger, exceptionHanlder);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static HttpServer CreateServer()
        {
            return new HttpServer();
        }

        private static HttpServer CreateServer(HttpConfiguration configuration)
        {
            return new HttpServer(configuration);
        }

        private static Mock<IExceptionHandler> CreateStubExceptionHandlerMock()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private static IExceptionLogger CreateStubExceptionLogger()
        {
            return CreateStubExceptionLoggerMock().Object;
        }

        private static Mock<IExceptionLogger> CreateStubExceptionLoggerMock()
        {
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private class LambdaHttpBatchHandler : HttpBatchHandler
        {
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _processBatchAsync;

            public LambdaHttpBatchHandler(HttpServer httpServer, IExceptionLogger exceptionLogger,
                IExceptionHandler exceptionHandler,
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> processBatchAsync)
                : base(httpServer)
            {
                Contract.Assert(processBatchAsync != null);
                _processBatchAsync = processBatchAsync;
                ExceptionLogger = exceptionLogger;
                ExceptionHandler = exceptionHandler;
            }

            public override Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _processBatchAsync.Invoke(request, cancellationToken);
            }
        }

        private class MockHttpBatchHandler : HttpBatchHandler
        {
            public MockHttpBatchHandler(HttpServer server)
                : base(server)
            {
            }

            public MockHttpBatchHandler(HttpServer httpServer, IExceptionLogger exceptionLogger,
                IExceptionHandler exceptionHanlder)
                : base(httpServer)
            {
                ExceptionLogger = exceptionLogger;
                ExceptionHandler = exceptionHanlder;
            }

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                return SendAsync(request, CancellationToken.None);
            }

            public override Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage());
            }
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class ExceptionFilterResultTests
    {
        [Fact]
        public async Task ExecuteAsync_IfInnerResultTaskIsSuccessful_ReturnsSuccessTask()
        {
            // Arrange
            List<string> log = new List<string>();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            var exceptionFilter = CreateExceptionFilter((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilter };
            IExceptionLogger exceptionLogger = CreateDummyExceptionLogger();
            IExceptionHandler exceptionHandler = CreateDummyExceptionHandler();
            var expectedResponse = new HttpResponseMessage();
            var actionResult = CreateStubActionResult(Task.FromResult(expectedResponse));

            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, exceptionLogger,
                exceptionHandler, actionResult);

            // Act
            var response = await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Same(expectedResponse, response);
            Assert.Equal(new string[] { }, log.ToArray());
        }

        [Fact]
        public async Task ExecuteAsync_IfInnerResultTaskIsCanceled_ReturnsCanceledTask()
        {
            // Arrange
            List<string> log = new List<string>();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();

            // ExceptionFilters still have a chance to see the cancellation exception
            var exceptionFilter = CreateExceptionFilter((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });

            var filters = new IExceptionFilter[] { exceptionFilter };
            IExceptionLogger exceptionLogger = new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
            IExceptionHandler exceptionHandler = new Mock<IExceptionHandler>(MockBehavior.Strict).Object;

            var actionResult = CreateStubActionResult(TaskHelpers.Canceled<HttpResponseMessage>());

            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, exceptionLogger,
                exceptionHandler, actionResult);

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => product.ExecuteAsync(CancellationToken.None));

            Assert.Equal(new string[] { "exceptionFilter" }, log.ToArray());
        }

        [Fact]
        public async Task ExecuteAsync_IfInnerResultTaskIsFaulted_ExecutesFiltersAndReturnsFaultedTaskIfNotHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            Exception exceptionSeenByFilter = null;
            var exceptionFilter = CreateExceptionFilter((ec, ct) =>
            {
                exceptionSeenByFilter = ec.Exception;
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilter };
            IExceptionLogger exceptionLogger = CreateExceptionLogger((c, i) =>
            {
                log.Add("exceptionLogger");
                return Task.FromResult(0);
            });
            IExceptionHandler exceptionHandler = CreateStubExceptionHandler();
            var expectedException = new Exception();
            var actionResult = CreateStubActionResult(TaskHelpers.FromError<HttpResponseMessage>(expectedException));

            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, exceptionLogger,
                exceptionHandler, actionResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => product.ExecuteAsync(CancellationToken.None));

            Assert.Same(expectedException, exception);
            Assert.Same(expectedException, exceptionSeenByFilter);
            Assert.Equal(new string[] { "exceptionLogger", "exceptionFilter" }, log.ToArray());
        }

        [Fact]
        public async Task ExecuteAsync_IfInnerResultTaskIsFaulted_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedException = CreateException();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpActionContext expectedActionContext = CreateActionContext(request);
                IExceptionFilter[] filters = new IExceptionFilter[0];

                Mock<IExceptionLogger> exceptionLoggerMock = CreateStubExceptionLoggerMock();
                IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

                IExceptionHandler exceptionHandler = CreateStubExceptionHandler();
                IHttpActionResult innerResult = CreateStubActionResult(
                    CreateFaultedTask<HttpResponseMessage>(expectedException));

                IHttpActionResult product = CreateProductUnderTest(expectedActionContext, filters, exceptionLogger,
                    exceptionHandler, innerResult);

                CancellationToken cancellationToken = CreateCancellationToken();

                // Act
                await Assert.ThrowsAsync<InvalidOperationException>(() => product.ExecuteAsync(cancellationToken));

                // Assert
                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == ExceptionCatchBlocks.IExceptionFilter
                    && c.ActionContext == expectedActionContext;

                exceptionLoggerMock.Verify(l => l.LogAsync(
                    It.Is<ExceptionLoggerContext>(c => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task ExecuteAsync_IfInnerResultTaskIsFaulted_ExecutesFiltersAndReturnsResultIfHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            var exception = new Exception();
            HttpResponseMessage globalFilterResponse = new HttpResponseMessage();
            HttpResponseMessage actionFilterResponse = new HttpResponseMessage();
            HttpResponseMessage resultSeenByGlobalFilter = null;
            var globalFilter = CreateExceptionFilter((ec, ct) =>
            {
                log.Add("globalFilter");
                resultSeenByGlobalFilter = ec.Response;
                ec.Response = globalFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var actionFilter = CreateExceptionFilter((ec, ct) =>
            {
                log.Add("actionFilter");
                ec.Response = actionFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { globalFilter, actionFilter };
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = CreateDummyExceptionHandler();
            var actionResult = CreateStubActionResult(TaskHelpers.FromError<HttpResponseMessage>(exception));

            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, exceptionLogger,
                exceptionHandler, actionResult);

            // Act
            var response = await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Same(globalFilterResponse, response);
            Assert.Same(actionFilterResponse, resultSeenByGlobalFilter);
            Assert.Equal(new string[] { "actionFilter", "globalFilter" }, log.ToArray());
        }

        [Fact]
        public async Task ExecuteAsync_IfInnerResultTaskIsFaulted_AndNoFilterHandles_RunsHandlerAndReturnsResultIfHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            Exception expectedException = CreateException();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext expectedActionContext = CreateActionContext(request);

                IExceptionFilter filter = CreateExceptionFilter((c, i) =>
                {
                    log.Add("filter");
                    return Task.FromResult(0);
                });

                IExceptionFilter[] filters = new IExceptionFilter[] { filter };

                IExceptionLogger exceptionLogger = CreateStubExceptionLogger();

                Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>();
                exceptionHandlerMock
                    .Setup(l => l.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                    .Callback<ExceptionHandlerContext, CancellationToken>((c, i) =>
                    {
                        log.Add("handler");
                        c.Result = new ResponseMessageResult(expectedResponse);
                    })
                    .Returns(Task.FromResult(0));
                IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

                IHttpActionResult innerResult = CreateStubActionResult(
                    CreateFaultedTask<HttpResponseMessage>(expectedException));

                IHttpActionResult product = CreateProductUnderTest(expectedActionContext, filters, exceptionLogger,
                    exceptionHandler, innerResult);

                CancellationToken cancellationToken = CreateCancellationToken();

                // Act
                HttpResponseMessage response = await product.ExecuteAsync(cancellationToken);

                // Assert
                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == ExceptionCatchBlocks.IExceptionFilter
                    && c.ActionContext == expectedActionContext;

                exceptionHandlerMock.Verify(h => h.HandleAsync(
                    It.Is<ExceptionHandlerContext>((c) => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());
                Assert.Same(expectedResponse, response);
                Assert.Equal(new string[] { "filter", "handler" }, log.ToArray());
            }
        }

        [Fact]
        public async Task ExecuteAsync_IfFilterChangesException_ThrowsUpdatedException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpActionContext context = CreateActionContext(request);

                Mock<IExceptionFilter> filterMock = new Mock<IExceptionFilter>();
                filterMock
                    .Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                        It.IsAny<CancellationToken>()))
                    .Callback<HttpActionExecutedContext, CancellationToken>((c, t) =>
                    {
                        c.Exception = expectedException;
                    })
                    .Returns(() => Task.FromResult<object>(null));
                IExceptionFilter filter = filterMock.Object;
                IExceptionFilter[] filters = new IExceptionFilter[] { filter };

                IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
                IExceptionHandler exceptionHandler = CreateStubExceptionHandler();

                IHttpActionResult innerResult = CreateStubActionResult(
                    CreateFaultedTask<HttpResponseMessage>(CreateException()));

                IHttpActionResult product = CreateProductUnderTest(context, filters, exceptionLogger, exceptionHandler,
                    innerResult);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<NotImplementedException>(
                    () => product.ExecuteAsync(CancellationToken.None));
                Assert.Same(expectedException, exception);
            }
        }

        [Fact]
        public async Task ExecuteAsync_IfFaultedTaskExceptionIsUnhandled_PreservesExceptionStackTrace()
        {
            // Arrange
            Exception originalException = CreateExceptionWithStackTrace();
            string expectedStackTrace = originalException.StackTrace;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpActionContext context = CreateActionContext(request);
                IExceptionFilter[] filters = new IExceptionFilter[0];

                IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
                IExceptionHandler exceptionHandler = CreateStubExceptionHandler();

                IHttpActionResult innerResult = CreateStubActionResult(
                    CreateFaultedTask<HttpResponseMessage>(originalException));

                IHttpActionResult product = CreateProductUnderTest(context, filters, exceptionLogger, exceptionHandler,
                    innerResult);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => product.ExecuteAsync(CancellationToken.None));

                Assert.NotNull(expectedStackTrace);
                Assert.NotNull(exception);
                Assert.NotNull(exception.StackTrace);
                Assert.StartsWith(expectedStackTrace, exception.StackTrace);
            }
        }

        private static HttpActionContext CreateActionContext()
        {
            return new HttpActionContext();
        }

        private static HttpActionContext CreateActionContext(HttpRequestMessage request)
        {
            HttpActionContext actionContext = CreateActionContext();
            actionContext.ControllerContext = new HttpControllerContext()
            {
                Request = request
            };
            return actionContext;
        }

        private static CancellationToken CreateCancellationToken()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return source.Token;
        }

        private static IExceptionHandler CreateDummyExceptionHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IExceptionLogger CreateDummyExceptionLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private IExceptionFilter CreateExceptionFilter(
            Func<HttpActionExecutedContext, CancellationToken, Task> executeExceptionFilterAsync)
        {
            Mock<IExceptionFilter> mock = new Mock<IExceptionFilter>();
            mock
                .Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                    It.IsAny<CancellationToken>()))
                .Returns(executeExceptionFilterAsync);
            return mock.Object;
        }

        private static IExceptionLogger CreateExceptionLogger(
            Func<ExceptionLoggerContext, CancellationToken, Task> logAsync)
        {
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>();
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(logAsync);
            return mock.Object;
        }

        private static Exception CreateException()
        {
            return new InvalidOperationException();
        }

        private static Exception CreateExceptionWithStackTrace()
        {
            Exception exception;

            try
            {
                throw CreateException();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception;
        }

        private static Task<TResult> CreateFaultedTask<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>();
            source.SetException(exception);
            return source.Task;
        }

        private static ExceptionFilterResult CreateProductUnderTest(HttpActionContext context, IExceptionFilter[] filters,
            IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler, IHttpActionResult innerResult)
        {
            return new ExceptionFilterResult(context, filters, exceptionLogger, exceptionHandler, innerResult);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static IHttpActionResult CreateStubActionResult(Task<HttpResponseMessage> task)
        {
            Mock<IHttpActionResult> actionResultMock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            actionResultMock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(() =>
            {
                return task;
            });
            return actionResultMock.Object;
        }

        private static IExceptionHandler CreateStubExceptionHandler()
        {
            return CreateStubExceptionHandlerMock().Object;
        }

        private static Mock<IExceptionHandler> CreateStubExceptionHandlerMock()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>();
            mock
                .Setup(l => l.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<HttpResponseMessage>(null));
            return mock;
        }

        private static IExceptionLogger CreateStubExceptionLogger()
        {
            return CreateStubExceptionLoggerMock().Object;
        }

        private static Mock<IExceptionLogger> CreateStubExceptionLoggerMock()
        {
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>();
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }
    }
}

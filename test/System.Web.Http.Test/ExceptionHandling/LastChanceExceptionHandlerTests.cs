// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class LastChanceExceptionHandlerTests
    {
        [Fact]
        public void Constructor_IfInnerHandlerIsNull_Throws()
        {
            // Arrange
            IExceptionHandler innerHandler = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(innerHandler), "innerHandler");
        }

        [Fact]
        public void InnerHandler_ReturnsSpecifiedInstance()
        {
            // Arrange
            IExceptionHandler expectedInnerHandler = CreateDummyHandler();
            LastChanceExceptionHandler product = CreateProductUnderTest(expectedInnerHandler);

            // Act
            IExceptionHandler innerHandler = product.InnerHandler;

            // Assert
            Assert.Same(expectedInnerHandler, innerHandler);
        }

        [Fact]
        public async Task HandleAsync_DelegatesToInnerHandler()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            Task expectedTask = CreateCompletedTask();
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(expectedTask);
            IExceptionHandler innerHander = mock.Object;

            IExceptionHandler product = CreateProductUnderTest(innerHander);

            ExceptionHandlerContext expectedContext = CreateContext();

            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                Task task = product.HandleAsync(expectedContext, expectedCancellationToken);

                // Assert
                Assert.Same(expectedTask, task);
                await task;
                mock.Verify(h => h.HandleAsync(expectedContext, expectedCancellationToken), Times.Once());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task HandleAsync_IfIsTopLevelCatchBlockAndCanCreateExceptionResult_InitializesResult(bool includeDetail)
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            IHttpActionResult result = null;
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                {
                    result = c != null ? c.Result : null;
                    return Task.FromResult(0);
                });
            IExceptionHandler innerHander = mock.Object;

            IExceptionHandler product = CreateProductUnderTest(innerHander);

            Exception expectedException = CreateDummyException();
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                configuration.Services.Replace(typeof(IContentNegotiator), expectedContentNegotiator);
                configuration.Formatters.Clear();
                MediaTypeFormatter expectedFormatter = CreateDummyFormatter();
                configuration.Formatters.Add(expectedFormatter);

                ExceptionHandlerContext context = new ExceptionHandlerContext(new ExceptionContext(
                    exception: expectedException,
                    catchBlock: CreateTopLevelCatchBlock(),
                    request: expectedRequest)
                {
                    RequestContext = new HttpRequestContext
                                    {
                                        Configuration = configuration,
                                        IncludeErrorDetail = includeDetail
                                    },
                });

                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                ExceptionResult exceptionResult = Assert.IsType<ExceptionResult>(result);
                Assert.Same(expectedException, exceptionResult.Exception);
                Assert.Equal(includeDetail, exceptionResult.IncludeErrorDetail);
                Assert.Same(expectedContentNegotiator, exceptionResult.ContentNegotiator);
                Assert.Same(expectedRequest, exceptionResult.Request);
                Assert.NotNull(exceptionResult.Formatters);
                MediaTypeFormatter formatter = Assert.Single(exceptionResult.Formatters);
                Assert.Same(expectedFormatter, formatter);
            }
        }

        [Fact]
        public async Task HandleAsync_IfNotIsTopLevelCatchBlock_LeavesResultNull()
        {
            // Arrange
            Exception exception = CreateDummyException();

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionContext context = new ExceptionContext(
                    exception,
                    CreateNonTopLevelCatchBlock(),
                    request)
                    {
                        RequestContext = new HttpRequestContext
                                        {
                                            Configuration = configuration
                                        },
                    };

                // More Arrange; then Act & Assert
                await TestHandleAsyncLeavesResultNull(context);
            }
        }

        [Fact]
        public Task HandleAsync_IfContextIsNull_LeavesResultNull()
        {
            // Arrange
            ExceptionHandlerContext context = null;

            // More Arrange; then Act & Assert
            return TestHandleAsyncLeavesResultNull(context);
        }

        [Fact]
        public async Task HandleAsync_IfRequestIsNull_LeavesResultNull()
        {
            // Arrange
            Exception exception = CreateDummyException();

            using (HttpConfiguration configuration = CreateConfiguration())
            {
                ExceptionContext context = new ExceptionContext(
                    exception,
                    CreateTopLevelCatchBlock())
                {
                    RequestContext = new HttpRequestContext
                    {
                        Configuration = configuration
                    },
                    Request = null
                };

                // More Arrange; then Act & Assert
                await TestHandleAsyncLeavesResultNull(context);
            }
        }

        [Fact]
        public async Task HandleAsync_IfRequestContextIsNull_LeavesResultNull()
        {
            // Arrange
            Exception exception = CreateDummyException();

            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionContext context = new ExceptionContext(
                    exception,
                    CreateTopLevelCatchBlock())
                {
                    RequestContext = null,
                    Request = request
                };

                // More Arrange; then Act & Assert
                await TestHandleAsyncLeavesResultNull(context);
            }
        }

        [Fact]
        public async Task HandleAsync_IfConfigurationIsNull_LeavesResultNull()
        {
            // Arrange
            Exception exception = CreateDummyException();

            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionContext context = new ExceptionContext(
                    exception,
                    CreateTopLevelCatchBlock())
                {
                    RequestContext = new HttpRequestContext
                    {
                        Configuration = null
                    },
                    Request = request
                };

                // More Arrange; then Act & Assert
                await TestHandleAsyncLeavesResultNull(context);
            }
        }

        [Fact]
        public async Task HandleAsync_IfContentNegotiatorIsNull_LeavesResultNull()
        {
            // Arrange
            Exception exception = CreateDummyException();

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                configuration.Services.Clear(typeof(IContentNegotiator));

                ExceptionContext context = new ExceptionContext(
                    exception,
                    CreateTopLevelCatchBlock())
                {
                    RequestContext = new HttpRequestContext
                    {
                        Configuration = configuration
                    },
                    Request = request
                };

                // More Arrange; then Act & Assert
                await TestHandleAsyncLeavesResultNull(context);
            }
        }

        private static Task TestHandleAsyncLeavesResultNull(ExceptionContext context)
        {
            return TestHandleAsyncLeavesResultNull(new ExceptionHandlerContext(context));
        }

        private static async Task TestHandleAsyncLeavesResultNull(ExceptionHandlerContext context)
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            IHttpActionResult result = null;
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                {
                    result = c != null ? c.Result : null;
                    return Task.FromResult(0);
                });
            IExceptionHandler innerHander = mock.Object;

            IExceptionHandler product = CreateProductUnderTest(innerHander);

            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            await product.HandleAsync(context, cancellationToken);

            // Assert
            Assert.Null(result);
        }

        private static CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static Task CreateCompletedTask()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            source.SetResult(null);
            return source.Task;
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static ExceptionHandlerContext CreateContext()
        {
            return new ExceptionHandlerContext(CreateMinimalValidExceptionContext());
        }

        private static ExceptionContext CreateMinimalValidExceptionContext()
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer);
        }

        private static ExceptionHandlerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }

        private static IContentNegotiator CreateDummyContentNegotiator()
        {
            return new Mock<IContentNegotiator>(MockBehavior.Strict).Object;
        }

        private static Exception CreateDummyException()
        {
            return new Mock<Exception>(MockBehavior.Strict).Object;
        }

        private static MediaTypeFormatter CreateDummyFormatter()
        {
            return new Mock<MediaTypeFormatter>(MockBehavior.Strict).Object;
        }

        private static IExceptionHandler CreateDummyHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static ExceptionContextCatchBlock CreateNonTopLevelCatchBlock()
        {
            return new ExceptionContextCatchBlock("IgnoreCaughtAt", isTopLevel: false, callsHandler: false);
        }

        private static LastChanceExceptionHandler CreateProductUnderTest(IExceptionHandler innerHandler)
        {
            return new LastChanceExceptionHandler(innerHandler);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static ExceptionContextCatchBlock CreateTopLevelCatchBlock()
        {
            return new ExceptionContextCatchBlock("IgnoreCaughtAt", isTopLevel: true, callsHandler: false);
        }
    }
}

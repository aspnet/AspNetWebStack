// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Dispatcher
{
    public class HttpControllerDispatcherTest
    {
        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDispatcher(configuration: null),
                "configuration");
        }

        [Fact]
        public void ExceptionLoggerGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            IExceptionLogger expectedExceptionLogger = CreateDummyExceptionLogger();
            IExceptionHandler exceptionHandler = CreateDummyExceptionHandler();

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpControllerDispatcher product = CreateProductUnderTest(configuration, expectedExceptionLogger,
                exceptionHandler))
            {
                // Act
                IExceptionLogger exceptionLogger = product.ExceptionLogger;

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

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpControllerDispatcher product = CreateProductUnderTest(configuration, exceptionLogger,
                expectedExceptionHandler))
            {
                // Act
                IExceptionHandler exceptionHandler = product.ExceptionHandler;

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

                using (HttpControllerDispatcher product = CreateProductUnderTest(configuration))
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
        public void ExceptionHandlerGet_IfUnset_UsesExceptionHandlerFromConfiguration()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                IExceptionHandler expectedExceptionHandler = CreateDummyExceptionHandler();
                configuration.Services.Replace(typeof(IExceptionHandler), expectedExceptionHandler);

                using (HttpControllerDispatcher product = CreateProductUnderTest(configuration))
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
        public async Task SendAsync_CallsControllerSelectorToGetControllerDescriptor()
        {
            var mockSelector = new Mock<IHttpControllerSelector>();
            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var request = CreateRequest(config, "http://localhost/api/foo");
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            await invoker.SendAsync(request, CancellationToken.None);

            mockSelector.Verify(s => s.SelectController(request), Times.Once());
        }

        [Fact]
        public async Task SendAsync_CallsControllerDescriptorToCreateController()
        {
            var mockSelector = new Mock<IHttpControllerSelector>();
            var mockDescriptor = new Mock<HttpControllerDescriptor>();
            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var request = CreateRequest(config, "http://localhost/api/foo");
            mockSelector.Setup(s => s.SelectController(request))
                        .Returns(mockDescriptor.Object);
            mockDescriptor.Setup(d => d.CreateController(request))
                          .Returns(new PrivateController())
                          .Verifiable();
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            await invoker.SendAsync(request, CancellationToken.None);

            mockDescriptor.Verify();
        }

        [Fact]
        public async Task SendAsync_CallsControllerExecuteAsyncWithPopulatedControllerContext()
        {
            HttpControllerContext calledContext = null;
            var mockSelector = new Mock<IHttpControllerSelector>();
            var mockDescriptor = new Mock<HttpControllerDescriptor>();
            var mockController = new Mock<IHttpController>();
            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var request = CreateRequest(config, "http://localhost/api/foo");
            mockSelector.Setup(s => s.SelectController(request))
                        .Returns(mockDescriptor.Object);
            mockDescriptor.Setup(d => d.CreateController(request))
                          .Returns(mockController.Object);
            mockDescriptor.Object.Initialize(config);
            mockController.Setup(c => c.ExecuteAsync(It.IsAny<HttpControllerContext>(), CancellationToken.None))
                          .Callback((HttpControllerContext ctxt, CancellationToken token) => { calledContext = ctxt; });
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            await invoker.SendAsync(request, CancellationToken.None);

            Assert.NotNull(calledContext);
            Assert.Same(mockController.Object, calledContext.Controller);
            Assert.Same(mockDescriptor.Object, calledContext.ControllerDescriptor);
            Assert.Same(config, calledContext.Configuration);
            Assert.Same(request, calledContext.Request);
            Assert.Same(request.GetRouteData(), calledContext.RouteData);
        }

        [Fact]
        public async Task SendAsync_Returns404WhenControllerSelectorReturnsNullControllerDescriptor()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/foo");
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // In this case the controller selector throws, so we don't get a controller context in the
        // exception handlers.
        [Fact]
        public async Task SendAsync_IfSendAsyncThrows_InControllerSelector_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();

            Mock<IExceptionLogger> exceptionLoggerMock = CreateStubExceptionLoggerMock();
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = CreateStubExceptionHandlerMock();
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            using (HttpRequestMessage expectedRequest = CreateRequestWithRouteData())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpMessageHandler product = CreateProductUnderTest(configuration, exceptionLogger,
                exceptionHandler))
            {
                configuration.Services.Replace(typeof(IHttpControllerSelector),
                    CreateThrowingControllerSelector(expectedException));

                CancellationToken cancellationToken = CreateCancellationToken();

                // Act
                await Assert.ThrowsAsync<Exception>(() => product.SendAsync(expectedRequest, cancellationToken));

                // Assert
                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == ExceptionCatchBlocks.HttpControllerDispatcher
                    && c.Request == expectedRequest
                    && c.ControllerContext == null;

                exceptionLoggerMock.Verify(l => l.LogAsync(
                    It.Is<ExceptionLoggerContext>(c => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());

                exceptionHandlerMock.Verify(h => h.HandleAsync(
                    It.Is<ExceptionHandlerContext>((c) => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());
            }
        }

        // In this case the controller itself throws, so we get a controller context in the
        // exception handlers.
        [Fact]
        public async Task SendAsync_IfSendAsyncThrows_Controller_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();

            Mock<IExceptionLogger> exceptionLoggerMock = CreateStubExceptionLoggerMock();
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = CreateStubExceptionHandlerMock();
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            var controller = new ThrowingController(expectedException);

            var controllerActivator = new Mock<IHttpControllerActivator>();
            controllerActivator
                .Setup(
                    activator => activator.Create(
                        It.IsAny<HttpRequestMessage>(),
                        It.IsAny<HttpControllerDescriptor>(),
                        It.IsAny<Type>()))
                .Returns(controller);

            using (HttpRequestMessage expectedRequest = CreateRequestWithRouteData())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpMessageHandler product = CreateProductUnderTest(configuration, exceptionLogger,
                exceptionHandler))
            {
                var controllerSelector = new Mock<IHttpControllerSelector>(MockBehavior.Strict);
                controllerSelector
                    .Setup(selector => selector.SelectController(It.IsAny<HttpRequestMessage>()))
                    .Returns(new HttpControllerDescriptor(configuration, "Throwing", controller.GetType()));

                configuration.Services.Replace(typeof(IHttpControllerSelector), controllerSelector.Object);
                configuration.Services.Replace(typeof(IHttpControllerActivator), controllerActivator.Object);

                CancellationToken cancellationToken = CreateCancellationToken();

                // Act
                await Assert.ThrowsAsync<Exception>(() => product.SendAsync(expectedRequest, cancellationToken));

                // Assert
                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == ExceptionCatchBlocks.HttpControllerDispatcher
                    && c.Request == expectedRequest
                    && c.ControllerContext != null
                    && c.ControllerContext == controller.ControllerContext
                    && c.ControllerContext.Controller == controller;

                exceptionLoggerMock.Verify(l => l.LogAsync(
                    It.Is<ExceptionLoggerContext>(c => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());

                exceptionHandlerMock.Verify(h => h.HandleAsync(
                    It.Is<ExceptionHandlerContext>((c) => exceptionContextMatches(c.ExceptionContext)),
                    cancellationToken), Times.Once());
            }
        }


        [Fact]
        public async Task SendAsync_IfSendAsyncCancels_InControllerSelector_DoesNotCallExceptionServices()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            Mock<IExceptionLogger> exceptionLoggerMock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            using (HttpRequestMessage expectedRequest = CreateRequestWithRouteData())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpMessageHandler product = CreateProductUnderTest(configuration, exceptionLogger,
                exceptionHandler))
            {
                configuration.Services.Replace(typeof(IHttpControllerSelector),
                    CreateThrowingControllerSelector(expectedException));

                CancellationToken cancellationToken = CreateCancellationToken();

                // Act & Assert
                await Assert.ThrowsAsync<OperationCanceledException>(() => product.SendAsync(expectedRequest, cancellationToken));
            }
        }

        [Fact]
        public async Task SendAsync_IfExceptionHandlerSetsNullResult_PropogatesFaultedTaskException()
        {
            // Arrange
            ExceptionDispatchInfo exceptionInfo = CreateExceptionInfo();
            string expectedStackTrace = exceptionInfo.SourceException.StackTrace;

            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();

            Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            exceptionHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Callback<ExceptionHandlerContext, CancellationToken>((c, i) => c.Result = null)
                .Returns(Task.FromResult(0));
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            using (HttpRequestMessage request = CreateRequestWithRouteData())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpMessageHandler product = CreateProductUnderTest(configuration, exceptionLogger,
                exceptionHandler))
            {
                configuration.Services.Replace(typeof(IHttpControllerSelector),
                    CreateThrowingControllerSelector(exceptionInfo));

                CancellationToken cancellationToken = CreateCancellationToken();

                // Act & Assert
                var exception = await Assert.ThrowsAsync<Exception>(() => product.SendAsync(request, cancellationToken));

                Assert.Same(exceptionInfo.SourceException, exception);
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

                using (HttpRequestMessage request = CreateRequestWithRouteData())
                using (HttpConfiguration configuration = new HttpConfiguration())
                using (HttpMessageHandler product = CreateProductUnderTest(configuration, exceptionLogger,
                    exceptionHandler))
                {
                    configuration.Services.Replace(typeof(IHttpControllerSelector),
                        CreateThrowingControllerSelector(CreateException()));

                    CancellationToken cancellationToken = CreateCancellationToken();

                    // Act
                    HttpResponseMessage response = await product.SendAsync(request, cancellationToken);

                    // Assert
                    Assert.Same(expectedResponse, response);
                }
            }
        }

        [Fact]
        public async Task SendAsync_IfExceptionHandlerIsDefault_Returns500WithHttpErrorWhenControllerThrows()
        {
            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            var request = CreateRequest(config, "http://localhost/api/HttpControllerDispatcherThrowing");
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var objectContent = Assert.IsType<ObjectContent<HttpError>>(response.Content);
            var error = Assert.IsType<HttpError>(objectContent.Value);
            Assert.Equal("Hello from the throwing controller", error["ExceptionMessage"]);
        }

        [Fact]
        public async Task SendAsync_CreatesControllerContext_WithRequestContextFromRequest()
        {
            // Arrange
            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpControllerDispatcher dispatcher = new HttpControllerDispatcher(configuration))
            using (HttpMessageInvoker invoker = new HttpMessageInvoker(dispatcher))
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                Mock<IHttpController> controllerMock = new Mock<IHttpController>();
                HttpRequestContext requestContext = null;
                controllerMock
                    .Setup(c => c.ExecuteAsync(It.IsAny<HttpControllerContext>(), CancellationToken.None))
                    .Callback<HttpControllerContext, CancellationToken>((c, t) =>
                    {
                        requestContext = c.RequestContext;
                    });
                Mock<HttpControllerDescriptor> controllerDescriptorMock = new Mock<HttpControllerDescriptor>();
                controllerDescriptorMock.Setup(d => d.CreateController(request)).Returns(controllerMock.Object);
                HttpControllerDescriptor controllerDescriptor = controllerDescriptorMock.Object;
                controllerDescriptor.Configuration = configuration;
                Mock<IHttpControllerSelector> controllerSelectorMock = new Mock<IHttpControllerSelector>();
                controllerSelectorMock.Setup(s => s.SelectController(request)).Returns(controllerDescriptor);
                configuration.Services.Replace(typeof(IHttpControllerSelector), controllerSelectorMock.Object);

                HttpRequestContext expectedRequestContext = new HttpRequestContext
                {
                    Configuration = configuration
                };
                request.SetRequestContext(expectedRequestContext);

                request.SetRouteData(new Mock<IHttpRouteData>(MockBehavior.Strict).Object);

                // Act
                HttpResponseMessage ignore = await invoker.SendAsync(request, CancellationToken.None);

                // Assert
                Assert.Same(expectedRequestContext, requestContext);
            }
        }

        [Fact]
        public async Task SendAsync_CreatesControllerContextWithRequestBackedRequestContext_WhenRequestRequestContextIsNull()
        {
            // Arrange
            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpControllerDispatcher dispatcher = new HttpControllerDispatcher(configuration))
            using (HttpMessageInvoker invoker = new HttpMessageInvoker(dispatcher))
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                Mock<IHttpController> controllerMock = new Mock<IHttpController>();
                HttpRequestContext requestContext = null;
                controllerMock
                    .Setup(c => c.ExecuteAsync(It.IsAny<HttpControllerContext>(), CancellationToken.None))
                    .Callback<HttpControllerContext, CancellationToken>((c, t) =>
                    {
                        requestContext = c.RequestContext;
                    });
                Mock<HttpControllerDescriptor> controllerDescriptorMock = new Mock<HttpControllerDescriptor>();
                controllerDescriptorMock.Setup(d => d.CreateController(request)).Returns(controllerMock.Object);
                HttpControllerDescriptor controllerDescriptor = controllerDescriptorMock.Object;
                controllerDescriptor.Configuration = configuration;
                Mock<IHttpControllerSelector> controllerSelectorMock = new Mock<IHttpControllerSelector>();
                controllerSelectorMock.Setup(s => s.SelectController(request)).Returns(controllerDescriptor);
                configuration.Services.Replace(typeof(IHttpControllerSelector), controllerSelectorMock.Object);

                request.SetRouteData(new Mock<IHttpRouteData>(MockBehavior.Strict).Object);

                // Act
                HttpResponseMessage ignore = await invoker.SendAsync(request, CancellationToken.None);

                // Assert
                RequestBackedHttpRequestContext typedRequestContext = Assert.IsType<RequestBackedHttpRequestContext>(requestContext);
                Assert.Same(request, typedRequestContext.Request);
                Assert.Same(configuration, typedRequestContext.Configuration);
            }
        }

        [Fact]
        public async Task SendAsync_SetsRequestBackedRequestContextOnRequest_WhenRequestRequestContextIsNull()
        {
            // Arrange
            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpControllerDispatcher dispatcher = new HttpControllerDispatcher(configuration))
            using (HttpMessageInvoker invoker = new HttpMessageInvoker(dispatcher))
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                Mock<IHttpController> controllerMock = new Mock<IHttpController>();
                HttpRequestContext requestContext = null;
                controllerMock
                    .Setup(c => c.ExecuteAsync(It.IsAny<HttpControllerContext>(), CancellationToken.None))
                    .Callback<HttpControllerContext, CancellationToken>((c, t) =>
                    {
                        requestContext = request.GetRequestContext();
                    });
                Mock<HttpControllerDescriptor> controllerDescriptorMock = new Mock<HttpControllerDescriptor>();
                controllerDescriptorMock.Setup(d => d.CreateController(request)).Returns(controllerMock.Object);
                HttpControllerDescriptor controllerDescriptor = controllerDescriptorMock.Object;
                controllerDescriptor.Configuration = configuration;
                Mock<IHttpControllerSelector> controllerSelectorMock = new Mock<IHttpControllerSelector>();
                controllerSelectorMock.Setup(s => s.SelectController(request)).Returns(controllerDescriptor);
                configuration.Services.Replace(typeof(IHttpControllerSelector), controllerSelectorMock.Object);

                request.SetRouteData(new Mock<IHttpRouteData>(MockBehavior.Strict).Object);

                // Act
                HttpResponseMessage ignore = await invoker.SendAsync(request, CancellationToken.None);

                // Assert
                RequestBackedHttpRequestContext typedRequestContext = Assert.IsType<RequestBackedHttpRequestContext>(requestContext);
                Assert.Same(request, typedRequestContext.Request);
                Assert.Same(configuration, typedRequestContext.Configuration);
            }
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

        private static ExceptionDispatchInfo CreateExceptionInfo()
        {
            try
            {
                throw CreateException();
            }
            catch (Exception exception)
            {
                return ExceptionDispatchInfo.Capture(exception);
            }
        }

        private static HttpControllerDispatcher CreateProductUnderTest(HttpConfiguration configuration)
        {
            return new HttpControllerDispatcher(configuration);
        }

        private static HttpControllerDispatcher CreateProductUnderTest(HttpConfiguration configuration,
            IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler)
        {
            return new HttpControllerDispatcher(configuration)
            {
                ExceptionLogger = exceptionLogger,
                ExceptionHandler = exceptionHandler
            };
        }

        private static HttpRequestMessage CreateRequestWithRouteData()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRouteData(new Mock<IHttpRouteData>(MockBehavior.Strict).Object);
            return request;
        }

        private static HttpRequestMessage CreateRequest(HttpConfiguration config, string requestUri)
        {
            IHttpRoute route = config.Routes.MapHttpRoute("default", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.SetRouteData(route.GetRouteData("/", request));
            request.SetConfiguration(config);
            return request;
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
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

        private static IHttpControllerSelector CreateThrowingControllerSelector(Exception exception)
        {
            Mock<IHttpControllerSelector> mock = new Mock<IHttpControllerSelector>(MockBehavior.Strict);
            mock
                .Setup(s => s.SelectController(It.IsAny<HttpRequestMessage>()))
                .Throws(exception);
            return mock.Object;
        }

        private static IHttpControllerSelector CreateThrowingControllerSelector(ExceptionDispatchInfo exceptionInfo)
        {
            return new ThrowingControllerSelector(exceptionInfo);
        }

        private class PrivateController : ApiController
        {
            public void Get() { }

            public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
            {
                // Empty. Skip all the logic of execcuting a controller.
                HttpResponseMessage response = new HttpResponseMessage();
                return Task.FromResult(response);
            }
        }

        private class ThrowingControllerSelector : IHttpControllerSelector
        {
            private readonly ExceptionDispatchInfo _exceptionInfo;

            public ThrowingControllerSelector(ExceptionDispatchInfo exceptionInfo)
            {
                Contract.Assert(exceptionInfo != null);
                _exceptionInfo = exceptionInfo;
            }

            public HttpControllerDescriptor SelectController(HttpRequestMessage request)
            {
                _exceptionInfo.Throw();
                return null; // We'll never get here, but the compiler doesn't know that.
            }

            public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
            {
                _exceptionInfo.Throw();
                return null; // We'll never get here, but the compiler doesn't know that.
            }
        }
    }

    public class ThrowingController : ApiController
    {
        private readonly Exception _exception;

        public ThrowingController(Exception exception)
        {
            _exception = exception;
        }

        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            ControllerContext = controllerContext;
            throw _exception;
        }
    }

    // This is used in SendAsync_IfExceptionHandlerIsDefault_Returns500WithHttpErrorWhenControllerThrows
    // Don't touch!
    public class HttpControllerDispatcherThrowingController : ApiController
    {
        public void Get()
        {
            throw new Exception("Hello from the throwing controller");
        }
    }
}

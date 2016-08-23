// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class ApiControllerActionInvokerTest
    {
        private readonly HttpActionContext _actionContext;
        private readonly Mock<HttpActionDescriptor> _actionDescriptorMock = new Mock<HttpActionDescriptor>();
        private readonly ApiControllerActionInvoker _actionInvoker = new ApiControllerActionInvoker();
        private readonly HttpControllerContext _controllerContext;
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly Mock<IActionResultConverter> _converterMock = new Mock<IActionResultConverter>();

        public ApiControllerActionInvokerTest()
        {
            _controllerContext = new HttpControllerContext()
            {
                Request = _request
            };
            _actionContext = new HttpActionContext(_controllerContext, _actionDescriptorMock.Object);
            _actionDescriptorMock.Setup(ad => ad.ResultConverter).Returns(_converterMock.Object);
        }

        [Fact]
        public void InvokeActionAsync_Throws_IfContextIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => _actionInvoker.InvokeActionAsync(null, CancellationToken.None),
                "actionContext");
        }

        [Fact]
        public async Task InvokeActionAsync_InvokesActionDescriptorExecuteAsync()
        {
            var cts = new CancellationTokenSource();
            _actionDescriptorMock.Setup(
                ad => ad.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((object)null));

            await _actionInvoker.InvokeActionAsync(_actionContext, cts.Token);

            _actionDescriptorMock.Verify(ad => ad.ExecuteAsync(_actionContext.ControllerContext, _actionContext.ActionArguments, cts.Token), Times.Once());
        }

        [Fact]
        public async Task InvokeActionAsync_PassesExecutionResultToConfiguredConverter()
        {
            var value = new object();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(_actionContext.ControllerContext, _actionContext.ActionArguments, CancellationToken.None))
                .Returns(Task.FromResult(value));

            await _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            _converterMock.Verify(c => c.Convert(_actionContext.ControllerContext, value), Times.Once());
        }

        [Fact]
        public async Task InvokeActionAsync_ReturnsResponseFromConverter()
        {
            var expectedResponse = new HttpResponseMessage();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(_actionContext.ControllerContext, _actionContext.ActionArguments, CancellationToken.None))
                .Returns(Task.FromResult(new object()));
            _converterMock.Setup(c => c.Convert(_actionContext.ControllerContext, It.IsAny<object>()))
                .Returns(expectedResponse);

            var response = await _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task InvokeActionAsync_WhenExecuteThrowsHttpResponseException_ReturnsResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws(new HttpResponseException(response));

            var result = await _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            Assert.Same(response, result);
            Assert.Same(_request, result.RequestMessage);
        }

        [Fact]
        public async Task InvokeActionAsync_WhenExecuteThrows_ReturnsFaultedTask()
        {
            Exception expectedException = new Exception();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task InvokeActionAsync_ForActionResult_ReturnsResultResponse()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();

            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            {
                IHttpActionResult result = CreateStubResult(expectedResponse);
                _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
                _actionDescriptorMock.Setup(d => d.ExecuteAsync(_actionContext.ControllerContext,
                    _actionContext.ActionArguments, cancellationToken)).Returns(Task.FromResult((object)result));

                // Act
                HttpResponseMessage response = await _actionInvoker.InvokeActionAsync(_actionContext, cancellationToken);

                // Assert
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public async Task InvokeActionAsync_ForActionResult_AddsRequestMessage_WhenMissing()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            {
                _controllerContext.Request = expectedRequest;

                using (HttpResponseMessage responseToReturn = new HttpResponseMessage())
                {
                    IHttpActionResult result = CreateStubResult(responseToReturn);
                    _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
                    _actionDescriptorMock.Setup(d => d.ExecuteAsync(It.IsAny<HttpControllerContext>(),
                        It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(
                        Task.FromResult((object)result));

                    // Act
                    HttpResponseMessage response = await _actionInvoker.InvokeActionAsync(_actionContext, cancellationToken);

                    // Assert
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task InvokeActionAsync_ForActionResult_LeavesRequestMessage_WhenPresent()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            {
                using (HttpRequestMessage otherRequest = new HttpRequestMessage())
                {
                    _controllerContext.Request = otherRequest;

                    using (HttpResponseMessage responseToReturn = CreateResponse(expectedRequest))
                    {
                        IHttpActionResult result = CreateStubResult(responseToReturn);
                        _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
                        _actionDescriptorMock.Setup(d => d.ExecuteAsync(It.IsAny<HttpControllerContext>(),
                            It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(
                            Task.FromResult((object)result));

                        // Act
                        HttpResponseMessage response = await _actionInvoker.InvokeActionAsync(_actionContext, cancellationToken);

                        // Assert
                        Assert.Same(expectedRequest, response.RequestMessage);
                    }
                }
            }
        }

        [Fact]
        public async Task InvokeActionAsync_ForActionResult_HandlesHttpResponseException()
        {
            // Arrange
            _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
            HttpResponseException expectedException = new HttpResponseException(HttpStatusCode.Ambiguous);
            _actionDescriptorMock.Setup(d => d.ExecuteAsync(It.IsAny<HttpControllerContext>(),
                It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Throws(expectedException);

            // Act
            HttpResponseMessage response = await _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.Ambiguous, response.StatusCode);
        }

        [Fact]
        public Task InvokeActionAsync_ForActionResult_Throws_WhenResultIsNull()
        {
            // Arrange
            _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
            _actionDescriptorMock.Setup(d => d.ExecuteAsync(It.IsAny<HttpControllerContext>(),
                It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(
                Task.FromResult<object>(null));

            // Act
            Task<HttpResponseMessage> task = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => task,
                "A null value was returned where an instance of IHttpActionResult was expected.");
        }

        [Fact]
        public Task InvokeActionAsync_ForActionResult_Throws_WhenResponseIsNull()
        {
            // Arrange
            _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
            IHttpActionResult emptyResult = CreateStubResult(null);
            _actionDescriptorMock.Setup(d => d.ExecuteAsync(It.IsAny<HttpControllerContext>(),
                It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(
                Task.FromResult((object)emptyResult));

            // Act
            Task<HttpResponseMessage> task = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => task,
                "A null value was returned where an instance of HttpResponseMessage was expected.");
        }

        [Fact]
        public Task InvokeActionAsync_ForActionResult_Throws_WhenResultIsNotActionResult()
        {
            // Arrange
            _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(IHttpActionResult));
            _actionDescriptorMock.Setup(d => d.ExecuteAsync(It.IsAny<HttpControllerContext>(),
                It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
                new object()));

            // Act
            Task<HttpResponseMessage> task = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => task,
                "An object of type 'System.Object' was returned where an instance of IHttpActionResult was expected.");
        }

        [Fact]
        public async Task InvokeActionAsync_DeclaredObject_ReturnsWidget()
        {
            using (var expectedResponse = new HttpResponseMessage())
            {
                var result = new Widget() { Name = "CoolWidget" };

                _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(object));
                _actionDescriptorMock.Setup(d => d.ExecuteAsync(
                    It.IsAny<HttpControllerContext>(),
                    It.IsAny<IDictionary<string, object>>(),
                    It.IsAny<CancellationToken>())).Returns(Task.FromResult((object)result));

                _converterMock.Setup(c => c.Convert(It.IsAny<HttpControllerContext>(), result)).Returns(expectedResponse);

                // Act
                HttpResponseMessage response = await _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

                // Assert
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public async Task InvokeActionAsync_WrongReturnType_Throws()
        {
            using (var expectedResponse = new HttpResponseMessage())
            {
                var result = new Mock<IHttpActionResult>().Object;

                _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(Widget));
                _actionDescriptorMock.SetupGet(d => d.ResultConverter).Returns(new ValueResultConverter<Widget>());
                _actionDescriptorMock.Setup(d => d.ExecuteAsync(
                    It.IsAny<HttpControllerContext>(),
                    It.IsAny<IDictionary<string, object>>(),
                    It.IsAny<CancellationToken>())).Returns(Task.FromResult((object)result));

                // Act & Assert
                await Assert.ThrowsAsync<InvalidCastException>(() => _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None));
            }
        }

        [Fact]
        public async Task InvokeActionAsync_DeclaredObject_ReturnsActionResult()
        {
            // Arrange
            using (var expectedResponse = new HttpResponseMessage())
            {
                var mockActionResult = new Mock<IHttpActionResult>();
                mockActionResult
                    .Setup(m => m.ExecuteAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(expectedResponse));

                _actionDescriptorMock.Setup(d => d.ReturnType).Returns(typeof(object));
                _actionDescriptorMock.Setup(d => d.ExecuteAsync(
                    It.IsAny<HttpControllerContext>(),
                    It.IsAny<IDictionary<string, object>>(),
                    It.IsAny<CancellationToken>())).Returns(Task.FromResult((object)mockActionResult.Object));

                // Act
                HttpResponseMessage response = await _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

                // Assert
                Assert.Same(expectedResponse, response);
            }
        }

        private static HttpResponseMessage CreateResponse(HttpRequestMessage request)
        {
            return new HttpResponseMessage
            {
                RequestMessage = request
            };
        }

        private static IHttpActionResult CreateStubResult(HttpResponseMessage response)
        {
            Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>();
            mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            return mock.Object;
        }

        private class Widget
        {
            public string Name
            {
                get;
                set;
            }
        }
    }
}

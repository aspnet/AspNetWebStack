// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.Dispatcher
{
    public class HttpRoutingDispatcherTest
    {
        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpRoutingDispatcher(configuration: null),
                "configuration");
            Assert.ThrowsArgumentNull(
                () => new HttpRoutingDispatcher(configuration: null, defaultHandler: new Mock<HttpMessageHandler>().Object),
                "configuration");
            Assert.ThrowsArgumentNull(
                () => new HttpRoutingDispatcher(new HttpConfiguration(), defaultHandler: null),
                "defaultHandler");
        }

        [Fact]
        public async Task SendAsync_PopulatesRouteDataWhenNotPresentInRequest()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName/42");
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            await invoker.SendAsync(request, CancellationToken.None);

            var routeData = request.GetRouteData();
            Assert.NotNull(routeData);
            Assert.Same(config.Routes.Single(), routeData.Route);
            Assert.Equal("controllerName", routeData.Values["controller"]);
            Assert.Equal("42", routeData.Values["id"]);
        }

        [Fact]
        public async Task SendAsync_RemovesRouteParameterOptionalValuesThatAreNotPresent()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName");
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            await invoker.SendAsync(request, CancellationToken.None);

            Assert.False(request.GetRouteData().Values.ContainsKey("id"));
        }

        [Fact]
        public async Task SendAsync_Returns404WhenNoMatchingRoute()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/noMatch");
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            var response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.True((bool)request.Properties[HttpPropertyKeys.NoRouteMatched]);
        }

        [Fact]
        public void SendAsync_CallsDefaultHandlerWhenRouteHandlerIsNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName", routeHandler: null);
            var dispatcher = new HttpRoutingDispatcher(config, defaultHandler: mockHandler.Object);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None);

            mockHandler.Protected().Verify("SendAsync", Times.Once(), request, CancellationToken.None);
        }

        [Fact]
        public void SendAsync_CallsRouterHandlerWhenRouteHandlerIsNotNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName", routeHandler: mockHandler.Object);
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None);

            mockHandler.Protected().Verify("SendAsync", Times.Once(), request, CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_UsesRouteDataFromRequestContext()
        {
            // Arrange
            Mock<HttpMessageHandler> doNotUseDefaultHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            doNotUseDefaultHandlerMock.As<IDisposable>().Setup(c => c.Dispose());

            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpMessageHandler doNoUseDefaultHandler = doNotUseDefaultHandlerMock.Object)
            using (HttpRoutingDispatcher dispatcher = new HttpRoutingDispatcher(configuration, doNoUseDefaultHandler))
            using (HttpMessageInvoker invoker = new HttpMessageInvoker(dispatcher))
            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            {
                SpyHttpMessageHandler routeHandler = new SpyHttpMessageHandler(expectedResponse);

                Mock<IHttpRoute> routeMock = new Mock<IHttpRoute>(MockBehavior.Strict);
                routeMock.Setup(r => r.Handler).Returns(routeHandler);

                Mock<IHttpRouteData> routeDataMock = new Mock<IHttpRouteData>(MockBehavior.Strict);
                routeDataMock.Setup(d => d.Route).Returns(routeMock.Object);
                routeDataMock.Setup(d => d.Values).Returns(new Dictionary<string, object>());

                HttpRequestContext context = new HttpRequestContext();
                context.RouteData = routeDataMock.Object;
                expectedRequest.SetRequestContext(context);

                // Act
                HttpResponseMessage response = await invoker.SendAsync(expectedRequest, CancellationToken.None);

                // Assert
                Assert.Same(expectedResponse, response);
                Assert.Same(expectedRequest, routeHandler.Request);
            }
        }

        [Fact]
        public async Task SendAsync_IgnoreRoute_UsesRouteDataWithStopRoutingHandlerFromRequestContext()
        {
            // Arrange
            Mock<HttpMessageHandler> doNotUseDefaultHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            doNotUseDefaultHandlerMock.As<IDisposable>().Setup(c => c.Dispose());

            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpMessageHandler doNoUseDefaultHandler = doNotUseDefaultHandlerMock.Object)
            using (HttpRoutingDispatcher dispatcher = new HttpRoutingDispatcher(configuration, doNoUseDefaultHandler))
            using (HttpMessageInvoker invoker = new HttpMessageInvoker(dispatcher))
            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            {
                HttpMessageHandler routeHandler = new StopRoutingHandler();

                Mock<IHttpRoute> routeMock = new Mock<IHttpRoute>(MockBehavior.Strict);
                routeMock.Setup(r => r.Handler).Returns(routeHandler);

                Mock<IHttpRouteData> routeDataMock = new Mock<IHttpRouteData>(MockBehavior.Strict);
                routeDataMock.Setup(d => d.Route).Returns(routeMock.Object);
                routeDataMock.Setup(d => d.Values).Returns(new Dictionary<string, object>());

                HttpRequestContext context = new HttpRequestContext();
                context.RouteData = routeDataMock.Object;
                expectedRequest.SetRequestContext(context);

                // Act
                HttpResponseMessage response = await invoker.SendAsync(expectedRequest, CancellationToken.None);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.True(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
            }
        }

        private static HttpRequestMessage CreateRequest(HttpConfiguration config, string requestUri)
        {
            return CreateRequest(config, requestUri, routeHandler: new EmptyResponseHandler());
        }

        private static HttpRequestMessage CreateRequest(HttpConfiguration config, string requestUri, HttpMessageHandler routeHandler)
        {
            var route = config.Routes.CreateRoute("api/{controller}/{id}",
                                                  defaults: new Dictionary<string, object> { { "id", RouteParameter.Optional } },
                                                  constraints: null,
                                                  dataTokens: null,
                                                  handler: routeHandler);
            config.Routes.Add("default", route);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.SetConfiguration(config);
            return request;
        }

        private class EmptyResponseHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(request.CreateResponse(HttpStatusCode.OK));
            }
        }

        private class SpyHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public HttpRequestMessage Request { get; private set; }
            public CancellationToken CancellationToken { get; private set; }

            public SpyHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                CancellationToken = cancellationToken;
                return Task.FromResult(_response);
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;

namespace System.Web.Http.Cors
{
    public class CorsMessageHandlerTest
    {
        [Fact]
        public void Constructor_NullConfig_Throws()
        {
            Assert.ThrowsArgumentNull(() => new CorsMessageHandler(null), "httpConfiguration");
        }

        [Fact]
        public async Task SendAsync_DoesNotAddHeaders_WhenOriginIsMissing()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            HttpServer server = new HttpServer(config);
            CorsMessageHandler corsHandler = new CorsMessageHandler(config);
            corsHandler.InnerHandler = server;
            HttpMessageInvoker invoker = new HttpMessageInvoker(corsHandler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/sample");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Empty(response.Headers);
        }

        [Theory]
        [InlineData("GET", "*")]
        [InlineData("DELETE", "*")]
        [InlineData("HEAD", "http://example.com")]
        public async Task SendAsync_ReturnsAllowAOrigin(string httpMethod, string expectedOrigin)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            HttpServer server = new HttpServer(config);
            CorsMessageHandler corsHandler = new CorsMessageHandler(config);
            corsHandler.InnerHandler = server;
            HttpMessageInvoker invoker = new HttpMessageInvoker(corsHandler);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/sample");
            request.Headers.Add(CorsConstants.Origin, "http://example.com");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);
            string origin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();

            Assert.Equal(expectedOrigin, origin);
        }

        [Theory]
        [InlineData("DELETE", "*", "foo,bar")]
        [InlineData("PUT", "http://localhost", "content-type,custom")]
        public async Task SendAsync_Preflight_ReturnsAllowMethodsAndAllowHeaders(string requestedMethod, string expectedOrigin, string requestedHeaders)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            HttpServer server = new HttpServer(config);
            CorsMessageHandler corsHandler = new CorsMessageHandler(config);
            corsHandler.InnerHandler = server;
            HttpMessageInvoker invoker = new HttpMessageInvoker(corsHandler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/sample");
            request.SetConfiguration(config);
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, requestedMethod);
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, requestedHeaders);

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);
            string origin = response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).FirstOrDefault();
            string allowMethod = response.Headers.GetValues(CorsConstants.AccessControlAllowMethods).FirstOrDefault();
            string[] allowHeaders = response.Headers.GetValues(CorsConstants.AccessControlAllowHeaders).FirstOrDefault().Split(',');
            string[] requestedHeaderArray = requestedHeaders.Split(',');

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOrigin, origin);
            Assert.Equal(requestedMethod, allowMethod);
            foreach (string requestedHeader in requestedHeaderArray)
            {
                Assert.Contains(requestedHeader, allowHeaders);
            }
        }

        [Fact]
        public async Task SendAsync_Preflight_ReturnsOriginalResponse_WhenNoCorsPolicyProviderIsFound()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            config.MessageHandlers.Add(new CorsMessageHandler(config));
            HttpServer server = new HttpServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/default");
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "GET");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_Preflight_ReturnsBadRequest_WhenOriginIsNotAllowed()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            config.MessageHandlers.Add(new CorsMessageHandler(config));
            HttpServer server = new HttpServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/default");
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "POST");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("{\"Message\":\"The origin 'http://localhost' is not allowed.\"}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SendAsync_Preflight_ReturnsSoftNotFound_WhenNoRouteMatches()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "foo");
            config.MessageHandlers.Add(new CorsMessageHandler(config));
            HttpServer server = new HttpServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/nomatch");
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "POST");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            bool noRouteMatched = Assert.IsType<bool>(request.Properties[HttpPropertyKeys.NoRouteMatched]);
            Assert.True(noRouteMatched);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("{\"Message\":\"No HTTP resource was found that matches the request URI 'http://localhost/nomatch'.\"}", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SendAsync_Preflight_ReturnsSoftNotFound_WhenControllerSelectionFails()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            config.MessageHandlers.Add(new CorsMessageHandler(config));
            HttpServer server = new HttpServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/nomatch");
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "POST");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_HandlesExceptions_ThrownDuringPreflight()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            HttpServer server = new HttpServer(config);
            CorsMessageHandler corsHandler = new CorsMessageHandler(config);
            corsHandler.InnerHandler = server;
            HttpMessageInvoker invoker = new HttpMessageInvoker(corsHandler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/sample");
            request.SetConfiguration(config);
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "RandomMethod");

            HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_Preflight_RethrowsExceptions_WhenRethrowFlagIsTrue()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            HttpServer server = new HttpServer(config);
            CorsMessageHandler corsHandler = new CorsMessageHandler(config, true);
            corsHandler.InnerHandler = server;
            HttpMessageInvoker invoker = new HttpMessageInvoker(corsHandler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/sample");
            request.SetConfiguration(config);
            request.Headers.Add(CorsConstants.Origin, "http://localhost");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "RandomMethod");

            await Assert.ThrowsAsync<HttpResponseException>(() => invoker.SendAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task SendAsync_RethrowsExceptions_WhenRethrowFlagIsTrue()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            config.Services.Replace(typeof(IExceptionHandler), new PassthroughExceptionHandler());
            HttpServer server = new HttpServer(config);
            CorsMessageHandler corsHandler = new CorsMessageHandler(config, true);
            corsHandler.InnerHandler = server;
            HttpMessageInvoker invoker = new HttpMessageInvoker(corsHandler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/throwing");
            request.SetConfiguration(config);
            request.Headers.Add(CorsConstants.Origin, "http://localhost");

            await Assert.ThrowsAsync<Exception>(() => invoker.SendAsync(request, CancellationToken.None));
        }

        [Fact]
        public Task HandleCorsRequestAsync_NullConfig_Throws()
        {
            CorsMessageHandler corsHandler = new CorsMessageHandler(new HttpConfiguration());
            return Assert.ThrowsArgumentNullAsync(
                () => corsHandler.HandleCorsRequestAsync(null, new CorsRequestContext(), CancellationToken.None),
                "request");
        }

        [Fact]
        public Task HandleCorsRequestAsync_NullContext_Throws()
        {
            CorsMessageHandler corsHandler = new CorsMessageHandler(new HttpConfiguration());
            return Assert.ThrowsArgumentNullAsync(
                () => corsHandler.HandleCorsRequestAsync(new HttpRequestMessage(), null, CancellationToken.None),
                "corsRequestContext");
        }

        [Fact]
        public async Task HandleCorsPreflightRequestAsync_ReturnsBadRequestWhenAccessControlRequestMethodIsInvalid()
        {
            CorsMessageHandler corsHandler = new CorsMessageHandler(new HttpConfiguration());
            HttpResponseMessage response = await corsHandler.HandleCorsPreflightRequestAsync(new HttpRequestMessage(),
                new CorsRequestContext
                {
                    AccessControlRequestMethod = "Get-http://localhost"
                },
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The \"Access-Control-Request-Method\" header value 'Get-http://localhost' is invalid.", (await response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task HandleCorsPreflightRequestAsync_ReturnsBadRequestWhenAccessControlRequestMethodIsNullOrEmpty(string accessControlRequestMethod)
        {
            CorsMessageHandler corsHandler = new CorsMessageHandler(new HttpConfiguration());
            HttpResponseMessage response = await corsHandler.HandleCorsPreflightRequestAsync(new HttpRequestMessage(),
                new CorsRequestContext
                {
                    AccessControlRequestMethod = accessControlRequestMethod
                },
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The \"Access-Control-Request-Method\" header value cannot be null or empty.", (await response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Fact]
        public Task HandleCorsPreflightRequestAsync_NullContext_Throws()
        {
            CorsMessageHandler corsHandler = new CorsMessageHandler(new HttpConfiguration());
            return Assert.ThrowsArgumentNullAsync(
                () => corsHandler.HandleCorsPreflightRequestAsync(new HttpRequestMessage(), null, CancellationToken.None),
                "corsRequestContext");
        }

        private class PassthroughExceptionHandler : IExceptionHandler
        {
            public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
            {
                throw context.Exception;
            }
        }
    }
}
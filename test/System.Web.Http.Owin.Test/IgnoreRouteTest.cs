// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Owin.ExceptionHandling;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class IgnoreRouteIntegrationTests
    {
        [Fact]
        public void Invoke_IfRouteIsIgnored_CallsNextMiddleware()
        {
            // Arrange
            int expectedStatusCode = 123;
            string pathToIgnoreRoute = "ignore";
            OwinMiddleware next = CreateStubMiddleware(expectedStatusCode);

            using (HttpServer server = new HttpServer())
            {
                server.Configuration.Routes.IgnoreRoute("IgnoreRouteName", pathToIgnoreRoute);
                server.Configuration.MapHttpAttributeRoutes(); // See IgnoreController

                OwinMiddleware product = CreateProductUnderTest(next, server);

                IOwinRequest request = CreateStubRequest(new Uri("http://somehost/" + pathToIgnoreRoute));

                Mock<IOwinResponse> mock = CreateStubResponseMock();
                int statusCode = 0;
                mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((s) => statusCode = s);
                IOwinResponse response = mock.Object;

                IOwinContext context = CreateStubContext(request, response);

                // Act
                Task task = product.Invoke(context);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();
                task.ThrowIfFaulted();

                Assert.Equal(expectedStatusCode, statusCode);
            }
        }

        [Fact]
        public void Invoke_IfRouteIsIgnored_WithConstraints_CallsNextMiddleware()
        {
            // Arrange
            int expectedStatusCode = 0;
            string pathToIgnoreRoute = "constraint/10";

            using (HttpServer server = new HttpServer())
            {
                server.Configuration.Routes.IgnoreRoute("Constraints", "constraint/{id}", constraints: new { constraint = new CustomConstraint() });
                server.Configuration.MapHttpAttributeRoutes(); // See IgnoreController

                OwinMiddleware product = CreateProductUnderTest(null, server);

                IOwinRequest request = CreateStubRequest(new Uri("http://somehost/" + pathToIgnoreRoute));

                Mock<IOwinResponse> mock = CreateStubResponseMock();
                int statusCode = 0;
                mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((s) => statusCode = s);
                IOwinResponse response = mock.Object;

                IOwinContext context = CreateStubContext(request, response);

                // Act
                Task task = product.Invoke(context);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();
                task.ThrowIfFaulted();

                Assert.Equal(expectedStatusCode, statusCode);
            }
        }

        private static IOwinContext CreateStubContext(IOwinRequest request, IOwinResponse response)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.SetupGet(c => c.Request).Returns(request);
            mock.SetupGet(c => c.Response).Returns(response);
            mock.Setup(c => c.Get<bool>("server.IsLocal")).Returns(true);
            return mock.Object;
        }

        private static OwinMiddleware CreateStubMiddleware(int statusCode)
        {
            Mock<OwinMiddleware> mock = new Mock<OwinMiddleware>(MockBehavior.Strict, null);
            mock
                .Setup(m => m.Invoke(It.IsAny<IOwinContext>()))
                .Callback<IOwinContext>((c) => c.Response.StatusCode = statusCode)
                .Returns(Task.FromResult(0));
            return mock.Object;
        }

        private static IOwinRequest CreateStubRequest(Uri uri)
        {
            Mock<IOwinRequest> mock = new Mock<IOwinRequest>(MockBehavior.Strict);
            mock.SetupGet(r => r.CallCancelled).Returns(CancellationToken.None);
            mock.SetupGet(r => r.Environment).Returns((IDictionary<string, object>)null);
            mock.SetupGet(r => r.Body).Returns(Stream.Null);
            mock.SetupGet(r => r.Method).Returns("GET");
            mock.SetupGet(r => r.Uri).Returns(uri);
            mock.SetupGet(r => r.PathBase).Returns(new PathString(String.Empty));
            mock.SetupGet(r => r.Headers).Returns(CreateFakeHeaders());
            mock.SetupGet(r => r.User).Returns((IPrincipal)null);
            return mock.Object;
        }

        private static Mock<IOwinResponse> CreateStubResponseMock()
        {
            Mock<IOwinResponse> mock = new Mock<IOwinResponse>(MockBehavior.Strict);
            mock.SetupSet(r => r.ReasonPhrase = It.IsAny<string>());
            mock.SetupGet(r => r.Environment).Returns((IDictionary<string, object>)null);
            mock.SetupGet(r => r.Headers).Returns(CreateFakeHeaders());
            mock.SetupGet(r => r.Body).Returns(Stream.Null);
            return mock;
        }

        private static IHeaderDictionary CreateFakeHeaders()
        {
            return new HeaderDictionary(new Dictionary<string, string[]>());
        }

        private static HttpMessageHandlerAdapter CreateProductUnderTest(OwinMiddleware next, HttpMessageHandler messageHandler)
        {
            return new HttpMessageHandlerAdapter(next: next, options: new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object,
                ExceptionLogger = new EmptyExceptionLogger(),
                ExceptionHandler = new Mock<IExceptionHandler>().Object
            });
        }

        public class IgnoreController : ApiController
        {
            [Route("ignore")]
            [Route("constraint/10")]
            public IHttpActionResult Get()
            {
                return Ok();
            }
        }

        public class CustomConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
                IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                long id;
                if (values.ContainsKey("id")
                    && Int64.TryParse(values["id"].ToString(), out id)
                    && (id == 10))
                {
                    return true;
                }

                return false;
            }
        }
    }
}

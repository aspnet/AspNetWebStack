// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Hosting
{
    public class SuppressHostPrincipalMessageHandlerTest
    {
        [Fact]
        public void SendAsync_DelegatesToInnerHandler()
        {
            // Arrange
            HttpRequestMessage request = null;
            CancellationToken cancellationToken = default(CancellationToken);
            Task<HttpResponseMessage> expectedResult = new Task<HttpResponseMessage>(() => null);
            HttpMessageHandler innerHandler = new LambdaHttpMessageHandler((r, c) =>
            {
                request = r;
                cancellationToken = c;
                return expectedResult;
            });
            HttpMessageHandler handler = CreateProductUnderTest(innerHandler);
            CancellationToken expectedCancellationToken = new CancellationToken(true);

            using (HttpRequestMessage expectedRequest = CreateRequestWithContext())
            {
                // Act
                Task<HttpResponseMessage> result = handler.SendAsync(expectedRequest, expectedCancellationToken);

                // Assert
                Assert.Same(expectedRequest, request);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Same(expectedResult, result);
            }
        }

        [Fact]
        public void SendAsync_Throws_WhenRequestContextIsNull()
        {
            // Arrange
            HttpMessageHandler innerHandler = CreateDummyHandler();
            HttpMessageHandler handler = CreateProductUnderTest(innerHandler);

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act & Assert
                Assert.ThrowsArgument(
                    () => { var ignore = handler.SendAsync(request, CancellationToken.None).Result; },
                    "request",
                    "The request must have a request context.");
            }
        }

        [Fact]
        public void SendAsync_SetsCurrentPrincipalToAnonymous_BeforeCallingInnerHandler()
        {
            // Arrange
            IPrincipal requestContextPrincipal = null;
            Mock<HttpRequestContext> requestContextMock = new Mock<HttpRequestContext>();
            requestContextMock
                .SetupSet(c => c.Principal = It.IsAny<IPrincipal>())
                .Callback<IPrincipal>((value) => requestContextPrincipal = value);
            IPrincipal principalBeforeInnerHandler = null;
            HttpMessageHandler inner = new LambdaHttpMessageHandler((ignore1, ignore2) =>
            {
                principalBeforeInnerHandler = requestContextPrincipal;
                return Task.FromResult<HttpResponseMessage>(null);
            });
            HttpMessageHandler handler = CreateProductUnderTest(inner);

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.SetRequestContext(requestContextMock.Object);

                // Act
                handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            Assert.NotNull(principalBeforeInnerHandler);
            IIdentity identity = principalBeforeInnerHandler.Identity;
            Assert.NotNull(identity);
            Assert.False(identity.IsAuthenticated);
            Assert.Null(identity.Name);
            Assert.Null(identity.AuthenticationType);
        }

        private static HttpMessageHandler CreateDummyHandler()
        {
            return new DummyHttpMessageHandler();
        }

        private static SuppressHostPrincipalMessageHandler CreateProductUnderTest(HttpMessageHandler innerHandler)
        {
            SuppressHostPrincipalMessageHandler handler = new SuppressHostPrincipalMessageHandler();
            handler.InnerHandler = innerHandler;
            return handler;
        }

        private static HttpRequestMessage CreateRequestWithContext()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRequestContext(new HttpRequestContext());
            return request;
        }

        private class DummyHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(Func<HttpRequestMessage, CancellationToken,
                Task<HttpResponseMessage>> sendAsync)
            {
                if (sendAsync == null)
                {
                    throw new ArgumentNullException("sendAsync");
                }

                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }
    }
}

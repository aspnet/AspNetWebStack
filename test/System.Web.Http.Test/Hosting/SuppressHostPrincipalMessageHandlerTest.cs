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
        public async Task SendAsync_DelegatesToInnerHandler()
        {
            // Arrange
            HttpRequestMessage request = null;
            var cancellationToken = default(CancellationToken);
            HttpMessageHandler innerHandler = new LambdaHttpMessageHandler((r, c) =>
            {
                request = r;
                cancellationToken = c;
                return Task.FromResult<HttpResponseMessage>(null);
            });
            HttpMessageHandler handler = CreateProductUnderTest(innerHandler);
            var expectedCancellationToken = new CancellationToken(true);

            using (var expectedRequest = CreateRequestWithContext())
            {
                // Act
                var result = await handler.SendAsync(expectedRequest, expectedCancellationToken);

                // Assert
                Assert.Same(expectedRequest, request);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task SendAsync_Throws_WhenRequestContextIsNull()
        {
            // Arrange
            HttpMessageHandler innerHandler = CreateDummyHandler();
            HttpMessageHandler handler = CreateProductUnderTest(innerHandler);

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act & Assert
                await Assert.ThrowsArgumentAsync(
                    () => handler.SendAsync(request, CancellationToken.None),
                    "request",
                    "The request must have a request context.");
            }
        }

        [Fact]
        public async Task SendAsync_SetsCurrentPrincipalToAnonymous_BeforeCallingInnerHandler()
        {
            // Arrange
            var requestContextMock = new Mock<HttpRequestContext>(MockBehavior.Strict);
            var sequence = new MockSequence();
            var initialPrincipal = new GenericPrincipal(new GenericIdentity("generic user"), new[] { "generic role" });
            IPrincipal requestContextPrincipal = null;
            requestContextMock
                .InSequence(sequence)
                .SetupGet(c => c.Principal)
                .Returns(initialPrincipal);
            requestContextMock
                .InSequence(sequence)
                .SetupSet(c => c.Principal = It.IsAny<IPrincipal>())
                .Callback<IPrincipal>(value => requestContextPrincipal = value);

            // SendAsync also restores the old principal.
            requestContextMock
                .InSequence(sequence)
                .SetupGet(c => c.Principal)
                .Returns(requestContextPrincipal);
            requestContextMock
                .InSequence(sequence)
                .SetupSet(c => c.Principal = initialPrincipal);

            IPrincipal principalBeforeInnerHandler = null;
            HttpMessageHandler inner = new LambdaHttpMessageHandler((ignore1, ignore2) =>
            {
                principalBeforeInnerHandler = requestContextPrincipal;
                return Task.FromResult<HttpResponseMessage>(null);
            });
            HttpMessageHandler handler = CreateProductUnderTest(inner);

            using (var request = new HttpRequestMessage())
            {
                request.SetRequestContext(requestContextMock.Object);

                // Act
                await handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            Assert.Equal(requestContextPrincipal, principalBeforeInnerHandler);
            Assert.NotNull(principalBeforeInnerHandler);
            var identity = principalBeforeInnerHandler.Identity;
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

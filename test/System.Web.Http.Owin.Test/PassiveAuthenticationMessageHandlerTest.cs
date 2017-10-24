// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class PassiveAuthenticationMessageHandlerTest
    {
        [Fact]
        public async Task SendAsync_DelegatesToInnerHandler()
        {
            // Arrange
            HttpRequestMessage request = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IOwinContext context = CreateOwinContext();

            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            {
                HttpMessageHandler innerHandler = new LambdaHttpMessageHandler((r, c) =>
                {
                    request = r;
                    cancellationToken = c;
                    return Task.FromResult(expectedResponse);
                });
                HttpMessageHandler handler = CreateProductUnderTest(innerHandler);
                CancellationToken expectedCancellationToken = new CancellationToken(true);

                using (HttpRequestMessage expectedRequest = CreateRequestWithOwinContextAndRequestContext(context))
                {
                    // Act
                    HttpResponseMessage response = await handler.SendAsync(expectedRequest, expectedCancellationToken);

                    // Assert
                    Assert.Same(expectedRequest, request);
                    Assert.Equal(expectedCancellationToken, cancellationToken);
                    Assert.Same(expectedResponse, response);
                }
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
        public async Task SendAsync_Throws_WhenOwinContextIsNull()
        {
            // Arrange
            HttpMessageHandler innerHandler = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(innerHandler);

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.SetRequestContext(new HttpRequestContext());

                // Act & Assert
                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => handler.SendAsync(request, CancellationToken.None));
                Assert.Equal("No OWIN authentication manager is associated with the request.", exception.Message);
            }
        }

        [Fact]
        public async Task SendAsync_Throws_WhenAuthenticationManagerIsNull()
        {
            // Arrange
            HttpMessageHandler innerHandler = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(innerHandler);
            IOwinContext context = CreateOwinContext(null);

            using (HttpRequestMessage request = CreateRequestWithOwinContextAndRequestContext(context))
            {
                // Act & Assert
                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => handler.SendAsync(request, CancellationToken.None));
                Assert.Equal("No OWIN authentication manager is associated with the request.", exception.Message);
            }
        }

        [Fact]
        public async Task SendAsync_SetsRequestContextPrincipalToAnonymous_BeforeCallingInnerHandler()
        {
            // Arrange
            IPrincipal requestContextPrincipal = null;
            Mock<HttpRequestContext> requestContextMock = new Mock<HttpRequestContext>(MockBehavior.Strict);
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
            IOwinContext context = CreateOwinContext();

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.SetOwinContext(context);
                request.SetRequestContext(requestContextMock.Object);

                // Act
                HttpResponseMessage ignore = await handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            Assert.NotNull(principalBeforeInnerHandler);
            IIdentity identity = principalBeforeInnerHandler.Identity;
            Assert.NotNull(identity);
            Assert.False(identity.IsAuthenticated);
            Assert.Null(identity.Name);
            Assert.Null(identity.AuthenticationType);
        }

        [Fact]
        public async Task SendAsync_SuppressesAuthenticationChallenges_WhenNoChallengeIsSet()
        {
            // Arrange
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(inner);
            IOwinContext context = CreateOwinContext();

            using (HttpRequestMessage request = CreateRequestWithOwinContextAndRequestContext(context))
            {
                // Act
                HttpResponseMessage ignore = await handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            IAuthenticationManager authenticationManager = context.Authentication;
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            string authenticationType = Assert.Single(authenticationTypes);
            Assert.Null(authenticationType);
        }

        [Fact]
        public async Task SendAsync_SuppressesAuthenticationChallenges_WhenExistingAuthenticationTypesIsNull()
        {
            // Arrange
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(inner);
            IOwinContext context = CreateOwinContext();
            IAuthenticationManager authenticationManager = context.Authentication;
            IDictionary<string, string> expectedExtra = new Dictionary<string, string>();
            AuthenticationProperties extraWrapper = new AuthenticationProperties(expectedExtra);
            context.Authentication.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(null,
                extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinContextAndRequestContext(context))
            {
                // Act
                HttpResponseMessage ignore = await handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            string authenticationType = Assert.Single(authenticationTypes);
            Assert.Null(authenticationType);
            AuthenticationProperties actualExtraWrapper = challenge.Properties;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Dictionary);
        }

        [Fact]
        public async Task SendAsync_SuppressesAuthenticationChallenges_WhenExistingAuthenticationTypesIsEmpty()
        {
            // Arrange
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(inner);
            IOwinContext context = CreateOwinContext();
            IAuthenticationManager authenticationManager = context.Authentication;
            AuthenticationProperties extraWrapper = new AuthenticationProperties();
            IDictionary<string, string> expectedExtra = extraWrapper.Dictionary;
            authenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(new string[0],
                extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinContextAndRequestContext(context))
            {
                // Act
                HttpResponseMessage ignore = await handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            string authenticationType = Assert.Single(authenticationTypes);
            Assert.Null(authenticationType);
            AuthenticationProperties actualExtraWrapper = challenge.Properties;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Dictionary);
        }

        [Fact]
        public async Task SendAsync_LeavesAuthenticationChallenges_WhenExistingAuthenticationTypesIsNonEmpty()
        {
            // Arrange
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(inner);
            IOwinContext context = CreateOwinContext();
            IAuthenticationManager authenticationManager = context.Authentication;
            AuthenticationProperties extraWrapper = new AuthenticationProperties();
            string[] expectedAuthenticationTypes = new string[] { "Existing" };
            IDictionary<string, string> expectedExtra = extraWrapper.Dictionary;
            authenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(
                expectedAuthenticationTypes, extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinContextAndRequestContext(context))
            {
                // Act
                HttpResponseMessage ignore = await handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            Assert.Same(expectedAuthenticationTypes, challenge.AuthenticationTypes);
            AuthenticationProperties actualExtraWrapper = challenge.Properties;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Dictionary);
        }

        private static HttpMessageHandler CreateDummyHandler()
        {
            return new DummyHttpMessageHandler();
        }

        private static IOwinContext CreateOwinContext()
        {
            return new OwinContext();
        }

        private static IOwinContext CreateOwinContext(IAuthenticationManager authenticationManager)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.SetupGet(m => m.Authentication).Returns(authenticationManager);
            return mock.Object;
        }

        private static PassiveAuthenticationMessageHandler CreateProductUnderTest(HttpMessageHandler innerHandler)
        {
            PassiveAuthenticationMessageHandler handler = new PassiveAuthenticationMessageHandler();
            handler.InnerHandler = innerHandler;
            return handler;
        }

        private static HttpRequestMessage CreateRequestWithOwinContextAndRequestContext(IOwinContext context)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetOwinContext(context);
            request.SetRequestContext(new HttpRequestContext());
            return request;
        }

        private static HttpMessageHandler CreateStubHandler()
        {
            return new LambdaHttpMessageHandler((ignore1, ignore2) =>
            {
                return Task.FromResult<HttpResponseMessage>(null);
            });
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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class AuthenticationFilterResultTests
    {
        [Fact]
        public async Task ExecuteAsync_DelegatesToInnerResult_WhenFiltersIsEmpty()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext context = CreateContext();
                ApiController controller = CreateController();
                IAuthenticationFilter[] filters = new IAuthenticationFilter[0];
                int calls = 0;
                CancellationToken cancellationToken;
                IHttpActionResult innerResult = CreateActionResult((t) =>
                {
                    calls++;
                    cancellationToken = t;
                    return Task.FromResult(expectedResponse);
                });
                IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);
                CancellationToken expectedCancellationToken = CreateCancellationToken();

                // Act
                HttpResponseMessage response = await product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.Equal(1, calls);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public async Task ExecuteAsync_CallsRequestContextPrincipalGet()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            ApiController controller = CreateController();
            IAuthenticationFilter filter = CreateStubFilter();
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            int calls = 0;
            Mock<HttpRequestContext> requestContextMock = new Mock<HttpRequestContext>();
            requestContextMock.Setup(c => c.Principal).Callback(() =>
                {
                    calls++;
                });
            controller.RequestContext = requestContextMock.Object;
            IHttpActionResult innerResult = CreateStubActionResult();

            IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);

            // Act
            HttpResponseMessage response = await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task ExecuteAsync_CallsFilterAuthenticateAsync()
        {
            // Arrange
            HttpActionContext expectedActionContext = CreateContext();
            ApiController controller = CreateController();
            int calls = 0;
            HttpAuthenticationContext authenticationContext = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
            {
                calls++;
                authenticationContext = c;
                cancellationToken = t;
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            Mock<HttpRequestContext> requestContextMock = new Mock<HttpRequestContext>(MockBehavior.Strict);
            requestContextMock.Setup(c => c.Principal).Returns(expectedPrincipal);
            controller.RequestContext = requestContextMock.Object;
            IHttpActionResult innerResult = CreateStubActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            IHttpActionResult product = CreateProductUnderTest(expectedActionContext, controller, filters,
                innerResult);

            // Act
            await product.ExecuteAsync(expectedCancellationToken);

            // Assert
            Assert.Equal(1, calls);
            Assert.NotNull(authenticationContext);
            HttpActionContext actionContext = authenticationContext.ActionContext;
            Assert.Same(expectedActionContext, actionContext);
            IPrincipal principal = authenticationContext.Principal;
            Assert.Same(expectedPrincipal, principal);
            Assert.Equal(expectedCancellationToken, cancellationToken);
        }

        [Fact]
        public async Task ExecuteAsync_DelegatesToErrorResult_WhenFilterReturnsFailure()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext context = CreateContext();
                ApiController controller = CreateController();
                int calls = 0;
                CancellationToken cancellationToken;
                IHttpActionResult errorResult = CreateActionResult((t) =>
                {
                    calls++;
                    cancellationToken = t;
                    return Task.FromResult(expectedResponse);
                });
                IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
                {
                    c.ErrorResult = errorResult;
                });
                IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
                IHttpActionResult innerResult = CreateDummyActionResult();
                IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);
                CancellationToken expectedCancellationToken = CreateCancellationToken();

                // Act
                HttpResponseMessage response = await product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.Equal(1, calls);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotCallSecondFilterAuthenticateOrInnerResult_WhenFirstFilterReturnsFailure()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext context = CreateContext();
                ApiController controller = CreateController();
                IHttpActionResult errorResult = CreateActionResult((t) => Task.FromResult(expectedResponse));
                IAuthenticationFilter firstFilter = CreateAuthenticationFilter((c, t) =>
                {
                    c.ErrorResult = errorResult;
                });
                int calls = 0;
                IAuthenticationFilter secondFilter = CreateAuthenticationFilter((c, t) =>
                    {
                        calls++;
                    });
                IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
                IHttpActionResult innerResult = CreateDummyActionResult();
                IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);
                CancellationToken expectedCancellationToken = CreateCancellationToken();

                // Act
                await product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.Equal(0, calls);
            }
        }

        [Fact]
        public async Task ExecuteAsync_UpdatesRequestContextPrincipal_WhenFilterReturnsSuccess()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            ApiController controller = CreateController();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
            {
                c.Principal = expectedPrincipal;
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IPrincipal principal = null;
            IHttpActionResult innerResult = CreateActionResult((c) =>
            {
                principal = controller.User;
                return Task.FromResult<HttpResponseMessage>(null);
            });

            IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);

            // Act
            await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public async Task ExecuteAsync_PassesPrincipalFromFirstFilterSuccessToSecondFilter()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            ApiController controller = CreateController();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            IAuthenticationFilter firstFilter = CreateAuthenticationFilter((c, t) =>
            {
                c.Principal = expectedPrincipal;
            });
            IPrincipal principal = null;
            IAuthenticationFilter secondFilter = CreateAuthenticationFilter((c, t) =>
            {
                if (c != null)
                {
                    principal = c.Principal;
                }
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
            IHttpActionResult innerResult = CreateStubActionResult();

            IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);

            // Act
            await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public async Task ExecuteAsync_OnlySetsPrincipalOnce_WhenMultipleFiltersReturnSuccess()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            ApiController controller = CreateController();
            IPrincipal principal = CreateDummyPrincipal();
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
            {
                c.Principal = principal;
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter, filter };
            int calls = 0;
            Mock<HttpRequestContext> requestContextMock = new Mock<HttpRequestContext>();
            requestContextMock
                .SetupSet(c => c.Principal = It.IsAny<IPrincipal>())
                .Callback<IPrincipal>((i) => { calls++; });
            controller.RequestContext = requestContextMock.Object;
            IHttpActionResult innerResult = CreateStubActionResult();

            IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);

            // Act
            await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotSetPrincipal_WhenNoFilterReturnsSuccess()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            ApiController controller = CreateController();
            IPrincipal principal = CreateDummyPrincipal();
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) => Task.FromResult<object>(null));
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter, filter };
            int calls = 0;
            Mock<HttpRequestContext> requestContextMock = new Mock<HttpRequestContext>();
            requestContextMock
                .SetupSet(c => c.Principal = It.IsAny<IPrincipal>())
                .Callback<IPrincipal>((i) => { calls++; });
            controller.RequestContext = requestContextMock.Object;
            IHttpActionResult innerResult = CreateStubActionResult();

            IHttpActionResult product = CreateProductUnderTest(context, controller, filters, innerResult);

            // Act
            await product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal(0, calls);
        }

        [Fact]
        public async Task ExecuteAsync_CallsFilterChallengeAsync_WithInnerResult_WhenNoFilterReturnsFailure()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            ApiController controller = CreateController();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter filter = CreateAuthenticationFilterChallenge((c, t) =>
            {
                calls++;
                context = c.ActionContext;
                innerResult = c.Result;
                cancellationToken = t;
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IHttpActionResult expectedInnerResult = CreateStubActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            IHttpActionResult product = CreateProductUnderTest(expectedContext, controller, filters,
                expectedInnerResult);

            // Act
            await product.ExecuteAsync(expectedCancellationToken);

            // Assert
            Assert.Equal(1, calls);
            Assert.Same(expectedContext, context);
            Assert.Same(expectedInnerResult, innerResult);
            Assert.Equal(expectedCancellationToken, cancellationToken);
        }

        [Fact]
        public async Task ExecuteAsync_CallsFilterChallengeAsync_WithErrorResult_WhenFilterReturnsFailure()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            ApiController controller = CreateController();
            IHttpActionResult expectedErrorResult = CreateDummyActionResult();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter filter = CreateAuthenticationFilter(
                (c, t) =>
                {
                    c.ErrorResult = expectedErrorResult;
                },
                (c, t) =>
                {
                    calls++;
                    context = c.ActionContext;
                    innerResult = c.Result;
                    cancellationToken = t;

                    c.Result = CreateStubActionResult();
                });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IHttpActionResult originalInnerResult = CreateDummyActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            IHttpActionResult product = CreateProductUnderTest(expectedContext, controller, filters,
                originalInnerResult);

            // Act
            await product.ExecuteAsync(expectedCancellationToken);

            // Assert
            Assert.Equal(1, calls);
            Assert.Same(expectedContext, context);
            Assert.Same(expectedErrorResult, innerResult);
            Assert.Equal(expectedCancellationToken, cancellationToken);
        }

        [Fact]
        public async Task ExecuteAsync_CallsSecondFilterChallengeAsync_WithFirstChallengeResult()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            ApiController controller = CreateController();
            IHttpActionResult expectedInnerResult = CreateDummyActionResult();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult result = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter firstFilter = CreateAuthenticationFilterChallenge((c, t) =>
            {
                c.Result = expectedInnerResult;
            });
            IAuthenticationFilter secondFilter = CreateAuthenticationFilterChallenge((c, t) =>
            {
                calls++;
                context = c.ActionContext;
                result = c.Result;
                cancellationToken = t;

                c.Result = CreateStubActionResult();
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
            IHttpActionResult originalInnerResult = CreateDummyActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            IHttpActionResult product = CreateProductUnderTest(expectedContext, controller, filters,
                originalInnerResult);

            // Act
            await product.ExecuteAsync(expectedCancellationToken);

            // Assert
            Assert.Equal(1, calls);
            Assert.Same(expectedContext, context);
            Assert.Same(expectedInnerResult, result);
            Assert.Equal(expectedCancellationToken, cancellationToken);
        }

        private static IHttpActionResult CreateActionResult(
            Func<CancellationToken, Task<HttpResponseMessage>> executeAsync)
        {
            Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            CancellationToken cancellationToken;
            mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((t) => { cancellationToken = t; })
                .Returns(() => executeAsync.Invoke(cancellationToken));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilter(
            Action<HttpAuthenticationContext, CancellationToken> authenticate)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationContext, CancellationToken>((c, t) =>
                {
                    authenticate.Invoke(c, t);
                })
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilter(
            Action<HttpAuthenticationContext, CancellationToken> authenticate,
            Action<HttpAuthenticationChallengeContext, CancellationToken> challenge)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationContext, CancellationToken>((c, t) =>
                    {
                        authenticate.Invoke(c, t);
                    })
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationChallengeContext, CancellationToken>((c, t) =>
                    {
                        challenge.Invoke(c, t);
                    })
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilterChallenge(
            Action<HttpAuthenticationChallengeContext, CancellationToken> challenge)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationChallengeContext, CancellationToken>((c, t) =>
                {
                    challenge.Invoke(c, t);
                })
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static CancellationToken CreateCancellationToken()
        {
            return new CancellationToken(canceled: true);
        }

        private static HttpActionContext CreateContext()
        {
            return new HttpActionContext();
        }

        private static ApiController CreateController()
        {
            return new Mock<ApiController>().Object;
        }

        private static IHttpActionResult CreateDummyActionResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static AuthenticationFilterResult CreateProductUnderTest(HttpActionContext context,
            ApiController controller, IAuthenticationFilter[] filters, IHttpActionResult innerResult)
        {
            return new AuthenticationFilterResult(context, controller, filters, innerResult);
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static IHttpActionResult CreateStubActionResult()
        {
            Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            HttpResponseMessage response = null;
            mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(response));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateStubFilter()
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>(MockBehavior.Strict);
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static HttpRequestContext CreateStubRequestContext()
        {
            Mock<HttpRequestContext> mock = new Mock<HttpRequestContext>(MockBehavior.Strict);
            mock.Setup(c => c.Principal).Returns((IPrincipal)null);
            return mock.Object;
        }
    }
}

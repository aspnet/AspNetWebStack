// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class AuthenticationFilterTracerTests
    {
        [Fact]
        public void Inner_ReturnsSpecifiedInstance()
        {
            // Arrange
            IAuthenticationFilter expectedInner = CreateDummyFilter();
            ITraceWriter tracer = CreateDummyTracer();
            AuthenticationFilterTracer product = CreateProductUnderTest(expectedInner, tracer);

            // Act
            IAuthenticationFilter inner = product.Inner;

            // Assert
            Assert.Same(expectedInner, inner);
        }

        [Fact]
        public async Task AuthenticateAsync_DelegatesToInnerFilter()
        {
            // Arrange
            HttpAuthenticationContext expectedAuthenticationContext = CreateAuthenticationContext();
            CancellationToken expectedCancellationToken = CreateCancellationToken();
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            int calls = 0;
            mock.Setup(f => f.AuthenticateAsync(expectedAuthenticationContext, expectedCancellationToken)).Callback(
                () => { calls++; }).Returns(() => Task.FromResult<object>(null));
            IAuthenticationFilter filter = mock.Object;
            ITraceWriter tracer = CreateStubTracer();
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);

            // Act
            await product.AuthenticateAsync(expectedAuthenticationContext, expectedCancellationToken);

            // Assert
            Assert.Equal(1, calls);
        }

        [Theory]
        [ReplaceCulture]
        [InlineData(true, "The authentication filter successfully set a principal "
                        + "to a known identity. Identity.Name='User'. "
                        + "Identity.AuthenticationType='Basic'.")]
        [InlineData(false, "The authentication filter set a principal to an unknown identity.")]
        public async Task AuthenticateAsync_Traces(bool withIdentity, string expectedMessage)
        {
            // Arrange
            CancellationToken cancellationToken = CreateCancellationToken();
            Mock<IAuthenticationFilter> filterMock = new Mock<IAuthenticationFilter>();
            filterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                                                      It.IsAny<CancellationToken>()))
                .Callback((HttpAuthenticationContext context,
                           CancellationToken token) => context.Principal = CreateDummyPrincipal(withIdentity))
                .Returns(Task.FromResult<object>(null));
            IAuthenticationFilter filter = filterMock.Object;
            TraceRecord record = null;
            ITraceWriter tracer = CreateTracer((r) => { record = r; });
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpAuthenticationContext authenticationContext = CreateAuthenticationContext(expectedRequest,
                                                                                              isPrincipalSet: false);

                // Act
                await product.AuthenticateAsync(authenticationContext, cancellationToken);

                // Assert
                Assert.NotNull(record);
                Assert.Same(expectedRequest, record.Request);
                Assert.Same(TraceCategories.FiltersCategory, record.Category);
                Assert.Equal(TraceLevel.Info, record.Level);
                Assert.Equal(TraceKind.End, record.Kind);
                Assert.Equal(filter.GetType().Name, record.Operator);
                Assert.Equal("AuthenticateAsync", record.Operation);
                Assert.Null(record.Exception);
                Assert.Equal(expectedMessage, record.Message);
            }
        }

        [Fact]
        [ReplaceCulture]
        public async Task AuthenticateAsync_Traces_ErrorResult()
        {
            // Arrange
            IHttpActionResult result = new AuthenticationFailureResult();
            CancellationToken cancellationToken = CreateCancellationToken();
            Mock<IAuthenticationFilter> filterMock = new Mock<IAuthenticationFilter>();
            filterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                                                      It.IsAny<CancellationToken>()))
                .Callback((HttpAuthenticationContext context,
                           CancellationToken token) => context.ErrorResult = result)
                .Returns(Task.FromResult<object>(null));
            TraceRecord record = null;
            ITraceWriter tracer = CreateTracer((r) => { record = r; });
            IAuthenticationFilter product = CreateProductUnderTest(filterMock.Object, tracer);
            const string expectedMessage = "The authentication filter encountered an error. ErrorResult="
                + "'System.Web.Http.Tracing.Tracers.AuthenticationFilterTracerTests+AuthenticationFailureResult'.";

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpAuthenticationContext authenticationContext = CreateAuthenticationContext(expectedRequest);

                // Act
                await product.AuthenticateAsync(authenticationContext, cancellationToken);

                // Assert
                Assert.NotNull(record);
                Assert.Same(expectedRequest, record.Request);
                Assert.Same(TraceCategories.FiltersCategory, record.Category);
                Assert.Equal(TraceLevel.Info, record.Level);
                Assert.Equal(TraceKind.End, record.Kind);
                Assert.Equal(filterMock.Object.GetType().Name, record.Operator);
                Assert.Equal("AuthenticateAsync", record.Operation);
                Assert.Null(record.Exception);
                Assert.Equal(expectedMessage, record.Message);
            }
        }

        [Fact]
        [ReplaceCulture]
        public async Task AuthenticateAsync_Traces_DidNothing()
        {
            // Arrange
            CancellationToken cancellationToken = CreateCancellationToken();
            IAuthenticationFilter filter = CreateStubFilter();
            TraceRecord record = null;
            ITraceWriter tracer = CreateTracer((r) => { record = r; });
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);
            const string expectedMessage = "The authentication filter did not encounter an error or set a principal.";

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpAuthenticationContext authenticationContext = CreateAuthenticationContext(expectedRequest);

                // Act
                await product.AuthenticateAsync(authenticationContext, cancellationToken);

                // Assert
                Assert.NotNull(record);
                Assert.Same(expectedRequest, record.Request);
                Assert.Same(TraceCategories.FiltersCategory, record.Category);
                Assert.Equal(TraceLevel.Info, record.Level);
                Assert.Equal(TraceKind.End, record.Kind);
                Assert.Equal(filter.GetType().Name, record.Operator);
                Assert.Equal("AuthenticateAsync", record.Operation);
                Assert.Null(record.Exception);
                Assert.Equal(expectedMessage, record.Message);
            }
        }

        [Fact]
        public async Task AuthenticateAsync_Traces_WhenContextIsNull()
        {
            // Arrange
            CancellationToken cancellationToken = CreateCancellationToken();
            IAuthenticationFilter filter = CreateStubFilter();
            TraceRecord record = null;
            ITraceWriter tracer = CreateTracer((r) => { record = r; });
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);
            HttpAuthenticationContext authenticationContext = null;

            // Act
            await product.AuthenticateAsync(authenticationContext, cancellationToken);

            // Assert
            Assert.NotNull(record);
            Assert.Null(record.Request);
        }

        [Fact]
        public async Task ChallengeAsync_DelegatesToInnerFilter()
        {
            // Arrange
            HttpAuthenticationChallengeContext expectedChallengeContext = CreateChallengeContext();
            CancellationToken expectedCancellationToken = CreateCancellationToken();
            IHttpActionResult expectedInnerResult = CreateDummyActionResult();
            IHttpActionResult expectedResult = CreateDummyActionResult();
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            int calls = 0;
            mock.Setup(f => f.ChallengeAsync(expectedChallengeContext, expectedCancellationToken))
                .Callback(() => { calls++; })
                .Returns(() => Task.FromResult(expectedResult));
            IAuthenticationFilter filter = mock.Object;
            ITraceWriter tracer = CreateStubTracer();
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);

            // Act
            await product.ChallengeAsync(expectedChallengeContext, expectedCancellationToken);

            // Assert
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task ChallengeAsync_Traces()
        {
            // Arrange
            CancellationToken cancellationToken = CreateCancellationToken();
            IAuthenticationFilter filter = CreateStubFilter();
            TraceRecord record = null;
            ITraceWriter tracer = CreateTracer((r) => { record = r; });
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);
            IHttpActionResult innerResult = CreateDummyActionResult();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpAuthenticationChallengeContext context = CreateChallengeContext(expectedRequest, innerResult);

                // Act
                await product.ChallengeAsync(context, cancellationToken);

                // Assert
                Assert.NotNull(record);
                Assert.Same(expectedRequest, record.Request);
                Assert.Same(TraceCategories.FiltersCategory, record.Category);
                Assert.Equal(TraceLevel.Info, record.Level);
                Assert.Equal(TraceKind.End, record.Kind);
                Assert.Equal(filter.GetType().Name, record.Operator);
                Assert.Equal("ChallengeAsync", record.Operation);
                Assert.Null(record.Exception);
                Assert.Null(record.Message);
            }
        }

        [Fact]
        public async Task ChallengeAsync_Traces_WhenContextIsNull()
        {
            // Arrange
            CancellationToken cancellationToken = CreateCancellationToken();
            IAuthenticationFilter filter = CreateStubFilter();
            TraceRecord record = null;
            ITraceWriter tracer = CreateTracer((r) => { record = r; });
            IAuthenticationFilter product = CreateProductUnderTest(filter, tracer);
            HttpAuthenticationChallengeContext context = null;

            // Act
            await product.ChallengeAsync(context, cancellationToken);

            // Assert
            Assert.NotNull(record);
            Assert.Null(record.Request);
        }

        private static HttpActionContext CreateActionContext()
        {
            return new HttpActionContext();
        }

        private static HttpActionContext CreateActionContext(HttpRequestMessage request)
        {
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = request;
            HttpActionContext actionContext = new HttpActionContext();
            actionContext.ControllerContext = controllerContext;
            return actionContext;
        }

        private static HttpAuthenticationContext CreateAuthenticationContext()
        {
            HttpActionContext actionContext = CreateActionContext();
            IPrincipal principal = CreateDummyPrincipal();
            return new HttpAuthenticationContext(actionContext, principal);
        }

        private static HttpAuthenticationContext CreateAuthenticationContext(HttpRequestMessage request,
                                                                             bool isPrincipalSet = true)
        {
            HttpActionContext actionContext = CreateActionContext(request);
            IPrincipal principal = (isPrincipalSet) ? CreateDummyPrincipal() : null;
            return new HttpAuthenticationContext(actionContext, principal);
        }

        private static CancellationToken CreateCancellationToken()
        {
            return new CancellationToken(canceled: true);
        }

        private static HttpAuthenticationChallengeContext CreateChallengeContext()
        {
            HttpActionContext actionContext = CreateActionContext();
            IHttpActionResult result = CreateDummyResult();
            return new HttpAuthenticationChallengeContext(actionContext, result);
        }

        private static HttpAuthenticationChallengeContext CreateChallengeContext(HttpRequestMessage request,
            IHttpActionResult result)
        {
            HttpActionContext actionContext = CreateActionContext(request);
            return new HttpAuthenticationChallengeContext(actionContext, result);
        }

        private IHttpActionResult CreateDummyActionResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static IAuthenticationFilter CreateDummyFilter()
        {
            return new Mock<IAuthenticationFilter>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal(bool withIdentity = true)
        {
            var principalMock = new Mock<IPrincipal>(MockBehavior.Strict);
            if (withIdentity)
            {
                principalMock.Setup(p => p.Identity.Name).Returns("User");
                principalMock.Setup(p => p.Identity.AuthenticationType).Returns("Basic");
            }
            else
            {
                principalMock.Setup(p => p.Identity).Returns((IIdentity)null);
            }
            return principalMock.Object;
        }

        private static IHttpActionResult CreateDummyResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static ITraceWriter CreateDummyTracer()
        {
            return new Mock<ITraceWriter>(MockBehavior.Strict).Object;
        }

        private static AuthenticationFilterTracer CreateProductUnderTest(IAuthenticationFilter innerFilter,
            ITraceWriter traceWriter)
        {
            return new AuthenticationFilterTracer(innerFilter, traceWriter);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static IAuthenticationFilter CreateStubFilter()
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<object>(null));
            return mock.Object;
        }

        private static ITraceWriter CreateStubTracer()
        {
            return new Mock<ITraceWriter>().Object;
        }

        private static ITraceWriter CreateTracer(Action<TraceRecord> trace)
        {
            Mock<ITraceWriter> mock = new Mock<ITraceWriter>(MockBehavior.Strict);
            TraceRecord record = null;
            mock.Setup(t => t.Trace(It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), It.IsAny<TraceLevel>(),
                It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((r, c, l, a) =>
                    {
                        record = new TraceRecord(r, c, l);
                        a.Invoke(record);
                        trace.Invoke(record);
                    });
            return mock.Object;
        }

        private class AuthenticationFailureResult : IHttpActionResult
        {
            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}

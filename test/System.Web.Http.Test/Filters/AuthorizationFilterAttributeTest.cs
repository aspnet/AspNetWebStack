// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Filters
{
    public class AuthorizationFilterAttributeTest
    {
        [Fact]
        public void AllowsMultiple_DefaultReturnsTrue()
        {
            AuthorizationFilterAttribute actionFilter = new TestableAuthorizationFilter();

            Assert.True(actionFilter.AllowMultiple);
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_IfContextParameterIsNull_ThrowsException()
        {
            var filter = new TestableAuthorizationFilter() as IAuthorizationFilter;
            Assert.ThrowsArgumentNull(() =>
            {
                filter.ExecuteAuthorizationFilterAsync(actionContext: null, cancellationToken: CancellationToken.None, continuation: () => null);
            }, "actionContext");
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_IfContinuationParameterIsNull_ThrowsException()
        {
            var filter = new TestableAuthorizationFilter() as IAuthorizationFilter;
            Assert.ThrowsArgumentNull(() =>
            {
                filter.ExecuteAuthorizationFilterAsync(actionContext: ContextUtil.CreateActionContext(), cancellationToken: CancellationToken.None, continuation: null);
            }, "continuation");
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_InvokesOnActionExecutingBeforeContinuation()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            bool onActionExecutingInvoked = false;
            filterMock.Setup(f => f.OnAuthorization(It.IsAny<HttpActionContext>())).Callback(() =>
            {
                onActionExecutingInvoked = true;
            });
            bool? flagWhenContinuationInvoked = null;
            Func<Task<HttpResponseMessage>> continuation = () =>
            {
                flagWhenContinuationInvoked = onActionExecutingInvoked;
                return Task.FromResult(new HttpResponseMessage());
            };
            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act
            await filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, continuation);

            // Assert
            Assert.True(flagWhenContinuationInvoked.Value);
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_IfOnActionExecutingSetsResult_ShortCircuits()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>
            {
                CallBase = true,
            };

            HttpResponseMessage response = new HttpResponseMessage();
            filterMock.Setup(f => f.OnAuthorization(It.IsAny<HttpActionContext>())).Callback<HttpActionContext>(c =>
            {
                c.Response = response;
            });

            bool continuationCalled = false;
            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act
            var result = await filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () =>
            {
                continuationCalled = true;
                return null;
            });

            // Assert
            Assert.False(continuationCalled);
            Assert.Same(response, result);
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_IfOnActionExecutingThrowsException_ReturnsFaultedTask()
        {
            // Arrange
            Exception expectedException = new Exception();
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            filterMock.Setup(f => f.OnAuthorization(It.IsAny<HttpActionContext>())).Throws(expectedException);
            var filter = (IAuthorizationFilter)filterMock.Object;
            bool continuationCalled = false;

            // Act & Assert
            Exception exception = await Assert.ThrowsAsync<Exception>(
                () => filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () =>
                {
                    continuationCalled = true;
                    return null;
                }));

            // Assert
            Assert.Same(expectedException, exception);
            Assert.False(continuationCalled);
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_OnActionExecutingMethodGetsPassedControllerContext()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act
            await filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () =>
            {
                return Task.FromResult(new HttpResponseMessage());
            });

            // Assert
            filterMock.Verify(f => f.OnAuthorization(context));
        }

        [Fact]
        public Task ExecuteAuthorizationFilterAsync_IfContinuationTaskWasCanceled_ReturnsCanceledTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act & Assert
            return Assert.ThrowsAsync<TaskCanceledException>(
                () => filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () => TaskHelpers.Canceled<HttpResponseMessage>()));
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_IfContinuationSucceeded_ReturnsSuccessTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            var filter = (IAuthorizationFilter)filterMock.Object;
            HttpResponseMessage expectedResponse = new HttpResponseMessage();

            // Act
            var response = await filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () => Task.FromResult(expectedResponse));

            // Assert
            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_IfContinuationFaulted_ReturnsFaultedTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            var filter = (IAuthorizationFilter)filterMock.Object;
            Exception expectedException = new Exception();

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(
                () => filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromError<HttpResponseMessage>(expectedException)));

            // Assert
            Assert.Same(expectedException, exception);
        }
    }

    public class TestableAuthorizationFilter : AuthorizationFilterAttribute
    {
    }
}

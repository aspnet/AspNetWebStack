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
        public void ExecuteAuthorizationFilterAsync_InvokesOnActionExecutingBeforeContinuation()
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
            filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, continuation).Wait();

            // Assert
            Assert.True(flagWhenContinuationInvoked.Value);
        }

        public void ExecuteAuthorizationFilterAsync_IfOnActionExecutingSetsResult_ShortCircuits()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>();
            HttpResponseMessage response = new HttpResponseMessage();
            filterMock.Setup(f => f.OnAuthorization(It.IsAny<HttpActionContext>())).Callback<HttpActionContext>(c =>
            {
                c.Response = response;
            });
            bool continuationCalled = false;
            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act
            var result = filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () =>
            {
                continuationCalled = true;
                return null;
            }).Result;

            // Assert
            Assert.False(continuationCalled);
            Assert.Same(response, result);
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_IfOnActionExecutingThrowsException_ReturnsFaultedTask()
        {
            // Arrange
            Exception e = new Exception();
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            filterMock.Setup(f => f.OnAuthorization(It.IsAny<HttpActionContext>())).Throws(e);
            var filter = (IAuthorizationFilter)filterMock.Object;
            bool continuationCalled = false;

            // Act
            var result = filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () =>
            {
                continuationCalled = true;
                return null;
            });

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsFaulted);
            Assert.Same(e, result.Exception.InnerException);
            Assert.False(continuationCalled);
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_OnActionExecutingMethodGetsPassedControllerContext()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act
            filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () =>
            {
                return Task.FromResult(new HttpResponseMessage());
            }).Wait();

            // Assert
            filterMock.Verify(f => f.OnAuthorization(context));
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_IfContinuationTaskWasCanceled_ReturnsCanceledTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            var filter = (IAuthorizationFilter)filterMock.Object;

            // Act
            var result = filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () => TaskHelpers.Canceled<HttpResponseMessage>());

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsCanceled);
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_IfContinuationSucceeded_ReturnsSuccessTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            var filter = (IAuthorizationFilter)filterMock.Object;
            HttpResponseMessage response = new HttpResponseMessage();

            // Act
            var result = filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () => Task.FromResult(response));

            // Assert
            result.WaitUntilCompleted();
            Assert.Same(response, result.Result);
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_IfContinuationFaulted_ReturnsFaultedTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<AuthorizationFilterAttribute> filterMock = new Mock<AuthorizationFilterAttribute>()
            {
                CallBase = true,
            };

            var filter = (IAuthorizationFilter)filterMock.Object;
            Exception exception = new Exception();

            // Act
            var result = filter.ExecuteAuthorizationFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromError<HttpResponseMessage>(exception));

            // Assert
            result.WaitUntilCompleted();
            Assert.Same(exception, result.Exception.InnerException);
        }
    }

    public class TestableAuthorizationFilter : AuthorizationFilterAttribute
    {
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class AuthorizationFilterResultTests
    {
        [Fact]
        public async Task ExecuteAsync_ChainsFiltersInOrderFollowedByInnerActionContinuation()
        {
            // Arrange
            HttpActionContext actionContextInstance = ContextUtil.CreateActionContext();
            List<string> log = new List<string>();
            Mock<IAuthorizationFilter> globalFilterMock = CreateAuthorizationFilterMock((ctx, ct, continuation) =>
            {
                log.Add("globalFilter");
                return continuation();
            });
            Mock<IAuthorizationFilter> actionFilterMock = CreateAuthorizationFilterMock((ctx, ct, continuation) =>
            {
                log.Add("actionFilter");
                return continuation();
            });
            Mock<IHttpActionResult> innerResultMock = new Mock<IHttpActionResult>();
            innerResultMock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(() =>
            {
                log.Add("innerAction");
                return Task.FromResult<HttpResponseMessage>(null);
            });
            IHttpActionResult innerResult = innerResultMock.Object;
            var filters = new IAuthorizationFilter[] {
                globalFilterMock.Object,
                actionFilterMock.Object,
            };
            IHttpActionResult authorizationFilter = new AuthorizationFilterResult(actionContextInstance, filters,
                innerResult);

            // Act
            await authorizationFilter.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "globalFilter", "actionFilter", "innerAction" }, log.ToArray());
            globalFilterMock.Verify();
            actionFilterMock.Verify();
        }

        private Mock<IAuthorizationFilter> CreateAuthorizationFilterMock(Func<HttpActionContext, CancellationToken,
            Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> implementation)
        {
            Mock<IAuthorizationFilter> filterMock = new Mock<IAuthorizationFilter>();
            filterMock.Setup(f => f.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(),
                                                                    CancellationToken.None,
                                                                    It.IsAny<Func<Task<HttpResponseMessage>>>()))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }
    }
}

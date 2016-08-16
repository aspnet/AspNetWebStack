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
    public class ActionFilterResultTests
    {
        [Fact]
        public async Task InvokeActionWithActionFilters_ChainsFiltersInOrderFollowedByInnerActionContinuation()
        {
            // Arrange
            HttpActionContext actionContextInstance = ContextUtil.CreateActionContext();
            List<string> log = new List<string>();
            Mock<IActionFilter> globalFilterMock = CreateActionFilterMock((ctx, ct, continuation) =>
            {
                log.Add("globalFilter");
                return continuation();
            });
            Mock<IActionFilter> actionFilterMock = CreateActionFilterMock((ctx, ct, continuation) =>
            {
                log.Add("actionFilter");
                return continuation();
            });
            Func<Task<HttpResponseMessage>> innerAction = () => Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                log.Add("innerAction");
                return null;
            });
            var filters = new IActionFilter[] {
                globalFilterMock.Object,
                actionFilterMock.Object,
            };

            // Act
            var result = ActionFilterResult.InvokeActionWithActionFilters(actionContextInstance,
                CancellationToken.None, filters, innerAction);

            // Assert
            Assert.NotNull(result);
            await result();

            Assert.Equal(new[] { "globalFilter", "actionFilter", "innerAction" }, log.ToArray());
            globalFilterMock.Verify();
            actionFilterMock.Verify();
        }

        private Mock<IActionFilter> CreateActionFilterMock(Func<HttpActionContext, CancellationToken,
            Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> implementation)
        {
            Mock<IActionFilter> filterMock = new Mock<IActionFilter>();
            filterMock.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(),
                                                             CancellationToken.None,
                                                             It.IsAny<Func<Task<HttpResponseMessage>>>()))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }
    }
}

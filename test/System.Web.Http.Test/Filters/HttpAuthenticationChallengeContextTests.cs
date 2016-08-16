// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class HttpAuthenticationChallengeContextTests
    {
        [Fact]
        public void Constructor_Throws_WhenActionContextIsNull()
        {
            // Arrange
            HttpActionContext actionContext = null;
            IHttpActionResult result = CreateDummyResult();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(actionContext, result); }, "actionContext");
        }

        [Fact]
        public void Constructor_Throws_WhenResultIsNull()
        {
            // Arrange
            HttpActionContext actionContext = CreateActionContext();
            IHttpActionResult result = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(actionContext, result); }, "result");
        }

        [Fact]
        public void ActionContext_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpActionContext expectedActionContext = CreateActionContext();
            IHttpActionResult result = CreateDummyResult();
            HttpAuthenticationChallengeContext product = CreateProductUnderTest(expectedActionContext, result);

            // Act
            HttpActionContext actionContext = product.ActionContext;

            // Assert
            Assert.Same(expectedActionContext, actionContext);
        }

        [Fact]
        public void Result_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpActionContext actionContext = CreateActionContext();
            IHttpActionResult expectedResult = CreateDummyResult();
            HttpAuthenticationChallengeContext product = CreateProductUnderTest(actionContext, expectedResult);

            // Act
            IHttpActionResult result = product.Result;

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void Request_ReturnsActionContextRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpActionContext actionContext = CreateActionContext(expectedRequest);
                IHttpActionResult result = CreateDummyResult();
                HttpAuthenticationChallengeContext product = CreateProductUnderTest(actionContext, result);

                // Act
                HttpRequestMessage request = product.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void ResultSetter_Throws_WhenNull()
        {
            // Arrange
            HttpActionContext actionContext = CreateActionContext();
            IHttpActionResult result = CreateDummyResult();
            HttpAuthenticationChallengeContext product = CreateProductUnderTest(actionContext, result);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { product.Result = null; }, "value");
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

        private static IHttpActionResult CreateDummyResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static HttpAuthenticationChallengeContext CreateProductUnderTest(HttpActionContext actionContext,
            IHttpActionResult result)
        {
            return new HttpAuthenticationChallengeContext(actionContext, result);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class HttpAuthenticationContextTests
    {
        [Fact]
        public void Constructor_Throws_WhenActionContextIsNull()
        {
            // Arrange
            HttpActionContext actionContext = null;
            IPrincipal principal = CreateDummyPrincipal();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(actionContext, principal); }, "actionContext");
        }

        [Fact]
        public void ActionContext_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpActionContext expectedActionContext = CreateActionContext();
            IPrincipal principal = CreateDummyPrincipal();
            HttpAuthenticationContext product = CreateProductUnderTest(expectedActionContext, principal);

            // Act
            HttpActionContext actionContext = product.ActionContext;

            // Assert
            Assert.Same(expectedActionContext, actionContext);
        }

        [Fact]
        public void Principal_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpActionContext actionContext = CreateActionContext();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpAuthenticationContext product = CreateProductUnderTest(actionContext, expectedPrincipal);

            // Act
            IPrincipal principal = product.Principal;

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void Request_ReturnsActionContextRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpActionContext actionContext = CreateActionContext(expectedRequest);
                IPrincipal principal = CreateDummyPrincipal();
                HttpAuthenticationContext product = CreateProductUnderTest(actionContext, principal);

                // Act
                HttpRequestMessage request = product.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
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

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static HttpAuthenticationContext CreateProductUnderTest(HttpActionContext actionContext,
            IPrincipal principal)
        {
            return new HttpAuthenticationContext(actionContext, principal);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }
    }
}

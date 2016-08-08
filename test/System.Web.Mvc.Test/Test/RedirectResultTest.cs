// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Properties;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class RedirectResultTest
    {
        private static string _baseUrl = "http://www.contoso.com/";

        [Fact]
        public void ConstructorSetsUrl()
        {
            // Act
            var result = new RedirectResult(_baseUrl);

            // Assert
            Assert.Same(_baseUrl, result.Url);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void ConstructorSetsUrlAndPermanent()
        {
            // Act
            var result = new RedirectResult(_baseUrl, permanent: true);

            // Assert
            Assert.Same(_baseUrl, result.Url);
            Assert.True(result.Permanent);
        }

        [Fact]
        public void ConstructorWithEmptyUrlThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new RedirectResult(String.Empty); },
                "url");

            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new RedirectResult(String.Empty, true); },
                "url");
        }

        [Fact]
        public void ConstructorWithNullUrlThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new RedirectResult(url: null); },
                "url");

            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new RedirectResult(url: null, permanent: true); },
                "url");
        }

        [Fact]
        public void ExecuteResultCallsResponseRedirect()
        {
            // Arrange
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(o => o.Redirect(_baseUrl, false /* endResponse */)).Verifiable();
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(o => o.Response).Returns(mockResponse.Object);
            ControllerContext context = new ControllerContext(mockContext.Object, new RouteData(), new Mock<ControllerBase>().Object);
            var result = new RedirectResult(_baseUrl);

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResponse.Verify();
        }

        [Fact]
        public void ExecuteResultWithPermanentCallsResponseRedirectPermanent()
        {
            // Arrange
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(o => o.RedirectPermanent(_baseUrl, false /* endResponse */)).Verifiable();
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(o => o.Response).Returns(mockResponse.Object);
            ControllerContext context = new ControllerContext(mockContext.Object, new RouteData(), new Mock<ControllerBase>().Object);
            var result = new RedirectResult(_baseUrl, permanent: true);

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResponse.Verify();
        }

        [Fact]
        public void ExecuteResultWithNullControllerContextThrows()
        {
            // Arrange
            var result = new RedirectResult(_baseUrl);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { result.ExecuteResult(null /* context */); },
                "context");
        }

        [Fact]
        public void RedirectInChildActionThrows()
        {
            // Arrange
            RouteData routeData = new RouteData();
            routeData.DataTokens[ControllerContext.ParentActionViewContextToken] = new ViewContext();
            ControllerContext context = new ControllerContext(new Mock<HttpContextBase>().Object, routeData, new Mock<ControllerBase>().Object);
            RedirectResult result = new RedirectResult(_baseUrl);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => result.ExecuteResult(context),
                MvcResources.RedirectAction_CannotRedirectInChildAction
                );
        }
    }
}

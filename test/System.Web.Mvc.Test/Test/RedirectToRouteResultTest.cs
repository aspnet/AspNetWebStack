// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class RedirectToRouteResultTest
    {
        [Fact]
        public void ConstructorWithNullValuesDictionary()
        {
            // Act
            var result = new RedirectToRouteResult(routeValues: null);

            // Assert
            Assert.NotNull(result.RouteValues);
            Assert.Empty(result.RouteValues);
            Assert.Equal(String.Empty, result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void ConstructorSetsValuesDictionary()
        {
            // Arrange
            RouteValueDictionary dict = new RouteValueDictionary();

            // Act
            var result = new RedirectToRouteResult(dict);

            // Assert
            Assert.Same(dict, result.RouteValues);
            Assert.Equal(String.Empty, result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void ConstructorSetsValuesDictionaryAndEmptyName()
        {
            // Arrange
            RouteValueDictionary dict = new RouteValueDictionary();

            // Act
            var result = new RedirectToRouteResult(null, dict);

            // Assert
            Assert.Same(dict, result.RouteValues);
            Assert.Equal(String.Empty, result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void ConstructorSetsValuesDictionaryAndName()
        {
            // Arrange
            RouteValueDictionary dict = new RouteValueDictionary();

            // Act
            var result = new RedirectToRouteResult("foo", dict);

            // Assert
            Assert.Same(dict, result.RouteValues);
            Assert.Equal("foo", result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void ConstructorSetsPermanent()
        {
            // Act
            var result = new RedirectToRouteResult(null, null, true);

            // Assert
            Assert.True(result.Permanent);
        }

        [Fact]
        public void ExecuteResultCallsResponseRedirect()
        {
            // Arrange
            Mock<Controller> mockController = new Mock<Controller>();
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.ApplicationPath).Returns("/somepath");
            mockControllerContext.Setup(c => c.HttpContext.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) => s);
            mockControllerContext.Setup(c => c.HttpContext.Response.Redirect("/somepath/c/a/i", false)).Verifiable();
            mockControllerContext.Setup(c => c.Controller).Returns(mockController.Object);

            var values = new { Controller = "c", Action = "a", Id = "i" };
            RedirectToRouteResult result = new RedirectToRouteResult(new RouteValueDictionary(values))
            {
                Routes = new RouteCollection() { new Route("{controller}/{action}/{id}", null) },
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultWithPermanentCallsResponseRedirectPermanent()
        {
            // Arrange
            Mock<Controller> mockController = new Mock<Controller>();
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.ApplicationPath).Returns("/somepath");
            mockControllerContext.Setup(c => c.HttpContext.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) => s);
            mockControllerContext.Setup(c => c.HttpContext.Response.RedirectPermanent("/somepath/c/a/i", false)).Verifiable();
            mockControllerContext.Setup(c => c.Controller).Returns(mockController.Object);

            var values = new { Controller = "c", Action = "a", Id = "i" };
            RedirectToRouteResult result = new RedirectToRouteResult(null, new RouteValueDictionary(values), permanent: true)
            {
                Routes = new RouteCollection() { new Route("{controller}/{action}/{id}", null) },
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultPreservesTempData()
        {
            // Arrange
            TempDataDictionary tempData = new TempDataDictionary();
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";
            Mock<Controller> mockController = new Mock<Controller>() { CallBase = true };
            mockController.Object.TempData = tempData;
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.ApplicationPath).Returns("/somepath");
            mockControllerContext.Setup(c => c.HttpContext.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) => s);
            mockControllerContext.Setup(c => c.HttpContext.Response.Redirect("/somepath/c/a/i", false)).Verifiable();
            mockControllerContext.Setup(c => c.Controller).Returns(mockController.Object);

            var values = new { Controller = "c", Action = "a", Id = "i" };
            RedirectToRouteResult result = new RedirectToRouteResult(new RouteValueDictionary(values))
            {
                Routes = new RouteCollection() { new Route("{controller}/{action}/{id}", null) },
            };

            // Act
            object value = tempData["Foo"];
            result.ExecuteResult(mockControllerContext.Object);
            mockController.Object.TempData.Save(mockControllerContext.Object, new Mock<ITempDataProvider>().Object);

            // Assert
            Assert.True(tempData.ContainsKey("Foo"));
            Assert.True(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void ExecuteResultThrowsIfVirtualPathDataIsNull()
        {
            // Arrange
            var result = new RedirectToRouteResult(null)
            {
                Routes = new RouteCollection()
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { result.ExecuteResult(ControllerContextTest.CreateEmptyContext()); },
                "No route in the route table matches the supplied values.");
        }

        [Fact]
        public void ExecuteResultWithNullControllerContextThrows()
        {
            // Arrange
            var result = new RedirectToRouteResult(null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { result.ExecuteResult(null /* context */); },
                "context");
        }

        [Fact]
        public void RoutesPropertyDefaultsToGlobalRouteTable()
        {
            // Act
            var result = new RedirectToRouteResult(new RouteValueDictionary());

            // Assert
            Assert.Same(RouteTable.Routes, result.Routes);
        }

        [Fact]
        public void RedirectInChildActionThrows()
        {
            // Arrange
            RouteData routeData = new RouteData();
            routeData.DataTokens[ControllerContext.ParentActionViewContextToken] = new ViewContext();
            ControllerContext context = new ControllerContext(new Mock<HttpContextBase>().Object, routeData, new Mock<ControllerBase>().Object);
            RedirectToRouteResult result = new RedirectToRouteResult(new RouteValueDictionary());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => result.ExecuteResult(context),
                "Child actions are not allowed to perform redirect actions.");
        }
    }
}

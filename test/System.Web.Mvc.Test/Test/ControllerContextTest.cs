// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Web.Routing;
using System.Web.TestUtil;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ControllerContextTest
    {
        [Fact]
        public void ConstructorThrowsIfControllerIsNull()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            Controller controller = null;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ControllerContext(requestContext, controller); }, "controller");
        }

        [Fact]
        public void ConstructorThrowsIfRequestContextIsNull()
        {
            // Arrange
            RequestContext requestContext = null;
            Controller controller = new Mock<Controller>().Object;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ControllerContext(requestContext, controller); }, "requestContext");
        }

        [Fact]
        public void ConstructorWithHttpContextAndRouteData()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            RouteData routeData = new RouteData();
            Controller controller = new Mock<Controller>().Object;

            // Act
            ControllerContext controllerContext = new ControllerContext(httpContext, routeData, controller);

            // Assert
            Assert.Equal(httpContext, controllerContext.HttpContext);
            Assert.Equal(routeData, controllerContext.RouteData);
            Assert.Equal(controller, controllerContext.Controller);
        }

        [Fact]
        public void ControllerProperty()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            RouteData routeData = new RouteData();
            Controller controller = new Mock<Controller>().Object;

            // Act
            ControllerContext controllerContext = new ControllerContext(httpContext, routeData, controller);

            // Assert
            Assert.Equal(controller, controllerContext.Controller);
        }

        [Fact]
        public void CopyConstructorSetsProperties()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            RequestContext requestContext = new RequestContext(httpContext.Object, new RouteData());
            Controller controller = new Mock<Controller>().Object;
            var displayMode = new DefaultDisplayMode("test");

            ControllerContext innerControllerContext = new ControllerContext(requestContext, controller);
            innerControllerContext.DisplayMode = displayMode;

            // Act
            ControllerContext outerControllerContext = new SubclassedControllerContext(innerControllerContext);

            // Assert
            Assert.Equal(requestContext, outerControllerContext.RequestContext);
            Assert.Equal(controller, outerControllerContext.Controller);

            // We don't actually set DisplayMode but verify it is identical to the inner controller context.
            Assert.Equal(displayMode, outerControllerContext.DisplayMode);
        }

        [Fact]
        public void DisplayModeDelegatesToHttpContext()
        {
            // Arrange
            IDisplayMode testDisplayMode = new DefaultDisplayMode("test");
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.Items).Returns(new Hashtable());
            Controller controller = new Mock<Controller>().Object;
            ControllerContext controllerContext = new ControllerContext(httpContext.Object, new RouteData(), controller);
            ControllerContext controllerContextWithIdenticalContext = new ControllerContext(httpContext.Object, new RouteData(), controller);

            // Act
            controllerContext.DisplayMode = testDisplayMode;

            // Assert
            Assert.Same(testDisplayMode, controllerContext.DisplayMode);
            Assert.Same(testDisplayMode, controllerContextWithIdenticalContext.DisplayMode);
        }

        [Fact]
        public void CopyConstructorThrowsIfControllerContextIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new SubclassedControllerContext(null); }, "controllerContext");
        }

        [Fact]
        public void HttpContextPropertyGetSetBehavior()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            ControllerContext controllerContext = new ControllerContext();

            // Act & assert
            MemberHelper.TestPropertyValue(controllerContext, "HttpContext", httpContext);
        }

        [Fact]
        public void HttpContextPropertyReturnsEmptyHttpContextIfRequestContextNotPresent()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();

            // Act
            HttpContextBase httpContext = controllerContext.HttpContext;
            HttpContextBase httpContext2 = controllerContext.HttpContext;

            // Assert
            Assert.NotNull(httpContext);
            Assert.Equal(httpContext, httpContext2);
        }

        [Fact]
        public void HttpContextPropertyReturnsRequestContextHttpContextIfPresent()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            RouteData routeData = new RouteData();
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            Controller controller = new Mock<Controller>().Object;

            // Act
            ControllerContext controllerContext = new ControllerContext(requestContext, controller);

            // Assert
            Assert.Equal(httpContext, controllerContext.HttpContext);
        }

        [Fact]
        public void RequestContextPropertyCreatesDummyHttpContextAndRouteDataIfNecessary()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext).Returns((HttpContextBase)null);
            mockControllerContext.Setup(c => c.RouteData).Returns((RouteData)null);
            ControllerContext controllerContext = mockControllerContext.Object;

            // Act
            RequestContext requestContext = controllerContext.RequestContext;
            RequestContext requestContext2 = controllerContext.RequestContext;

            // Assert
            Assert.Equal(requestContext, requestContext2);
            Assert.NotNull(requestContext.HttpContext);
            Assert.NotNull(requestContext.RouteData);
        }

        [Fact]
        public void RequestContextPropertyUsesExistingHttpContextAndRouteData()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            RouteData routeData = new RouteData();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext).Returns(httpContext);
            mockControllerContext.Setup(c => c.RouteData).Returns(routeData);
            ControllerContext controllerContext = mockControllerContext.Object;

            // Act
            RequestContext requestContext = controllerContext.RequestContext;
            RequestContext requestContext2 = controllerContext.RequestContext;

            // Assert
            Assert.Equal(requestContext, requestContext2);
            Assert.Equal(httpContext, requestContext.HttpContext);
            Assert.Equal(routeData, requestContext.RouteData);
        }

        [Fact]
        public void RouteDataPropertyGetSetBehavior()
        {
            // Arrange
            RouteData routeData = new RouteData();
            ControllerContext controllerContext = new ControllerContext();

            // Act & assert
            MemberHelper.TestPropertyValue(controllerContext, "RouteData", routeData);
        }

        [Fact]
        public void RouteDataPropertyReturnsEmptyRouteDataIfRequestContextNotPresent()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();

            // Act
            RouteData routeData = controllerContext.RouteData;
            RouteData routeData2 = controllerContext.RouteData;

            // Assert
            Assert.Equal(routeData, routeData2);
            Assert.Empty(routeData.Values);
        }

        [Fact]
        public void RouteDataPropertyReturnsRequestContextRouteDataIfPresent()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            RouteData routeData = new RouteData();
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            Controller controller = new Mock<Controller>().Object;

            // Act
            ControllerContext controllerContext = new ControllerContext(requestContext, controller);

            // Assert
            Assert.Equal(routeData, controllerContext.RouteData);
        }

        [Fact]
        public void IsChildActionReturnsFalseByDefault()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            RouteData routeData = new RouteData();
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            Controller controller = new Mock<Controller>().Object;
            ControllerContext controllerContext = new ControllerContext(requestContext, controller);

            // Act
            bool result = controllerContext.IsChildAction;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsChildActionReturnsTrueWhenRouteDataTokenIsSet()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            ViewContext viewContext = new ViewContext();
            RouteData routeData = new RouteData();
            routeData.DataTokens[ControllerContext.ParentActionViewContextToken] = viewContext;
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            Controller controller = new Mock<Controller>().Object;
            ControllerContext controllerContext = new ControllerContext(requestContext, controller);

            // Act
            bool result = controllerContext.IsChildAction;

            // Assert
            Assert.True(result);
            Assert.Same(viewContext, controllerContext.ParentActionViewContext);
        }

        public static ControllerContext CreateEmptyContext()
        {
            return new ControllerContext(new Mock<HttpContextBase>().Object, new RouteData(), new Mock<Controller>().Object);
        }

        private class SubclassedControllerContext : ControllerContext
        {
            public SubclassedControllerContext(ControllerContext controllerContext)
                : base(controllerContext)
            {
            }
        }
    }
}

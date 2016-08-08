// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class MvcRouteHandlerTest
    {
        [Fact]
        public void GetHttpHandlerReturnsMvcHandlerWithRouteData()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            var context = new RequestContext(new Mock<HttpContextBase>().Object, routeData);
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(f => f.GetControllerSessionBehavior(context, "controllerName"))
                .Returns(SessionStateBehavior.Default)
                .Verifiable();
            IRouteHandler rh = new MvcRouteHandler(controllerFactory.Object);

            // Act
            IHttpHandler httpHandler = rh.GetHttpHandler(context);

            // Assert
            MvcHandler h = httpHandler as MvcHandler;
            Assert.NotNull(h);
            Assert.Equal(context, h.RequestContext);
        }

        [Fact]
        public void GetHttpHandlerAsksControllerFactoryForSessionBehaviorOfController()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            var requestContext = new RequestContext(httpContext.Object, routeData);
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(f => f.GetControllerSessionBehavior(requestContext, "controllerName"))
                .Returns(SessionStateBehavior.ReadOnly)
                .Verifiable();
            IRouteHandler routeHandler = new MvcRouteHandler(controllerFactory.Object);

            // Act
            routeHandler.GetHttpHandler(requestContext);

            // Assert
            controllerFactory.Verify();
            httpContext.Verify(c => c.SetSessionStateBehavior(SessionStateBehavior.ReadOnly));
        }

        [Fact]
        public void GetHttpHandlerThrowsIfTheRouteValuesDoesNotIncludeAControllerName()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            var requestContext = new RequestContext(httpContext.Object, routeData);
            IRouteHandler routeHandler = new MvcRouteHandler();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => routeHandler.GetHttpHandler(requestContext),
                "The matched route does not include a 'controller' route value, which is required."
                );
        }
    }
}

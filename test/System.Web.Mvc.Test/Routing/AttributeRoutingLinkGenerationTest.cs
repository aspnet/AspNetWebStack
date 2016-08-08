// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Routing.Test
{
    public class AttributeRoutingLinkGenerationTest
    {
        [Fact]
        public void GenerateLink_ToArea_ControllerRoute_WithPrefix()
        {
            // Arrange
            Type[] controllerTypes = new Type[] { typeof(AreaWithPrefixWithControllerRouteController) };
            RouteCollection routes = MapControllers(controllerTypes);
            RequestContext requestContext = GetRequestContext();

            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "controller", "AreaWithPrefixWithControllerRoute" },
                { "action", "A1" },
                { "area", "Administration" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal("/Admin/Home/A1", vpd.VirtualPath);
        }

        [Fact]
        public void GenerateLink_ToArea_ActionRoute_WithPrefix()
        {
            // Arrange
            Type[] controllerTypes = new Type[] { typeof(AreaWithPrefixWithControllerRouteController) };
            RouteCollection routes = MapControllers(controllerTypes);
            RequestContext requestContext = GetRequestContext();

            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "controller", "AreaWithPrefixWithControllerRoute" },
                { "action", "A2" },
                { "area", "Administration" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal("/Admin", vpd.VirtualPath);
        }

        [Fact]
        public void GenerateLink_ToArea_ActionRoute_WithPrefix_WithinController()
        {
            // Arrange
            Type[] controllerTypes = new Type[] { typeof(AreaWithPrefixWithControllerRouteController) };
            RouteCollection routes = MapControllers(controllerTypes);
            RequestContext requestContext = GetRequestContext();

            requestContext.RouteData.DataTokens.Add("area", "Administration");
            requestContext.RouteData.DataTokens.Add("controller", "AreaWithPrefixWithControllerRoute");

            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "action", "A2" },
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal("/Admin", vpd.VirtualPath);
        }

        [Fact]
        public void GenerateLink_ToController_PrefersActionRoute()
        {
            // Arrange
            Type[] controllerTypes = new Type[] { typeof(MixedRoutingController) };
            RouteCollection routes = MapControllers(controllerTypes);
            RequestContext requestContext = GetRequestContext();

            requestContext.RouteData.DataTokens.Add("controller", "MixedRouting");

            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "action", "A2" },
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal("/A2", vpd.VirtualPath);
            Assert.True(((Route)vpd.Route).GetTargetIsAction());
        }

        [Fact]
        public void GenerateLink_ToController_PrefersControllerRouteWithOrder()
        {
            // Arrange
            Type[] controllerTypes = new Type[] { typeof(MixedRoutingWithOrderController) };
            RouteCollection routes = MapControllers(controllerTypes);
            RequestContext requestContext = GetRequestContext();

            requestContext.RouteData.DataTokens.Add("controller", "MixedRoutingWithOrder");

            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "action", "A2" },
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal("/mixedroutingwithorder/A2", vpd.VirtualPath);
            Assert.False(((Route)vpd.Route).GetTargetIsAction());
        }

        /// <summary>
        /// This test validates that these routes aren't overly greedy. We don't want the route
        /// for C2 to match when we're looking for C1.
        /// </summary>
        [Theory]
        [InlineData(typeof(Controller1Controller), typeof(Controller2Controller))]
        [InlineData(typeof(Controller2Controller), typeof(Controller1Controller))]
        public void GenerateLink_MultiplControllerRoutesMatch(Type c1, Type c2)
        {
            // Arrange
            Type[] controllerTypes = new Type[] { c1, c2 };
            RouteCollection routes = MapControllers(controllerTypes);
            RequestContext requestContext = GetRequestContext();

            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "action", "A1" },
                { "controller", "Controller1" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal("/c1/A1", vpd.VirtualPath);
        }

        private static RequestContext GetRequestContext()
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Request.ApplicationPath).Returns("/");
            mockHttpContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(virtualPath => virtualPath);

            return new RequestContext(mockHttpContext.Object, new RouteData());
        }

        private RouteCollection MapControllers(Type[] types)
        {
            RouteCollection routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, types);
            return routes;
        }

        [Route("Home/{action}")]
        [RouteArea("Administration", AreaPrefix = "Admin")]
        private class AreaWithPrefixWithControllerRouteController : Controller
        {
            public void A1()
            {
            }

            [Route]
            public void A2()
            {
            }
        }

        [Route("c1/{action}")]
        private class Controller1Controller : Controller
        {
            public void A1()
            {
            }
        }

        [Route("c2/{action}")]
        private class Controller2Controller : Controller
        {
            public void A1()
            {
            }
        }

        [Route("mixedrouting/{action}")]
        private class MixedRoutingController : Controller
        {
            public void A1()
            {
            }

            // The catch-all parameter is here to make sure this route has a worse precedence
            // than the controller-level route.
            [Route("A2/{*params}")]
            public void A2()
            {
            }
        }

        [Route("mixedroutingwithorder/{action}")]
        private class MixedRoutingWithOrderController : Controller
        {
            public void A1()
            {
            }

            // The order makes this 'worse' than the controller level route - if a user does this it's likely
            // because they have two actions with the same name.
            [Route("A2/{*params}", Order=55)]
            public void A2()
            {
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class RouteCollectionExtensionsTest
    {
        private static string[] _nameSpaces = new string[] { "nsA.nsB.nsC", "ns1.ns2.ns3" };

        [Fact]
        public void GetVirtualPathForAreaDoesNotStripAreaTokenIfAreasNotInUse()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            routes.MapRoute(
                "Default",
                "no-area/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
                );

            RequestContext requestContext = GetRequestContext(null);
            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "controller", "home" },
                { "action", "about" },
                { "area", "some-area" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal(routes["Default"], vpd.Route);

            // note presence of 'area' query string parameter; RVD should not be modified if areas not in use
            Assert.Equal("/app/no-area/home/about?area=some-area", vpd.VirtualPath);
        }

        [Fact]
        public void GetVirtualPathForAreaForwardsCallIfRouteNameSpecified()
        {
            // Arrange
            RouteCollection routes = GetRouteCollection();
            RequestContext requestContext = GetRequestContext(null);
            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "controller", "home" },
                { "action", "index" },
                { "area", "some-area" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, "admin_default", values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal(routes["admin_default"], vpd.Route);

            // note presence of 'area' query string parameter; RVD should not be modified if route name was provided
            Assert.Equal("/app/admin-area?area=some-area", vpd.VirtualPath);
        }

        [Fact]
        public void GetVirtualPathForAreaThrowsIfRoutesIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { RouteCollectionExtensions.GetVirtualPathForArea(null, null, null); }, "routes");
        }

        [Fact]
        public void GetVirtualPathForAreaWillJumpBetweenAreasExplicitly()
        {
            // Arrange
            RouteCollection routes = GetRouteCollection();
            RequestContext requestContext = GetRequestContext(null);
            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "controller", "home" },
                { "action", "tenmostrecent" },
                { "tag", "some-tag" },
                { "area", "blog" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal(routes["blog_whatsnew"], vpd.Route);
            Assert.Equal("/app/whats-new/some-tag", vpd.VirtualPath);
        }

        [Fact]
        public void GetVirtualPathForAreaWillNotJumpBetweenAreasImplicitly()
        {
            // Arrange
            RouteCollection routes = GetRouteCollection();
            RequestContext requestContext = GetRequestContext("admin");
            RouteValueDictionary values = new RouteValueDictionary()
            {
                { "controller", "home" },
                { "action", "tenmostrecent" },
                { "tag", "some-tag" }
            };

            // Act
            VirtualPathData vpd = routes.GetVirtualPathForArea(requestContext, values);

            // Assert
            Assert.NotNull(vpd);
            Assert.Equal(routes["admin_default"], vpd.Route);
            Assert.Equal("/app/admin-area/home/tenmostrecent?tag=some-tag", vpd.VirtualPath);
        }

        [Fact]
        public void MapRoute3()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();

            // Act
            routes.MapRoute("RouteName", "SomeUrl");

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Same(route, routes["RouteName"]);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Empty(route.Defaults);
            Assert.Empty(route.Constraints);
            Assert.Empty(route.DataTokens);
        }

        [Fact]
        public void MapRoute3WithNameSpaces()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            //string[] namespaces = new string[] { "nsA.nsB.nsC", "ns1.ns2.ns3" };

            // Act
            routes.MapRoute("RouteName", "SomeUrl", _nameSpaces);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.NotNull(route.DataTokens);
            Assert.NotNull(route.DataTokens["Namespaces"]);
            string[] routeNameSpaces = route.DataTokens["Namespaces"] as string[];
            Assert.Equal(2, routeNameSpaces.Length);
            Assert.Same(route, routes["RouteName"]);
            Assert.Same(routeNameSpaces, _nameSpaces);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Empty(route.Defaults);
            Assert.Empty(route.Constraints);
        }

        [Fact]
        public void MapRoute3WithEmptyNameSpaces()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();

            // Act
            routes.MapRoute("RouteName", "SomeUrl", new string[] { });

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Same(route, routes["RouteName"]);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Empty(route.Defaults);
            Assert.Empty(route.Constraints);
            Assert.Empty(route.DataTokens);
        }

        [Fact]
        public void MapRoute4()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            var defaults = new { Foo = "DefaultFoo" };

            // Act
            routes.MapRoute("RouteName", "SomeUrl", defaults);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Same(route, routes["RouteName"]);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Equal("DefaultFoo", route.Defaults["Foo"]);
            Assert.Empty(route.Constraints);
            Assert.Empty(route.DataTokens);
        }

        [Fact]
        public void MapRoute4WithNameSpaces()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            var defaults = new { Foo = "DefaultFoo" };

            // Act
            routes.MapRoute("RouteName", "SomeUrl", defaults, _nameSpaces);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.NotNull(route.DataTokens);
            Assert.NotNull(route.DataTokens["Namespaces"]);
            string[] routeNameSpaces = route.DataTokens["Namespaces"] as string[];
            Assert.Equal(2, routeNameSpaces.Length);
            Assert.Same(route, routes["RouteName"]);
            Assert.Same(routeNameSpaces, _nameSpaces);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Equal("DefaultFoo", route.Defaults["Foo"]);
            Assert.Empty(route.Constraints);
        }

        [Fact]
        public void MapRoute4WithDefaultsAsDictionary()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            var defaults = new Dictionary<string, object> { { "Foo", "DefaultFoo" } };

            // Act
            routes.MapRoute("RouteName", "SomeUrl", defaults);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Same(route, routes["RouteName"]);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Equal("DefaultFoo", route.Defaults["Foo"]);
            Assert.Empty(route.Constraints);
            Assert.Empty(route.DataTokens);
        }

        [Fact]
        public void MapRoute5()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            var defaults = new { Foo = "DefaultFoo" };
            var constraints = new { Foo = "ConstraintFoo" };

            // Act
            routes.MapRoute("RouteName", "SomeUrl", defaults, constraints);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Same(route, routes["RouteName"]);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Equal("DefaultFoo", route.Defaults["Foo"]);
            Assert.Equal("ConstraintFoo", route.Constraints["Foo"]);
            Assert.Empty(route.DataTokens);
        }

        [Fact]
        public void MapRoute5WithDefaultsAndConstraintsAsDictionary()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            var defaults = new Dictionary<string, object> { { "Foo", "DefaultFoo" } };
            var constraints = new Dictionary<string, object> { { "Foo", "ConstraintFoo" } };

            // Act
            routes.MapRoute("RouteName", "SomeUrl", defaults, constraints);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Same(route, routes["RouteName"]);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<MvcRouteHandler>(route.RouteHandler);
            Assert.Equal("DefaultFoo", route.Defaults["Foo"]);
            Assert.Equal("ConstraintFoo", route.Constraints["Foo"]);
            Assert.Empty(route.DataTokens);
        }

        [Fact]
        public void MapRoute5WithNullRouteCollectionThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { RouteCollectionExtensions.MapRoute(null, null, null, null, null); },
                "routes");
        }

        [Fact]
        public void MapRoute5WithNullUrlThrows()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { routes.MapRoute(null, null /* url */, null, null); },
                "url");
        }

        [Fact]
        public void IgnoreRoute1WithNullRouteCollectionThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { RouteCollectionExtensions.IgnoreRoute(null, "foo"); },
                "routes");
        }

        [Fact]
        public void IgnoreRoute1WithNullUrlThrows()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { routes.IgnoreRoute(null); },
                "url");
        }

        [Fact]
        public void IgnoreRoute3()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();

            // Act
            routes.IgnoreRoute("SomeUrl");

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<StopRoutingHandler>(route.RouteHandler);
            Assert.Null(route.Defaults);
            Assert.Empty(route.Constraints);
        }

        [Fact]
        public void IgnoreRoute4()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            var constraints = new { Foo = "DefaultFoo" };

            // Act
            routes.IgnoreRoute("SomeUrl", constraints);

            // Assert
            Route route = Assert.Single(routes.Cast<Route>());
            Assert.NotNull(route);
            Assert.Equal("SomeUrl", route.Url);
            Assert.IsType<StopRoutingHandler>(route.RouteHandler);
            Assert.Null(route.Defaults);
            Assert.Single(route.Constraints);
            Assert.Equal("DefaultFoo", route.Constraints["Foo"]);
        }

        [Fact]
        public void IgnoreRouteInternalNeverMatchesUrlGeneration()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            routes.IgnoreRoute("SomeUrl");
            Route route = routes[0] as Route;

            // Act
            VirtualPathData vpd = route.GetVirtualPath(new RequestContext(new Mock<HttpContextBase>().Object, new RouteData()), null);

            // Assert
            Assert.Null(vpd);
        }

        [Fact]
        public void MapRoute_ValidatesConstraintType_IRouteConstraint()
        {
            // Arrange
            var routes = new RouteCollection();

            var constraint = new CustomConstraint();
            var constraints = new RouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.MapRoute("default", "{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);
        }

        [Fact]
        public void MapRoute_ValidatesConstraintType_StringRegex()
        {
            // Arrange
            var routes = new RouteCollection();

            // We can't easily mock the ValidateConstraint method because all of this logic is in extension methods,
            // so we're just assuming here that it was called.
            var constraint = "product|products";
            var constraints = new RouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.MapRoute("default", "{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);
        }

        [Fact]
        public void MapRoute_ValidatesConstraintType_InvalidType()
        {
            // Arrange
            var routes = new RouteCollection();

            // We can't easily mock the ValidateConstraint method because all of this logic is in extension methods,
            // so we're just assuming here that it was called.
            var constraint = new Uri("http://localhost/");
            var constraints = new RouteValueDictionary();
            constraints.Add("custom", constraint);

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template '{controller}/{id}' " +
                "must have a string value or be of a type which implements 'System.Web.Routing.IRouteConstraint'.";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => routes.MapRoute("default", "{controller}/{id}", null, constraints), expectedMessage);
        }

        [Fact]
        public void IgnoreRoute_ValidatesConstraintType_InvalidType()
        {
            // Arrange
            var routes = new RouteCollection();

            var constraint = new Uri("http://localhost/");
            var constraints = new RouteValueDictionary();
            constraints.Add("custom", constraint);

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template '{controller}/{id}' " +
                "must have a string value or be of a type which implements 'System.Web.Routing.IRouteConstraint'.";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => routes.IgnoreRoute("{controller}/{id}", constraints), expectedMessage);
        }

        private static RequestContext GetRequestContext(string currentAreaName)
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Request.ApplicationPath).Returns("/app");
            mockHttpContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(virtualPath => virtualPath);

            RouteData routeData = new RouteData();
            routeData.DataTokens["area"] = currentAreaName;
            return new RequestContext(mockHttpContext.Object, routeData);
        }

        private static RouteCollection GetRouteCollection()
        {
            RouteCollection routes = new RouteCollection();
            routes.MapRoute(
                "Default",
                "no-area/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
                );

            AreaRegistrationContext blogContext = new AreaRegistrationContext("blog", routes);
            blogContext.MapRoute(
                "Blog_WhatsNew",
                "whats-new/{tag}",
                new { controller = "Home", action = "TenMostRecent", tag = "" }
                );
            blogContext.MapRoute(
                "Blog_Default",
                "blog-area/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
                );

            AreaRegistrationContext adminContext = new AreaRegistrationContext("admin", routes);
            adminContext.MapRoute(
                "Admin_Default",
                "admin-area/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
                );

            return routes;
        }

        private class CustomConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }
    }
}

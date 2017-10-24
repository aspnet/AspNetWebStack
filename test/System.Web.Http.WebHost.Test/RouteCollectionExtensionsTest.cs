// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class RouteCollectionExtensionsTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(RouteCollectionExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void MapHttpRoute1ThrowsOnNullRouteCollection()
        {
            Assert.ThrowsArgumentNull(() => RouteCollectionExtensions.MapHttpRoute(null, "", "", null), "routes");
        }

        [Fact]
        public void MapHttpRoute1CreatesRoute()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            object defaults = new { d1 = "D1" };

            // Act
            Route route = routes.MapHttpRoute("name", "template", defaults);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.Url);
            Assert.Single(route.Defaults);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute1WithDefaultsAsDictionaryCreatesRoute()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            object defaults = new Dictionary<string, object> { { "d1", "D1" } };

            // Act
            Route route = routes.MapHttpRoute("name", "template", defaults);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.Url);
            Assert.Single(route.Defaults);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute2ThrowsOnNullRouteCollection()
        {
            Assert.ThrowsArgumentNull(() => RouteCollectionExtensions.MapHttpRoute(null, "", "", null, null), "routes");
        }

        [Fact]
        public void MapHttpRoute2CreatesRoute()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            object defaults = new { d1 = "D1" };
            object constraints = new { c1 = "C1" };

            // Act
            Route route = routes.MapHttpRoute("name", "template", defaults, constraints);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.Url);
            Assert.Single(route.Defaults);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Single(route.Constraints);
            Assert.Equal("C1", route.Constraints["c1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute2WithDefaultsAndConstraintsAsDictionaryCreatesRoute()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            object defaults = new Dictionary<string, object> { { "d1", "D1" } };
            object constraints = new Dictionary<string, object> { { "c1", "C1" } };

            // Act
            Route route = routes.MapHttpRoute("name", "template", defaults, constraints);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.Url);
            Assert.Single(route.Defaults);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Single(route.Constraints);
            Assert.Equal("C1", route.Constraints["c1"]);
            Assert.Same(route, routes["name"]);
        }
    }
}

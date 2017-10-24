// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Batch;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpRouteCollectionExtensionsTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(HttpRouteCollectionExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void MapHttpRoute1ThrowsOnNullRouteCollection()
        {
            Assert.ThrowsArgumentNull(() => HttpRouteCollectionExtensions.MapHttpRoute(null, "", "", null), "routes");
        }

        [Fact]
        public void MapHttpRoute1CreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new { d1 = "D1" };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute1WithDefaultsAsDictionaryCreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new Dictionary<string, object> { { "d1", "D1" } };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute2ThrowsOnNullRouteCollection()
        {
            Assert.ThrowsArgumentNull(() => HttpRouteCollectionExtensions.MapHttpRoute(null, "", "", null, null), "routes");
        }

        [Fact]
        public void MapHttpRoute2CreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new { d1 = "D1" };
            object constraints = new { c1 = "C1" };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults, constraints);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("C1", route.Constraints["c1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpRoute2WithDefaultsAndConstraintsAsDictionaryCreatesRoute()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            object defaults = new Dictionary<string, object> { { "d1", "D1" } };
            object constraints = new Dictionary<string, object> { { "c1", "C1" } };

            // Act
            IHttpRoute route = routes.MapHttpRoute("name", "template", defaults, constraints);

            // Assert
            Assert.NotNull(route);
            Assert.Equal("template", route.RouteTemplate);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("D1", route.Defaults["d1"]);
            Assert.Equal(1, route.Defaults.Count);
            Assert.Equal("C1", route.Constraints["c1"]);
            Assert.Same(route, routes["name"]);
        }

        [Fact]
        public void MapHttpBatchRoute_CreatesRoutesUsingCustomBatchHandler()
        {
            HttpRouteCollection routes = new HttpRouteCollection();
            HttpBatchHandler mockBatchHandler = new Mock<HttpBatchHandler>(new HttpServer()).Object;
            IHttpRoute route = routes.MapHttpBatchRoute("batch", "api/batch", mockBatchHandler);

            Assert.NotNull(route);
            Assert.Equal("api/batch", route.RouteTemplate);
            Assert.Same(route, routes["batch"]);
            Assert.Same(mockBatchHandler, route.Handler);
        }

        [Fact]
        public void IgnoreRouteWithNullRouteCollectionThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { HttpRouteCollectionExtensions.IgnoreRoute(null, "bar", "foo"); },
                "routes");
        }

        [Fact]
        public void IgnoreRouteWithNullRouteNameThrows()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { routes.IgnoreRoute(null, "foo"); },
                "routeName");
        }

        [Fact]
        public void IgnoreRouteWithNullUrlThrows()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { routes.IgnoreRoute("foo", null); },
                "routeTemplate");
        }

        [Fact]
        public void IgnoreRouteWithoutConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();

            // Act
            routes.IgnoreRoute("SomeRouteName", "SomeRouteTemplate");

            // Assert
            HttpRoute route = Assert.Single(routes.Cast<HttpRoute>());
            Assert.NotNull(route);
            Assert.Equal("SomeRouteTemplate", route.RouteTemplate);
            Assert.IsType<StopRoutingHandler>(route.Handler);
            Assert.Empty(route.Defaults);
            Assert.Empty(route.Constraints);
        }

        [Fact]
        public void IgnoreRouteWithConstraints()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            var constraints = new { Foo = "DefaultFoo" };

            // Act
            routes.IgnoreRoute("SomeRouteName", "SomeRouteTemplate", constraints);

            // Assert
            HttpRoute route = Assert.Single(routes.Cast<HttpRoute>());
            Assert.NotNull(route);
            Assert.Equal("SomeRouteTemplate", route.RouteTemplate);
            Assert.IsType<StopRoutingHandler>(route.Handler);
            Assert.Empty(route.Defaults);
            Assert.Single(route.Constraints);
            Assert.Equal("DefaultFoo", route.Constraints["Foo"]);
        }

        [Fact]
        public void IgnoreRouteInternalNeverMatchesUrlGeneration()
        {
            // Arrange
            HttpRouteCollection routes = new HttpRouteCollection();
            IHttpRoute route = routes.IgnoreRoute("Foo", "SomeRouteTemplate");

            // Act
            IHttpVirtualPathData vpd = route.GetVirtualPath(new HttpRequestMessage(HttpMethod.Get, "SomeRouteTemplate"), null);

            // Assert
            Assert.Null(vpd);
        }
    }
}

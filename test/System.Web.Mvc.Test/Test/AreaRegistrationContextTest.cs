// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class AreaRegistrationContextTest
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            // Arrange
            string areaName = "the_area";
            RouteCollection routes = new RouteCollection();

            // Act
            AreaRegistrationContext context = new AreaRegistrationContext(areaName, routes);

            // Assert
            Assert.Equal(areaName, context.AreaName);
            Assert.Same(routes, context.Routes);
        }

        [Fact]
        public void ConstructorThrowsIfAreaNameIsEmpty()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new AreaRegistrationContext("", new RouteCollection()); }, "areaName");
        }

        [Fact]
        public void ConstructorThrowsIfAreaNameIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new AreaRegistrationContext(null, new RouteCollection()); }, "areaName");
        }

        [Fact]
        public void ConstructorThrowsIfRoutesIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new AreaRegistrationContext("the_area", null); }, "routes");
        }

        [Fact]
        public void MapRouteWithEmptyStringNamespaces()
        {
            // Arrange
            string[] implicitNamespaces = new string[] { "implicit_1", "implicit_2" };
            string[] explicitNamespaces = new string[0];

            RouteCollection routes = new RouteCollection();
            AreaRegistrationContext context = new AreaRegistrationContext("the_area", routes);
            ReplaceCollectionContents(context.Namespaces, implicitNamespaces);

            // Act
            Route route = context.MapRoute("the_name", "the_url", explicitNamespaces);

            // Assert
            Assert.Equal(route, routes["the_name"]);
            Assert.Equal("the_area", route.DataTokens["area"]);
            Assert.Equal((object)true, route.DataTokens["UseNamespaceFallback"]);
            Assert.Null(route.DataTokens["namespaces"]);
        }

        [Fact]
        public void MapRouteWithExplicitNamespaces()
        {
            // Arrange
            string[] implicitNamespaces = new string[] { "implicit_1", "implicit_2" };
            string[] explicitNamespaces = new string[] { "explicit_1", "explicit_2" };

            RouteCollection routes = new RouteCollection();
            AreaRegistrationContext context = new AreaRegistrationContext("the_area", routes);
            ReplaceCollectionContents(context.Namespaces, implicitNamespaces);

            // Act
            Route route = context.MapRoute("the_name", "the_url", explicitNamespaces);

            // Assert
            Assert.Equal(route, routes["the_name"]);
            Assert.Equal("the_area", route.DataTokens["area"]);
            Assert.Equal((object)false, route.DataTokens["UseNamespaceFallback"]);
            Assert.Equal(explicitNamespaces, (string[])route.DataTokens["namespaces"]);
        }

        [Fact]
        public void MapRouteWithImplicitNamespaces()
        {
            // Arrange
            string[] implicitNamespaces = new string[] { "implicit_1", "implicit_2" };
            string[] explicitNamespaces = new string[] { "explicit_1", "explicit_2" };

            RouteCollection routes = new RouteCollection();
            AreaRegistrationContext context = new AreaRegistrationContext("the_area", routes);
            ReplaceCollectionContents(context.Namespaces, implicitNamespaces);

            // Act
            Route route = context.MapRoute("the_name", "the_url");

            // Assert
            Assert.Equal(route, routes["the_name"]);
            Assert.Equal("the_area", route.DataTokens["area"]);
            Assert.Equal((object)false, route.DataTokens["UseNamespaceFallback"]);
            Assert.Equal(implicitNamespaces, (string[])route.DataTokens["namespaces"]);
        }

        [Fact]
        public void MapRouteWithoutNamespaces()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            AreaRegistrationContext context = new AreaRegistrationContext("the_area", routes);

            // Act
            Route route = context.MapRoute("the_name", "the_url");

            // Assert
            Assert.Equal(route, routes["the_name"]);
            Assert.Equal("the_area", route.DataTokens["area"]);
            Assert.Null(route.DataTokens["namespaces"]);
            Assert.Equal((object)true, route.DataTokens["UseNamespaceFallback"]);
        }

        private static void ReplaceCollectionContents(ICollection<string> collectionToReplace, IEnumerable<string> newContents)
        {
            collectionToReplace.Clear();
            foreach (string item in newContents)
            {
                collectionToReplace.Add(item);
            }
        }
    }
}

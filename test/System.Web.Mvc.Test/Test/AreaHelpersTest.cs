// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class AreaHelpersTest
    {
        [Fact]
        public void GetAreaNameFromAreaRouteCollectionRoute()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            AreaRegistrationContext context = new AreaRegistrationContext("area_name", routes);
            Route route = context.MapRoute(null, "the_url");

            // Act
            string areaName = AreaHelpers.GetAreaName(route);

            // Assert
            Assert.Equal("area_name", areaName);
        }

        [Fact]
        public void GetAreaNameFromIAreaAssociatedItem()
        {
            // Arrange
            CustomRouteWithArea route = new CustomRouteWithArea();

            // Act
            string areaName = AreaHelpers.GetAreaName(route);

            // Assert
            Assert.Equal("area_name", areaName);
        }

        [Fact]
        public void GetAreaNameFromRouteData()
        {
            // Arrange
            RouteData routeData = new RouteData();
            routeData.DataTokens["area"] = "area_name";

            // Act
            string areaName = AreaHelpers.GetAreaName(routeData);

            // Assert
            Assert.Equal("area_name", areaName);
        }

        [Fact]
        public void GetAreaNameFromRouteDataFallsBackToRoute()
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            AreaRegistrationContext context = new AreaRegistrationContext("area_name", routes);
            Route route = context.MapRoute(null, "the_url");
            RouteData routeData = new RouteData(route, new MvcRouteHandler());

            // Act
            string areaName = AreaHelpers.GetAreaName(routeData);

            // Assert
            Assert.Equal("area_name", areaName);
        }

        [Fact]
        public void GetAreaNameReturnsNullIfRouteNotAreaAware()
        {
            // Arrange
            Route route = new Route("the_url", new MvcRouteHandler());

            // Act
            string areaName = AreaHelpers.GetAreaName(route);

            // Assert
            Assert.Null(areaName);
        }

        private class CustomRouteWithArea : RouteBase, IRouteWithArea
        {
            public string Area
            {
                get { return "area_name"; }
            }

            public override RouteData GetRouteData(HttpContextBase httpContext)
            {
                throw new NotImplementedException();
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
            {
                throw new NotImplementedException();
            }
        }
    }
}

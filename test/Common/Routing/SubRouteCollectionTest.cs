// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !ASPNETWEBAPI
using System.Web.Routing;
#endif
using Microsoft.TestCommon;
using Moq;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class SubRouteCollectionTest
    {
#if ASPNETWEBAPI
        [Fact]
        public void SubRouteCollection_Throws_OnDuplicateNamedRoute_WebAPI()
        {
            // Arrange
            var collection = new SubRouteCollection();
            var route1 = new HttpRoute("api/Person");
            var route2 = new HttpRoute("api/Car");

            collection.Add(new RouteEntry("route", route1));

            var expectedError =
                "A route named 'route' is already in the route collection. Route names must be unique.\r\n\r\n" +
                "Duplicates:" + Environment.NewLine +
                "api/Car" + Environment.NewLine +
                "api/Person";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => collection.Add(new RouteEntry("route", route2)), expectedError);
        }
#else
        [Fact]
        public void SubRouteCollection_Throws_OnDuplicateNamedRoute_MVC()
        {
            // Arrange
            var collection = new SubRouteCollection();
            var route1 = new Route("Home/Index", new Mock<IRouteHandler>().Object);
            var route2 = new Route("Person/Index", new Mock<IRouteHandler>().Object);

            collection.Add(new RouteEntry("route", route1));

            var expectedError =
                "A route named 'route' is already in the route collection. Route names must be unique.\r\n\r\n" +
                "Duplicates:" + Environment.NewLine +
                "Person/Index" + Environment.NewLine +
                "Home/Index";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => collection.Add(new RouteEntry("route", route2)), expectedError);
        }
#endif
    }
}

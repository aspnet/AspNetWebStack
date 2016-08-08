// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProviderTest
    {
        [Fact]
        public void GetRouteValues_Returns_RouteValues()
        {
            // Arrange
            HttpRouteData routeData = new HttpRouteData(new HttpRoute());
            routeData.Values.Add("key1", "value1");
            routeData.Values.Add("key2", "value2");

            // Act
            IEnumerable<KeyValuePair<string, string>> result = RouteDataValueProvider.GetRouteValues(routeData);

            // Assert
            Assert.Equal(result, new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } });
        }

        [Fact]
        public void GetRouteValues_IfRouteDataHasNullValue_ReturnsKeyValuePairWithNullValue()
        {
            // Arrange
            HttpRouteData routeData = new HttpRouteData(new HttpRoute());
            routeData.Values.Add("key", null);

            // Act
            IEnumerable<KeyValuePair<string, string>> result = RouteDataValueProvider.GetRouteValues(routeData);

            // Assert
            Assert.Equal(result, new[] { new KeyValuePair<string, string>("key", null) });
        }
    }
}

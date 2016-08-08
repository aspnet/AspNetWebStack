// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProviderFactoryTest
    {
        private readonly RouteDataValueProviderFactory _factory = new RouteDataValueProviderFactory();

        [Fact]
        public void GetValueProvider_WhenActionContextParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _factory.GetValueProvider(actionContext: null), "actionContext");
        }

        [Fact]
        public void GetValueProvider_ReturnsQueryStringValueProviderInstaceWithInvariantCulture()
        {
            var routeData = new Mock<IHttpRouteData>();
            routeData.Setup(r => r.Values).Returns(() => new Dictionary<string, object>());
            var controllerContext = new HttpControllerContext() { Request = new HttpRequestMessage(), RouteData = routeData.Object };
            var context = new HttpActionContext() { ControllerContext = controllerContext };

            IValueProvider result = _factory.GetValueProvider(context);

            var valueProvider = Assert.IsType<RouteDataValueProvider>(result);
            Assert.Equal(CultureInfo.InvariantCulture, valueProvider.Culture);
        }
    }
}

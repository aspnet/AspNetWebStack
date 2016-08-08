// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class LinkGenerationRouteTests
    {
        [Fact]
        public void GenerateRoute_DoesNotClaimData()
        {
            LinkGenerationRoute route = new LinkGenerationRoute(new InnerRoute());

            IHttpRouteData data = route.GetRouteData(string.Empty, new HttpRequestMessage());

            Assert.Null(data);
        }

        [Fact]
        public void GenerateRoute_ForwardsInnerProperties()
        {
            IHttpRoute innerRoute = new InnerRoute();
            LinkGenerationRoute route = new LinkGenerationRoute(innerRoute);

            Assert.NotNull(route.Defaults);
            Assert.Equal(innerRoute.Defaults, route.Defaults);

            Assert.NotNull(route.Constraints);
            Assert.Equal(innerRoute.Constraints, route.Constraints);

            Assert.NotNull(route.DataTokens);
            Assert.Equal(innerRoute.DataTokens, route.DataTokens);

            Assert.NotNull(innerRoute.Handler);
            Assert.Null(route.Handler);

            Assert.NotNull(route.RouteTemplate);
            Assert.Equal(innerRoute.RouteTemplate, route.RouteTemplate);
        }

        [Fact]
        public void GenerateRoute_GetVirtualPathIsForwarded()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IDictionary<string, object> values = new Dictionary<string, object>();

            IHttpVirtualPathData data = new Mock<IHttpVirtualPathData>().Object;

            Mock<IHttpRoute> inner = new Mock<IHttpRoute>();
            inner.Setup(r => r.GetVirtualPath(request, values)).Returns(data);

            LinkGenerationRoute route = new LinkGenerationRoute(inner.Object);

            IHttpVirtualPathData result = route.GetVirtualPath(request, values);

            Assert.Equal(data, result);
        }

        // Route where everything is not implemented. Tests that the generated route is not forwarding calls. 
        private class InnerRoute : IHttpRoute
        {
            private readonly IHttpRouteData _routeData = new Mock<IHttpRouteData>().Object;
            private readonly IHttpVirtualPathData _virtualPathData = new Mock<IHttpVirtualPathData>().Object;

            public InnerRoute()
            {
                Defaults = new Dictionary<string, object>();
                Defaults.Add("default", "value");

                Constraints = new Dictionary<string, object>();
                Constraints.Add("constraint", "value");

                DataTokens = new Dictionary<string, object>();
                DataTokens.Add("token", "value");

                Handler = new Mock<HttpMessageHandler>().Object;
            }

            public string RouteTemplate
            {
                get { return "InnerRoute"; }
            }

            public IDictionary<string, object> Defaults { get; private set; }

            public IDictionary<string, object> Constraints { get; private set; }

            public IDictionary<string, object> DataTokens { get; private set; }

            public HttpMessageHandler Handler { get; private set; }

            public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
            {
                return _routeData;
            }

            public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, Collections.Generic.IDictionary<string, object> values)
            {
                return _virtualPathData;
            }
        }
    }
}

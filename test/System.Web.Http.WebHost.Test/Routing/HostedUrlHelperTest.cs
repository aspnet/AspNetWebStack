// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;
using UrlHelper = System.Web.Http.Routing.UrlHelper;

namespace System.Web.Http.WebHost.Routing
{
    public class HostedUrlHelperTest
    {
        [Theory]
        [InlineData(WhichRoute.ApiRoute1)]
        [InlineData(WhichRoute.ApiRoute2)]
        [InlineData(WhichRoute.WebRoute1)]
        public void UrlHelper_GeneratesApiUrl_ForMatchingData(WhichRoute whichRoute)
        {
            // Mixed mode app with Web API generating URLs to other APIs
            var url = GetUrlHelperForMixedApp(whichRoute);

            string generatedUrl = url.Route("apiroute2", new { controller = "something", action = "someaction", id = 789 });

            Assert.Equal("$APP$/SOMEAPP/api/something/someaction", generatedUrl);
        }

        [Theory]
        [InlineData(WhichRoute.ApiRoute1)]
        [InlineData(WhichRoute.ApiRoute2)]
        [InlineData(WhichRoute.WebRoute1)]
        public void UrlHelper_GeneratesNonApiUrl_ForMatchingData(WhichRoute whichRoute)
        {
            // Mixed mode app with Web API generating URLs to other non-API routes
            var url = GetUrlHelperForMixedApp(whichRoute);

            string generatedUrl = url.Route("webroute1", new { controller = "something", action = "someaction", id = 789 });

            Assert.Equal("$APP$/SOMEAPP/something/someaction/789", generatedUrl);
        }

        [Theory]
        [InlineData(WhichRoute.ApiRoute1)]
        [InlineData(WhichRoute.ApiRoute2)]
        [InlineData(WhichRoute.WebRoute1)]
        public void UrlHelper_GeneratesNullUrl_ForDataNotMatching(WhichRoute whichRoute)
        {
            // Mixed mode app with Web API generating URLs to other non-API routes
            var url = GetUrlHelperForMixedApp(whichRoute);

            string generatedUrl = url.Route("webroute1", new { foo = "bar" });

            Assert.Null(generatedUrl);
        }

        [Theory]
        [InlineData(WhichRoute.ApiRoute1)]
        [InlineData(WhichRoute.ApiRoute2)]
        [InlineData(WhichRoute.WebRoute1)]
        public void UrlHelper_SkipsApiRoutesAndMatchesMvcUrl_ForMatchingData(WhichRoute whichRoute)
        {
            // Mixed mode app with MVC generating URLs to other MVC URLs
            RouteCollection routes;
            RequestContext requestContext;
            var url = GetUrlHelperForMixedApp(whichRoute, out routes, out requestContext);

            // Note: This is generating a URL the "hard" way because it's simulating what a regular MVC
            // app would do when generating a URL. If we went through the Web API functionality it wouldn't
            // be testing what would really happen in a mixed app.
            VirtualPathData virtualPathData = routes.GetVirtualPath(requestContext, new RouteValueDictionary(new { controller = "something", action = "someaction", id = 789 }));

            Assert.NotNull(virtualPathData);

            string generatedUrl = virtualPathData.VirtualPath;

            Assert.Equal("$APP$/SOMEAPP/something/someaction/789", generatedUrl);
        }

        [Theory]
        [InlineData(WhichRoute.ApiRoute1)]
        [InlineData(WhichRoute.ApiRoute2)]
        [InlineData(WhichRoute.WebRoute1)]
        public void UrlHelper_MvcAppGeneratesApiRoute_WithSpecialHttpRouteKey(WhichRoute whichRoute)
        {
            // Mixed mode app with MVC generating URLs to Web APIs
            RouteCollection routes;
            RequestContext requestContext;
            var url = GetUrlHelperForMixedApp(whichRoute, out routes, out requestContext);

            // Note: This is generating a URL the "hard" way because it's simulating what a regular MVC
            // app would do when generating a URL. If we went through the Web API functionality it wouldn't
            // be testing what would really happen in a mixed app.
            VirtualPathData virtualPathData = routes.GetVirtualPath(requestContext, new RouteValueDictionary(new { controller = "something", action = "someotheraction", id = 789, httproute = true }));

            Assert.NotNull(virtualPathData);

            string generatedUrl = virtualPathData.VirtualPath;

            Assert.Equal("$APP$/SOMEAPP/api/something/someotheraction", generatedUrl);
        }

        [Theory]
        [InlineData("httproute")]
        [InlineData("httpRoute")]
        [InlineData("HTTPROUTE")]
        [InlineData("hTtProuTE")]
        public void UrlHelper_DoesntAddDuplicateHttpRoute_ForHttpRouteInDifferentCasing(string httpRoute)
        {
            // Mixed mode app with MVC generating URLs to Web APIs
            RouteCollection routes;
            RequestContext requestContext;
            var url = GetUrlHelperForMixedApp(WhichRoute.ApiRoute1, out routes, out requestContext);

            string generatedUrl = url.Route("apiroute1", new Dictionary<string, object> { { "controller", "something" }, { "id", 789 }, { httpRoute, true } });

            Assert.Equal("$APP$/SOMEAPP/api/something/789", generatedUrl);
        }

        private static UrlHelper GetUrlHelperForMixedApp(WhichRoute whichRoute)
        {
            RouteCollection routes;
            RequestContext requestContext;
            return GetUrlHelperForMixedApp(whichRoute, out routes, out requestContext);
        }

        private static UrlHelper GetUrlHelperForMixedApp(WhichRoute whichRoute, out RouteCollection routes, out RequestContext requestContext)
        {
            routes = new RouteCollection();

            HttpRequestMessage request = new HttpRequestMessage();
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockHttpRequest = new Mock<HttpRequestBase>();
            mockHttpRequest.SetupGet<string>(x => x.ApplicationPath).Returns("/SOMEAPP/");
            var mockHttpResponse = new Mock<HttpResponseBase>();
            mockHttpResponse.Setup<string>(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(x => "$APP$" + x);
            mockHttpContext.SetupGet<HttpRequestBase>(x => x.Request).Returns(mockHttpRequest.Object);
            mockHttpContext.SetupGet<HttpResponseBase>(x => x.Response).Returns(mockHttpResponse.Object);
            request.Properties["MS_HttpContext"] = mockHttpContext.Object;

            // Set up routes
            var hostedRoutes = new HostedHttpRouteCollection(routes);
            Route apiRoute1 = routes.MapHttpRoute("apiroute1", "api/{controller}/{id}", new { action = "someaction" });
            Route apiRoute2 = routes.MapHttpRoute("apiroute2", "api/{controller}/{action}", new { id = 789 });
            Route webRoute1 = routes.MapRoute("webroute1", "{controller}/{action}/{id}");
            request.SetConfiguration(new HttpConfiguration(hostedRoutes));

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "people");
            routeData.Values.Add("id", "123");

            // Specify which route we came in on (e.g. what request matching the incoming URL) because
            // it can affect the generated URL due to the ambient route data.
            switch (whichRoute)
            {
                case WhichRoute.ApiRoute1:
                    routeData.Route = apiRoute1;
                    break;
                case WhichRoute.ApiRoute2:
                    routeData.Route = apiRoute2;
                    break;
                case WhichRoute.WebRoute1:
                    routeData.Route = webRoute1;
                    break;
                default:
                    throw new ArgumentException("Invalid route specified.", "whichRoute");
            }
            request.SetRouteData(new HostedHttpRouteData(routeData));

            requestContext = new RequestContext(mockHttpContext.Object, routeData);

            return request.GetUrlHelper();
        }

        public enum WhichRoute
        {
            ApiRoute1,
            ApiRoute2,
            WebRoute1,
        }
    }
}

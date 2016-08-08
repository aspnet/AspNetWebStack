// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.Hosting
{
    public class HttpRouteTest
    {
        [Fact]
        public void Ctor_PreservesWhitespaceInRouteTemplate()
        {
            string whitespace = "   ";

            HttpRoute httpRoute = new HttpRoute(whitespace);

            Assert.Equal(whitespace, httpRoute.RouteTemplate);
        }

        [Theory]
        [InlineData("{controller}/{id}", "/SelfHostServer", "http://localhost/SelfHostServer/Customer/999")]
        [InlineData("{controller}/{id}", "", "http://localhost/Customer/999")]
        [InlineData("{controller}", "", "http://localhost/")]
        [InlineData("{controller}", "/SelfHostServer", "http://localhost/SelfHostServer")]
        [InlineData("{controller}", "", "http://localhost")]
        [InlineData("{controller}/{id}", "", "http://localhost/")]
        [InlineData("{controller}/{id}", "/SelfHostServer", "http://localhost/SelfHostServer")]
        [InlineData("{controller}/{id}", "", "http://localhost")]
        [InlineData("api", "", "http://localhost/api")]
        [InlineData("api", "", "http://LOCALHOST/API")]
        [InlineData("{controller}/{id}", "/SelfHostServer/Customer/999", "http://localhost/SelfHostServer/Customer/999")]
        [InlineData("{controller}/{id}", "/SelfHostServer/Customer/999", "http://localhost/SelfHostServer/Customer/999/")]
        [InlineData("{controller}/{id}", "/SelfHostServer/Customer/999/", "http://localhost/SelfHostServer/Customer/999/")]
        [InlineData("{controller}", "/SelfHostServer", "http://localhost/SelfHostServer/")]
        [InlineData("{controller}", "/SelfHostServer/", "http://localhost/SelfHostServer/")]
        public void GetRouteDataShouldMatch(string uriTemplate, string virtualPathRoot, string requestUri)
        {
            HttpRoute route = new HttpRoute(uriTemplate);
            route.Defaults.Add("controller", "Customer");
            route.Defaults.Add("id", "999");
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(requestUri);

            IHttpRouteData data = route.GetRouteData(virtualPathRoot, request);

            // Assert
            Assert.NotNull(data);
            IDictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["controller"] = "Customer";
            expectedResult["id"] = "999";
            Assert.Equal(expectedResult, data.Values, new DictionaryEqualityComparer());
        }

        [Theory]
        [InlineData("{controller}/{id}", "/SelfHostServer/Customer/999/Invalid", "http://localhost/SelfHostServer/Customer/999")]
        [InlineData("{controller}", "/SelfHostServer/", "http://localhost/SelfHostServer")]
        public void GetRouteDataDoesNotMatch(string uriTemplate, string virtualPathRoot, string requestUri)
        {
            HttpRoute route = new HttpRoute(uriTemplate);
            route.Defaults.Add("controller", "Customer");
            route.Defaults.Add("id", "999");
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(requestUri);

            IHttpRouteData data = route.GetRouteData(virtualPathRoot, request);

            // Assert
            Assert.Null(data);
        }

        [Theory]
        [InlineData("controller")]
        [InlineData("cOnTrOlLeR")]
        [InlineData("CONTROLLER")]
        public void GetVirtualPath_GetsValuesInCaseInsensitiveWay(string controllerKey)
        {
            var route = new HttpRoute("{controller}");
            var request = new HttpRequestMessage();
            request.SetRouteData(
                new HttpRouteData(route, new HttpRouteValueDictionary() {
                    { "controller", "Employees" }
                }));
            var values = new HttpRouteValueDictionary()
            {
                { "httproute", true },
                { controllerKey, "Customers" }
            };

            IHttpVirtualPathData virtualPath = route.GetVirtualPath(request, values);

            Assert.NotNull(virtualPath);
            Assert.Equal("Customers", virtualPath.VirtualPath);
        }

        [Fact]
        public void GetVirtualPath_GeneratesPathWithoutRouteData()
        {
            var route = new HttpRoute("{controller}");
            var request = new HttpRequestMessage();
            var values = new HttpRouteValueDictionary()
            {
                { "httproute", true },
                { "controller", "Customers" }
            };

            IHttpVirtualPathData virtualPath = route.GetVirtualPath(request, values);

            Assert.NotNull(virtualPath);
            Assert.Equal("Customers", virtualPath.VirtualPath);
        }
    }
}

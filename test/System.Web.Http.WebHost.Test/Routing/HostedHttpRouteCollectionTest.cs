// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.WebHost.Routing
{
    public class HostedHttpRouteCollectionTest
    {
        RouteCollection _aspNetRoutes;
        HostedHttpRouteCollection _webApiRoutes;

        public HostedHttpRouteCollectionTest()
        {
            _aspNetRoutes = new RouteCollection();
            _webApiRoutes = new HostedHttpRouteCollection(_aspNetRoutes);
        }

        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => new HostedHttpRouteCollection(routeCollection: null), "routeCollection");
        }

        [Fact]
        public void Add_WithHostedHttpRoute_RegistersInnerRouteWithAspNetRouteCollection()
        {
            var route = new HostedHttpRoute("uri", null, null, null, null);

            _webApiRoutes.Add("name", route);

            var httpWebRoute = Assert.IsType<HttpWebRoute>(_aspNetRoutes["name"]);
            Assert.Same(route.OriginalRoute, httpWebRoute);
            Assert.Same(route, httpWebRoute.HttpRoute);
        }

        [Fact]
        public void Add_WithNonHostedHttpRoute_WrapsCustomRouteWithHttpWebRoute()
        {
            var route = new Mock<IHttpRoute>().Object;

            _webApiRoutes.Add("name", route);

            var httpWebRoute = Assert.IsType<HttpWebRoute>(_aspNetRoutes["name"]);
            Assert.Same(route, httpWebRoute.HttpRoute);
        }

        [Fact]
        public void Clear_RemovesAllValuesFromAspNetRouteCollection()
        {
            _aspNetRoutes.Add(new Mock<RouteBase>().Object);

            _webApiRoutes.Clear();

            Assert.Empty(_aspNetRoutes);
        }

        [Fact]
        public void Contains_OnlyMatchesRegisteredHttpRouteInstances()
        {
            var route = new Mock<IHttpRoute>().Object;
            _webApiRoutes.Add("bar", route);

            Assert.True(_webApiRoutes.Contains(route));
            Assert.False(_webApiRoutes.Contains(new Mock<IHttpRoute>().Object));
        }

        [Fact]
        public void ContainsKey_MatchesAllRoutesInAspNetRouteCollection()
        {
            _aspNetRoutes.Add("foo", new Mock<RouteBase>().Object);
            _webApiRoutes.Add("bar", new Mock<IHttpRoute>().Object);

            Assert.True(_webApiRoutes.ContainsKey("foo"));
            Assert.True(_webApiRoutes.ContainsKey("bar"));
        }

        [Fact]
        public void Count_ReturnsCountOfAllRoutesInAspNetRouteCollection()
        {
            _aspNetRoutes.Add("foo", new Mock<RouteBase>().Object);
            _webApiRoutes.Add("bar", new Mock<IHttpRoute>().Object);

            Assert.Equal(2, _webApiRoutes.Count);
        }

        [Fact]
        public void CreateRoute_CreatesHostedHttpRoute()
        {
            var defaults = new HttpRouteValueDictionary { { "Foo", "Bar" } };
            var constraints = new HttpRouteValueDictionary { { "Bar", "Baz" } };
            var dataTokens = new HttpRouteValueDictionary { { "Baz", "Biff" } };
            var handler = new Mock<HttpMessageHandler>().Object;

            IHttpRoute result = _webApiRoutes.CreateRoute("uri", defaults, constraints, dataTokens, handler);

            Assert.IsType<HostedHttpRoute>(result);
            Assert.Equal("Bar", result.Defaults["Foo"]);
            Assert.Equal("Baz", result.Constraints["Bar"]);
            Assert.Equal("Biff", result.DataTokens["Baz"]);
            Assert.Same(handler, result.Handler);
        }

        [Fact]
        public void Enumerating_OnlyIncludesHttpRoutes()
        {
            var aspNetRoute = new Mock<RouteBase>().Object;
            _aspNetRoutes.Add("foo", aspNetRoute);
            var httpRoute = new Mock<IHttpRoute>().Object;
            _webApiRoutes.Add("bar", httpRoute);

            List<object> objects = new List<object>(_webApiRoutes);

            Assert.Contains(httpRoute, objects);
            Assert.DoesNotContain(aspNetRoute, objects);
        }

        [Fact]
        public void GetRouteData_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => _webApiRoutes.GetRouteData(request: null), "request");
        }

        [Fact]
        public void GetRouteData_WithHttpContext_UnmatchedRoute_ReturnsNull()
        {
            var request = new HttpRequestMessage();
            request.SetHttpContext(CreateHttpContext("~/api2"));
            IHttpRoute route = _webApiRoutes.CreateRoute("api", null, null);
            _webApiRoutes.Add("default", route);

            IHttpRouteData result = _webApiRoutes.GetRouteData(request);

            Assert.Null(result);
        }

        [Fact]
        public void GetRouteData_WithHttpContext_MatchedRoute_ReturnsRouteData()
        {
            var request = new HttpRequestMessage();
            request.SetHttpContext(CreateHttpContext("~/api"));
            IHttpRoute route = _webApiRoutes.CreateRoute("api", null, null);
            _webApiRoutes.Add("default", route);

            IHttpRouteData result = _webApiRoutes.GetRouteData(request);

            Assert.Same(route, result.Route);
        }

        [Fact]
        public void SendAsync_CallsDefaultHandlerWhenCustomASPNETRoute()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/controllerName");
            request.SetConfiguration(config);
            HttpDomainRoute domainRoute = new HttpDomainRoute("test", new { controller = "Values", action = "GetTenant" });
            request.SetRouteData(new HostedHttpRouteData(domainRoute.GetRouteData(null)));
            var dispatcher = new HttpRoutingDispatcher(config, defaultHandler: mockHandler.Object);
            var invoker = new HttpMessageInvoker(dispatcher);

            // Act
            invoker.SendAsync(request, CancellationToken.None);

            // Assert
            mockHandler.Protected().Verify("SendAsync", Times.Once(), request, CancellationToken.None);
        }

        [Fact]
        public void GetVirtualPath_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => _webApiRoutes.GetVirtualPath(request: null, name: null, values: null), "request");
        }

        [Fact]
        public void GetVirtualPath_ReturnsVirtualPathData()
        {
            var request = new HttpRequestMessage();
            request.SetHttpContext(CreateHttpContext("~/api", "APP PATH MODIFIER RETURN VALUE"));
            var config = new HttpConfiguration(_webApiRoutes);
            IHttpRoute route = _webApiRoutes.CreateRoute("api", null, null);
            _webApiRoutes.Add("default", route);
            request.SetRouteData(_webApiRoutes.GetRouteData(request));
            request.SetConfiguration(config);

            IHttpVirtualPathData result = _webApiRoutes.GetVirtualPath(request, null, new HttpRouteValueDictionary { { "httproute", true } });

            Assert.NotNull(result);
            Assert.Same(route, result.Route);
            Assert.Equal("APP PATH MODIFIER RETURN VALUE", result.VirtualPath);
        }

        [Fact]
        public void Indexer_ForHttpRoute_ReturnsRoute()
        {
            var route = new Mock<IHttpRoute>().Object;
            _webApiRoutes.Add("foo", route);

            var result = _webApiRoutes["foo"];

            Assert.NotNull(result);
            Assert.Same(route, result);
        }

        [Fact]
        public void Indexer_ForAspNetRoute_Throws()
        {
            _aspNetRoutes.Add("foo", new Mock<RouteBase>().Object);

            Assert.Throws<KeyNotFoundException>(() => _webApiRoutes["foo"], "The given key was not present in the dictionary.");
        }

        [Fact]
        public void Indexer_ForUnknownRoute_Throws()
        {
            Assert.Throws<KeyNotFoundException>(() => _webApiRoutes["foo"], "The given key was not present in the dictionary.");
        }

        [Fact]
        public void UnsupportedFunctions()
        {
            Assert.Throws<NotSupportedException>(() => _webApiRoutes.CopyTo((IHttpRoute[])null, 0), "This operation is only supported by directly calling it on 'RouteCollection'.");

            Assert.Throws<NotSupportedException>(() => _webApiRoutes.CopyTo((KeyValuePair<string, IHttpRoute>[])null, 0), "This operation is not supported by 'HostedHttpRouteCollection'.");
            Assert.Throws<NotSupportedException>(() => _webApiRoutes.Insert(0, null, null), "This operation is not supported by 'HostedHttpRouteCollection'.");
            Assert.Throws<NotSupportedException>(() => _webApiRoutes.Remove(null), "This operation is not supported by 'HostedHttpRouteCollection'.");
        }

        [Fact]
        public void ConvertHttpRouteDataToRouteDataRunsCustomHttpRoute()
        {
            // Arrange
            DomainHttpRoute route = new DomainHttpRoute("myDomain", "api/{controller}/{action}", new { controller = "Values", action = "GetTenant" });
            HostedHttpRouteCollection collection = new HostedHttpRouteCollection(new RouteCollection());
            collection.Add("domainRoute", route);
            HttpRequestMessage request = CreateHttpRequestMessageWithContext();
            IHttpRouteData httpRouteData = collection.GetRouteData(request);

            // Act
            RouteData routeData = httpRouteData.ToRouteData();

            // Assert
            Assert.NotNull(routeData.Values);
            Assert.Equal(3, routeData.Values.Count);
            Assert.Equal("controllerName", routeData.Values["controller"]);
            Assert.Equal("actionName", routeData.Values["action"]);
            Assert.Equal("myDomain", routeData.Values["domain"]);
        }

        [Fact]
        public void CustomHttpRouteGetVitualPathRunsCustomHttpRoute()
        {
            // Arrange
            DomainHttpRoute route = new DomainHttpRoute("myDomain", "api/{controller}/{action}", new { controller = "SomeValue", action = "SomeAction" });
            HostedHttpRouteCollection collection = new HostedHttpRouteCollection(new RouteCollection());
            collection.Add("domainRoute", route);
            HttpRequestMessage request = CreateHttpRequestMessageWithContext();
            HttpRouteValueDictionary routeValues = new HttpRouteValueDictionary()
                {
                    {"controller", "controllerName"},
                    {"action", "actionName"},
                    {"httproute", true}
                };

            request.SetRouteData(new HttpRouteData(route, routeValues));

            // Act
            IHttpVirtualPathData httpvPathData = collection.GetVirtualPath(request, "domainRoute", routeValues);

            // Assert
            Assert.NotNull(httpvPathData);
            Assert.Equal("/api/controllerName/actionNameFromDomain", httpvPathData.VirtualPath);
        }

        [Fact]
        public void IgnoreRoute_GetVirtualPathReturnsNull()
        {
            // Arrange
            DomainHttpRoute route = new DomainHttpRoute("myDomain", "api/{controller}/{action}", new { controller = "SomeValue", action = "SomeAction" });
            HostedHttpRouteCollection collection = new HostedHttpRouteCollection(new RouteCollection());
            collection.IgnoreRoute("domainRoute", route.RouteTemplate);
            HttpRequestMessage request = CreateHttpRequestMessageWithContext();
            HttpRouteValueDictionary routeValues = new HttpRouteValueDictionary()
                {
                    {"controller", "controllerName"},
                    {"action", "actionName"},
                    {"httproute", true}
                };

            request.SetRouteData(new HttpRouteData(route, routeValues));

            // Act
            IHttpVirtualPathData httpvPathData = collection.GetVirtualPath(request, "domainRoute", routeValues);

            // Assert
            // Altough it contains the ignore route, GetVirtualPath from the ignored route will always return null.
            Assert.Equal(1, collection.Count);
            Assert.Null(httpvPathData);
        }

        [Theory]
        [InlineData("people/")]
        [InlineData("people/1")]
        [InlineData("people/literal")]
        [InlineData("people/name?id=20")]
        public async Task IgnoreRoute_GetSoft404IfRouteIgnored(string requestPath)
        {
            var request = CreateHttpRequestMessageWithContext(requestPath);

            var response = await SubmitRequestAsync(request);

            Assert.Equal(Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_IHttpRouteConstraint()
        {
            // Arrange
            var routes = new MockHostedHttpRouteCollection(new RouteCollection());

            var constraint = new CustomHttpConstraint();
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.CreateRoute("{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);

            Assert.Equal(1, routes.TimesValidateConstraintCalled);
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_IRouteConstraint()
        {
            // Arrange
            var routes = new MockHostedHttpRouteCollection(new RouteCollection());

            var constraint = new CustomConstraint();
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.CreateRoute("{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);
            Assert.Equal(1, routes.TimesValidateConstraintCalled);
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_StringRegex()
        {
            // Arrange
            var routes = new MockHostedHttpRouteCollection(new RouteCollection());

            var constraint = "product|products";
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.CreateRoute("{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);
            Assert.Equal(1, routes.TimesValidateConstraintCalled);
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_InvalidType()
        {
            // Arrange
            var routes = new HostedHttpRouteCollection(new RouteCollection());

            var constraint = new Uri("http://localhost/");
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template '{controller}/{id}' " +
                "must have a string value or be of a type which implements 'System.Web.Http.Routing.IHttpRouteConstraint' or 'System.Web.Routing.IRouteConstraint'.";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => routes.CreateRoute("{controller}/{id}", null, constraints), expectedMessage);
        }

        [Theory]
        [InlineData("values/10")]
        [InlineData("values/15")]
        [InlineData("values/20")]
        public async Task IgnoreRoute_WithConstraints_GetSoft404IfRouteIgnored(string requestPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/constraint/" + requestPath);
            request.SetConfiguration(new HttpConfiguration());
            request.SetHttpContext(CreateHttpContext("~/constraint"));

            var response = await SubmitRequestAsync(request);

            Assert.Equal(Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
        }

        [Theory]
        [InlineData("values/1")]
        [InlineData("values/25")]
        [InlineData("values/40")]
        public async Task IgnoreRoute_WithConstraints_GetValueIfNotIgnored(string requestPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/constraint/" + requestPath);
            request.SetConfiguration(new HttpConfiguration());
            request.SetHttpContext(CreateHttpContext("~/constraint"));

            var response = await SubmitRequestAsync(request);

            Assert.Equal(Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(String.Concat("values/", await response.Content.ReadAsStringAsync()), requestPath);
        }

        public class CustomIgnoreRouteConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
                IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                long id;
                if (values.ContainsKey("id")
                    && Int64.TryParse(values["id"].ToString(), out id)
                    && (id == 10 || id == 15 || id == 20))
                {
                    return true;
                }

                return false;
            }
        }

        private static async Task<HttpResponseMessage> SubmitRequestAsync(HttpRequestMessage request)
        {
            HttpConfiguration config = new HttpConfiguration(new HostedHttpRouteCollection(new RouteCollection()));
            config.Routes.IgnoreRoute("Bar", "api/{*pathInfo}");
            config.Routes.IgnoreRoute("Constraints", "constraint/values/{id}", constraints: new { constraint = new CustomIgnoreRouteConstraint() });
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");
            config.MapHttpAttributeRoutes();

            HttpResponseMessage response;
            HttpServer server = new HttpServer(config);
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                response = await client.SendAsync(request, CancellationToken.None);
            }

            return response;
        }

        [RoutePrefix("constraint")]
        public class IgnoreRouteWithConstraintsTestController : ApiController
        {
            [Route("values/{id:int}")]
            public int Get(int id)
            {
               return id;
            }
        }

        private static T GetContentValue<T>(HttpResponseMessage response)
        {
            T value;
            response.TryGetContentValue<T>(out value);
            return value;
        }

        private static HttpRequestMessage CreateHttpRequestMessageWithContext(string requestPath = "controllerName/actionName")
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/" + requestPath);
            request.SetConfiguration(new HttpConfiguration());
            request.SetHttpContext(CreateHttpContext("~/api"));

            return request;
        }

        private static HttpContextBase CreateHttpContext(string relativeUrl, string appPathModifierReturnValue = "")
        {
            var mockContext = new Mock<HttpContextBase>();

            mockContext.SetupGet(c => c.Request.ApplicationPath).Returns(String.Empty);
            mockContext.SetupGet(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns(relativeUrl);
            mockContext.SetupGet(c => c.Request.PathInfo).Returns("");
            mockContext.SetupGet(c => c.Items).Returns(new Dictionary<string, object>());
            mockContext.SetupGet(c => c.Request.HttpMethod).Returns("GET");
            mockContext.SetupGet(c => c.Request.InputStream).Returns(new MemoryStream());
            mockContext.SetupGet(c => c.Request.Headers).Returns(new NameValueCollection());
            mockContext.SetupGet(c => c.Request.ApplicationPath).Returns("/");

            if (appPathModifierReturnValue == string.Empty)
            {
                mockContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                               .Returns((string s) => { return s; });
            }
            else
            {
                mockContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                             .Returns(appPathModifierReturnValue);
            }

            return mockContext.Object;
        }

        public class HttpDomainRoute : Route
        {
            public HttpDomainRoute(string routeTemplate, object defaults, object constraints = null)
                : base(routeTemplate, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), new RouteValueDictionary(), HttpControllerRouteHandler.Instance)
            {
            }

            public override RouteData GetRouteData(HttpContextBase context)
            {
                RouteData data = new RouteData(this, RouteHandler);
                data.Values.Add("domain", "customer");
                return data;
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
            {
                return base.GetVirtualPath(requestContext, values);
            }
        }

        public class DomainHttpRoute : HttpRoute
        {
            public DomainHttpRoute(string domain, string routeTemplate, object defaults, object constraints = null)
                : base(routeTemplate, new HttpRouteValueDictionary(defaults), new HttpRouteValueDictionary(constraints))
            {
                Domain = domain;
            }

            public string Domain { get; set; }

            public override IHttpRouteData GetRouteData(string virtualPathRoot, System.Net.Http.HttpRequestMessage request)
            {
                // Route data
                IHttpRouteData data = base.GetRouteData(virtualPathRoot, request);
                data.Values.Add("domain", Domain);
                return data;
            }

            public override IHttpVirtualPathData GetVirtualPath(System.Net.Http.HttpRequestMessage request, IDictionary<string, object> values)
            {
                // customize the action token
                values["action"] = "actionNameFromDomain";

                return base.GetVirtualPath(request, values);
            }
        }

        private class CustomHttpConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }

        private class CustomConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }

        private class MockHostedHttpRouteCollection : HostedHttpRouteCollection
        {
            public MockHostedHttpRouteCollection(RouteCollection routes)
                : base(routes)
            {
            }

            public int TimesValidateConstraintCalled
            {
                get;
                private set;
            }

            protected override void ValidateConstraint(string routeTemplate, string name, object constraint)
            {
                TimesValidateConstraintCalled++;
                base.ValidateConstraint(routeTemplate, name, constraint);
            }
        }
    }
}

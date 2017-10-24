// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Test
{
    public class UrlHelperTest
    {
        [Fact]
        public void IsLocalUrl_RejectsNull()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl(null));
        }

        [Fact]
        public void IsLocalUrl_RejectsEmptyString()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl(String.Empty));
            Assert.False(helper.IsLocalUrl(" "));
        }

        [Fact]
        public void IsLocalUrl_AcceptsRootedUrls()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();
            Assert.True(helper.IsLocalUrl("/fooo"));
            Assert.True(helper.IsLocalUrl("/www.hackerz.com"));
            Assert.True(helper.IsLocalUrl("/"));
        }

        [Fact]
        public void IsLocalUrl_AcceptsAppRelativeUrls()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();
            Assert.True(helper.IsLocalUrl("~/"));
            Assert.True(helper.IsLocalUrl("~/foobar.html"));
        }

        [Fact]
        public void IsLocalUrl_RejectsRelativeUrls()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();
            Assert.False(helper.IsLocalUrl("foobar.html"));
            Assert.False(helper.IsLocalUrl("../foobar.html"));
            Assert.False(helper.IsLocalUrl("fold/foobar.html"));
        }

        [Fact]
        public void IsLocalUrl_RejectValidButUnsafeRelativeUrls()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("http:/foobar.html"));
            Assert.False(helper.IsLocalUrl("hTtP:foobar.html"));
            Assert.False(helper.IsLocalUrl("http:/www.hackerz.com"));
            Assert.False(helper.IsLocalUrl("HtTpS:/www.hackerz.com"));
        }

        [Fact]
        public void IsLocalUrl_RejectsUrlsOnTheSameHost()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("http://www.mysite.com/appDir/foobar.html"));
            Assert.False(helper.IsLocalUrl("http://WWW.MYSITE.COM"));
        }

        [Fact]
        public void IsLocalUrl_RejectsUrlsOnLocalHost()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("http://localhost/foobar.html"));
            Assert.False(helper.IsLocalUrl("http://127.0.0.1/foobar.html"));
        }

        [Fact]
        public void IsLocalUrl_RejectsUrlsOnTheSameHostButDifferentScheme()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("https://www.mysite.com/"));
        }

        [Fact]
        public void IsLocalUrl_RejectsUrlsOnDifferentHost()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("http://www.hackerz.com"));
            Assert.False(helper.IsLocalUrl("https://www.hackerz.com"));
            Assert.False(helper.IsLocalUrl("hTtP://www.hackerz.com"));
            Assert.False(helper.IsLocalUrl("HtTpS://www.hackerz.com"));
        }

        [Fact]
        public void IsLocalUrl_RejectsUrlsWithTooManySchemeSeparatorCharacters()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("http://///www.hackerz.com/foobar.html"));
            Assert.False(helper.IsLocalUrl("https://///www.hackerz.com/foobar.html"));
            Assert.False(helper.IsLocalUrl("HtTpS://///www.hackerz.com/foobar.html"));

            Assert.False(helper.IsLocalUrl("http:///www.hackerz.com/foobar.html"));
            Assert.False(helper.IsLocalUrl("http:////www.hackerz.com/foobar.html"));
            Assert.False(helper.IsLocalUrl("http://///www.hackerz.com/foobar.html"));
        }

        [Fact]
        public void IsLocalUrl_RejectsUrlsWithMissingSchemeName()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl("//www.hackerz.com"));
            Assert.False(helper.IsLocalUrl("//www.hackerz.com/foobar.html"));
            Assert.False(helper.IsLocalUrl("///www.hackerz.com"));
            Assert.False(helper.IsLocalUrl("//////www.hackerz.com"));
        }

        [Fact]
        public void IsLocalUrl_RejectsInvalidUrls()
        {
            UrlHelper helper = GetUrlHelperForIsLocalUrl();

            Assert.False(helper.IsLocalUrl(@"http:\\www.hackerz.com"));
            Assert.False(helper.IsLocalUrl(@"http:\\www.hackerz.com\"));
        }

        [Fact]
        public void RequestContextProperty()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            UrlHelper urlHelper = new UrlHelper(requestContext);

            // Assert
            Assert.Equal(requestContext, urlHelper.RequestContext);
        }

        [Fact]
        public void ConstructorWithNullRequestContextThrows()
        {
            // Assert
            Assert.ThrowsArgumentNull(
                delegate { new UrlHelper(null); },
                "requestContext");
        }

        [Fact]
        public void ConstructorWithNullRouteCollectionThrows()
        {
            // Assert
            Assert.ThrowsArgumentNull(
                delegate { new UrlHelper(GetRequestContext(), null); },
                "routeCollection");
        }

        [Fact]
        public void ActionEmpty()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action();

            // Assert
            Assert.Equal("/app/", url);
        }

        [Fact]
        public void Action()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction");

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home/newaction", url);
        }

        [Fact]
        public void ActionWithControllerName()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", "home2");

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction", url);
        }

        [Fact]
        public void ActionWithControllerNameAndDictionary()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", "home2", new RouteValueDictionary(new { id = "someid" }));

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ActionWithControllerNameAndObjectProperties()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", "home2", new { id = "someid" });

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ActionWithDictionary()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", new RouteValueDictionary(new { Controller = "home2", id = "someid" }));

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ActionWithNullActionName()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action(null);

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home/oldaction", url);
        }

        [Fact]
        public void ActionWithNullProtocol()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", "home2", new { id = "someid" }, null /* protocol */);

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ActionParameterOverridesObjectProperties()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", new { Action = "action" });

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home/newaction", url);
        }

        [Fact]
        public void ActionWithObjectProperties()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", new { Controller = "home2", id = "someid" });

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ActionWithProtocol()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", "home2", new { id = "someid" }, "https");

            // Assert
            Assert.Equal("https://localhost" + MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ActionWithProtocolAndRouteValueDictionary()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Action("newaction", "home2", new RouteValueDictionary(new { id = "someid" }), "https");

            // Assert
            Assert.Equal("https://localhost" + MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void ContentWithAbsolutePath()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Content("/Content/Image.jpg");

            // Assert
            Assert.Equal("/Content/Image.jpg", url);
        }

        [Fact]
        public void ContentWithAppRelativePath()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Content("~/Content/Image.jpg");

            // Assert
            Assert.Equal("/app/Content/Image.jpg", url);
        }

        [Fact]
        public void ContentWithEmptyPathThrows()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate() { urlHelper.Content(String.Empty); },
                "contentPath");
        }

        [Fact]
        public void ContentWithRelativePath()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Content("Content/Image.jpg");

            // Assert
            Assert.Equal("Content/Image.jpg", url);
        }

        [Fact]
        public void ContentWithExternalUrl()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.Content("http://www.asp.net/App_Themes/Standard/i/logo.png");

            // Assert
            Assert.Equal("http://www.asp.net/App_Themes/Standard/i/logo.png", url);
        }

        [Fact]
        public void Encode()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string encodedUrl = urlHelper.Encode(@"SomeUrl /+\");

            // Assert
            Assert.Equal("SomeUrl+%2f%2b%5c", encodedUrl);
        }

        [Fact]
        public void GenerateContentUrlWithNullContentPathThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate() { UrlHelper.GenerateContentUrl(null, null); },
                "contentPath");
        }

        [Fact]
        public void GenerateContentUrlWithNullContextThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate() { UrlHelper.GenerateContentUrl("Content/foo.png", null); },
                "httpContext");
        }

        [Fact]
        public void GenerateUrlWithNullRequestContextThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate() { UrlHelper.GenerateUrl(null /* routeName */, null /* actionName */, null /* controllerName */, null /* routeValues */, new RouteCollection(), null /* requestContext */, false); },
                "requestContext");
        }

        [Fact]
        public void GenerateUrlWithNullRouteCollectionThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate() { UrlHelper.GenerateUrl(null /* routeName */, null /* actionName */, null /* controllerName */, null /* routeValues */, null /* routeCollection */, null /* requestContext */, false); },
                "routeCollection");
        }

        [Fact]
        public void GenerateUrlWithEmptyCollectionsReturnsNull()
        {
            // Arrange
            RequestContext requestContext = GetRequestContext();

            // Act
            string url = UrlHelper.GenerateUrl(null, null, null, null, new RouteCollection(), requestContext, true);

            // Assert
            Assert.Null(url);
        }

        [Fact]
        public void GenerateUrlWithAction()
        {
            // Arrange
            RequestContext requestContext = GetRequestContext(GetRouteData());

            // Act
            string url = UrlHelper.GenerateUrl(null, "newaction", null, null, GetRouteCollection(), requestContext, true);

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home/newaction", url);
        }

        [Fact]
        public void GenerateUrlWithActionAndController()
        {
            // Arrange
            RequestContext requestContext = GetRequestContext(GetRouteData());

            // Act
            string url = UrlHelper.GenerateUrl(null, "newaction", "newcontroller", null, GetRouteCollection(), requestContext, true);

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/newcontroller/newaction", url);
        }

        [Fact]
        public void GenerateUrlWithImplicitValues()
        {
            // Arrange
            RequestContext requestContext = GetRequestContext(GetRouteData());

            // Act
            string url = UrlHelper.GenerateUrl(null, null, null, null, GetRouteCollection(), requestContext, true);

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home/oldaction", url);
        }

        [Fact]
        public void RouteUrlCanUseNamedRouteWithoutSpecifyingDefaults()
        {
            // DevDiv 217072: Non-mvc specific helpers should not give default values for controller and action

            // Arrange
            UrlHelper urlHelper = GetUrlHelper();
            urlHelper.RouteCollection.MapRoute("MyRouteName", "any/url", new { controller = "Charlie" });

            // Act
            string result = urlHelper.RouteUrl("MyRouteName");

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/any/url", result);
        }

        [Fact]
        public void RouteUrlWithDictionary()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl(new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }));

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithEmptyHostName()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }), "http", String.Empty /* hostName */);

            // Assert
            Assert.Equal("http://localhost" + MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithEmptyProtocol()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }), String.Empty /* protocol */, "foo.bar.com");

            // Assert
            Assert.Equal("http://foo.bar.com" + MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithNullProtocol()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }), null /* protocol */, "foo.bar.com");

            // Assert
            Assert.Equal("http://foo.bar.com" + MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithNullProtocolAndNullHostName()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }), null /* protocol */, null /* hostName */);

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithObjectProperties()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl(new { Action = "newaction", Controller = "home2", id = "someid" });

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithProtocol()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new { Action = "newaction", Controller = "home2", id = "someid" }, "https");

            // Assert
            Assert.Equal("https://localhost" + MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithRouteNameAndDefaults()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute");

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/named/home/oldaction", url);
        }

        [Fact]
        public void RouteUrlWithRouteNameAndDictionary()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }));

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithRouteNameAndObjectProperties()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.RouteUrl("namedroute", new { Action = "newaction", Controller = "home2", id = "someid" });

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void HttpRouteUrlFindsHttpRoute()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();

            // Act
            string url = urlHelper.HttpRouteUrl("httproute", new { Action = "newaction", Controller = "home2", id = "someid" });

            // Assert
            Assert.Equal(MvcHelper.AppPathModifier + "/app/mock/http/route", url);
        }

        [Fact]
        public void UrlGenerationDoesNotChangeProvidedDictionary()
        {
            // Arrange
            UrlHelper urlHelper = GetUrlHelper();
            RouteValueDictionary valuesDictionary = new RouteValueDictionary();

            // Act
            urlHelper.Action("actionName", valuesDictionary);

            // Assert
            Assert.Empty(valuesDictionary);
            Assert.False(valuesDictionary.ContainsKey("action"));
        }

        [Fact]
        public void UrlGenerationReturnsNullWhenSubsequentSegmentHasValue()
        { // Dev10 Bug #924729
            // Arrange
            RouteCollection routes = new RouteCollection();
            routes.MapRoute("SampleRoute", "testing/{a}/{b}/{c}",
                            new
                            {
                                controller = "controller",
                                action = "action",
                                b = UrlParameter.Optional,
                                c = UrlParameter.Optional
                            });

            UrlHelper helper = GetUrlHelper(routeCollection: routes);

            // Act
            string url = helper.Action("action", "controller", new { a = 42, c = 2112 });

            // Assert
            Assert.Null(url);
        }

        private static RequestContext GetRequestContext()
        {
            HttpContextBase httpcontext = MvcHelper.GetHttpContext("/app/", null, null);
            RouteData rd = new RouteData();
            return new RequestContext(httpcontext, rd);
        }

        private static RequestContext GetRequestContext(RouteData routeData)
        {
            HttpContextBase httpcontext = MvcHelper.GetHttpContext("/app/", null, null);
            return new RequestContext(httpcontext, routeData);
        }

        private static RouteCollection GetRouteCollection()
        {
            RouteCollection rt = new RouteCollection();
            rt.Add(new Route("{controller}/{action}/{id}", null) { Defaults = new RouteValueDictionary(new { id = "defaultid" }) });
            rt.Add("namedroute", new Route("named/{controller}/{action}/{id}", null) { Defaults = new RouteValueDictionary(new { id = "defaultid" }) });

            // Adding a route that recognizes the httproute value for the HttpRouteUrl tests
            Mock<RouteBase> mockHttpRoute = new Mock<RouteBase>();
            mockHttpRoute.Setup(mock => mock.GetVirtualPath(It.IsAny<RequestContext>(), It.Is<RouteValueDictionary>(routeValues => routeValues.ContainsKey("httproute"))))
                .Returns(new VirtualPathData(null, "mock/http/route"));
            rt.Add("httproute", mockHttpRoute.Object);

            return rt;
        }

        private static RouteData GetRouteData()
        {
            RouteData rd = new RouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "oldaction");
            return rd;
        }

        private static UrlHelper GetUrlHelper()
        {
            return GetUrlHelper(GetRouteData(), GetRouteCollection());
        }

        private static UrlHelper GetUrlHelper(RouteData routeData = null, RouteCollection routeCollection = null)
        {
            HttpContextBase httpcontext = MvcHelper.GetHttpContext("/app/", null, null);
            UrlHelper urlHelper = new UrlHelper(new RequestContext(httpcontext, routeData ?? new RouteData()), routeCollection ?? new RouteCollection());
            return urlHelper;
        }

        private static UrlHelper GetUrlHelperForIsLocalUrl()
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.SetupGet(context => context.Request.Url).Returns(new Uri("http://www.mysite.com/"));
            RequestContext requestContext = new RequestContext(contextMock.Object, new RouteData());
            UrlHelper helper = new UrlHelper(requestContext);
            return helper;
        }
    }
}

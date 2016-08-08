// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class RenderActionTest
    {
        [Fact]
        public void RenderActionUsingExpressionWithParametersInViewContextRendersCorrectly()
        {
            // Arrange
            Func<RequestContext> requestContextAccessor;
            HtmlHelper html = GetHtmlHelper(out requestContextAccessor);
            html.ViewContext.RouteData.Values.Add("stuff", "42");

            // Act
            html.RenderAction<TestController>(c => c.Stuff());
            RequestContext requestContext = requestContextAccessor();

            // Assert
            Assert.NotNull(requestContext);
            Assert.Equal("Test", requestContext.RouteData.Values["controller"]);
            Assert.Equal("Stuff", requestContext.RouteData.Values["action"]);
            Assert.Equal("42", requestContext.RouteData.Values["stuff"]);
        }

        [Fact]
        public void RenderActionUsingExpressionRendersCorrectly()
        {
            // Arrange
            Func<RequestContext> requestContextAccessor;
            HtmlHelper html = GetHtmlHelper(out requestContextAccessor);

            // Act
            html.RenderAction<TestController>(c => c.About(76));
            RequestContext requestContext = requestContextAccessor();

            // Assert
            Assert.NotNull(requestContext);
            Assert.Equal("Test", requestContext.RouteData.Values["controller"]);
            Assert.Equal("About", requestContext.RouteData.Values["action"]);
            Assert.Equal(76, requestContext.RouteData.Values["page"]);
        }

        [Fact]
        public void RenderRouteWithNullRouteValueDictionaryThrowsException()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary(), "/");
            Assert.ThrowsArgumentNull(() => html.RenderRoute(null), "routeValues");
        }

        [Fact]
        public void RenderRouteWithActionAndControllerSpecifiedRendersCorrectAction()
        {
            // Arrange
            Func<RequestContext> requestContextAccessor;
            HtmlHelper html = GetHtmlHelper(out requestContextAccessor);

            // Act
            html.RenderRoute(new RouteValueDictionary(new { action = "Index", controller = "Test" }));
            RequestContext requestContext = requestContextAccessor();

            // Assert
            Assert.NotNull(requestContext);
            Assert.Equal("Test", requestContext.RouteData.Values["controller"]);
            Assert.Equal("Index", requestContext.RouteData.Values["action"]);
        }

        private static HtmlHelper GetHtmlHelper(out Func<RequestContext> requestContextAccessor)
        {
            RequestContext requestContext = null;

            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary(), "/");

            html.RouteCollection.MapRoute(null, "{*dummy}");
            Mock.Get(html.ViewContext.HttpContext)
                .Setup(o => o.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((_h, _w, _pf) =>
                {
                    MvcHandler mvcHandler = _h.GetType().GetProperty("InnerHandler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_h, null) as MvcHandler;
                    requestContext = mvcHandler.RequestContext;
                });

            requestContextAccessor = () => requestContext;
            return html;
        }

        public class TestController : Controller
        {
            public string Index()
            {
                return "It Worked!";
            }

            public string About(int page)
            {
                return "This is page #" + page;
            }

            public string Stuff()
            {
                string stuff = ControllerContext.RouteData.Values["stuff"] as string;
                return "Argument was " + stuff;
            }
        }
    }
}

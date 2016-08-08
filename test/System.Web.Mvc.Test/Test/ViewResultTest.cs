// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ViewResultTest
    {
        private const string _viewName = "My cool view.";
        private const string _masterName = "My cool master.";

        [Fact]
        public void EmptyViewNameUsesActionNameAsViewName()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            HttpContextBase httpContext = CreateHttpContext();
            RouteData routeData = new RouteData();
            routeData.Values["action"] = _viewName;
            ControllerContext context = new ControllerContext(httpContext, routeData, controller);
            Mock<IViewEngine> viewEngine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            List<IViewEngine> viewEngines = new List<IViewEngine>();
            viewEngines.Add(viewEngine.Object);
            Mock<ViewEngineCollection> viewEngineCollection = new Mock<ViewEngineCollection>(MockBehavior.Strict, viewEngines);
            ViewResult result = new ViewResultHelper { ViewEngineCollection = viewEngineCollection.Object };
            viewEngineCollection
                .Setup(e => e.FindView(It.IsAny<ControllerContext>(), _viewName, _masterName))
                .Returns(new ViewEngineResult(view.Object, viewEngine.Object));
            viewEngine
                .Setup(e => e.FindView(It.IsAny<ControllerContext>(), _viewName, _masterName, It.IsAny<bool>()))
                .Callback<ControllerContext, string, string, bool>(
                    (controllerContext, viewName, masterName, useCache) =>
                    {
                        Assert.Same(httpContext, controllerContext.HttpContext);
                        Assert.Same(routeData, controllerContext.RouteData);
                    })
                .Returns(new ViewEngineResult(view.Object, viewEngine.Object));
            view
                .Setup(o => o.Render(It.IsAny<ViewContext>(), httpContext.Response.Output))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.Same(view.Object, viewContext.View);
                        Assert.Same(result.ViewData, viewContext.ViewData);
                        Assert.Same(result.TempData, viewContext.TempData);
                        Assert.Same(controller, viewContext.Controller);
                    });
            viewEngine
                .Setup(e => e.ReleaseView(context, It.IsAny<IView>()))
                .Callback<ControllerContext, IView>(
                    (controllerContext, releasedView) => { Assert.Same(releasedView, view.Object); });

            // Act
            result.ExecuteResult(context);

            // Assert
            viewEngine.Verify();
            viewEngineCollection.Verify();
            view.Verify();
        }

        [Fact]
        public void EngineLookupFailureThrows()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            HttpContextBase httpContext = CreateHttpContext();
            RouteData routeData = new RouteData();
            routeData.Values["action"] = _viewName;
            ControllerContext context = new ControllerContext(httpContext, routeData, controller);
            Mock<IViewEngine> viewEngine = new Mock<IViewEngine>(MockBehavior.Strict);
            List<IViewEngine> viewEngines = new List<IViewEngine>();
            viewEngines.Add(viewEngine.Object);
            Mock<ViewEngineCollection> viewEngineCollection = new Mock<ViewEngineCollection>(MockBehavior.Strict, viewEngines);
            ViewResult result = new ViewResultHelper { ViewEngineCollection = viewEngineCollection.Object };
            viewEngineCollection
                .Setup(e => e.FindView(It.IsAny<ControllerContext>(), _viewName, _masterName))
                .Returns(new ViewEngineResult(new[] { "location1", "location2" }));
            viewEngine
                .Setup(e => e.FindView(It.IsAny<ControllerContext>(), _viewName, _masterName, It.IsAny<bool>()))
                .Callback<ControllerContext, string, string, bool>(
                    (controllerContext, viewName, masterName, useCache) =>
                    {
                        Assert.Same(httpContext, controllerContext.HttpContext);
                        Assert.Same(routeData, controllerContext.RouteData);
                    })
                .Returns(new ViewEngineResult(new[] { "location1", "location2" }));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => result.ExecuteResult(context),
                "The view '" + _viewName + "' or its master was not found or no view engine supports the searched locations. The following locations were searched:" + Environment.NewLine
              + "location1" + Environment.NewLine
              + "location2");

            viewEngine.Verify();
            viewEngineCollection.Verify();
        }

        [Fact]
        public void EngineLookupSuccessRendersView()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            HttpContextBase httpContext = CreateHttpContext();
            RouteData routeData = new RouteData();
            ControllerContext context = new ControllerContext(httpContext, routeData, controller);
            Mock<IViewEngine> viewEngine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            List<IViewEngine> viewEngines = new List<IViewEngine>();
            viewEngines.Add(viewEngine.Object);
            Mock<ViewEngineCollection> viewEngineCollection = new Mock<ViewEngineCollection>(MockBehavior.Strict, viewEngines);
            ViewResult result = new ViewResultHelper { ViewName = _viewName, ViewEngineCollection = viewEngineCollection.Object };
            view
                .Setup(o => o.Render(It.IsAny<ViewContext>(), httpContext.Response.Output))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.Same(view.Object, viewContext.View);
                        Assert.Same(result.ViewData, viewContext.ViewData);
                        Assert.Same(result.TempData, viewContext.TempData);
                        Assert.Same(controller, viewContext.Controller);
                    });
            viewEngineCollection
                .Setup(e => e.FindView(It.IsAny<ControllerContext>(), _viewName, _masterName))
                .Returns(new ViewEngineResult(view.Object, viewEngine.Object));
            viewEngine
                .Setup(e => e.FindView(It.IsAny<ControllerContext>(), _viewName, _masterName, It.IsAny<bool>()))
                .Callback<ControllerContext, string, string, bool>(
                    (controllerContext, viewName, masterName, useCache) =>
                    {
                        Assert.Same(httpContext, controllerContext.HttpContext);
                        Assert.Same(routeData, controllerContext.RouteData);
                    })
                .Returns(new ViewEngineResult(view.Object, viewEngine.Object));
            viewEngine
                .Setup(e => e.ReleaseView(context, It.IsAny<IView>()))
                .Callback<ControllerContext, IView>(
                    (controllerContext, releasedView) => { Assert.Same(releasedView, view.Object); });

            // Act
            result.ExecuteResult(context);

            // Assert
            viewEngine.Verify();
            viewEngineCollection.Verify();
            view.Verify();
        }

        [Fact]
        public void ExecuteResultWithExplicitViewObject()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            HttpContextBase httpContext = CreateHttpContext();
            RouteData routeData = new RouteData();
            routeData.Values["action"] = _viewName;
            ControllerContext context = new ControllerContext(httpContext, routeData, controller);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            ViewResult result = new ViewResultHelper { View = view.Object };
            view
                .Setup(o => o.Render(It.IsAny<ViewContext>(), httpContext.Response.Output))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.Same(view.Object, viewContext.View);
                        Assert.Same(result.ViewData, viewContext.ViewData);
                        Assert.Same(result.TempData, viewContext.TempData);
                        Assert.Same(controller, viewContext.Controller);
                    });

            // Act
            result.ExecuteResult(context);

            // Assert
            view.Verify();
        }

        [Fact]
        public void ExecuteResultWithNullControllerContextThrows()
        {
            // Arrange
            ViewResult result = new ViewResultHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => result.ExecuteResult(null),
                "context");
        }

        [Fact]
        public void MasterNameProperty()
        {
            // Arrange
            ViewResult result = new ViewResult();

            // Act & Assert
            MemberHelper.TestStringProperty(result, "MasterName", String.Empty);
        }

        private static HttpContextBase CreateHttpContext()
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Response.Output).Returns(TextWriter.Null);
            return mockHttpContext.Object;
        }

        private class ViewResultHelper : ViewResult
        {
            public ViewResultHelper()
            {
                ViewEngineCollection = new ViewEngineCollection(new IViewEngine[] { new WebFormViewEngine() });
                MasterName = _masterName;
            }
        }
    }
}

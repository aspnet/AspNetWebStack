// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class RazorViewTest
    {
        [Fact]
        public void Constructor_RunViewStartPagesParam()
        {
            var context = new ControllerContext();
            Assert.True(new RazorView(context, "~/view", "~/master", runViewStartPages: true, viewStartFileExtensions: null).RunViewStartPages);
            Assert.False(new RazorView(context, "~/view", "~/master", runViewStartPages: false, viewStartFileExtensions: null).RunViewStartPages);
            Assert.True(new RazorView(context, "~/view", "~/master", runViewStartPages: true, viewStartFileExtensions: null, viewPageActivator: new Mock<IViewPageActivator>().Object).RunViewStartPages);
            Assert.False(new RazorView(context, "~/view", "~/master", runViewStartPages: false, viewStartFileExtensions: null, viewPageActivator: new Mock<IViewPageActivator>().Object).RunViewStartPages);
        }

        [Fact]
        public void ConstructorWithEmptyViewPathThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new RazorView(new ControllerContext(), String.Empty, "~/master", false, Enumerable.Empty<string>()),
                "viewPath"
                );
        }

        [Fact]
        public void ConstructorWithNullViewPathThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new RazorView(new ControllerContext(), null, "~/master", false, Enumerable.Empty<string>()),
                "viewPath"
                );
        }

        [Fact]
        public void ConstructorWithNullControllerContextThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new RazorView(null, "view path", "~/master", false, Enumerable.Empty<string>()),
                "controllerContext"
                );
        }

        [Fact]
        public void LayoutPathProperty()
        {
            //Arrange
            ControllerContext controllerContext = new ControllerContext();

            // Act
            RazorView view = new RazorView(new ControllerContext(), "view path", "master path", false, Enumerable.Empty<string>());

            // Assert
            Assert.Equal("master path", view.LayoutPath);
        }

        [Fact]
        public void LayoutPathPropertyReturnsEmptyStringIfNullLayoutSpecified()
        {
            // Act
            RazorView view = new RazorView(new ControllerContext(), "view path", null, false, Enumerable.Empty<string>());

            // Assert
            Assert.Equal(String.Empty, view.LayoutPath);
        }

        [Fact]
        public void LayoutPathPropertyReturnsEmptyStringIfLayoutNotSpecified()
        {
            // Act
            RazorView view = new RazorView(new ControllerContext(), "view path", null, false, Enumerable.Empty<string>());

            // Assert
            Assert.Equal(String.Empty, view.LayoutPath);
        }

        [Fact]
        public void RenderWithNullWriterThrows()
        {
            // Arrange
            RazorView view = new RazorView(new ControllerContext(), "~/viewPath", null, false, Enumerable.Empty<string>());
            Mock<ViewContext> viewContextMock = new Mock<ViewContext>();

            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            view.BuildManager = buildManager;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => view.Render(viewContextMock.Object, null),
                "writer"
                );
        }

        [Fact]
        public void RenderWithUnsupportedTypeThrows()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManagerMock = new MockBuildManager("view path", typeof(object));
            RazorView view = new RazorView(new ControllerContext(), "view path", null, false, Enumerable.Empty<string>());
            view.BuildManager = buildManagerMock;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => view.Render(context, new Mock<TextWriter>().Object),
                "The view at 'view path' must derive from WebViewPage, or WebViewPage<TModel>."
                );
        }

        [Fact]
        public void RenderWithViewPageAndNoStartPageLookupRendersView()
        {
            // Arrange
            StubWebViewPage viewPage = new StubWebViewPage();
            Mock<ViewContext> viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(vc => vc.HttpContext.Items).Returns(new Dictionary<object, object>());
            viewContextMock.Setup(vc => vc.HttpContext.Request.IsLocal).Returns(false);
            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            RazorView view = new RazorView(controllerContext, "~/viewPath", null, false, Enumerable.Empty<string>(), activator.Object);
            view.StartPageLookup = (WebPageRenderingBase p, string n, IEnumerable<string> e) =>
            {
                Assert.True(false, "ViewStart page lookup should not be called");
                return null;
            };
            view.BuildManager = buildManager;

            // Act
            view.Render(viewContextMock.Object, new Mock<TextWriter>().Object);

            // Assert
            Assert.Null(viewPage.Layout);
            Assert.Equal("", viewPage.OverridenLayoutPath);
            Assert.Same(viewContextMock.Object, viewPage.ViewContext);
            Assert.Equal("~/viewPath", viewPage.VirtualPath);
        }

        [Fact]
        public void RenderWithViewPageAndStartPageLookupExecutesStartPage()
        {
            // Arrange
            StubWebViewPage viewPage = new StubWebViewPage();
            Mock<ViewContext> viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(vc => vc.HttpContext.Items).Returns(new Dictionary<object, object>());
            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            RazorView view = new RazorView(controllerContext, "~/viewPath", null, true, new[] { "cshtml" }, activator.Object);
            Mock<ViewStartPage> startPage = new Mock<ViewStartPage>();
            startPage.Setup(sp => sp.ExecutePageHierarchy()).Verifiable();
            view.StartPageLookup = (WebPageRenderingBase page, string fileName, IEnumerable<string> extensions) =>
            {
                Assert.Equal(viewPage, page);
                Assert.Equal("_ViewStart", fileName);
                Assert.Equal(new[] { "cshtml" }, extensions.ToArray());
                return startPage.Object;
            };
            view.BuildManager = buildManager;

            // Act
            view.Render(viewContextMock.Object, new Mock<TextWriter>().Object);

            // Assert
            startPage.Verify(sp => sp.ExecutePageHierarchy(), Times.Once());
        }

        // TODO: This throws in WebPages and needs to be tracked down.
        [Fact]
        public void RenderWithViewPageAndLayoutPageRendersView()
        {
            // Arrange
            StubWebViewPage viewPage = new StubWebViewPage();
            Mock<ViewContext> viewContext = new Mock<ViewContext>();
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> httpRequest = new Mock<HttpRequestBase>();

            httpRequest.SetupGet(r => r.IsLocal).Returns(false);
            httpRequest.SetupGet(r => r.Browser.IsMobileDevice).Returns(false);
            httpRequest.SetupGet(r => r.Cookies).Returns(new HttpCookieCollection());

            httpContext.SetupGet(c => c.Request).Returns(httpRequest.Object);
            httpContext.SetupGet(c => c.Response.Cookies).Returns(new HttpCookieCollection());
            httpContext.SetupGet(c => c.Items).Returns(new Hashtable());

            viewContext.SetupGet(v => v.HttpContext).Returns(httpContext.Object);

            MockBuildManager buildManager = new MockBuildManager("~/viewPath", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);

            Mock<WebPage> layoutPage = new Mock<WebPage> { CallBase = true };
            layoutPage.Setup(c => c.Execute()).Callback(() => layoutPage.Object.RenderBody());
            Mock<IVirtualPathFactory> virtualPathFactory = new Mock<IVirtualPathFactory>(MockBehavior.Strict);
            virtualPathFactory.Setup(f => f.Exists("~/layoutPath")).Returns(true);
            virtualPathFactory.Setup(f => f.CreateInstance("~/layoutPath")).Returns(layoutPage.Object);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            RazorView view = new RazorView(controllerContext, "~/viewPath", "~/layoutPath", false, Enumerable.Empty<string>(), activator.Object);
            view.BuildManager = buildManager;
            view.VirtualPathFactory = virtualPathFactory.Object;
            view.DisplayModeProvider = DisplayModeProvider.Instance;

            // Act
            view.Render(viewContext.Object, TextWriter.Null);

            // Assert
            Assert.Equal("~/layoutPath", viewPage.Layout);
            Assert.Equal("~/layoutPath", viewPage.OverridenLayoutPath);
            Assert.Same(viewContext.Object, viewPage.ViewContext);
            Assert.Equal("~/viewPath", viewPage.VirtualPath);
        }

        public class StubWebViewPage : WebViewPage
        {
            public bool InitHelpersCalled;
            public string ResultLayoutPage;
            public string ResultOverridenLayoutPath;
            public ViewContext ResultViewContext;
            public string ResultVirtualPath;

            public override void Execute()
            {
                ResultLayoutPage = Layout;
                ResultOverridenLayoutPath = OverridenLayoutPath;
                ResultViewContext = ViewContext;
                ResultVirtualPath = VirtualPath;
            }

            public override void InitHelpers()
            {
                base.InitHelpers();
                InitHelpersCalled = true;
            }
        }
    }
}

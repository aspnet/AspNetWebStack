// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class WebFormViewTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new WebFormView(new ControllerContext(), String.Empty, "~/master"),
                "viewPath"
                );

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new WebFormView(new ControllerContext(), null, "~/master"),
                "viewPath"
                );

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new WebFormView(null, "view path", "~/master"),
                "controllerContext"
                );
        }

        [Fact]
        public void MasterPathProperty()
        {
            // Act
            WebFormView view = new WebFormView(new ControllerContext(), "view path", "master path");

            // Assert
            Assert.Equal("master path", view.MasterPath);
        }

        [Fact]
        public void MasterPathPropertyReturnsEmptyStringIfMasterNotSpecified()
        {
            // Act
            WebFormView view = new WebFormView(new ControllerContext(), "view path", null);

            // Assert
            Assert.Equal(String.Empty, view.MasterPath);
        }

        [Fact]
        public void RenderWithUnsupportedTypeThrows()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManagerMock = new MockBuildManager("view path", typeof(int));
            WebFormView view = new WebFormView(new ControllerContext(), "view path", null);
            view.BuildManager = buildManagerMock;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => view.Render(context, null),
                "The view at 'view path' must derive from ViewPage, ViewPage<TModel>, ViewUserControl, or ViewUserControl<TModel>."
                );
        }

        [Fact]
        public void RenderWithViewPageAndMasterRendersView()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            StubViewPage viewPage = new StubViewPage();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            WebFormView view = new WebFormView(controllerContext, "view path", "master path", activator.Object);
            view.BuildManager = buildManager;

            // Act
            view.Render(context, null);

            // Assert
            Assert.Equal(context, viewPage.ResultViewContext);
            Assert.Equal("master path", viewPage.MasterLocation);
        }

        [Fact]
        public void RenderWithViewPageRendersView()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            StubViewPage viewPage = new StubViewPage();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewPage);
            WebFormView view = new WebFormView(controllerContext, "view path", null, activator.Object);
            view.BuildManager = buildManager;

            // Act
            view.Render(context, null);

            // Assert
            Assert.Equal(context, viewPage.ResultViewContext);
            Assert.Equal(String.Empty, viewPage.MasterLocation);
        }

        [Fact]
        public void RenderWithViewUserControlAndMasterThrows()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManagerMock = new MockBuildManager("view path", typeof(StubViewUserControl));
            WebFormView view = new WebFormView(new ControllerContext(), "view path", "master path");
            view.BuildManager = buildManagerMock;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => view.Render(context, null),
                "A master name cannot be specified when the view is a ViewUserControl."
                );
        }

        [Fact]
        public void RenderWithViewUserControlRendersView()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            StubViewUserControl viewUserControl = new StubViewUserControl();
            activator.Setup(l => l.Create(controllerContext, typeof(object))).Returns(viewUserControl);
            WebFormView view = new WebFormView(controllerContext, "view path", null, activator.Object) { BuildManager = buildManager };

            // Act
            view.Render(context, null);

            // Assert
            Assert.Equal(context, viewUserControl.ResultViewContext);
        }

        public sealed class StubViewPage : ViewPage
        {
            public ViewContext ResultViewContext;

            public override void RenderView(ViewContext viewContext)
            {
                ResultViewContext = viewContext;
            }
        }

        public sealed class StubViewUserControl : ViewUserControl
        {
            public ViewContext ResultViewContext;

            public override void RenderView(ViewContext viewContext)
            {
                ResultViewContext = viewContext;
            }
        }
    }
}

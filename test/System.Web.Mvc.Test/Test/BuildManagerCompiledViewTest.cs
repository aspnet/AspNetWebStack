// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class CompiledTypeViewTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new TestableBuildManagerCompiledView(new ControllerContext(), String.Empty),
                "viewPath"
                );

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new TestableBuildManagerCompiledView(new ControllerContext(), null),
                "viewPath"
                );

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new TestableBuildManagerCompiledView(null, "view path"),
                "controllerContext"
                );
        }

        [Fact]
        public void RenderWithNullContextThrows()
        {
            // Arrange
            TestableBuildManagerCompiledView view = new TestableBuildManagerCompiledView(new ControllerContext(), "~/view");

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => view.Render(null, new Mock<TextWriter>().Object),
                "viewContext"
                );
        }

        [Fact]
        public void RenderWithNullViewInstanceThrows()
        {
            // Arrange
            ViewContext context = new Mock<ViewContext>().Object;
            MockBuildManager buildManager = new MockBuildManager("view path", compiledType: null);
            TestableBuildManagerCompiledView view = new TestableBuildManagerCompiledView(new ControllerContext(), "view path");
            view.BuildManager = buildManager;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => view.Render(context, new Mock<TextWriter>().Object),
                "The view found at 'view path' was not created."
                );
        }

        [Fact]
        public void ViewPathProperty()
        {
            // Act
            BuildManagerCompiledView view = new TestableBuildManagerCompiledView(new ControllerContext(), "view path");

            // Assert
            Assert.Equal("view path", view.ViewPath);
        }

        [Fact]
        public void ViewCreationConsultsSetActivator()
        {
            // Arrange
            object viewInstance = new object();
            Mock<IViewPageActivator> activator = new Mock<IViewPageActivator>(MockBehavior.Strict);
            ControllerContext controllerContext = new ControllerContext();
            activator.Setup(a => a.Create(controllerContext, typeof(object))).Returns(viewInstance).Verifiable();
            MockBuildManager buildManager = new MockBuildManager("view path", typeof(object));
            BuildManagerCompiledView view = new TestableBuildManagerCompiledView(controllerContext, "view path", activator.Object) { BuildManager = buildManager };

            // Act
            view.Render(new Mock<ViewContext>().Object, new Mock<TextWriter>().Object);

            // Assert
            activator.Verify();
        }

        [Fact]
        public void ViewCreationDelegatesToDependencyResolverWhenActivatorIsNull()
        {
            // Arrange
            var viewInstance = new object();
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(object));
            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            dependencyResolver.Setup(dr => dr.GetService(typeof(object))).Returns(viewInstance).Verifiable();
            var view = new TestableBuildManagerCompiledView(controllerContext, "view path", dependencyResolver: dependencyResolver.Object) { BuildManager = buildManager };

            // Act
            view.Render(new Mock<ViewContext>().Object, new Mock<TextWriter>().Object);

            // Assert
            dependencyResolver.Verify();
        }

        [Fact]
        public void ViewCreationDelegatesToActivatorCreateInstanceWhenDependencyResolverReturnsNull()
        {
            // Arrange
            var controllerContext = new ControllerContext();
            var buildManager = new MockBuildManager("view path", typeof(NoParameterlessCtor));
            var dependencyResolver = new Mock<IDependencyResolver>();
            var view = new TestableBuildManagerCompiledView(controllerContext, "view path", dependencyResolver: dependencyResolver.Object) { BuildManager = buildManager };

            // Act & Assert, confirming type name and full stack are available in Exception
            // Depend on the fact that Activator.CreateInstance cannot create an object without a parameterless ctor
            MissingMethodException ex = Assert.Throws<MissingMethodException>(
                () => view.Render(new Mock<ViewContext>().Object, new Mock<TextWriter>().Object),
                "No parameterless constructor defined for this object. Object type 'System.Web.Mvc.Test.CompiledTypeViewTest+NoParameterlessCtor'.");
            Assert.Contains("System.Activator.CreateInstance(", ex.ToString());
        }

        private class NoParameterlessCtor
        {
            public NoParameterlessCtor(int x)
            {
            }
        }

        private sealed class TestableBuildManagerCompiledView : BuildManagerCompiledView
        {
            public TestableBuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator = null, IDependencyResolver dependencyResolver = null)
                : base(controllerContext, viewPath, viewPageActivator, dependencyResolver)
            {
            }

            protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance)
            {
                return;
            }
        }
    }
}

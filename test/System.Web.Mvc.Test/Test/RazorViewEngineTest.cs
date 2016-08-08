// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class RazorViewEngineTest
    {
        [Fact]
        public void AreaMasterLocationFormats()
        {
            // Arrange
            string[] expected = new[]
            {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.AreaMasterLocationFormats);
        }

        [Fact]
        public void AreaPartialViewLocationFormats()
        {
            // Arrange
            string[] expected = new[]
            {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.AreaPartialViewLocationFormats);
        }

        [Fact]
        public void AreaViewLocationFormats()
        {
            // Arrange
            string[] expected = new[]
            {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.AreaViewLocationFormats);
        }

        [Fact]
        public void RazorViewEngineSetsViewPageActivator()
        {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine(viewPageActivator.Object);

            //Act & Assert
            Assert.Equal(viewPageActivator.Object, viewEngine.ViewPageActivator);
        }

        [Fact]
        public void CreatePartialView_PassesViewPageActivator()
        {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine(viewPageActivator.Object);

            // Act
            RazorView result = (RazorView)viewEngine.CreatePartialView("partial path");

            // Assert
            Assert.Equal(viewEngine.ViewPageActivator, result.ViewPageActivator);
        }

        [Fact]
        public void CreateView_PassesViewPageActivator()
        {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine(viewPageActivator.Object);

            // Act
            RazorView result = (RazorView)viewEngine.CreateView("partial path", "master path");

            // Assert
            Assert.Equal(viewEngine.ViewPageActivator, result.ViewPageActivator);
        }

        [Fact]
        public void CreatePartialView_ReturnsRazorView()
        {
            // Arrange
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine();

            // Act
            RazorView result = (RazorView)viewEngine.CreatePartialView("partial path");

            // Assert
            Assert.Equal("partial path", result.ViewPath);
            Assert.Equal(String.Empty, result.LayoutPath);
            Assert.False(result.RunViewStartPages);
        }

        [Fact]
        public void CreateView_ReturnsRazorView()
        {
            // Arrange
            TestableRazorViewEngine viewEngine = new TestableRazorViewEngine()
            {
                FileExtensions = new[] { "cshtml", "vbhtml", "razor" }
            };

            // Act
            RazorView result = (RazorView)viewEngine.CreateView("partial path", "master path");

            // Assert
            Assert.Equal("partial path", result.ViewPath);
            Assert.Equal("master path", result.LayoutPath);
            Assert.Equal(new[] { "cshtml", "vbhtml", "razor" }, result.ViewStartFileExtensions.ToArray());
            Assert.True(result.RunViewStartPages);
        }

        [Fact]
        public void FileExtensionsProperty()
        {
            // Arrange
            string[] expected = new[]
            {
                "cshtml",
                "vbhtml",
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.FileExtensions);
        }

        [Fact]
        public void MasterLocationFormats()
        {
            // Arrange
            string[] expected = new[]
            {
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.MasterLocationFormats);
        }

        [Fact]
        public void PartialViewLocationFormats()
        {
            // Arrange
            string[] expected = new[]
            {
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.PartialViewLocationFormats);
        }

        [Fact]
        public void ViewLocationFormats()
        {
            // Arrange
            string[] expected = new[]
            {
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            // Act
            RazorViewEngine viewEngine = new RazorViewEngine();

            // Assert
            Assert.Equal(expected, viewEngine.ViewLocationFormats);
        }

        [Fact]
        public void ViewStartFileName()
        {
            Assert.Equal("_ViewStart", RazorViewEngine.ViewStartFileName);
        }

        private sealed class TestableRazorViewEngine : RazorViewEngine
        {
            public TestableRazorViewEngine()
                : base()
            {
            }

            public TestableRazorViewEngine(IViewPageActivator viewPageActivator)
                : base(viewPageActivator)
            {
            }

            public new IViewPageActivator ViewPageActivator
            {
                get { return base.ViewPageActivator; }
            }

            public IView CreatePartialView(string partialPath)
            {
                return base.CreatePartialView(new ControllerContext(), partialPath);
            }

            public IView CreateView(string viewPath, string masterPath)
            {
                return base.CreateView(new ControllerContext(), viewPath, masterPath);
            }

            // This method should remain overridable in derived view engines
            protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
            {
                return base.FileExists(controllerContext, virtualPath);
            }
        }
    }
}

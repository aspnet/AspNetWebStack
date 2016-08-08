// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Hosting;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class WebFormViewEngineTest
    {
        [Fact]
        public void CreatePartialViewCreatesWebFormView()
        {
            // Arrange
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Act
            WebFormView result = (WebFormView)engine.CreatePartialView("partial path");

            // Assert
            Assert.Equal("partial path", result.ViewPath);
            Assert.Equal(String.Empty, result.MasterPath);
        }

        [Fact]
        public void CreateViewCreatesWebFormView()
        {
            // Arrange
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Act
            WebFormView result = (WebFormView)engine.CreateView("view path", "master path");

            // Assert
            Assert.Equal("view path", result.ViewPath);
            Assert.Equal("master path", result.MasterPath);
        }

        [Fact]
        public void WebFormViewEngineSetsViewPageActivator()
        {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableWebFormViewEngine viewEngine = new TestableWebFormViewEngine(viewPageActivator.Object);

            //Act & Assert
            Assert.Equal(viewPageActivator.Object, viewEngine.ViewPageActivator);
        }

        [Fact]
        public void CreatePartialView_PassesViewPageActivator()
        {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableWebFormViewEngine viewEngine = new TestableWebFormViewEngine(viewPageActivator.Object);

            // Act
            WebFormView result = (WebFormView)viewEngine.CreatePartialView("partial path");

            // Assert
            Assert.Equal(viewEngine.ViewPageActivator, result.ViewPageActivator);
        }

        [Fact]
        public void CreateView_PassesViewPageActivator()
        {
            // Arrange
            Mock<IViewPageActivator> viewPageActivator = new Mock<IViewPageActivator>();
            TestableWebFormViewEngine viewEngine = new TestableWebFormViewEngine(viewPageActivator.Object);

            // Act
            WebFormView result = (WebFormView)viewEngine.CreateView("partial path", "master path");

            // Assert
            Assert.Equal(viewEngine.ViewPageActivator, result.ViewPageActivator);
        }

        [Fact]
        public void MasterLocationFormatsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "~/Views/{1}/{0}.master",
                "~/Views/Shared/{0}.master"
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.MasterLocationFormats);
        }

        [Fact]
        public void AreaMasterLocationFormatsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "~/Areas/{2}/Views/{1}/{0}.master",
                "~/Areas/{2}/Views/Shared/{0}.master",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.AreaMasterLocationFormats);
        }

        [Fact]
        public void PartialViewLocationFormatsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx"
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.PartialViewLocationFormats);
        }

        [Fact]
        public void AreaPartialViewLocationFormatsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.AreaPartialViewLocationFormats);
        }

        [Fact]
        public void ViewLocationFormatsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx"
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.ViewLocationFormats);
        }

        [Fact]
        public void AreaViewLocationFormatsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.AreaViewLocationFormats);
        }

        [Fact]
        public void FileExtensionsProperty()
        {
            // Arrange
            string[] expected = new string[]
            {
                "aspx",
                "ascx",
                "master",
            };

            // Act
            TestableWebFormViewEngine engine = new TestableWebFormViewEngine();

            // Assert
            Assert.Equal(expected, engine.FileExtensions);
        }

        private sealed class TestableWebFormViewEngine : WebFormViewEngine
        {
            public TestableWebFormViewEngine()
                : base()
            {
            }

            public TestableWebFormViewEngine(IViewPageActivator viewPageActivator)
                : base(viewPageActivator)
            {
            }

            public new IViewPageActivator ViewPageActivator
            {
                get { return base.ViewPageActivator; }
            }

            public new VirtualPathProvider VirtualPathProvider
            {
                get { return base.VirtualPathProvider; }
                set { base.VirtualPathProvider = value; }
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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AjaxHelperTest
    {
        [Fact]
        public void ConstructorWithNullViewContextThrows()
        {
            // Assert
            Assert.ThrowsArgumentNull(
                delegate { AjaxHelper ajaxHelper = new AjaxHelper(null, new Mock<IViewDataContainer>().Object); },
                "viewContext");
        }

        [Fact]
        public void ConstructorWithNullViewDataContainerThrows()
        {
            // Assert
            Assert.ThrowsArgumentNull(
                delegate { AjaxHelper ajaxHelper = new AjaxHelper(new Mock<ViewContext>().Object, null); },
                "viewDataContainer");
        }

        [Fact]
        public void ConstructorSetsProperties1()
        {
            // Arrange
            ViewContext viewContext = new Mock<ViewContext>().Object;
            IViewDataContainer vdc = new Mock<IViewDataContainer>().Object;

            // Act
            AjaxHelper ajaxHelper = new AjaxHelper(viewContext, vdc);

            // Assert
            Assert.Equal(viewContext, ajaxHelper.ViewContext);
            Assert.Equal(vdc, ajaxHelper.ViewDataContainer);
            Assert.Equal(RouteTable.Routes, ajaxHelper.RouteCollection);
        }

        [Fact]
        public void ConstructorSetsProperties2()
        {
            // Arrange
            ViewContext viewContext = new Mock<ViewContext>().Object;
            IViewDataContainer vdc = new Mock<IViewDataContainer>().Object;
            RouteCollection rc = new RouteCollection();

            // Act
            AjaxHelper ajaxHelper = new AjaxHelper(viewContext, vdc, rc);

            // Assert
            Assert.Equal(viewContext, ajaxHelper.ViewContext);
            Assert.Equal(vdc, ajaxHelper.ViewDataContainer);
            Assert.Equal(rc, ajaxHelper.RouteCollection);
        }

        [Fact]
        public void GenericHelperConstructorSetsProperties1()
        {
            // Arrange
            ViewContext viewContext = new Mock<ViewContext>().Object;
            ViewDataDictionary<Controller> vdd = new ViewDataDictionary<Controller>(new Mock<Controller>().Object);
            Mock<IViewDataContainer> vdc = new Mock<IViewDataContainer>();
            vdc.Setup(v => v.ViewData).Returns(vdd);

            // Act
            AjaxHelper<Controller> ajaxHelper = new AjaxHelper<Controller>(viewContext, vdc.Object);

            // Assert
            Assert.Equal(viewContext, ajaxHelper.ViewContext);
            Assert.Equal(vdc.Object, ajaxHelper.ViewDataContainer);
            Assert.Equal(RouteTable.Routes, ajaxHelper.RouteCollection);
            Assert.Equal(vdd.Model, ajaxHelper.ViewData.Model);
        }

        [Fact]
        public void GenericHelperConstructorSetsProperties2()
        {
            // Arrange
            ViewContext viewContext = new Mock<ViewContext>().Object;
            ViewDataDictionary<Controller> vdd = new ViewDataDictionary<Controller>(new Mock<Controller>().Object);
            Mock<IViewDataContainer> vdc = new Mock<IViewDataContainer>();
            vdc.Setup(v => v.ViewData).Returns(vdd);
            RouteCollection rc = new RouteCollection();

            // Act
            AjaxHelper<Controller> ajaxHelper = new AjaxHelper<Controller>(viewContext, vdc.Object, rc);

            // Assert
            Assert.Equal(viewContext, ajaxHelper.ViewContext);
            Assert.Equal(vdc.Object, ajaxHelper.ViewDataContainer);
            Assert.Equal(rc, ajaxHelper.RouteCollection);
            Assert.Equal(vdd.Model, ajaxHelper.ViewData.Model);
        }

        [Fact]
        public void GlobalizationScriptPathPropertyDefault()
        {
            try
            {
                // Act
                AjaxHelper.GlobalizationScriptPath = null;

                // Assert
                Assert.Equal("~/Scripts/Globalization", AjaxHelper.GlobalizationScriptPath);
            }
            finally
            {
                AjaxHelper.GlobalizationScriptPath = null;
            }
        }

        [Fact]
        public void GlobalizationScriptPathPropertySet()
        {
            try
            {
                // Act
                AjaxHelper.GlobalizationScriptPath = "/Foo/Bar";

                // Assert
                Assert.Equal("/Foo/Bar", AjaxHelper.GlobalizationScriptPath);
            }
            finally
            {
                AjaxHelper.GlobalizationScriptPath = null;
            }
        }

        [Fact]
        public void JavaScriptStringEncodeReturnsEmptyStringIfMessageIsEmpty()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();

            // Act
            string encoded = ajaxHelper.JavaScriptStringEncode(String.Empty);

            // Assert
            Assert.Equal(String.Empty, encoded);
        }

        [Fact]
        public void JavaScriptStringEncodeReturnsEncodedMessage()
        {
            // Arrange
            string message = "I said, \"Hello, world!\"\nHow are you?";
            AjaxHelper ajaxHelper = GetAjaxHelper();

            // Act
            string encoded = ajaxHelper.JavaScriptStringEncode(message);

            // Assert
            Assert.Equal(@"I said, \""Hello, world!\""\nHow are you?", encoded);
        }

        [Fact]
        public void JavaScriptStringEncodeReturnsNullIfMessageIsNull()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();

            // Act
            string encoded = ajaxHelper.JavaScriptStringEncode(null /* message */);

            // Assert
            Assert.Null(encoded);
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            // Act
            AjaxHelper ajaxHelper = new AjaxHelper(new Mock<ViewContext>().Object, viewDataContainer.Object);

            // Assert
            Assert.Equal(1, ajaxHelper.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataContainerInstance()
        {
            // Arrange
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            ViewDataDictionary otherViewDataDictionary = new ViewDataDictionary() { { "A", 2 } };
            Mock<IViewDataContainer> otherViewDataContainer = new Mock<IViewDataContainer>();
            otherViewDataContainer.Setup(container => container.ViewData).Returns(otherViewDataDictionary);

            AjaxHelper ajaxHelper = new AjaxHelper(new Mock<ViewContext>().Object, viewDataContainer.Object, new RouteCollection());

            // Act
            ajaxHelper.ViewDataContainer = otherViewDataContainer.Object;

            // Assert
            Assert.Equal(2, ajaxHelper.ViewBag.A);
        }

        [Fact]
        public void ViewBag_PropagatesChangesToViewData()
        {
            // Arrange
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            AjaxHelper ajaxHelper = new AjaxHelper(new Mock<ViewContext>().Object, viewDataContainer.Object, new RouteCollection());

            // Act
            ajaxHelper.ViewBag.A = "foo";
            ajaxHelper.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", ajaxHelper.ViewData["A"]);
            Assert.Equal(2, ajaxHelper.ViewData["B"]);
        }

        private static AjaxHelper GetAjaxHelper()
        {
            ViewContext viewContext = new Mock<ViewContext>().Object;
            IViewDataContainer viewDataContainer = new Mock<IViewDataContainer>().Object;
            return new AjaxHelper(viewContext, viewDataContainer);
        }
    }
}

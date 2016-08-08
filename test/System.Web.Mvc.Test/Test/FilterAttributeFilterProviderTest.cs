// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FilterAttributeFilterProviderTest
    {
        [Fact]
        public void GetFilters_WithNullController_ReturnsEmptyList()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var provider = new FilterAttributeFilterProvider();

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Assert.Empty(result);
        }

        [MyFilter(Order = 2112)]
        private class ControllerWithTypeAttribute : Controller
        {
        }

        [Fact]
        public void GetFilters_IncludesAttributesOnControllerType()
        {
            // Arrange
            var context = new ControllerContext { Controller = new ControllerWithTypeAttribute() };
            var controllerDescriptorMock = new Mock<ControllerDescriptor>();
            controllerDescriptorMock.Setup(cd => cd.GetFilterAttributes(It.IsAny<bool>()))
                .Returns(new FilterAttribute[] { new MyFilterAttribute { Order = 2112 } });
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.ControllerDescriptor).Returns(controllerDescriptorMock.Object);
            var provider = new FilterAttributeFilterProvider();

            // Act
            Filter filter = provider.GetFilters(context, actionDescriptorMock.Object).Single();

            // Assert
            MyFilterAttribute attrib = filter.Instance as MyFilterAttribute;
            Assert.NotNull(attrib);
            Assert.Equal(FilterScope.Controller, filter.Scope);
            Assert.Equal(2112, filter.Order);
        }

        private class ControllerWithActionAttribute : Controller
        {
            [MyFilter(Order = 1234)]
            public ActionResult MyActionMethod()
            {
                return null;
            }
        }

        [Fact]
        public void GetFilters_IncludesAttributesOnActionMethod()
        {
            // Arrange
            var context = new ControllerContext { Controller = new ControllerWithActionAttribute() };
            var controllerDescriptor = new ReflectedControllerDescriptor(context.Controller.GetType());
            var action = context.Controller.GetType().GetMethod("MyActionMethod");
            var actionDescriptor = new ReflectedActionDescriptor(action, "MyActionMethod", controllerDescriptor);
            var provider = new FilterAttributeFilterProvider();

            // Act
            Filter filter = provider.GetFilters(context, actionDescriptor).Single();

            // Assert
            MyFilterAttribute attrib = filter.Instance as MyFilterAttribute;
            Assert.NotNull(attrib);
            Assert.Equal(FilterScope.Action, filter.Scope);
            Assert.Equal(1234, filter.Order);
        }

        private abstract class BaseController : Controller
        {
            public ActionResult MyActionMethod()
            {
                return null;
            }
        }

        [MyFilter]
        private class DerivedController : BaseController
        {
        }

        [Fact]
        public void GetFilters_IncludesTypeAttributesFromDerivedTypeWhenMethodIsOnBaseClass()
        { // DDB #208062
            // Arrange
            var context = new ControllerContext { Controller = new DerivedController() };
            var controllerDescriptor = new ReflectedControllerDescriptor(context.Controller.GetType());
            var action = context.Controller.GetType().GetMethod("MyActionMethod");
            var actionDescriptor = new ReflectedActionDescriptor(action, "MyActionMethod", controllerDescriptor);
            var provider = new FilterAttributeFilterProvider();

            // Act
            IEnumerable<Filter> filters = provider.GetFilters(context, actionDescriptor);

            // Assert
            Assert.NotNull(filters.Select(f => f.Instance).Cast<MyFilterAttribute>().Single());
        }

        private class MyFilterAttribute : FilterAttribute
        {
        }

        [Fact]
        public void GetFilters_RetrievesCachedAttributesByDefault()
        {
            // Arrange
            var provider = new FilterAttributeFilterProvider();
            var context = new ControllerContext { Controller = new DerivedController() };
            var controllerDescriptorMock = new Mock<ControllerDescriptor>();
            controllerDescriptorMock.Setup(cd => cd.GetFilterAttributes(true)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.GetFilterAttributes(true)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            actionDescriptorMock.Setup(ad => ad.ControllerDescriptor).Returns(controllerDescriptorMock.Object);

            // Act
            var result = provider.GetFilters(context, actionDescriptorMock.Object).ToList();

            // Assert
            controllerDescriptorMock.Verify();
            actionDescriptorMock.Verify();
        }

        [Fact]
        public void GetFilters_RetrievesNonCachedAttributesWhenConfiguredNotTo()
        {
            // Arrange
            var provider = new FilterAttributeFilterProvider(false);
            var context = new ControllerContext { Controller = new DerivedController() };
            var controllerDescriptorMock = new Mock<ControllerDescriptor>();
            controllerDescriptorMock.Setup(cd => cd.GetFilterAttributes(false)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.GetFilterAttributes(false)).Returns(Enumerable.Empty<FilterAttribute>()).Verifiable();
            actionDescriptorMock.Setup(ad => ad.ControllerDescriptor).Returns(controllerDescriptorMock.Object);

            // Act
            var result = provider.GetFilters(context, actionDescriptorMock.Object).ToList();

            // Assert
            controllerDescriptorMock.Verify();
            actionDescriptorMock.Verify();
        }
    }
}

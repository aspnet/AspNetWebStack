// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public abstract class OverrideFiltersAttributeTests
    {
        protected abstract Type ProductUnderTestType { get; }

        protected abstract Type ExpectedFiltersToOverride { get; }

        [Fact]
        public void FiltersToOverride_IsIActionFilter()
        {
            // Arrange
            IOverrideFilter product = CreateProductUnderTest();

            // Act
            Type filtersToOverride = product.FiltersToOverride;

            // Assert
            Assert.Same(ExpectedFiltersToOverride, filtersToOverride);
        }

        [Fact]
        public void AttributeUsage_IsAsSpecified()
        {
            // Act
            AttributeUsageAttribute usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(ProductUnderTestType,
                typeof(AttributeUsageAttribute));

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Method, usage.ValidOn);
            Assert.True(usage.Inherited);
            Assert.False(usage.AllowMultiple);
        }

        [Fact]
        public void Integration_FilterAttributeFilterProvider_PassesThroughInstance()
        {
            // Arrange
            object expected = CreateProductUnderTest();
            FilterAttributeFilterProvider integrator = new FilterAttributeFilterProvider();
            ActionDescriptor actionDescriptor = CreateActionDescriptor(expected);
            ControllerContext controllerContext = CreateControllerContext();

            // Act
            IEnumerable<Filter> filters = integrator.GetFilters(controllerContext, actionDescriptor);

            // Assert
            Assert.NotNull(filters);
            Filter filter = Assert.Single(filters);
            Assert.NotNull(filter);
            Assert.Same(expected, filter.Instance);
        }

        private static ActionDescriptor CreateActionDescriptor(object filter)
        {
            Mock<ActionDescriptor> mock = new Mock<ActionDescriptor>(MockBehavior.Strict);
            FilterAttribute attribute = filter as FilterAttribute;
            mock.Setup(d => d.GetFilterAttributes(It.IsAny<bool>()))
                .Returns(attribute != null ? new FilterAttribute[] { attribute } : new FilterAttribute[0]);
            mock.Setup(d => d.ControllerDescriptor).Returns(new Mock<ControllerDescriptor>().Object);
            return mock.Object;
        }

        private static ControllerContext CreateControllerContext()
        {
            Mock<ControllerContext> mock = new Mock<ControllerContext>();
            mock.SetupGet(c => c.Controller).Returns(new Mock<ControllerBase>(MockBehavior.Strict).Object);
            return mock.Object;
        }

        protected abstract IOverrideFilter CreateProductUnderTest();
    }
}

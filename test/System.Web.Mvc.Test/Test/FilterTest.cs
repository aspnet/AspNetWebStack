// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FilterTest
    {
        [Fact]
        public void GuardClause()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new Filter(null, FilterScope.Action, null),
                "instance"
                );
        }

        [Fact]
        public void FilterDoesNotImplementIOrderedFilter()
        {
            // Arrange
            var filterInstance = new object();

            // Act
            var filter = new Filter(filterInstance, FilterScope.Action, null);

            // Assert
            Assert.Same(filterInstance, filter.Instance);
            Assert.Equal(FilterScope.Action, filter.Scope);
            Assert.Equal(Filter.DefaultOrder, filter.Order);
        }

        [Fact]
        public void FilterImplementsIOrderedFilter()
        {
            // Arrange
            var filterInstance = new Mock<IMvcFilter>();
            filterInstance.SetupGet(f => f.Order).Returns(42);

            // Act
            var filter = new Filter(filterInstance.Object, FilterScope.Controller, null);

            // Assert
            Assert.Same(filterInstance.Object, filter.Instance);
            Assert.Equal(FilterScope.Controller, filter.Scope);
            Assert.Equal(42, filter.Order);
        }

        [Fact]
        public void ExplicitOrderOverridesIOrderedFilter()
        {
            // Arrange
            var filterInstance = new Mock<IMvcFilter>();
            filterInstance.SetupGet(f => f.Order).Returns(42);

            // Act
            var filter = new Filter(filterInstance.Object, FilterScope.Controller, 2112);

            // Assert
            Assert.Same(filterInstance.Object, filter.Instance);
            Assert.Equal(FilterScope.Controller, filter.Scope);
            Assert.Equal(2112, filter.Order);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FilterInfoTest
    {
        [Fact]
        public void Constructor_Default()
        {
            // Arrange + Act
            FilterInfo filterInfo = new FilterInfo();

            // Assert
            Assert.Empty(filterInfo.ActionFilters);
            Assert.Empty(filterInfo.AuthorizationFilters);
            Assert.Empty(filterInfo.AuthenticationFilters);
            Assert.Empty(filterInfo.ExceptionFilters);
            Assert.Empty(filterInfo.ResultFilters);
        }

        [Fact]
        public void Constructor_PopulatesFilterCollections()
        {
            // Arrange
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            IAuthorizationFilter expectedAuthorizationFilter = CreateDummyAuthorizationFilter();
            IAuthenticationFilter expectedAuthenticationFilter = CreateDummyAuthenticationFilter();
            IExceptionFilter expectedExceptionFilter = CreateDummyExceptionFilter();
            IResultFilter expectedResultFilter = CreateDummyResultFilter();

            List<Filter> filters = new List<Filter>()
            {
                CreateFilter(expectedActionFilter),
                CreateFilter(expectedAuthorizationFilter),
                CreateFilter(expectedAuthenticationFilter),
                CreateFilter(expectedExceptionFilter),
                CreateFilter(expectedResultFilter),
            };

            // Act
            FilterInfo filterInfo = new FilterInfo(filters);

            // Assert
            Assert.Same(expectedActionFilter, filterInfo.ActionFilters.SingleOrDefault());
            Assert.Same(expectedAuthorizationFilter, filterInfo.AuthorizationFilters.SingleOrDefault());
            Assert.Same(expectedAuthenticationFilter, filterInfo.AuthenticationFilters.SingleOrDefault());
            Assert.Same(expectedExceptionFilter, filterInfo.ExceptionFilters.SingleOrDefault());
            Assert.Same(expectedResultFilter, filterInfo.ResultFilters.SingleOrDefault());
        }

        [Fact]
        public void Constructor_IteratesOverFiltersOnlyOnce()
        {
            // Arrange
            var filtersMock = new Mock<IEnumerable<Filter>>();
            filtersMock.Setup(f => f.GetEnumerator()).Returns(new List<Filter>().GetEnumerator());

            // Act
            FilterInfo filterInfo = new FilterInfo(filtersMock.Object);

            // Assert
            filtersMock.Verify(f => f.GetEnumerator(), Times.Once());
        }

        [Fact]
        public void ActionFilters_ReturnsAllActionFilters_WhenOverrideScopeIsFirst()
        {
            // Arrange
            IActionFilter expectedFirstFilter = CreateDummyActionFilter();
            Filter firstFilter = CreateFilter(expectedFirstFilter, FilterScope.First);
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            Filter globalFilter = CreateFilter(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            Filter controllerFilter = CreateFilter(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            Filter actionFilter = CreateFilter(expectedActionFilter, FilterScope.Action);
            IActionFilter expectedLastFilter = CreateDummyActionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IActionFilter)), FilterScope.First);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(5, actionFilters.Count);
            Assert.Same(expectedFirstFilter, actionFilters[0]);
            Assert.Same(expectedGlobalFilter, actionFilters[1]);
            Assert.Same(expectedControllerFilter, actionFilters[2]);
            Assert.Same(expectedActionFilter, actionFilters[3]);
            Assert.Same(expectedLastFilter, actionFilters[4]);
        }

        [Fact]
        public void ActionFilters_ReturnsGlobalAndBelowActionFilters_WhenOverrideScopeIsGlobal()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.First);
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            Filter globalFilter = CreateFilter(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            Filter controllerFilter = CreateFilter(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            Filter actionFilter = CreateFilter(expectedActionFilter, FilterScope.Action);
            IActionFilter expectedLastFilter = CreateDummyActionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IActionFilter)), FilterScope.Global);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(4, actionFilters.Count);
            Assert.Same(expectedGlobalFilter, actionFilters[0]);
            Assert.Same(expectedControllerFilter, actionFilters[1]);
            Assert.Same(expectedActionFilter, actionFilters[2]);
            Assert.Same(expectedLastFilter, actionFilters[3]);
        }

        [Fact]
        public void ActionFilters_ReturnsControllerAndBelowActionFilters_WhenOverrideScopeIsController()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            Filter controllerFilter = CreateFilter(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            Filter actionFilter = CreateFilter(expectedActionFilter, FilterScope.Action);
            IActionFilter expectedLastFilter = CreateDummyActionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IActionFilter)), FilterScope.Controller);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(3, actionFilters.Count);
            Assert.Same(expectedControllerFilter, actionFilters[0]);
            Assert.Same(expectedActionFilter, actionFilters[1]);
            Assert.Same(expectedLastFilter, actionFilters[2]);
        }

        [Fact]
        public void ActionFilters_ReturnsActionScopeAndBelowActionFilters_WhenOverrideScopeIsAction()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.Global);
            Filter controllerFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            Filter actionFilter = CreateFilter(expectedActionFilter, FilterScope.Action);
            IActionFilter expectedLastFilter = CreateDummyActionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IActionFilter)), FilterScope.Action);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(2, actionFilters.Count);
            Assert.Same(expectedActionFilter, actionFilters[0]);
            Assert.Same(expectedLastFilter, actionFilters[1]);
        }

        [Fact]
        public void ActionFilters_ReturnLastActionFilters_WhenOverrideScopeIsLast()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.Global);
            Filter controllerFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.Controller);
            Filter actionFilter = CreateFilter(CreateDummyActionFilter(), FilterScope.Action);
            IActionFilter expectedLastFilter = CreateDummyActionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IActionFilter)), FilterScope.Last);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(1, actionFilters.Count);
            Assert.Same(expectedLastFilter, actionFilters[0]);
        }

        [Fact]
        public void ActionFilters_ReturnsAllActionFilters_WhenOtherFilterIsOverriddenAtActionLevel()
        {
            // Arrange
            IActionFilter expectedFirstFilter = CreateDummyActionFilter();
            Filter firstFilter = CreateFilter(expectedFirstFilter, FilterScope.First);
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            Filter globalFilter = CreateFilter(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            Filter controllerFilter = CreateFilter(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            Filter actionFilter = CreateFilter(expectedActionFilter, FilterScope.Action);
            IActionFilter expectedLastFilter = CreateDummyActionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(object)), FilterScope.Action);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(5, actionFilters.Count);
            Assert.Same(expectedFirstFilter, actionFilters[0]);
            Assert.Same(expectedGlobalFilter, actionFilters[1]);
            Assert.Same(expectedControllerFilter, actionFilters[2]);
            Assert.Same(expectedActionFilter, actionFilters[3]);
            Assert.Same(expectedLastFilter, actionFilters[4]);
        }

        [Fact]
        public void AuthorizationFilters_ReturnsLastAuthorizationFilters_WhenOverrideScopeIsLast()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyAuthorizationFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyAuthorizationFilter(), FilterScope.Global);
            Filter controllerFilter = CreateFilter(CreateDummyAuthorizationFilter(), FilterScope.Controller);
            Filter actionFilter = CreateFilter(CreateDummyAuthorizationFilter(), FilterScope.Action);
            IAuthorizationFilter expectedLastFilter = CreateDummyAuthorizationFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IAuthorizationFilter)), FilterScope.Last);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IAuthorizationFilter> authorizationFilters = product.AuthorizationFilters;

            // Assert
            Assert.NotNull(authorizationFilters);
            Assert.Equal(1, authorizationFilters.Count);
            Assert.Same(expectedLastFilter, authorizationFilters[0]);
        }

        [Fact]
        public void AuthenticationFilters_ReturnsLastAuthenticationFilters_WhenOverrideScopeIsLast()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyAuthenticationFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyAuthenticationFilter(), FilterScope.Global);
            Filter controllerFilter = CreateFilter(CreateDummyAuthenticationFilter(), FilterScope.Controller);
            Filter actionFilter = CreateFilter(CreateDummyAuthenticationFilter(), FilterScope.Action);
            IAuthenticationFilter expectedLastFilter = CreateDummyAuthenticationFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IAuthenticationFilter)), FilterScope.Last);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IAuthenticationFilter> authenticationFilters = product.AuthenticationFilters;

            // Assert
            Assert.NotNull(authenticationFilters);
            Assert.Equal(1, authenticationFilters.Count);
            Assert.Same(expectedLastFilter, authenticationFilters[0]);
        }

        [Fact]
        public void ExceptionFilters_ReturnsLastExceptionFilters_WhenOverrideScopeIsLast()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyExceptionFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyExceptionFilter(), FilterScope.Global);
            Filter controllerFilter = CreateFilter(CreateDummyExceptionFilter(), FilterScope.Controller);
            Filter actionFilter = CreateFilter(CreateDummyExceptionFilter(), FilterScope.Action);
            IExceptionFilter expectedLastFilter = CreateDummyExceptionFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IExceptionFilter)), FilterScope.Last);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IExceptionFilter> exceptionFilters = product.ExceptionFilters;

            // Assert
            Assert.NotNull(exceptionFilters);
            Assert.Equal(1, exceptionFilters.Count);
            Assert.Same(expectedLastFilter, exceptionFilters[0]);
        }

        [Fact]
        public void ResultFilters_ReturnsLastResultFilters_WhenOverrideScopeIsLast()
        {
            // Arrange
            Filter firstFilter = CreateFilter(CreateDummyResultFilter(), FilterScope.First);
            Filter globalFilter = CreateFilter(CreateDummyResultFilter(), FilterScope.Global);
            Filter controllerFilter = CreateFilter(CreateDummyResultFilter(), FilterScope.Controller);
            Filter actionFilter = CreateFilter(CreateDummyResultFilter(), FilterScope.Action);
            IResultFilter expectedLastFilter = CreateDummyResultFilter();
            Filter lastFilter = CreateFilter(expectedLastFilter, FilterScope.Last);
            Filter overrideFilter = CreateFilter(CreateOverride(typeof(IResultFilter)), FilterScope.Last);
            IEnumerable<Filter> filters = new Filter[] { firstFilter, globalFilter, controllerFilter, actionFilter,
                lastFilter, overrideFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IResultFilter> resultFilters = product.ResultFilters;

            // Assert
            Assert.NotNull(resultFilters);
            Assert.Equal(1, resultFilters.Count);
            Assert.Same(expectedLastFilter, resultFilters[0]);
        }

        [Fact]
        public void FilterImplementingMultipleType_WhereOneTypeIsOverridden_AppearsOnlyInTheOtherList()
        {
            // Arrange
            object expectedInstance = new ActionAndExceptionFilter();
            Filter actionAndExceptionFilter = CreateFilter(expectedInstance, FilterScope.Global);
            Filter overrideExceptionFilter = CreateFilter(CreateOverride(typeof(IExceptionFilter)),
                FilterScope.Action);
            IEnumerable<Filter> filters = new Filter[] { actionAndExceptionFilter, overrideExceptionFilter };
            FilterInfo product = CreateProductUnderTest(filters);

            // Act
            IList<IActionFilter> actionFilters = product.ActionFilters;
            IList<IExceptionFilter> exceptionFilters = product.ExceptionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(1, actionFilters.Count);
            Assert.Same(expectedInstance, actionFilters[0]);
            Assert.NotNull(exceptionFilters);
            Assert.Equal(0, exceptionFilters.Count);
        }

        private static IActionFilter CreateDummyActionFilter()
        {
            return new Mock<IActionFilter>(MockBehavior.Strict).Object;
        }

        private static IAuthenticationFilter CreateDummyAuthenticationFilter()
        {
            return new Mock<IAuthenticationFilter>(MockBehavior.Strict).Object;
        }

        private static IAuthorizationFilter CreateDummyAuthorizationFilter()
        {
            return new Mock<IAuthorizationFilter>(MockBehavior.Strict).Object;
        }

        private static IExceptionFilter CreateDummyExceptionFilter()
        {
            return new Mock<IExceptionFilter>(MockBehavior.Strict).Object;
        }

        private static IResultFilter CreateDummyResultFilter()
        {
            return new Mock<IResultFilter>(MockBehavior.Strict).Object;
        }

        private static IEnumerable<Filter> CreateEmptyFilters()
        {
            return new Filter[0];
        }

        private static IOverrideFilter CreateOverride(Type filtersToOverride)
        {
            Mock<IOverrideFilter> mock = new Mock<IOverrideFilter>();
            mock.Setup(f => f.FiltersToOverride).Returns(filtersToOverride);
            return mock.Object;
        }

        private static FilterInfo CreateProductUnderTest(IEnumerable<Filter> filters)
        {
            return new FilterInfo(filters);
        }

        private static Filter CreateFilter(object instance)
        {
            return CreateFilter(instance, FilterScope.Global);
        }

        private static Filter CreateFilter(object instance, FilterScope scope)
        {
            return new Filter(instance, scope, null);
        }

        private class ActionAndExceptionFilter : IActionFilter, IExceptionFilter
        {
            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                throw new NotImplementedException();
            }

            public void OnActionExecuted(ActionExecutedContext filterContext)
            {
                throw new NotImplementedException();
            }

            public void OnException(ExceptionContext filterContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}

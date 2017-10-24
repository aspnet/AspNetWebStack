// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class FilterGroupingTests
    {
        [Fact]
        public void ActionFilters_ReturnsEmptyArray_WhenFiltersIsEmpty()
        {
            // Arrange
            IEnumerable<FilterInfo> filters = CreateEmptyFilters();
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Empty(actionFilters);
        }

        [Fact]
        public void ActionFilters_ReturnsActionFilters()
        {
            // Arrange
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(3, actionFilters.Length);
            Assert.Same(expectedGlobalFilter, actionFilters[0]);
            Assert.Same(expectedControllerFilter, actionFilters[1]);
            Assert.Same(expectedActionFilter, actionFilters[2]);
        }

        [Fact]
        public void ActionFilters_ReturnsAllActionFilters_WhenOverrideScopeIsGlobal()
        {
            // Arrange
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(IActionFilter)), FilterScope.Global);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(3, actionFilters.Length);
            Assert.Same(expectedGlobalFilter, actionFilters[0]);
            Assert.Same(expectedControllerFilter, actionFilters[1]);
            Assert.Same(expectedActionFilter, actionFilters[2]);
        }

        [Fact]
        public void ActionFilters_ReturnsControllerAndBelowActionFilters_WhenOverrideScopeIsController()
        {
            // Arrange
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(IActionFilter)), FilterScope.Controller);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(2, actionFilters.Length);
            Assert.Same(expectedControllerFilter, actionFilters[0]);
            Assert.Same(expectedActionFilter, actionFilters[1]);
        }

        [Fact]
        public void ActionFilters_ReturnsActionScopeActionFilters_WhenOverrideScopeIsAction()
        {
            // Arrange
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(IActionFilter)), FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            IActionFilter singleFilter = Assert.Single(actionFilters);
            Assert.Same(expectedActionFilter, singleFilter);
        }

        [Fact]
        public void ActionFilters_ReturnsAllActionFilters_WhenOtherFilterIsOverriddenAtActionLevel()
        {
            // Arrange
            IActionFilter expectedGlobalFilter = CreateDummyActionFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IActionFilter expectedControllerFilter = CreateDummyActionFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IActionFilter expectedActionFilter = CreateDummyActionFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(object)), FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            Assert.Equal(3, actionFilters.Length);
            Assert.Same(expectedGlobalFilter, actionFilters[0]);
            Assert.Same(expectedControllerFilter, actionFilters[1]);
            Assert.Same(expectedActionFilter, actionFilters[2]);
        }

        [Fact]
        public void AuthorizationFilters_ReturnsActionScopeAuthorizationFilters_WhenOverrideScopeIsAction()
        {
            // Arrange
            IAuthorizationFilter expectedGlobalFilter = CreateDummyAuthorizationFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IAuthorizationFilter expectedControllerFilter = CreateDummyAuthorizationFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IAuthorizationFilter expectedActionFilter = CreateDummyAuthorizationFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(IAuthorizationFilter)),
                FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IAuthorizationFilter[] authorizationFilters = product.AuthorizationFilters;

            // Assert
            Assert.NotNull(authorizationFilters);
            IAuthorizationFilter authorizationFilter = Assert.Single(authorizationFilters);
            Assert.Same(expectedActionFilter, authorizationFilter);
        }

        [Fact]
        public void AuthenticationFilters_ReturnsActionScopeAuthenticationFilters_WhenOverrideScopeIsAction()
        {
            // Arrange
            IAuthenticationFilter expectedGlobalFilter = CreateDummyAuthenticationFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IAuthenticationFilter expectedControllerFilter = CreateDummyAuthenticationFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IAuthenticationFilter expectedActionFilter = CreateDummyAuthenticationFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(IAuthenticationFilter)),
                FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IAuthenticationFilter[] authenticationFilters = product.AuthenticationFilters;

            // Assert
            Assert.NotNull(authenticationFilters);
            IAuthenticationFilter authenticationFilter = Assert.Single(authenticationFilters);
            Assert.Same(expectedActionFilter, authenticationFilter);
        }

        [Fact]
        public void ExceptionFilters_ReturnsActionScopeExceptionFilters_WhenOverrideScopeIsAction()
        {
            // Arrange
            IExceptionFilter expectedGlobalFilter = CreateDummyExceptionFilter();
            FilterInfo globalFilter = new FilterInfo(expectedGlobalFilter, FilterScope.Global);
            IExceptionFilter expectedControllerFilter = CreateDummyExceptionFilter();
            FilterInfo controllerFilter = new FilterInfo(expectedControllerFilter, FilterScope.Controller);
            IExceptionFilter expectedActionFilter = CreateDummyExceptionFilter();
            FilterInfo actionFilter = new FilterInfo(expectedActionFilter, FilterScope.Action);
            FilterInfo overrideFilter = new FilterInfo(CreateOverride(typeof(IExceptionFilter)), FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { globalFilter, controllerFilter, actionFilter, overrideFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IExceptionFilter[] exceptionFilters = product.ExceptionFilters;

            // Assert
            Assert.NotNull(exceptionFilters);
            IExceptionFilter exceptionFilter = Assert.Single(exceptionFilters);
            Assert.Same(expectedActionFilter, exceptionFilter);
        }

        [Fact]
        public void FilterImplementingMultipleType_WhereOneTypeIsOverridden_AppearsOnlyInTheOtherList()
        {
            // Arrange
            IFilter expectedInstance = new ActionAndExceptionFilter();
            FilterInfo actionAndExceptionFilter = new FilterInfo(expectedInstance, FilterScope.Global);
            FilterInfo overrideExceptionFilter = new FilterInfo(CreateOverride(typeof(IExceptionFilter)),
                FilterScope.Action);
            IEnumerable<FilterInfo> filters = new FilterInfo[] { actionAndExceptionFilter, overrideExceptionFilter };
            FilterGrouping product = CreateProductUnderTest(filters);

            // Act
            IActionFilter[] actionFilters = product.ActionFilters;
            IExceptionFilter[] exceptionFilters = product.ExceptionFilters;

            // Assert
            Assert.NotNull(actionFilters);
            IActionFilter actionFilter = Assert.Single(actionFilters);
            Assert.Same(expectedInstance, actionFilter);
            Assert.NotNull(exceptionFilters);
            Assert.Empty(exceptionFilters);
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

        private static IEnumerable<FilterInfo> CreateEmptyFilters()
        {
            return new FilterInfo[0];
        }

        private static IOverrideFilter CreateOverride(Type filtersToOverride)
        {
            Mock<IOverrideFilter> mock = new Mock<IOverrideFilter>();
            mock.Setup(f => f.FiltersToOverride).Returns(filtersToOverride);
            return mock.Object;
        }

        private static FilterGrouping CreateProductUnderTest(IEnumerable<FilterInfo> filters)
        {
            return new FilterGrouping(filters);
        }

        private class ActionAndExceptionFilter : IActionFilter, IExceptionFilter
        {
            public bool AllowMultiple
            {
                get { throw new NotImplementedException(); }
            }

            public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext,
                CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
            {
                throw new NotImplementedException();
            }

            public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}

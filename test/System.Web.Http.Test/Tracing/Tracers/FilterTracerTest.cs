// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class FilterTracerTest
    {
        [Fact]
        public void CreateFilterTracers_IFilter_With_IFilter_Returns_Single_Wrapped_IFilter()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<FilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IActionFilter_Returns_Single_Wrapped_IActionFilter()
        {
            // Arrange
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IExceptionFilter_Returns_Single_Wrapped_IExceptionFilter()
        {
            // Arrange
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IAuthenticationFilter_Returns_Single_Wrapped_IAuthenticationFilter()
        {
            // Arrange
            IAuthenticationFilter expectedInner = new Mock<IAuthenticationFilter>().Object;
            ITraceWriter expectedTraceWriter = new TestTraceWriter();

            // Act
            IEnumerable<IFilter> tracers = FilterTracer.CreateFilterTracers(expectedInner, expectedTraceWriter);

            // Assert
            Assert.NotNull(tracers);
            Assert.Equal(1, tracers.Count());
            IFilter untypedFilter = tracers.Single();
            Assert.IsType<AuthenticationFilterTracer>(untypedFilter);
            AuthenticationFilterTracer tracer = (AuthenticationFilterTracer)untypedFilter;
            Assert.Same(expectedInner, tracer.InnerFilter);
            Assert.Same(expectedTraceWriter, tracer.TraceWriter);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IAuthorizationFilter_Returns_Single_Wrapped_IAuthorizationFilter()
        {
            // Arrange
            Mock<IAuthorizationFilter> mockFilter = new Mock<IAuthorizationFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IOverrideFilter_Returns_Single_Wrapped_IOverrideFilter()
        {
            // Arrange
            IOverrideFilter expectedInner = new Mock<IOverrideFilter>().Object;
            ITraceWriter expectedTraceWriter = new TestTraceWriter();

            // Act
            IEnumerable<IFilter> tracers = FilterTracer.CreateFilterTracers(expectedInner, expectedTraceWriter);

            // Assert
            Assert.NotNull(tracers);
            Assert.Equal(1, tracers.Count());
            IFilter untypedFilter = tracers.Single();
            Assert.IsType<OverrideFilterTracer>(untypedFilter);
            OverrideFilterTracer tracer = (OverrideFilterTracer)untypedFilter;
            Assert.Same(expectedInner, tracer.InnerFilter);
            Assert.Same(expectedTraceWriter, tracer.TraceWriter);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_ActionFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockFilter = new Mock<ActionFilterAttribute>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterAttributeTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_ExceptionFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockFilter = new Mock<ExceptionFilterAttribute>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterAttributeTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_AuthorizationFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockFilter = new Mock<AuthorizationFilterAttribute>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterAttributeTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_All_Filter_Interfaces_Returns_Wrapped_Filters_For_Each_Filter()
        {
            // Arrange
            IFilter filter = new TestFilterAllBehaviors();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(5, wrappedFilters.Length);
            Assert.Equal(1, wrappedFilters.OfType<ActionFilterTracer>().Count());
            Assert.Equal(1, wrappedFilters.OfType<AuthorizationFilterTracer>().Count());
            Assert.Equal(1, wrappedFilters.OfType<AuthenticationFilterTracer>().Count());
            Assert.Equal(1, wrappedFilters.OfType<ExceptionFilterTracer>().Count());
            Assert.Equal(1, wrappedFilters.OfType<OverrideFilterTracer>().Count());
        }

        [Fact]
        public void CreateFilterTracers_With_IFilter_Returns_Single_Wrapped_IFilter()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<FilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IActionFilter_Returns_Single_Wrapped_IActionFilter()
        {
            // Arrange
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IExceptionFilter_Returns_Single_Wrapped_IExceptionFilter()
        {
            // Arrange
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IAuthenticationFilter_Returns_Single_Wrapped_IAuthenticationFilter()
        {
            // Arrange
            IAuthenticationFilter expectedInner = new Mock<IAuthenticationFilter>().Object;
            FilterInfo inputFilterInfo = new FilterInfo(expectedInner, FilterScope.Action);
            ITraceWriter expectedTraceWriter = new TestTraceWriter();

            // Act
            IEnumerable<FilterInfo> filters = FilterTracer.CreateFilterTracers(inputFilterInfo, expectedTraceWriter);

            // Assert
            Assert.NotNull(filters);
            Assert.Equal(1, filters.Count());
            FilterInfo filterInfo = filters.Single();
            Assert.NotNull(filterInfo);
            IFilter untypedFilter = filterInfo.Instance;
            Assert.IsType<AuthenticationFilterTracer>(untypedFilter);
            AuthenticationFilterTracer tracer = (AuthenticationFilterTracer)untypedFilter;
            Assert.Same(expectedInner, tracer.InnerFilter);
            Assert.Same(expectedTraceWriter, tracer.TraceWriter);
        }

        [Fact]
        public void CreateFilterTracers_With_IAuthorizationFilter_Returns_Single_Wrapped_IAuthorizationFilter()
        {
            // Arrange
            Mock<IAuthorizationFilter> mockFilter = new Mock<IAuthorizationFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IOverrideFilter_Returns_Single_Wrapped_IOverrideFilter()
        {
            // Arrange
            IOverrideFilter expectedInner = new Mock<IOverrideFilter>().Object;
            FilterInfo inputFilterInfo = new FilterInfo(expectedInner, FilterScope.Action);
            ITraceWriter expectedTraceWriter = new TestTraceWriter();

            // Act
            IEnumerable<FilterInfo> filters = FilterTracer.CreateFilterTracers(inputFilterInfo, expectedTraceWriter);

            // Assert
            Assert.NotNull(filters);
            Assert.Equal(1, filters.Count());
            FilterInfo filterInfo = filters.Single();
            Assert.NotNull(filterInfo);
            IFilter untypedFilter = filterInfo.Instance;
            Assert.IsType<OverrideFilterTracer>(untypedFilter);
            OverrideFilterTracer tracer = (OverrideFilterTracer)untypedFilter;
            Assert.Same(expectedInner, tracer.InnerFilter);
            Assert.Same(expectedTraceWriter, tracer.TraceWriter);
        }

        [Fact]
        public void CreateFilterTracers_With_ActionFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockFilter = new Mock<ActionFilterAttribute>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterAttributeTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_ExceptionFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockFilter = new Mock<ExceptionFilterAttribute>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterAttributeTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_AuthorizationFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockFilter = new Mock<AuthorizationFilterAttribute>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterAttributeTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_All_Filter_Interfaces_Returns_Wrapped_Filters_For_Each_Filter()
        {
            // Arrange
            FilterInfo filter = new FilterInfo(new TestFilterAllBehaviors(), FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(5, wrappedFilters.Length);
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(ActionFilterTracer)).Count());
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(AuthorizationFilterTracer)).Count());
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(AuthenticationFilterTracer)).Count());
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(ExceptionFilterTracer)).Count());
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(OverrideFilterTracer)).Count());
        }

        [Fact]
        public void Inner_Property_On_FilterTracer_Returns_IFilter()
        {
            // Arrange
            IFilter expectedInner = new Mock<IFilter>().Object;
            FilterTracer productUnderTest = new FilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IFilter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_Property_On_FilterTracer_Returns_IFilter()
        {
            // Arrange
            IFilter expectedInner = new Mock<IFilter>().Object;
            FilterTracer productUnderTest = new FilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IFilter actualInner = Decorator.GetInner(productUnderTest as IFilter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        // Test filter class that exposes all filter behaviors will cause separate filters for each
        class TestFilterAllBehaviors : IActionFilter, IExceptionFilter, IAuthorizationFilter, IAuthenticationFilter,
            IOverrideFilter
        {
            Task<Net.Http.HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(Controllers.HttpActionContext actionContext, Threading.CancellationToken cancellationToken, Func<Threading.Tasks.Task<Net.Http.HttpResponseMessage>> continuation)
            {
                throw new NotImplementedException();
            }

            bool IFilter.AllowMultiple
            {
                get { throw new NotImplementedException(); }
            }

            Task IExceptionFilter.ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<Net.Http.HttpResponseMessage>> continuation)
            {
                throw new NotImplementedException();
            }

            Task IAuthenticationFilter.AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            Task IAuthenticationFilter.ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Type FiltersToOverride
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}

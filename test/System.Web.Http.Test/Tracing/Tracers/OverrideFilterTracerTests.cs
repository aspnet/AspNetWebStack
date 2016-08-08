// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
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
    public class OverrideFilterTracerTests
    {
        [Fact]
        public void InnerFilter_IsSpecifiedInstance()
        {
            // Arrange
            IOverrideFilter expectedInnerFilter = CreateDummyInnerFilter();
            ITraceWriter traceWriter = CreateDummyTraceWriter();
            FilterTracer product = CreateProductUnderTest(expectedInnerFilter, traceWriter);

            // Act
            IFilter innerFilter = product.InnerFilter;

            // Assert
            Assert.Same(expectedInnerFilter, innerFilter);
        }

        [Fact]
        public void Filter_IsSpecifiedInstance()
        {
            // Arrange
            IOverrideFilter expectedInnerFilter = CreateDummyInnerFilter();
            ITraceWriter traceWriter = CreateDummyTraceWriter();
            IDecorator<IOverrideFilter> product = CreateProductUnderTest(expectedInnerFilter, traceWriter);

            // Act
            IOverrideFilter innerFilter = product.Inner;

            // Assert
            Assert.Same(expectedInnerFilter, innerFilter);
        }

        [Fact]
        public void TraceWriter_IsSpecifiedInstance()
        {
            // Arrange
            IOverrideFilter innerFilter = CreateDummyInnerFilter();
            ITraceWriter expectedTraceWriter = CreateDummyTraceWriter();
            FilterTracer product = CreateProductUnderTest(innerFilter, expectedTraceWriter);

            // Act
            ITraceWriter traceWriter = product.TraceWriter;

            // Assert
            Assert.Same(expectedTraceWriter, traceWriter);
        }

        [Fact]
        public void FiltersToOverride_IsInnerFilterSpecifiedInstance()
        {
            // Arrange
            Type expectedFiltersToOverride = typeof(IActionFilter);
            IOverrideFilter innerFilter = CreateStubInnerFilter(expectedFiltersToOverride);
            ITraceWriter traceWriter = CreateDummyTraceWriter();
            IOverrideFilter product = CreateProductUnderTest(innerFilter, traceWriter);

            // Act
            Type filtersToOverride = product.FiltersToOverride;

            // Assert
            Assert.Same(expectedFiltersToOverride, filtersToOverride);
        }

        private static IOverrideFilter CreateDummyInnerFilter()
        {
            return new Mock<IOverrideFilter>(MockBehavior.Strict).Object;
        }

        private static ITraceWriter CreateDummyTraceWriter()
        {
            return new Mock<ITraceWriter>(MockBehavior.Strict).Object;
        }

        private static OverrideFilterTracer CreateProductUnderTest(IOverrideFilter innerFilter,
            ITraceWriter traceWriter)
        {
            return new OverrideFilterTracer(innerFilter, traceWriter);
        }

        private static IOverrideFilter CreateStubInnerFilter(Type filtersToOverride)
        {
            Mock<IOverrideFilter> mock = new Mock<IOverrideFilter>();
            mock.Setup(f => f.FiltersToOverride).Returns(filtersToOverride);
            return mock.Object;
        }
    }
}

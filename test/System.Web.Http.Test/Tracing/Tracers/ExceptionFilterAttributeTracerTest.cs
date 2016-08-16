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
    public class ExceptionFilterAttributeTracerTest
    {
        [Fact]
        public void Equals_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.Equals(randomObject)).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.Equals(randomObject);

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void GetHashCode_Calls_Inner()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.GetHashCode()).Returns(1).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            int valueReturned = tracer.GetHashCode();

            // Assert
            Assert.Equal(1, valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void IsDefaultAttribute_Calls_Inner()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.IsDefaultAttribute()).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.IsDefaultAttribute();

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void Match_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.Match(randomObject)).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.Match(randomObject);

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void TypeId_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.TypeId).Returns(randomObject).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            object valueReturned = tracer.TypeId;

            // Assert
            Assert.Same(randomObject, valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void AllowMultiple_Calls_Inner()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.AllowMultiple).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.AllowMultiple;

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public async Task ExecuteExceptionFilterAsync_Traces()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<ExceptionFilterAttribute> mockAttr = new Mock<ExceptionFilterAttribute>() { CallBase = true };
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnExceptionAsync" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "OnExceptionAsync" },
            };

            // Act
            var filter = (IExceptionFilter)tracer;
            await filter.ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task ExecuteExceptionFilterAsync_Throws_And_Traces_When_Inner_OnException_Throws()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<ExceptionFilterAttribute> mockAttr = new Mock<ExceptionFilterAttribute>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockAttr.Setup(a => a.OnException(It.IsAny<HttpActionExecutedContext>())).Callback(() => { throw exception; });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            IExceptionFilter tracer = new ExceptionFilterAttributeTracer(mockAttr.Object, traceWriter) as IExceptionFilter;
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnExceptionAsync" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "OnExceptionAsync" }
            };

            // Act
            // We separate the task creation and the wait, to verify we don't throw at task creation
            // and that the exception flows inside the task (even if an Aggregate Exception was thrown).
            var task = tracer.ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);

            // Assert
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Inner_Property_On_ExceptionFilterAttributeTracer_Returns_ExceptionFilterAttribute()
        {
            // Arrange
            ExceptionFilterAttribute expectedInner = new Mock<ExceptionFilterAttribute>().Object;
            ExceptionFilterAttributeTracer productUnderTest = new ExceptionFilterAttributeTracer(expectedInner, new TestTraceWriter());

            // Act
            ExceptionFilterAttribute actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_ExceptionFilterAttributeTracer_Returns_ExceptionFilterAttribute()
        {
            // Arrange
            ExceptionFilterAttribute expectedInner = new Mock<ExceptionFilterAttribute>().Object;
            ExceptionFilterAttributeTracer productUnderTest = new ExceptionFilterAttributeTracer(expectedInner, new TestTraceWriter());

            // Act
            ExceptionFilterAttribute actualInner = Decorator.GetInner(productUnderTest as ExceptionFilterAttribute);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

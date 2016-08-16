// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class ExceptionFilterTracerTest
    {
        [Fact]
        public async Task ExecuteExceptionFilterAsync_Traces()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>() { CallBase = true };
            mockFilter.Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(), It.IsAny<CancellationToken>()))
                .Returns(TaskHelpers.Completed());
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterTracer tracer = new ExceptionFilterTracer(mockFilter.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteExceptionFilterAsync" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ExecuteExceptionFilterAsync" },
            };

            // Act
            var filter = (IExceptionFilter)tracer;
            await filter.ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task ExecuteExceptionFilterAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);
            mockFilter.Setup(a => a.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterTracer tracer = new ExceptionFilterTracer(mockFilter.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteExceptionFilterAsync" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ExecuteExceptionFilterAsync" }
            };

            // Act
            Task task = ((IExceptionFilter)tracer).ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);

            // Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Inner_Property_On_ExceptionFilterTracer_Returns_IExceptionFilter()
        {
            // Arrange
            IExceptionFilter expectedInner = new Mock<IExceptionFilter>().Object;
            ExceptionFilterTracer productUnderTest = new ExceptionFilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IExceptionFilter actualInner = productUnderTest.Inner as IExceptionFilter;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_ExceptionFilterTracer_Returns_IExceptionFilter()
        {
            // Arrange
            IExceptionFilter expectedInner = new Mock<IExceptionFilter>().Object;
            ExceptionFilterTracer productUnderTest = new ExceptionFilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IExceptionFilter actualInner = Decorator.GetInner(productUnderTest.Inner as IExceptionFilter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

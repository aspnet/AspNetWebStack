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
    public class ActionFilterTracerTest
    {
        [Fact]
        public async Task ExecuteActionAsync_Traces_ExecuteActionFilterAsync()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>() { CallBase = true };
            mockFilter.Setup(
                f =>
                f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>(),
                                           It.IsAny<Func<Task<HttpResponseMessage>>>())).Returns(
                                               Task.FromResult<HttpResponseMessage>(response));
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterTracer tracer = new ActionFilterTracer(mockFilter.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteActionFilterAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ExecuteActionFilterAsync" },
            };

            // Act
            var filter = (IActionFilter)tracer;
            await filter.ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task ExecuteActionAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>() { CallBase = true };
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(null);
            tcs.TrySetException(exception);
            mockFilter.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>(),
                                It.IsAny<Func<Task<HttpResponseMessage>>>())).Returns(tcs.Task);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterTracer tracer = new ActionFilterTracer(mockFilter.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteActionFilterAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ExecuteActionFilterAsync" },
            };

            // Act
            Task<HttpResponseMessage> task = ((IActionFilter)tracer).ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);


            // Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_ActionFilterTracer_Returns_IActionFilter()
        {
            // Arrange
            IActionFilter expectedInner = new Mock<IActionFilter>().Object;
            ActionFilterTracer productUnderTest = new ActionFilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IActionFilter actualInner = productUnderTest.Inner as IActionFilter;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_Property_On_ActionFilterTracer_Returns_IActionFilter()
        {
            // Arrange
            IActionFilter expectedInner = new Mock<IActionFilter>().Object;
            ActionFilterTracer productUnderTest = new ActionFilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IActionFilter actualInner = Decorator.GetInner(productUnderTest as IActionFilter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

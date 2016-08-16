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
    public class AuthorizationFilterTracerTest
    {
        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_Traces()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IAuthorizationFilter> mockFilter = new Mock<IAuthorizationFilter>() { CallBase = true };
            mockFilter.Setup(f => f.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<HttpResponseMessage>>>())).Returns(Task.FromResult(response));
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            Func<Task<HttpResponseMessage>> continuation = () => Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            AuthorizationFilterTracer tracer = new AuthorizationFilterTracer(mockFilter.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteAuthorizationFilterAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ExecuteAuthorizationFilterAsync" },
            };

            // Act
            var filter = (IAuthorizationFilter)tracer;
            await filter.ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, continuation);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            Mock<IAuthorizationFilter> mockAttr = new Mock<IAuthorizationFilter>() { CallBase = true };
            HttpResponseMessage response = new HttpResponseMessage();
            InvalidOperationException exception = new InvalidOperationException("test");
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(response);
            tcs.TrySetException(exception);
            mockAttr.Setup(a => a.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<HttpResponseMessage>>>())).Returns(tcs.Task);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            Func<Task<HttpResponseMessage>> continuation = () => Task.FromResult<HttpResponseMessage>(response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            AuthorizationFilterTracer tracer = new AuthorizationFilterTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteAuthorizationFilterAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ExecuteAuthorizationFilterAsync" }
            };

            // Act & Assert
            Task task = ((IAuthorizationFilter)tracer).ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, continuation);
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);

            // Assert
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Inner_Property_On_AuthorizationFilterTracer_Returns_IAuthorizationFilter()
        {
            // Arrange
            IAuthorizationFilter expectedInner = new Mock<IAuthorizationFilter>().Object;
            AuthorizationFilterTracer productUnderTest = new AuthorizationFilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IAuthorizationFilter actualInner = productUnderTest.Inner as IAuthorizationFilter;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_AuthorizationFilterTracer_Returns_IAuthorizationFilter()
        {
            // Arrange
            IAuthorizationFilter expectedInner = new Mock<IAuthorizationFilter>().Object;
            AuthorizationFilterTracer productUnderTest = new AuthorizationFilterTracer(expectedInner, new TestTraceWriter());

            // Act
            IAuthorizationFilter actualInner = Decorator.GetInner(productUnderTest as IAuthorizationFilter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

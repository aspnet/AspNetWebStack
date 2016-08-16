// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpControllerTracerTest
    {
        private Mock<HttpActionDescriptor> _mockActionDescriptor;
        private HttpControllerDescriptor _controllerDescriptor;

        public HttpControllerTracerTest()
        {
            _mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            _mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            _mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            _controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));
        }

        [Fact]
        public void Dispose_TracesAndInvokesInnerDisposeWhenControllerIsDisposable()
        {
            // Arrange
            var mockController = new Mock<IHttpController>();
            var mockDisposable = mockController.As<IDisposable>();
            var request = new HttpRequestMessage();
            var traceWriter = new TestTraceWriter();
            var tracer = new HttpControllerTracer(request, mockController.Object, traceWriter);
            var expectedTraces = new[]
            {
                new TraceRecord(request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "Dispose" },
                new TraceRecord(request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "Dispose" }
            };

            // Act
            ((IDisposable)tracer).Dispose();

            // Assert
            Assert.Equal(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            mockDisposable.Verify(d => d.Dispose(), Times.Once());
        }

        [Fact]
        public void Dispose_DoesNotTraceWhenControllerIsNotDisposable()
        {
            // Arrange
            var mockController = new Mock<IHttpController>();
            var request = new HttpRequestMessage();
            var traceWriter = new TestTraceWriter();
            var tracer = new HttpControllerTracer(request, mockController.Object, traceWriter);

            // Act
            ((IDisposable)tracer).Dispose();

            // Assert
            Assert.Empty(traceWriter.Traces);
        }

        [Fact]
        public async Task ExecuteAsync_RemovesInnerControllerFromReleaseListAndAddsItselfInstead()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var context = ContextUtil.CreateControllerContext(request: request);
            var mockController = new Mock<IHttpController>();
            var mockDisposable = mockController.As<IDisposable>();
            mockController.Setup(c => c.ExecuteAsync(context, CancellationToken.None))
                          .Callback<HttpControllerContext, CancellationToken>((cc, ct) => cc.Request.RegisterForDispose(mockDisposable.Object))
                          .Returns(() => Task.FromResult(new HttpResponseMessage()))
                          .Verifiable();
            context.ControllerDescriptor = _controllerDescriptor;
            context.Controller = mockController.Object;
            var traceWriter = new TestTraceWriter();
            var tracer = new HttpControllerTracer(request, mockController.Object, traceWriter);

            // Act
            var controller = (IHttpController)tracer;
            await controller.ExecuteAsync(context, CancellationToken.None);

            // Assert
            IEnumerable<IDisposable> disposables = (IEnumerable<IDisposable>)request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey];
            Assert.Contains(tracer, disposables);
            Assert.DoesNotContain(mockDisposable.Object, disposables);
        }

        [Fact]
        public async Task ExecuteAsync_Invokes_Inner_And_Traces()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<ApiController> mockController = new Mock<ApiController>() { CallBase = true };
            mockController.Setup(b => b.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<HttpResponseMessage>(response));

            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: request);
            controllerContext.ControllerDescriptor = _controllerDescriptor;
            controllerContext.Controller = mockController.Object;

            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext, actionDescriptor: _mockActionDescriptor.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerTracer tracer = new HttpControllerTracer(request, mockController.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            var task = ((IHttpController)tracer).ExecuteAsync(controllerContext, CancellationToken.None);
            HttpResponseMessage actualResponse = await task;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(response, actualResponse);
        }

        [Fact]
        public async Task ExecuteAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException();
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.TrySetException(exception);
            Mock<ApiController> mockController = new Mock<ApiController>() { CallBase = true };
            mockController.Setup(b => b.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);

            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: request);
            controllerContext.ControllerDescriptor = _controllerDescriptor;
            controllerContext.Controller = mockController.Object;

            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext, actionDescriptor: _mockActionDescriptor.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerTracer tracer = new HttpControllerTracer(request, mockController.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => ((IHttpController)tracer).ExecuteAsync(controllerContext, CancellationToken.None));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public async Task ExecuteAsync_IsCancelled_And_Traces_When_Inner_IsCancelled()
        {
            // Arrange
            Mock<ApiController> mockController = new Mock<ApiController>() { CallBase = true };
            mockController.Setup(b => b.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<CancellationToken>())).Returns(TaskHelpers.Canceled<HttpResponseMessage>());

            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: request);
            controllerContext.ControllerDescriptor = _controllerDescriptor;
            controllerContext.Controller = mockController.Object;

            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext, actionDescriptor: _mockActionDescriptor.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerTracer tracer = new HttpControllerTracer(request, mockController.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Warn) { Kind = TraceKind.End }
            };

            // Act
            Task task = ((IHttpController)tracer).ExecuteAsync(controllerContext, CancellationToken.None);
            Exception thrown = await Assert.ThrowsAsync<TaskCanceledException>(() => task);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Inner_Property_On_HttpControllerTracer_Returns_IHttpController()
        {
            // Arrange
            IHttpController expectedInner = new Mock<IHttpController>().Object;
            HttpControllerTracer productUnderTest = new HttpControllerTracer(new HttpRequestMessage(), expectedInner, new TestTraceWriter());

            // Act
            IHttpController actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpControllerTracer_Returns_IHttpController()
        {
            // Arrange
            IHttpController expectedInner = new Mock<IHttpController>().Object;
            HttpControllerTracer productUnderTest = new HttpControllerTracer(new HttpRequestMessage(), expectedInner, new TestTraceWriter());

            // Act
            IHttpController actualInner = Decorator.GetInner(productUnderTest as IHttpController);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

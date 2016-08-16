// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionInvokerTracerTest
    {
        private HttpActionContext _actionContext;
        private ApiController _apiController;

        public HttpActionInvokerTracerTest()
        {
            UsersController controller = new UsersController();
            _apiController = controller;

            Func<HttpResponseMessage> actionMethod = controller.Get;
            _actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(instance: _apiController),
                new ReflectedHttpActionDescriptor { MethodInfo = actionMethod.Method });
            HttpRequestMessage request = new HttpRequestMessage();
            _actionContext.ControllerContext.Request = request;
        }

        [Fact]
        public void InvokeActionAsync_Invokes_Inner_InvokeActionAsync()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var mockInnerInvoker = new Mock<IHttpActionInvoker>();
            IHttpActionInvoker invoker = new HttpActionInvokerTracer(mockInnerInvoker.Object, new Mock<ITraceWriter>().Object);

            // Act
            invoker.InvokeActionAsync(_actionContext, cts.Token);

            // Assert
            mockInnerInvoker.Verify(i => i.InvokeActionAsync(_actionContext, cts.Token), Times.Once());
        }

        [Fact]
        public async Task InvokeActionAsync_Traces_Begin_And_End_Info()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            var actionInvoker = (IHttpActionInvoker)tracer;
            await actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            Assert.Equal(2, traceWriter.Traces.Count);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task InvokeActionAsync_Returns_Cancelled_Inner_Task()
        {
            // Arrange
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), new TestTraceWriter());

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, cancellationSource.Token);

            // Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => response);
        }

        [Fact]
        public async Task InvokeActionAsync_Traces_Cancelled_Inner_Task()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), traceWriter);
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Warn) { Kind = TraceKind.End }
            };

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, cancellationSource.Token);

            // Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => response);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task InvokeActionAsync_Returns_Faulted_Inner_Task()
        {
            // Arrange
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(null);
            tcs.TrySetException(expectedException);
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, new TestTraceWriter());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task InvokeActionAsync_Traces_Faulted_Inner_Task()
        {
            // Arrange
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(null);
            tcs.TrySetException(expectedException);
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => response);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal(expectedException, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void InvokeActionAsync_Throws_When_ActionContext_Is_Null()
        {
            // Arrange
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), new TestTraceWriter());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(null, CancellationToken.None),
                "actionContext");
        }

        [Fact]
        public void InvokeActionAsync_Throws_Exception_Thrown_From_Inner()
        {
            // Arrange
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Throws(expectedException);
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, new TestTraceWriter());

            // Act & Assert
            InvalidOperationException thrownException = Assert.Throws<InvalidOperationException>(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None)
            );

            // Assert
            Assert.Equal(expectedException, thrownException);
        }

        [Fact]
        public void InvokeActionAsync_Traces_Exception_Thrown_From_Inner()
        {
            // Arrange
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Throws(expectedException);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None)
            );

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal(expectedException, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_HttpActionInvokerTracer_Returns_IHttpActionInvoker()
        {
            // Arrange
            IHttpActionInvoker expectedInner = new Mock<IHttpActionInvoker>().Object;
            HttpActionInvokerTracer productUnderTest = new HttpActionInvokerTracer(expectedInner, new TestTraceWriter());

            // Act
            IHttpActionInvoker actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpActionInvokerTracer_Returns_IHttpActionInvoker()
        {
            // Arrange
            IHttpActionInvoker expectedInner = new Mock<IHttpActionInvoker>().Object;
            HttpActionInvokerTracer productUnderTest = new HttpActionInvokerTracer(expectedInner, new TestTraceWriter());

            // Act
            IHttpActionInvoker actualInner = Decorator.GetInner(productUnderTest as IHttpActionInvoker);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

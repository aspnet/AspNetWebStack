// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class MessageHandlerTracerTest
    {
        [Fact]
        public async Task SendAsync_Traces_And_Invokes_Inner()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            MockDelegatingHandler mockHandler = new MockDelegatingHandler((rqst, cancellation) =>
                                                 Task.FromResult<HttpResponseMessage>(response));

            TestTraceWriter traceWriter = new TestTraceWriter();
            MessageHandlerTracer tracer = new MessageHandlerTracer(mockHandler, traceWriter);
            MockHttpMessageHandler mockInnerHandler = new MockHttpMessageHandler((rqst, cancellation) =>
                                     Task.FromResult<HttpResponseMessage>(response));
            tracer.InnerHandler = mockInnerHandler;

            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.MessageHandlersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "SendAsync" },
                new TraceRecord(request, TraceCategories.MessageHandlersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "SendAsync" }
            };

            MethodInfo method = typeof(DelegatingHandler).GetMethod("SendAsync",
                                                                     BindingFlags.Public | BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

            // Act
            Task<HttpResponseMessage> task = method.Invoke(tracer, new object[] { request, CancellationToken.None }) as Task<HttpResponseMessage>;
            HttpResponseMessage actualResponse = await task;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(response, actualResponse);
        }

        [Fact]
        public void SendAsync_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            MockDelegatingHandler mockHandler = new MockDelegatingHandler((rqst, cancellation) => { throw exception; });

            TestTraceWriter traceWriter = new TestTraceWriter();
            MessageHandlerTracer tracer = new MessageHandlerTracer(mockHandler, traceWriter);

            // DelegatingHandlers require an InnerHandler to run.  We create a mock one to simulate what
            // would happen when a DelegatingHandler executing after the tracer throws.
            MockHttpMessageHandler mockInnerHandler = new MockHttpMessageHandler((rqst, cancellation) => { throw exception; });
            tracer.InnerHandler = mockInnerHandler;

            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.MessageHandlersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "SendAsync" },
                new TraceRecord(request, TraceCategories.MessageHandlersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "SendAsync" }
            };

            MethodInfo method = typeof(DelegatingHandler).GetMethod("SendAsync",
                                                                     BindingFlags.Public | BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

            // Act
            Exception thrown =
                Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(tracer, new object[] { request, CancellationToken.None }));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown.InnerException);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public async Task SendAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.TrySetException(exception);
            MockDelegatingHandler mockHandler = new MockDelegatingHandler((rqst, cancellation) => { return tcs.Task; });

            TestTraceWriter traceWriter = new TestTraceWriter();
            MessageHandlerTracer tracer = new MessageHandlerTracer(mockHandler, traceWriter);

            // DelegatingHandlers require an InnerHandler to run.  We create a mock one to simulate what
            // would happen when a DelegatingHandler executing after the tracer returns a Task that throws.
            MockHttpMessageHandler mockInnerHandler = new MockHttpMessageHandler((rqst, cancellation) => { return tcs.Task; });
            tracer.InnerHandler = mockInnerHandler;

            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.MessageHandlersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "SendAsync" },
                new TraceRecord(request, TraceCategories.MessageHandlersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "SendAsync" }
            };

            MethodInfo method = typeof(DelegatingHandler).GetMethod("SendAsync",
                                                                     BindingFlags.Public | BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

            // Act
            Task<HttpResponseMessage> task =
                method.Invoke(tracer, new object[] { request, CancellationToken.None }) as Task<HttpResponseMessage>;

            // Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }


        // DelegatingHandler cannot be mocked with Moq
        private class MockDelegatingHandler : DelegatingHandler
        {
            private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _callback;

            public MockDelegatingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback)
                : base()
            {
                _callback = callback;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _callback(request, cancellationToken);
            }
        }

        // HttpMessageHandler cannot be mocked with Moq
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _callback;

            public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback)
                : base()
            {
                _callback = callback;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _callback(request, cancellationToken);
            }
        }

        [Fact]
        public void Inner_Property_On_MessageHandlerTracer_Returns_DelegatingHandler()
        {
            // Arrange
            DelegatingHandler expectedInner = new Mock<DelegatingHandler>().Object;
            MessageHandlerTracer productUnderTest = new MessageHandlerTracer(expectedInner, new TestTraceWriter());

            // Act
            DelegatingHandler actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_MessageHandlerTracer_Returns_DelegatingHandler()
        {
            // Arrange
            DelegatingHandler expectedInner = new Mock<DelegatingHandler>().Object;
            MessageHandlerTracer productUnderTest = new MessageHandlerTracer(expectedInner, new TestTraceWriter());

            // Act
            DelegatingHandler actualInner = Decorator.GetInner(productUnderTest as DelegatingHandler);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

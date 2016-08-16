// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Tracing.Tracers
{
    public class RequestMessageHandlerTracerTest
    {
        [Fact]
        public async Task SendAsync_Traces_And_Invokes_Inner()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            TestTraceWriter traceWriter = new TestTraceWriter();
            RequestMessageHandlerTracer tracer = new RequestMessageHandlerTracer(traceWriter);
            MockHttpMessageHandler mockInnerHandler = new MockHttpMessageHandler((rqst, cancellation) =>
                                     Task.FromResult<HttpResponseMessage>(response));
            tracer.InnerHandler = mockInnerHandler;

            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.RequestCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(request, TraceCategories.RequestCategory, TraceLevel.Info) { Kind = TraceKind.End }
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
            TestTraceWriter traceWriter = new TestTraceWriter();
            RequestMessageHandlerTracer tracer = new RequestMessageHandlerTracer(traceWriter);

            // DelegatingHandlers require an InnerHandler to run.  We create a mock one to simulate what
            // would happen when a DelegatingHandler executing after the tracer throws.
            MockHttpMessageHandler mockInnerHandler = new MockHttpMessageHandler((rqst, cancellation) => { throw exception; });
            tracer.InnerHandler = mockInnerHandler;

            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.RequestCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(request, TraceCategories.RequestCategory, TraceLevel.Error) { Kind = TraceKind.End }
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
            TestTraceWriter traceWriter = new TestTraceWriter();
            RequestMessageHandlerTracer tracer = new RequestMessageHandlerTracer(traceWriter);

            // DelegatingHandlers require an InnerHandler to run.  We create a mock one to simulate what
            // would happen when a DelegatingHandler executing after the tracer returns a Task that throws.
            MockHttpMessageHandler mockInnerHandler = new MockHttpMessageHandler((rqst, cancellation) => { return tcs.Task; });
            tracer.InnerHandler = mockInnerHandler;

            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.RequestCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(request, TraceCategories.RequestCategory, TraceLevel.Error) { Kind = TraceKind.End }
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
    }
}

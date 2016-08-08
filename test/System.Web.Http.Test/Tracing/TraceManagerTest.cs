// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Services;
using System.Web.Http.Tracing.Tracers;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.Tracing
{
    public class TraceManagerTest
    {
        public static TheoryDataSet<List<DelegatingHandler>> MultipleMessageHandlers
        {
            get
            {
                TestTraceWriter traceWriter = new TestTraceWriter();
                DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
                DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
                RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
                DelegatingHandler messageHandlerDummy = new Mock<DelegatingHandler>().Object;
                DelegatingHandler msgHandlerTracerDummy = new MessageHandlerTracer(messageHandlerDummy, traceWriter);
                return new TheoryDataSet<List<DelegatingHandler>>
                {
                    { new List<DelegatingHandler>() },
                    { new List<DelegatingHandler>() { messageHandler } },
                    { new List<DelegatingHandler>() { messageHandler, messageHandler } },
                    { new List<DelegatingHandler>() { messageHandler, messageHandler, messageHandler } },
                    { new List<DelegatingHandler>() { msgHandlerTracer } },
                    { new List<DelegatingHandler>() { msgHandlerTracer, msgHandlerTracer } },
                    { new List<DelegatingHandler>() { msgHandlerTracer, msgHandlerTracer, msgHandlerTracer } },
                    { new List<DelegatingHandler>() { requestMsgtracer } },
                    { new List<DelegatingHandler>() { requestMsgtracer, requestMsgtracer } },
                    { new List<DelegatingHandler>() { requestMsgtracer, requestMsgtracer, requestMsgtracer } },
                    { new List<DelegatingHandler>() { messageHandler, msgHandlerTracer } },
                    { new List<DelegatingHandler>() { msgHandlerTracer, messageHandler } },
                    { new List<DelegatingHandler>() { requestMsgtracer, messageHandler, msgHandlerTracer } },
                    { new List<DelegatingHandler>() { messageHandler, requestMsgtracer, msgHandlerTracer } },
                    { new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer } },
                    { new List<DelegatingHandler>() { requestMsgtracer, msgHandlerTracer, messageHandler } },
                    { new List<DelegatingHandler>() { msgHandlerTracer, requestMsgtracer, messageHandler } },
                    { new List<DelegatingHandler>() { msgHandlerTracer, messageHandler, requestMsgtracer } },
                    { new List<DelegatingHandler>() { messageHandler, msgHandlerTracerDummy, requestMsgtracer } }
                };
            }
        }

        [Fact]
        public void TraceManager_Is_In_Default_ServiceResolver()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            ITraceManager traceManager = config.Services.GetService(typeof(ITraceManager)) as ITraceManager;

            // Assert
            Assert.IsType<TraceManager>(traceManager);
        }

        [Theory]
        [InlineData(typeof(IHttpControllerSelector))]
        [InlineData(typeof(IHttpControllerActivator))]
        [InlineData(typeof(IHttpActionSelector))]
        [InlineData(typeof(IHttpActionInvoker))]
        [InlineData(typeof(IActionValueBinder))]
        [InlineData(typeof(IContentNegotiator))]
        [InlineData(typeof(IHttpControllerTypeResolver))]
        public void Initialize_Does_Not_Alter_Configuration_When_No_TraceWriter_Present(Type serviceType)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            object defaultService = config.Services.GetService(serviceType);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.Same(defaultService.GetType(), config.Services.GetService(serviceType).GetType());
        }

        [Theory]
        [InlineData(typeof(IHttpControllerSelector))]
        [InlineData(typeof(IHttpControllerActivator))]
        [InlineData(typeof(IHttpActionSelector))]
        [InlineData(typeof(IHttpActionInvoker))]
        [InlineData(typeof(IActionValueBinder))]
        [InlineData(typeof(IContentNegotiator))]
        [InlineData(typeof(IHttpControllerTypeResolver))]
        public void Initialize_Alters_Configuration_When_TraceWriter_Present(Type serviceType)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> traceWriter = new Mock<ITraceWriter>() { CallBase = true };
            config.Services.Replace(typeof(ITraceWriter), traceWriter.Object);
            object defaultService = config.Services.GetService(serviceType);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.NotSame(defaultService.GetType(), config.Services.GetService(serviceType).GetType());
        }

        [Fact]
        public void Initialize_Does_Not_Alter_MessageHandlers_When_No_TraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<DelegatingHandler> mockHandler = new Mock<DelegatingHandler>() { CallBase = true };
            config.MessageHandlers.Add(mockHandler.Object);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.Equal(config.MessageHandlers[config.MessageHandlers.Count - 1].GetType(), mockHandler.Object.GetType());
        }

        [Fact]
        public void Initialize_Alters_MessageHandlers_WhenTraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> traceWriter = new Mock<ITraceWriter>() { CallBase = true };
            config.Services.Replace(typeof(ITraceWriter), traceWriter.Object);
            Mock<DelegatingHandler> mockHandler = new Mock<DelegatingHandler>() { CallBase = true };
            config.MessageHandlers.Add(mockHandler.Object);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.IsAssignableFrom<RequestMessageHandlerTracer>(config.MessageHandlers[0]);
            Assert.IsAssignableFrom<MessageHandlerTracer>(config.MessageHandlers[config.MessageHandlers.Count - 2]);
            Assert.Equal(3, config.MessageHandlers.Count);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void Initialize_Does_Not_Alter_MediaTypeFormatters_When_No_TraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            new TraceManager().Initialize(config);

            // Assert
            foreach (var formatter in config.Formatters)
            {
                Assert.False(typeof(IFormatterTracer).IsAssignableFrom(formatter.GetType()));
            }
        }

        [Fact]
        public void Initialize_Alters_MediaTypeFormatters_WhenTraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> traceWriter = new Mock<ITraceWriter>() { CallBase = true };
            config.Services.Replace(typeof(ITraceWriter), traceWriter.Object);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            foreach (var formatter in config.Formatters)
            {
                Assert.IsAssignableFrom<IFormatterTracer>(formatter);
            }
        }

        [Fact]
        public void Multiple_Initialize_DoesNotAlter_Num_Of_MessageHandlers_With_No_TraceWriter()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            int expectedHandlerCount = config.MessageHandlers.Count;

            // Act
            traceManager.Initialize(config);

            // Assert
            int actualHandlerCount = config.MessageHandlers.Count;
            Assert.Equal(expectedHandlerCount, actualHandlerCount);
        }

        [Theory]
        [PropertyData("MultipleMessageHandlers")]
        public void Multiple_Initialize_DoesNotAlter_MessageHandlerCollection(List<DelegatingHandler> handlerList)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;

            // Act
            traceManager.Initialize(config);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void HttpServer_Initialize_After_Trace_Manager_Initialize_DoesNotAlter_MessageHandlerCollection()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            config.MessageHandlers.Add(messageHandler);
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<HttpControllerDispatcher> dispatcherMock = new Mock<HttpControllerDispatcher>(config);
            dispatcherMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None)
                .Returns(Task.FromResult<HttpResponseMessage>(request.CreateResponse()));

            HttpServer server = new HttpServer(config, dispatcherMock.Object);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            // Act
            Task<HttpResponseMessage> response = invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void TraceManager_Initialize_After_HttpServer_Initialize_DoesNotAlter_MessageHandlerCollection()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            config.MessageHandlers.Add(messageHandler);
            TraceManager traceManager = new TraceManager();
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<HttpControllerDispatcher> dispatcherMock = new Mock<HttpControllerDispatcher>(config);
            dispatcherMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None)
                .Returns(Task.FromResult<HttpResponseMessage>(request.CreateResponse()));

            HttpServer server = new HttpServer(config, dispatcherMock.Object);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            // Act
            Task<HttpResponseMessage> response = invoker.SendAsync(request, CancellationToken.None);
            traceManager.Initialize(config);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void HttpServer_CallsCustomMessageHandlers_InTheCorrectOrder()
        {
            // Arrange
            List<int> order = new List<int>();
            order.Add(1);
            order.Add(2);
            order.Add(3);
            order.Add(4);
            List<int>.Enumerator e = order.GetEnumerator();
            OrderAwareMessageHandler messageHandler1 = new OrderAwareMessageHandler(e, 1);
            OrderAwareMessageHandler messageHandler2 = new OrderAwareMessageHandler(e, 2);
            OrderAwareMessageHandler messageHandler3 = new OrderAwareMessageHandler(e, 3);
            OrderAwareMessageHandler messageHandler4 = new OrderAwareMessageHandler(e, 4);
            HttpRequestMessage request = new HttpRequestMessage();
            TraceManager traceManager = new TraceManager();
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            config.MessageHandlers.Add(messageHandler1);
            config.MessageHandlers.Add(messageHandler2);
            config.MessageHandlers.Add(messageHandler3);
            config.MessageHandlers.Add(messageHandler4);
            Mock<HttpControllerDispatcher> dispatcherMock = new Mock<HttpControllerDispatcher>(config);
            dispatcherMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None)
                .Returns(Task.FromResult<HttpResponseMessage>(request.CreateResponse()));
            HttpServer server = new HttpServer(config, dispatcherMock.Object);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            // Act
            Task<HttpResponseMessage> response = invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        public class OrderAwareMessageHandler : DelegatingHandler
        {
            private static IEnumerator<int> _order;
            private int _expectedOrder;

            public OrderAwareMessageHandler(IEnumerator<int> order, int expectedOrder)
            {
                _order = order;
                _expectedOrder = expectedOrder;
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _order.MoveNext();
                Assert.True(_order.Current == _expectedOrder);
                return base.SendAsync(request, cancellationToken);
            }
        }

        [Fact]
        public void Initialize_Repairs_Handler_Collection_If_MsgHandler_Deleted()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            TestTraceWriter traceWriter = new TestTraceWriter();
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
            RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
            List<DelegatingHandler> handlerList = new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer };
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            int expectedHandlerCount = config.MessageHandlers.Count;

            // Act
            config.MessageHandlers.RemoveAt(expectedHandlerCount - 1);
            traceManager.Initialize(config);

            // Assert
            int actualHandlerCount = config.MessageHandlers.Count;
            Assert.Equal(expectedHandlerCount - 2, actualHandlerCount);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void Initialize_Repairs_Handler_Collection_If_Tracer_Deleted()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            TestTraceWriter traceWriter = new TestTraceWriter();
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
            RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
            List<DelegatingHandler> handlerList = new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer };
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            int handlerCount = config.MessageHandlers.Count;
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;

            // Act
            config.MessageHandlers.RemoveAt(handlerCount - 2);
            traceManager.Initialize(config);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void Initialize_Repairs_Handler_Collection_If_RequestTracer_Deleted()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            TestTraceWriter traceWriter = new TestTraceWriter();
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
            RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
            List<DelegatingHandler> handlerList = new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer };
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;

            // Act
            config.MessageHandlers.RemoveAt(2);
            traceManager.Initialize(config);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void Initialize_Repairs_Handler_Collection_If_MsgHandler_Inserted()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            TestTraceWriter traceWriter = new TestTraceWriter();
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
            RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
            List<DelegatingHandler> handlerList = new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer };
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            int expectedHandlerCount = config.MessageHandlers.Count;

            // Act
            config.MessageHandlers.Insert(1, messageHandler);
            traceManager.Initialize(config);

            // Assert
            int actualHandlerCount = config.MessageHandlers.Count;
            Assert.Equal(expectedHandlerCount + 2, actualHandlerCount);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void Initialize_Repairs_Handler_Collection_If_Tracer_Inserted()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            TestTraceWriter traceWriter = new TestTraceWriter();
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
            RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
            List<DelegatingHandler> handlerList = new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer };
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;

            // Act
            config.MessageHandlers.Insert(0, msgHandlerTracer);
            traceManager.Initialize(config);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        [Fact]
        public void Initialize_Repairs_Handler_Collection_If_RequestTracer_Inserted()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), new TestTraceWriter());
            TestTraceWriter traceWriter = new TestTraceWriter();
            DelegatingHandler messageHandler = new Mock<DelegatingHandler>().Object;
            DelegatingHandler msgHandlerTracer = new MessageHandlerTracer(messageHandler, traceWriter);
            RequestMessageHandlerTracer requestMsgtracer = new RequestMessageHandlerTracer(traceWriter);
            List<DelegatingHandler> handlerList = new List<DelegatingHandler>() { messageHandler, msgHandlerTracer, requestMsgtracer };
            foreach (var eachHandler in handlerList)
            {
                config.MessageHandlers.Add(eachHandler);
            }
            TraceManager traceManager = new TraceManager();
            traceManager.Initialize(config);
            Collection<DelegatingHandler> expectedMessageHandlers = config.MessageHandlers;

            // Act
            config.MessageHandlers.Insert(0, requestMsgtracer);
            traceManager.Initialize(config);

            // Assert
            Collection<DelegatingHandler> actualMessageHandlers = config.MessageHandlers;
            Assert.Equal(expectedMessageHandlers, actualMessageHandlers);
            Assert.True(IsMessageHandlerCollectionValid(config.MessageHandlers));
        }

        private static bool IsMessageHandlerCollectionValid(Collection<DelegatingHandler> messageHandlers)
        {
            int handlerCount = messageHandlers.Count;

            // if the handler count is zero, exit early.
            if (handlerCount == 0)
            {
                return false;
            }

            // if RequestMessageHandlerTracer is absent, exit early.
            if (!(messageHandlers[0] is RequestMessageHandlerTracer))
            {
                return false;
            }

            // Message handler list must be an odd number (2*N+1) for N message handlers.
            if (handlerCount % 2 != 1)
            {
                return false;
            }

            // Check if all odd positions have tracers and even positions have their corresponding handlers.
            for (int i = 2; i < handlerCount; i += 2)
            {
                DelegatingHandler tracer = messageHandlers[i - 1];
                DelegatingHandler messageHandler = messageHandlers[i];
                if (!(tracer is MessageHandlerTracer))
                {
                    return false;
                }

                DelegatingHandler innerHandler = Decorator.GetInner(tracer);
                if (innerHandler != messageHandler)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

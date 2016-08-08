// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpControllerDescriptorTracerTest
    {
        private static readonly HttpControllerContext _controllerContext = ContextUtil.CreateControllerContext(instance: _controller);
        private static readonly HttpRequestMessage _request = _controllerContext.Request;
        private static readonly IHttpController _controller = new Mock<IHttpController>().Object;
        private static readonly InvalidOperationException _exception = new InvalidOperationException("test");

        [Fact]
        public void Properties_Calls_Inner()
        {
            // Arrange
            ConcurrentDictionary<object, object> properties = new ConcurrentDictionary<object, object>();
            Mock<HttpControllerDescriptor> mockControllerDescriptor = new Mock<HttpControllerDescriptor>();
            mockControllerDescriptor.Setup(d => d.Properties).Returns(properties).Verifiable();
            HttpControllerDescriptorTracer tracer = GetHttpControllerDescriptorTracer(mockControllerDescriptor.Object, new TestTraceWriter());

            // Act and Assert
            Assert.Same(properties, tracer.Properties);
            mockControllerDescriptor.Verify();
        }

        [Fact]
        public void CreateController_Invokes_Inner_And_Traces()
        {
            // Arrange
            Mock<HttpControllerDescriptor> mockControllerDescriptor = CreateMockControllerDescriptor();
            mockControllerDescriptor.Setup(b => b.CreateController(It.IsAny<HttpRequestMessage>())).Returns(_controller);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerDescriptorTracer tracer = GetHttpControllerDescriptorTracer(mockControllerDescriptor.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "CreateController" },
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "CreateController" }
            };

            // Act
            IHttpController controller = tracer.CreateController(_request);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.IsAssignableFrom<HttpControllerTracer>(controller);
        }

        [Fact]
        public void CreateController_Throws_And_Traces_When_Inner_Throws()
        {
            // Arrange
            Mock<HttpControllerDescriptor> mockControllerDescriptor = CreateMockControllerDescriptor();
            mockControllerDescriptor.Setup(b => b.CreateController(It.IsAny<HttpRequestMessage>())).Throws(_exception);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerDescriptorTracer tracer = GetHttpControllerDescriptorTracer(mockControllerDescriptor.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "CreateController" },
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "CreateController" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.CreateController(_request));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(_exception, thrown);
            Assert.Same(_exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_HttpControllerDescriptorTracer_Returns_HttpControllerDescriptor()
        {
            // Arrange
            HttpControllerDescriptor expectedInner = BuildHttpControllerDescriptor(_controller);

            HttpControllerDescriptorTracer productUnderTest = new HttpControllerDescriptorTracer(expectedInner, new TestTraceWriter());

            // Act
            HttpControllerDescriptor actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpControllerDescriptorTracer_Returns_HttpControllerDescriptor()
        {
            // Arrange
            HttpControllerDescriptor expectedInner = BuildHttpControllerDescriptor(_controller);

            HttpControllerDescriptorTracer productUnderTest = new HttpControllerDescriptorTracer(expectedInner, new TestTraceWriter());

            // Act
            HttpControllerDescriptor actualInner = Decorator.GetInner(productUnderTest as HttpControllerDescriptor);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_DoesNotCauseControllerInitializationToHappenAgainAndAgain()
        {
            // Arrange
            int raisedCount = 0;
            RaiseWhenInitializedAttribute.Initialized += delegate { ++raisedCount; };

            var descriptor = new HttpControllerDescriptor(new HttpConfiguration(), "my", typeof(MyController));
            Assert.Equal(1, raisedCount);

            // Act
            new HttpControllerDescriptorTracer(descriptor, new TestTraceWriter());
            new HttpControllerDescriptorTracer(descriptor, new TestTraceWriter());
            new HttpControllerDescriptorTracer(descriptor, new TestTraceWriter());

            // Assert
            Assert.Equal(1, raisedCount);
        }

        private static HttpControllerDescriptor BuildHttpControllerDescriptor(IHttpController controller, string controllerName = "AnyController", HttpConfiguration httpConfiguration = null)
        {
            HttpControllerDescriptor expectedInner = new Mock<HttpControllerDescriptor>().Object;
            expectedInner.ControllerName = controllerName;
            expectedInner.ControllerType = controller.GetType();
            expectedInner.Configuration = httpConfiguration ?? new HttpConfiguration();
            return expectedInner;
        }

        private static HttpControllerDescriptorTracer GetHttpControllerDescriptorTracer(HttpControllerDescriptor controllerDescriptor, ITraceWriter traceWriter)
        {
            if (controllerDescriptor.Configuration == null)
            {
                controllerDescriptor.Configuration = new HttpConfiguration();
            }

            if (controllerDescriptor.ControllerName == null)
            {
                controllerDescriptor.ControllerName = "AnyController";
            }

            if (controllerDescriptor.ControllerType == null)
            {
                controllerDescriptor.ControllerType = _controller.GetType();
            }

            return new HttpControllerDescriptorTracer(
                innerDescriptor: controllerDescriptor,
                traceWriter: traceWriter);
        }

        private static Mock<HttpControllerDescriptor> CreateMockControllerDescriptor()
        {
            Mock<HttpControllerDescriptor> mockControllerDescriptor = new Mock<HttpControllerDescriptor>(_controllerContext.Configuration, "AnyController", _controller.GetType());
            return mockControllerDescriptor;
        }

        private class RaiseWhenInitializedAttribute : Attribute, IControllerConfiguration
        {
            public static event EventHandler Initialized;
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                Initialized(this, EventArgs.Empty);
            }
        }

        [RaiseWhenInitialized]
        private class MyController : ApiController
        {
            public string Foo()
            {
                return "bar";
            }
        }
    }
}

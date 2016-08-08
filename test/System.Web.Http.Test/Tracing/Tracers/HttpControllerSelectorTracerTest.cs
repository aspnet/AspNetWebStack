// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpControllerSelectorTracerTest
    {
        private static readonly IHttpController _controller = new Mock<IHttpController>().Object;
        private static readonly HttpControllerContext _controllerContext = ContextUtil.CreateControllerContext(instance: _controller);
        private static readonly HttpRequestMessage _request = _controllerContext.Request;

        [Fact]
        public void SelectController_Invokes_Inner_And_Traces()
        {
            // Arrange
            Mock<HttpControllerDescriptor> mockControllerDescriptor = new Mock<HttpControllerDescriptor>(_controllerContext.Configuration, "AnyController", _controller.GetType());
            Mock<IHttpControllerSelector> mockSelector = new Mock<IHttpControllerSelector>();
            mockSelector.Setup(b => b.SelectController(It.IsAny<HttpRequestMessage>())).Returns(mockControllerDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerSelectorTracer tracer = new HttpControllerSelectorTracer(mockSelector.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "SelectController" },
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "SelectController" }
            };

            // Act
            HttpControllerDescriptor controllerDescriptor = ((IHttpControllerSelector)tracer).SelectController(_request);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.IsAssignableFrom<HttpControllerDescriptorTracer>(controllerDescriptor);
        }

        [Fact]
        public void SelectController_Throws_And_Traces_When_Inner_Throws()
        {
            // Arrange
            Mock<IHttpControllerSelector> mockSelector = new Mock<IHttpControllerSelector>();
            InvalidOperationException exception = new InvalidOperationException("test");
            mockSelector.Setup(b => b.SelectController(It.IsAny<HttpRequestMessage>())).Throws(exception);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerSelectorTracer tracer = new HttpControllerSelectorTracer(mockSelector.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "SelectController" },
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "SelectController" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => ((IHttpControllerSelector)tracer).SelectController(_request));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_HttpControllerSelectorTracer_Returns_IHttpControllerSelector()
        {
            // Arrange
            IHttpControllerSelector expectedInner = new Mock<IHttpControllerSelector>().Object;
            HttpControllerSelectorTracer productUnderTest = new HttpControllerSelectorTracer(expectedInner, new TestTraceWriter());

            // Act
            IHttpControllerSelector actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpControllerSelectorTracer_Returns_IHttpControllerSelector()
        {
            // Arrange
            IHttpControllerSelector expectedInner = new Mock<IHttpControllerSelector>().Object;
            HttpControllerSelectorTracer productUnderTest = new HttpControllerSelectorTracer(expectedInner, new TestTraceWriter());

            // Act
            IHttpControllerSelector actualInner = Decorator.GetInner(productUnderTest as IHttpControllerSelector);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

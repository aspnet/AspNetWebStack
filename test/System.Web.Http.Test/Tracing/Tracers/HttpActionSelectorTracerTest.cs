// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionSelectorTracerTest
    {
        private Mock<HttpActionDescriptor> _mockActionDescriptor;
        private HttpActionContext _actionContext;
        private HttpControllerContext _controllerContext;
        private HttpControllerDescriptor _controllerDescriptor;

        public HttpActionSelectorTracerTest()
        {
            _mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            _mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            _mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            _controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));

            _controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            _controllerContext.ControllerDescriptor = _controllerDescriptor;

            _actionContext = ContextUtil.CreateActionContext(_controllerContext, actionDescriptor: _mockActionDescriptor.Object);
        }

        [Fact]
        public void SelectAction_Traces_And_Returns_ActionDescriptor_Tracer()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            Mock<IHttpActionSelector> mockSelector = new Mock<IHttpActionSelector>();
            mockSelector.Setup(s => s.SelectAction(_controllerContext)).Returns(_mockActionDescriptor.Object);
            HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(mockSelector.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            HttpActionDescriptor selectedActionDescriptor = ((IHttpActionSelector)tracer).SelectAction(_controllerContext);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.IsAssignableFrom<HttpActionDescriptorTracer>(selectedActionDescriptor);
        }

        [Fact]
        public void SelectAction_DoesNotWrapHttpActionDescriptorTracer()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            Mock<IHttpActionSelector> mockSelector = new Mock<IHttpActionSelector>();

            HttpActionDescriptorTracer actionDescriptorTracer = new HttpActionDescriptorTracer(_controllerContext, _mockActionDescriptor.Object, traceWriter);
            mockSelector.Setup(s => s.SelectAction(_controllerContext)).Returns(actionDescriptorTracer);
            HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(mockSelector.Object, traceWriter);

            // Act
            HttpActionDescriptor selectedActionDescriptor = ((IHttpActionSelector)tracer).SelectAction(_controllerContext);

            // Assert
            Assert.Same(actionDescriptorTracer, selectedActionDescriptor);
        }

        [Fact]
        public void SelectAction_Traces_And_Throws_Exception_Thrown_From_Inner()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            Mock<IHttpActionSelector> mockSelector = new Mock<IHttpActionSelector>();
            InvalidOperationException exception = new InvalidOperationException();
            mockSelector.Setup(s => s.SelectAction(_controllerContext)).Throws(exception);
            HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(mockSelector.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => ((IHttpActionSelector)tracer).SelectAction(_controllerContext));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_HttpActionSelectorTracer_Returns_IHttpActionSelector()
        {
            // Arrange
            IHttpActionSelector expectedInner = new Mock<IHttpActionSelector>().Object;
            HttpActionSelectorTracer productUnderTest = new HttpActionSelectorTracer(expectedInner, new TestTraceWriter());

            // Act
            IHttpActionSelector actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpActionSelectorTracer_Returns_IHttpActionSelector()
        {
            // Arrange
            IHttpActionSelector expectedInner = new Mock<IHttpActionSelector>().Object;
            HttpActionSelectorTracer productUnderTest = new HttpActionSelectorTracer(expectedInner, new TestTraceWriter());

            // Act
            IHttpActionSelector actualInner = Decorator.GetInner(productUnderTest as IHttpActionSelector);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}
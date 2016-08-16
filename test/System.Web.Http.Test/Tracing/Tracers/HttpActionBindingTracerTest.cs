// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionBindingTracerTest
    {
        private Mock<HttpActionDescriptor> _mockActionDescriptor;
        private Mock<HttpParameterDescriptor> _mockParameterDescriptor;
        private Mock<HttpParameterBinding> _mockParameterBinding;
        private HttpActionBinding _actionBinding;
        private HttpActionContext _actionContext;
        private HttpControllerContext _controllerContext;
        private HttpControllerDescriptor _controllerDescriptor;

        public HttpActionBindingTracerTest()
        {
            _mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            _mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            _mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            _mockParameterDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            _mockParameterBinding = new Mock<HttpParameterBinding>(_mockParameterDescriptor.Object) { CallBase = true };
            _actionBinding = new HttpActionBinding(_mockActionDescriptor.Object, new HttpParameterBinding[] { _mockParameterBinding.Object });

            _controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));

            _controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            _controllerContext.ControllerDescriptor = _controllerDescriptor;

            _actionContext = ContextUtil.CreateActionContext(_controllerContext, actionDescriptor: _mockActionDescriptor.Object);

        }

        [Fact]
        public void ActionDescriptor_Uses_Inners()
        {
            // Arrange
            HttpActionBinding binding = new Mock<HttpActionBinding>() { CallBase = true }.Object;
            binding.ActionDescriptor = _mockActionDescriptor.Object;
            HttpActionBindingTracer tracer = new HttpActionBindingTracer(binding, new TestTraceWriter());

            // Assert
            Assert.Same(binding.ActionDescriptor, tracer.ActionDescriptor);
        }

        [Fact]
        public void ParameterBindings_Uses_Inners()
        {
            // Arrange
            HttpActionBinding binding = new Mock<HttpActionBinding>() { CallBase = true }.Object;
            HttpParameterBinding[] parameterBindings = new HttpParameterBinding[0];
            binding.ParameterBindings = parameterBindings;
            HttpActionBindingTracer tracer = new HttpActionBindingTracer(binding, new TestTraceWriter());

            // Assert
            Assert.Same(parameterBindings, tracer.ParameterBindings);
        }

        [Fact]
        public async Task BindValuesAsync_Invokes_Inner_And_Traces()
        {
            // Arrange
            bool wasInvoked = false;
            Mock<HttpActionBinding> mockBinder = new Mock<HttpActionBinding>() { CallBase = true };
            mockBinder.Setup(b => b.ExecuteBindingAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                .Callback(() => wasInvoked = true).Returns(TaskHelpers.Completed());

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionBindingTracer tracer = new HttpActionBindingTracer(mockBinder.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            await tracer.ExecuteBindingAsync(_actionContext, CancellationToken.None);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.True(wasInvoked);
        }

        [Fact]
        public async Task ExecuteBindingAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(exception);
            Mock<HttpActionBinding> mockBinder = new Mock<HttpActionBinding>() { CallBase = true };
            mockBinder.Setup(b => b.ExecuteBindingAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                .Returns(tcs.Task);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionBindingTracer tracer = new HttpActionBindingTracer(mockBinder.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act
            Task task = tracer.ExecuteBindingAsync(_actionContext, CancellationToken.None);

            // Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_HttpActionBindingTracer_Returns_HttpActionBinding()
        {
            // Arrange
            HttpActionBinding expectedInner = new HttpActionBinding();
            HttpActionBindingTracer productUnderTest = new HttpActionBindingTracer(expectedInner, new TestTraceWriter());

            // Act
            HttpActionBinding actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpActionBindingTracer_Returns_HttpActionBinding()
        {
            // Arrange
            HttpActionBinding expectedInner = new HttpActionBinding();
            HttpActionBindingTracer productUnderTest = new HttpActionBindingTracer(expectedInner, new TestTraceWriter());

            // Act
            HttpActionBinding actualInner = Decorator.GetInner(productUnderTest as HttpActionBinding);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

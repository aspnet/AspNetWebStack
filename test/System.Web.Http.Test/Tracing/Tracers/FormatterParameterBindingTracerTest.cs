// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Services;
using System.Web.Http.Validation;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class FormatterParameterBindingTracerTest
    {
        [Fact]
        public void ErrorMessage_Calls_Inner()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null);
            mockBinding.Setup(b => b.ErrorMessage).Returns("errorMessage").Verifiable();
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, new TestTraceWriter());

            // Act & Assert
            Assert.Equal("errorMessage", tracer.ErrorMessage);
            mockBinding.Verify();
        }

        [Fact]
        public void WillReadBody_Calls_Inner()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null);
            mockBinding.Setup(b => b.WillReadBody).Returns(true).Verifiable();
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, new TestTraceWriter());

            // Act & Assert
            Assert.True(tracer.WillReadBody);
            mockBinding.Verify();
        }

        [Fact]
        public void Descriptor_Uses_Inners()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null) { CallBase = true };
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, new TestTraceWriter());

            // Act & Assert
            Assert.Same(mockBinding.Object.Descriptor, tracer.Descriptor);
        }

        [Fact]
        public void Formatters_Uses_Inners()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null) { CallBase = true };
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, new TestTraceWriter());

            // Act & Assert
            Assert.Equal(mockBinding.Object.Formatters, tracer.Formatters);
        }

        [Fact]
        public void BodyModelValidator_Uses_Inners()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<IBodyModelValidator> mockValidator = new Mock<IBodyModelValidator>();
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), mockValidator.Object) { CallBase = true };
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, new TestTraceWriter());

            // Act & Assert
            Assert.Equal(mockBinding.Object.BodyModelValidator, tracer.BodyModelValidator);
        }

        /// <summary>
        /// This test verifies that our <see cref="FormatterParameterBindingTracer"/>
        /// intercepts the async bind request and redirects it to use tracing formatters
        /// correlated to the request.
        /// </summary>
        [Fact]
        public async Task ExecuteBindingAsync_Traces_And_Invokes_Inner_ReadAsync()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            FormatterParameterBinding binding = new FormatterParameterBinding(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null);
            TestTraceWriter traceWriter = new TestTraceWriter();
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(binding, traceWriter);
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            actionContext.Request.Content = new StringContent("true");
            actionContext.Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            ModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteBindingAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "ExecuteBindingAsync" }
            };

            // Act
            await tracer.ExecuteBindingAsync(metadataProvider, actionContext, CancellationToken.None);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal("true", actionContext.ActionArguments["paramName"]);
        }

        [Fact]
        public void ExecuteBindingAsync_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null) { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockBinding.Setup(
                b =>
                b.ExecuteBindingAsync(It.IsAny<ModelMetadataProvider>(), It.IsAny<HttpActionContext>(),
                                      It.IsAny<CancellationToken>())).Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, traceWriter);
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            ModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteBindingAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "ExecuteBindingAsync" }
            };

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.ExecuteBindingAsync(metadataProvider, actionContext, CancellationToken.None));

            // Assert
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task ExecuteBindingAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange

            Mock<HttpParameterDescriptor> mockParamDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            mockParamDescriptor.Setup(d => d.ParameterName).Returns("paramName");
            mockParamDescriptor.Setup(d => d.ParameterType).Returns(typeof(string));
            Mock<FormatterParameterBinding> mockBinding = new Mock<FormatterParameterBinding>(mockParamDescriptor.Object, new MediaTypeFormatterCollection(), null) { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(exception);

            mockBinding.Setup(
                b =>
                b.ExecuteBindingAsync(It.IsAny<ModelMetadataProvider>(), It.IsAny<HttpActionContext>(),
                                      It.IsAny<CancellationToken>())).Returns(tcs.Task);

            TestTraceWriter traceWriter = new TestTraceWriter();
            FormatterParameterBindingTracer tracer = new FormatterParameterBindingTracer(mockBinding.Object, traceWriter);
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            ModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteBindingAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.ModelBindingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "ExecuteBindingAsync" }
            };

            // Act & Assert
            Task task = tracer.ExecuteBindingAsync(metadataProvider, actionContext, CancellationToken.None);

            // Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Inner_Property_On_FormatterParameterBindingTracer_Returns_FormatterParameterBinding()
        {
            // Arrange
            HttpParameterDescriptor httpParameterDescriptor = new Mock<HttpParameterDescriptor>().Object;
            MediaTypeFormatterCollection mediaTypeFormatterCollection = new MediaTypeFormatterCollection();
            FormatterParameterBinding expectedInner = new FormatterParameterBinding(httpParameterDescriptor, mediaTypeFormatterCollection, null);
            FormatterParameterBindingTracer productUnderTest = new FormatterParameterBindingTracer(expectedInner, new TestTraceWriter());

            // Act
            FormatterParameterBinding actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_FormatterParameterBindingTracer_Returns_FormatterParameterBinding()
        {
            // Arrange
            HttpParameterDescriptor httpParameterDescriptor = new Mock<HttpParameterDescriptor>().Object;
            MediaTypeFormatterCollection mediaTypeFormatterCollection = new MediaTypeFormatterCollection();
            FormatterParameterBinding expectedInner = new FormatterParameterBinding(httpParameterDescriptor, mediaTypeFormatterCollection, null);
            FormatterParameterBindingTracer productUnderTest = new FormatterParameterBindingTracer(expectedInner, new TestTraceWriter());

            // Act
            FormatterParameterBinding actualInner = Decorator.GetInner(productUnderTest as FormatterParameterBinding);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

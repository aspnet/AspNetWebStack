// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionDescriptorTracerTest
    {
        [Fact]
        public void ActionName_Calls_Inner()
        {
            // Arrange
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.ActionName).Returns("actionName").Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same("actionName", tracer.ActionName);
            mockDescriptor.Verify();
        }

        [Fact]
        public void SupportedHttpMethods_Calls_Inner()
        {
            // Arrange
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Collection<HttpMethod> methods = new Collection<HttpMethod>() { HttpMethod.Delete };
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.SupportedHttpMethods).Returns(methods).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Equal(methods, tracer.SupportedHttpMethods);
            mockDescriptor.Verify();
        }

        [Fact]
        public void ActionBinding_Calls_Inner()
        {
            // Arrange
            HttpActionBinding binding = new Mock<HttpActionBinding>().Object;
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.ActionBinding).Returns(binding).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(binding, tracer.ActionBinding);
            mockDescriptor.Verify();
        }

        [Fact]
        public void ReturnType_Calls_Inner()
        {
            // Arrange
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.ReturnType).Returns(typeof(string)).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Equal(typeof(string), tracer.ReturnType);
            mockDescriptor.Verify();
        }

        [Fact]
        public void ResultConverter_Calls_Inner()
        {
            // Arrange
            IActionResultConverter resultConverter = new Mock<IActionResultConverter>().Object;
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.ResultConverter).Returns(resultConverter).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(resultConverter, tracer.ResultConverter);
            mockDescriptor.Verify();
        }

        [Fact]
        public void Properties_Calls_Inner()
        {
            // Arrange
            ConcurrentDictionary<object, object> properties = new ConcurrentDictionary<object, object>();
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.Properties).Returns(properties).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(properties, tracer.Properties);
            mockDescriptor.Verify();
        }

        [Fact]
        public void GetCustomAttributes_Calls_Inner()
        {
            // Arrange
            Collection<Attribute> customAttributes = new Collection<Attribute>();
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.GetCustomAttributes<Attribute>()).Returns(customAttributes).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(customAttributes, tracer.GetCustomAttributes<Attribute>());
            mockDescriptor.Verify();
        }

        [Fact]
        public void GetParameters_Calls_Inner()
        {
            // Arrange
            Collection<HttpParameterDescriptor> parameters = new Collection<HttpParameterDescriptor>();
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.GetParameters()).Returns(parameters).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(parameters, tracer.GetParameters());
            mockDescriptor.Verify();
        }

        [Fact]
        public void GetFilters_Calls_Inner()
        {
            // Arrange
            Collection<IFilter> filters = new Collection<IFilter>();
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.GetFilters()).Returns(filters).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act
            tracer.GetFilters();

            // Assert
            mockDescriptor.Verify();
        }

        [Fact]
        public void GetFilterPipeline_Calls_Inner()
        {
            // Arrange
            Collection<FilterInfo> filters = new Collection<FilterInfo>();
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            mockDescriptor.Setup(d => d.GetFilterPipeline()).Returns(filters).Verifiable();
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act
            tracer.GetFilterPipeline();

            // Assert
            mockDescriptor.Verify();
        }
        [Fact]
        public void Configuration_Uses_Inners()
        {
            // Assert
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(actionDescriptor.Configuration, tracer.Configuration);
        }

        [Fact]
        public void ControllerDescriptor_Uses_Inners()
        {
            // Assert
            HttpControllerDescriptor controllerDescriptor = new Mock<HttpControllerDescriptor>().Object;
            controllerDescriptor.Configuration = new HttpConfiguration();
            Mock<HttpActionDescriptor> mockDescriptor = new Mock<HttpActionDescriptor>(controllerDescriptor);
            HttpActionDescriptor actionDescriptor = mockDescriptor.Object;
            HttpControllerContext controllerContext = new Mock<HttpControllerContext>().Object;
            controllerContext.Configuration = controllerDescriptor.Configuration;
            controllerContext.ControllerDescriptor = controllerDescriptor;
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, actionDescriptor, new TestTraceWriter());

            // Act and Assert
            Assert.Same(actionDescriptor.ControllerDescriptor, tracer.ControllerDescriptor);
        }

        // This test verifies only one kind of filter is wrapped, proving
        // the static FilterTracer.CreateFilterTracer was called from GetFilterPipeline.
        // Deeper testing of FilterTracer.CreateFilterTracer is in FilterTracerTest.
        [Fact]
        public void GetFilterPipeline_Returns_Wrapped_Filters()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Global);
            Collection<FilterInfo> filterCollection = new Collection<FilterInfo>(new FilterInfo[] { filter });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetFilterPipeline()).Returns(filterCollection);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, new TestTraceWriter());

            // Act
            Collection<FilterInfo> wrappedFilterCollection = tracer.GetFilterPipeline();

            // Assert
            Assert.IsType<FilterTracer>(wrappedFilterCollection[0].Instance);
        }

        // This test verifies only one kind of filter is wrapped, proving
        // the static FilterTracer.CreateFilterTracer was called from GetFilterPipeline.
        // Deeper testing of FilterTracer.CreateFilterTracer is in FilterTracerTest.
        [Fact]
        public void GetFilters_Returns_Wrapped_IFilters()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();
            Collection<IFilter> filters = new Collection<IFilter>(new IFilter[] { mockFilter.Object });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetFilters()).Returns(filters);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, new TestTraceWriter());

            // Act
            IFilter[] wrappedFilters = tracer.GetFilters().ToArray();

            // Assert
            Assert.IsType<FilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void ExecuteAsync_Invokes_Inner_ExecuteAsync()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            var controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            var arguments = new Dictionary<string, object>();
            var tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, new TestTraceWriter());

            // Act
            tracer.ExecuteAsync(controllerContext, arguments, cts.Token);

            // Assert
            mockActionDescriptor.Verify(a => a.ExecuteAsync(controllerContext, arguments, cts.Token), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Traces()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Returns(Task.FromResult<object>(null));
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            IDictionary<string, object> arguments = new Dictionary<string, object>();
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteAsync" },
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            await tracer.ExecuteAsync(controllerContext, arguments, CancellationToken.None);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteAsync_Throws_What_Inner_Throws_And_Traces()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockActionDescriptor.Setup(
                a => a.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>(), CancellationToken.None)).Throws(exception);
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            IDictionary<string, object> arguments = new Dictionary<string, object>();
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => tracer.ExecuteAsync(controllerContext, arguments, CancellationToken.None));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_HttpActionDescriptorTracer_Returns_HttpActionDescriptor()
        {
            // Arrange
            HttpActionDescriptor expectedInner = new Mock<HttpActionDescriptor>().Object;
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            HttpActionDescriptorTracer productUnderTest = new HttpActionDescriptorTracer(controllerContext, expectedInner, new TestTraceWriter());

            // Act
            HttpActionDescriptor actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_HttpActionDescriptorTracer_Returns_HttpActionDescriptor()
        {
            // Arrange
            HttpActionDescriptor expectedInner = new Mock<HttpActionDescriptor>().Object;
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            HttpActionDescriptorTracer productUnderTest = new HttpActionDescriptorTracer(controllerContext, expectedInner, new TestTraceWriter());

            // Act
            HttpActionDescriptor actualInner = Decorator.GetInner(productUnderTest as HttpActionDescriptor);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
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
    public class AuthorizationFilterAttributeTracerTest
    {
        [Fact]
        public void Equals_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<AuthorizationFilterAttribute> mockAttribute = new Mock<AuthorizationFilterAttribute>();
            mockAttribute.Setup(a => a.Equals(randomObject)).Returns(true).Verifiable();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.Equals(randomObject);

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void GetHashCode_Calls_Inner()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttribute = new Mock<AuthorizationFilterAttribute>();
            mockAttribute.Setup(a => a.GetHashCode()).Returns(1).Verifiable();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            int valueReturned = tracer.GetHashCode();

            // Assert
            Assert.Equal(1, valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void IsDefaultAttribute_Calls_Inner()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttribute = new Mock<AuthorizationFilterAttribute>();
            mockAttribute.Setup(a => a.IsDefaultAttribute()).Returns(true).Verifiable();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.IsDefaultAttribute();

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void Match_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<AuthorizationFilterAttribute> mockAttribute = new Mock<AuthorizationFilterAttribute>();
            mockAttribute.Setup(a => a.Match(randomObject)).Returns(true).Verifiable();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.Match(randomObject);

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void TypeId_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<AuthorizationFilterAttribute> mockAttribute = new Mock<AuthorizationFilterAttribute>();
            mockAttribute.Setup(a => a.TypeId).Returns(randomObject).Verifiable();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            object valueReturned = tracer.TypeId;

            // Assert
            Assert.Same(randomObject, valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void AllowMultiple_Calls_Inner()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttribute = new Mock<AuthorizationFilterAttribute>();
            mockAttribute.Setup(a => a.AllowMultiple).Returns(true).Verifiable();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.AllowMultiple;

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_Traces()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttr = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            Func<Task<HttpResponseMessage>> continuation = () => Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnAuthorizationAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "OnAuthorizationAsync" },
            };

            // Act
            var filter = (IAuthorizationFilter)tracer;
            await filter.ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, continuation);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task ExecuteAuthorizationFilterAsync_Throws_And_Traces_When_Inner_OnException_Throws()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttr = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockAttr.Setup(a => a.OnAuthorization(It.IsAny<HttpActionContext>())).Throws(exception);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            Func<Task<HttpResponseMessage>> continuation = () => Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnAuthorizationAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "OnAuthorizationAsync" }
            };

            // Act
            Exception thrown =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => ((IAuthorizationFilter)tracer).ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, continuation));

            // Assert
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Inner_Property_On_AuthorizationFilterAttributeTracer_Returns_AuthorizationFilterAttribute()
        {
            // Arrange
            AuthorizationFilterAttribute expectedInner = new Mock<AuthorizationFilterAttribute>().Object;
            AuthorizationFilterAttributeTracer productUnderTest = new AuthorizationFilterAttributeTracer(expectedInner, new TestTraceWriter());

            // Act
            AuthorizationFilterAttribute actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_AuthorizationFilterAttributeTracer_Returns_AuthorizationFilterAttribute()
        {
            // Arrange
            AuthorizationFilterAttribute expectedInner = new Mock<AuthorizationFilterAttribute>().Object;
            AuthorizationFilterAttributeTracer productUnderTest = new AuthorizationFilterAttributeTracer(expectedInner, new TestTraceWriter());

            // Act
            AuthorizationFilterAttribute actualInner = Decorator.GetInner(productUnderTest as AuthorizationFilterAttribute);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class ContentNegotiatorTracerTest
    {
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly Mock<IContentNegotiator> _mockNegotiator = new Mock<IContentNegotiator>();
        private readonly ContentNegotiatorTracer _tracer;
        private readonly TestTraceWriter _traceWriter = new TestTraceWriter();

        public ContentNegotiatorTracerTest()
        {
            _tracer = new ContentNegotiatorTracer(_mockNegotiator.Object, _traceWriter);
        }

        [Fact]
        public void Negotiate_Calls_Inner_Negotiate()
        {
            // Act
            ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            _mockNegotiator.Verify(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>()), Times.Once());
        }

        [Fact]
        public void Negotiate_Returns_Inner_MediaType()
        {
            // Arrange
            MediaTypeHeaderValue expectedMediaType = new MediaTypeHeaderValue("application/xml");
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Returns(
                                new ContentNegotiationResult(new JsonMediaTypeFormatter(), expectedMediaType));

            // Act
            var result = ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            Assert.Same(expectedMediaType, result.MediaType);
        }

        [Fact]
        public void Negotiate_Returns_Wrapped_Inner_XmlFormatter()
        {
            // Arrange
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Returns(
                                new ContentNegotiationResult(expectedFormatter, null));

            // Act
            var result = ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            Assert.IsType<XmlMediaTypeFormatterTracer>(result.Formatter);
        }

        [Fact]
        public void Negotiate_Returns_Wrapped_Inner_JsonFormatter()
        {
            // Arrange
            MediaTypeFormatter expectedFormatter = new JsonMediaTypeFormatter();
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Returns(
                                new ContentNegotiationResult(expectedFormatter, null));

            // Act
            var result = ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            Assert.IsType<JsonMediaTypeFormatterTracer>(result.Formatter);
        }

        [Fact]
        public void Negotiate_Returns_Wrapped_Inner_FormUrlEncodedFormatter()
        {
            // Arrange
            MediaTypeFormatter expectedFormatter = new FormUrlEncodedMediaTypeFormatter();
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Returns(
                                new ContentNegotiationResult(expectedFormatter, null));

            // Act
            var result = ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            Assert.IsType<FormUrlEncodedMediaTypeFormatterTracer>(result.Formatter);
        }

        [Fact]
        public void Negotiate_Returns_Null_Inner_Formatter()
        {
            // Arrange
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Returns(
                                value: null);

            // Act
            var result = ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Negotiate_Traces_BeginEnd()
        {
            // Arrange
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Returns(
                                new ContentNegotiationResult(expectedFormatter, null));
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, _traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Negotiate_Throws_When_Inner_Throws()
        {
            // Arrange
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            InvalidOperationException expectedException = new InvalidOperationException("test");
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Throws(expectedException);

            // Act & Assert
            InvalidOperationException actualException = Assert.Throws<InvalidOperationException>(() => ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]));

            // Assert
            Assert.Same(expectedException, actualException);
        }

        [Fact]
        public void Negotiate_Traces_BeginEnd_When_Inner_Throws()
        {
            // Arrange
            MediaTypeFormatter expectedFormatter = new XmlMediaTypeFormatter();
            InvalidOperationException expectedException = new InvalidOperationException("test");
            _mockNegotiator.Setup(
                n =>
                n.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                            It.IsAny<IEnumerable<MediaTypeFormatter>>())).Throws(expectedException);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ((IContentNegotiator)_tracer).Negotiate(typeof(int), _request, new MediaTypeFormatter[0]));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, _traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(expectedException, _traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_ContentNegotiatiorTracer_Returns_IContentNegotiator()
        {
            // Arrange
            IContentNegotiator expectedInner = new Mock<IContentNegotiator>().Object;
            ContentNegotiatorTracer productUnderTest = new ContentNegotiatorTracer(expectedInner, new TestTraceWriter());

            // Act
            IContentNegotiator actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_ContentNegotiatiorTracer_Returns_IContentNegotiator()
        {
            // Arrange
            IContentNegotiator expectedInner = new Mock<IContentNegotiator>().Object;
            ContentNegotiatorTracer productUnderTest = new ContentNegotiatorTracer(expectedInner, new TestTraceWriter());

            // Act
            IContentNegotiator actualInner = Decorator.GetInner(productUnderTest as IContentNegotiator);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class BufferedMediaTypeFormatterTracerTest : MediaTypeFormatterTracerTestBase<BufferedMediaTypeFormatter>
    {
        public override MediaTypeFormatter CreateTracer(BufferedMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new BufferedMediaTypeFormatterTracer(formatter, traceWriter, request);
        }

        [Fact]
        public void BufferSize_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            BufferedMediaTypeFormatter innerFormatter = mockFormatter.Object;
            innerFormatter.BufferSize = innerFormatter.BufferSize + 1;
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.BufferSize, tracer.BufferSize);
        }

        [Fact]
        public void ReadFromStream_Traces()
        {
            // Arrange
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(f => f.ReadFromStream(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<IFormatterLogger>()))
                .Returns("sampleValue");
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "ReadFromStream" }
            };

            // Act
            string valueReturned = tracer.ReadFromStream(typeof(string), new MemoryStream(), request.Content, null) as string;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal("sampleValue", valueReturned);
        }

        [Fact]
        public void ReadFromStreamWithCancellationToken_Traces()
        {
            // Arrange
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(f => f.ReadFromStream(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<IFormatterLogger>()))
                .Returns("sampleValue");
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "ReadFromStream" }
            };

            // Act
            string valueReturned = tracer.ReadFromStream(typeof(string), new MemoryStream(), request.Content, null, CancellationToken.None) as string;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal("sampleValue", valueReturned);
        }

        [Fact]
        public void ReadFromStream_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.ReadFromStream(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<IFormatterLogger>())).Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "ReadFromStream" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.ReadFromStream(typeof(string), new MemoryStream(), request.Content, null));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void WriteToStream_Traces()
        {
            // Arrange
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.WriteToStream(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>()));
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "WriteToStream" }
            };

            // Act
            tracer.WriteToStream(typeof(string), "sampleValue", new MemoryStream(), request.Content);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void WriteToStreamWithCancellationToken_Traces()
        {
            // Arrange
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(
                f => f.WriteToStream(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>()));
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "WriteToStream" }
            };

            // Act
            tracer.WriteToStream(typeof(string), "sampleValue", new MemoryStream(), request.Content, CancellationToken.None);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void WriteToStream_Traces_And_Throws_When_Inner_Throws()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<BufferedMediaTypeFormatter> mockFormatter = new Mock<BufferedMediaTypeFormatter>() { CallBase = true };
            mockFormatter.Setup(f => f.WriteToStream(It.IsAny<Type>(), It.IsAny<Object>(), It.IsAny<Stream>(),
                                                     It.IsAny<HttpContent>()))
                         .Throws(exception);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            BufferedMediaTypeFormatterTracer tracer = new BufferedMediaTypeFormatterTracer(mockFormatter.Object, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStream" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "WriteToStream" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.WriteToStream(typeof(string), "sampleValue", new MemoryStream(), request.Content));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void Inner_Property_On_BufferedMediaTypeFormatterTracer_Returns_BufferedMediaTypeFormatter()
        {
            // Arrange
            BufferedMediaTypeFormatter expectedInner = new Mock<BufferedMediaTypeFormatter>().Object;
            BufferedMediaTypeFormatterTracer productUnderTest = new BufferedMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            BufferedMediaTypeFormatter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_BufferedMediaTypeFormatterTracer_Returns_BufferedMediaTypeFormatter()
        {
            // Arrange
            BufferedMediaTypeFormatter expectedInner = new Mock<BufferedMediaTypeFormatter>().Object;
            BufferedMediaTypeFormatterTracer productUnderTest = new BufferedMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            BufferedMediaTypeFormatter actualInner = Decorator.GetInner(productUnderTest as BufferedMediaTypeFormatter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

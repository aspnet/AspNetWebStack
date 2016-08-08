// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class MediaTypeFormatterTracerTest
    {
        public static TheoryDataSet<MediaTypeFormatter> AllKnownFormatters
        {
            get
            {
                return new TheoryDataSet<MediaTypeFormatter>
                {
                    new XmlMediaTypeFormatter(), 
                    new JsonMediaTypeFormatter(),
                    new FormUrlEncodedMediaTypeFormatter(),
                    new Mock<BufferedMediaTypeFormatter>().Object
                };
            }
        }

        [Theory]
        [PropertyData("AllKnownFormatters")]
        public void CreateTracer_Returns_Tracing_Formatter(MediaTypeFormatter formatter)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            IFormatterTracer tracer = Assert.IsAssignableFrom<IFormatterTracer>(tracingFormatter);
            Assert.Same(formatter, tracer.InnerFormatter);
        }

        [Theory]
        [PropertyData("AllKnownFormatters")]
        public void Inner_Property_On_All_MediaTypeFormatterTracers_Returns_Object_Of_Type_MediaTypeFormatter(MediaTypeFormatter formatter)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatterTracer = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Act
            MediaTypeFormatter innerFormatter = (formatterTracer as IDecorator<MediaTypeFormatter>).Inner;

            // Assert
            Assert.Same(formatter, innerFormatter);
        }

        [Theory]
        [PropertyData("AllKnownFormatters")]
        public void Decorator_GetInner_On_All_MediaTypeFormatterTracers_Returns_Object_Of_Type_MediaTypeFormatter(MediaTypeFormatter formatter)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatterTracer = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Act
            MediaTypeFormatter innerFormatter = Decorator.GetInner(formatterTracer);

            // Assert
            Assert.Same(formatter, innerFormatter);
        }

        [Fact]
        public async Task ReadFromStream_Traces()
        {
            // Arrange
            string contentType = "text/plain";
            object value = new object();
            CustomMediaTypeFormatter formatter = new CustomMediaTypeFormatter(value);
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(contentType));

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("42", Encoding.Default, contentType);
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(formatter, traceWriter, request);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ReadFromStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "ReadFromStreamAsync" }
            };

            // Act
            object valueReturned = await request.Content.ReadAsAsync<object>(new[] { tracer });

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal(value, valueReturned);
        }

        [Fact]
        public async Task WriteToStream_Traces()
        {
            // Arrange
            object value = new object();
            CustomMediaTypeFormatter formatter = new CustomMediaTypeFormatter(value);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatterTracer tracer = new MediaTypeFormatterTracer(formatter, traceWriter, request);
            request.Content = new ObjectContent<object>(value, tracer);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "WriteToStreamAsync" },
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "WriteToStreamAsync" }
            };

            // Act
            await request.Content.CopyToAsync(new MemoryStream());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        [ReplaceCulture]
        public void FormatterLoggerTracer_LogErrorException()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            string operatorName = this.GetType().Name;
            string operationName = "FormatterLoggerTracer_LogErrorException";
            var loggerMock = new Mock<IFormatterLogger>();
            loggerMock.Setup(o => o.LogError(It.IsAny<string>(), It.IsAny<Exception>()));
            IFormatterLogger tracer = new FormatterLoggerTraceWrapper(loggerMock.Object, traceWriter, request, operatorName, operationName);
            Exception exception = new Exception("message");
            string errorPath = "errorPath";
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error) 
                {
                    Kind = TraceKind.Trace, Operation = operationName, Exception = exception, Operator = operatorName
                },
            };

            // Act
            tracer.LogError(errorPath, exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        [ReplaceCulture]
        public void FormatterLoggerTracer_LogErrorMessage()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            string operatorName = this.GetType().Name;
            string operationName = "FormatterLoggerTracer_LogErrorMessage";
            var loggerMock = new Mock<IFormatterLogger>();
            loggerMock.Setup(o => o.LogError(It.IsAny<string>(), It.IsAny<string>()));
            IFormatterLogger tracer = new FormatterLoggerTraceWrapper(loggerMock.Object, traceWriter, request, operatorName, operationName);
            string errorMessage = "errorMessage";
            string errorPath = "errorPath";
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error)
                {
                    Kind = TraceKind.Trace, Operation = operationName, Message = errorMessage, Operator = operatorName 
                },
            };

            // Act
            tracer.LogError(errorPath, errorMessage);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        private class CustomMediaTypeFormatter : MediaTypeFormatter
        {
            private object _result;

            public CustomMediaTypeFormatter(object result)
            {
                Contract.Assert(result != null);
                _result = result;
            }

            public override bool CanReadType(Type type)
            {
                return true;
            }

            public override bool CanWriteType(Type type)
            {
                return true;
            }

            public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
                IFormatterLogger formatterLogger, CancellationToken cancellationToken)
            {
                return Task.FromResult(_result);
            }

            public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                TransportContext transportContext, CancellationToken cancellationToken)
            {
                return Task.FromResult(_result);
            }
        }
    }
}

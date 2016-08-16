// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;
using Newtonsoft.Json;

namespace System.Web.Http.Tracing.Tracers
{
    public class JsonMediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<JsonMediaTypeFormatter>
    {
        public override MediaTypeFormatter CreateTracer(JsonMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new JsonMediaTypeFormatterTracer(formatter, traceWriter, request);
        }

        [Fact]
        public void UseDataContractJsonSerializer_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter();
            innerFormatter.UseDataContractJsonSerializer = !innerFormatter.UseDataContractJsonSerializer;
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.UseDataContractJsonSerializer, tracer.UseDataContractJsonSerializer);
        }

        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void Indent_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter();
            innerFormatter.Indent = !innerFormatter.Indent;
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.Indent, tracer.Indent);
        }

        [Fact]
        public void SerializerSettings_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter() { SerializerSettings = serializerSettings };
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Same(innerFormatter.SerializerSettings, tracer.SerializerSettings);
        }

        [Fact]
        public void Inner_Property_On_JsonMediaTypeFormatterTracerTest_Returns_JsonMediaTypeFormatter()
        {
            // Arrange
            JsonMediaTypeFormatter expectedInner = new JsonMediaTypeFormatter();
            JsonMediaTypeFormatterTracer productUnderTest = new JsonMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            JsonMediaTypeFormatter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_JsonMediaTypeFormatterTracerTest_Returns_JsonMediaTypeFormatter()
        {
            // Arrange
            JsonMediaTypeFormatter expectedInner = new JsonMediaTypeFormatter();
            JsonMediaTypeFormatterTracer productUnderTest = new JsonMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            JsonMediaTypeFormatter actualInner = Decorator.GetInner(productUnderTest as JsonMediaTypeFormatter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        public static IEnumerable<object[]> RequestBodies
        {
            get
            {
                HttpRequestMessage request = new HttpRequestMessage();
                return new[]
                {
                    new object[]
                    {
                        new List<TraceRecord>
                        {
                            new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info)
                            {
                                Kind = TraceKind.Begin,
                                Operation = "ReadFromStreamAsync",
                                Message = "Type='SampleType', content-type='application/json'",
                                Operator = "JsonMediaTypeFormatter"
                            },
                            new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info)
                            {
                                Kind = TraceKind.End,
                                Operation = "ReadFromStreamAsync",
                                Message = "Value read='System.Net.Http.Formatting.SampleType'",
                                Operator = "JsonMediaTypeFormatter"
                            },
                        },
                        request,
                        "{\"Number\":42}"
                    },
                    new object[]
                    {
                        new List<TraceRecord>
                        {
                            new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info)
                            {
                                Kind = TraceKind.Begin,
                                Operation = "ReadFromStreamAsync",
                                Message = "Type='SampleType', content-type='application/json'",
                                Operator = "JsonMediaTypeFormatter"
                            },
                            new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Error)
                            {
                                Kind = TraceKind.Trace,
                                Operation = "ReadFromStreamAsync",
                                Operator = "JsonMediaTypeFormatter",
                                Exception = new JsonReaderException(
                                    "Unterminated string. Expected delimiter: \". Path '', line 1, position 12.")
                            },
                            new TraceRecord(request, TraceCategories.FormattingCategory, TraceLevel.Info)
                            {
                                Kind = TraceKind.End,
                                Operation = "ReadFromStreamAsync",
                                Message = "Value read='null'",
                                Operator = "JsonMediaTypeFormatter"
                            },
                        },
                        request,
                        "{\"Number:42}"
                    }
                };
            }
        }

        [Theory]
        [ReplaceCulture]
        [PropertyData("RequestBodies")]
        public async Task ReadFromStreamAsync_LogErrorFromJsonRequestBody(IList<TraceRecord> expectedTraces,
                                                                    HttpRequestMessage request,
                                                                    string requestBody)
        {
            // Arrange
            var formatter = new JsonMediaTypeFormatter();
            formatter.UseDataContractJsonSerializer = false;
            HttpContent content = new StringContent(requestBody);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var loggerMock = new Mock<IFormatterLogger>();
            loggerMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()));
            TestTraceWriter traceWriter = new TestTraceWriter();
            var tracer = new MediaTypeFormatterTracer(formatter, traceWriter, request);

            // Act
            await tracer.ReadFromStreamAsync(typeof(SampleType),
                                       await content.ReadAsStreamAsync(),
                                       content, loggerMock.Object
                                      );

            // Assert
            // Error must always be marked as handled at ReadFromStream in BaseJsonMediaTypeFormatters,
            // so it would ﻿not propagate to here.
            // Note that regarding the exception's comparison in the record we only compare its message,
            // because we cannot get the exact exception and message would be enough for logging.
            Assert.Equal<TraceRecord>(expectedTraces,
                                      traceWriter.Traces,
                                      new TraceRecordComparer() { IgnoreExceptionReference = true });
        }
    }
}

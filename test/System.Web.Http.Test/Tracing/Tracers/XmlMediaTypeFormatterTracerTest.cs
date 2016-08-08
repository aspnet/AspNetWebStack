// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.Tracing.Tracers
{
    public class XmlMediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<XmlMediaTypeFormatter>
    {
        public override MediaTypeFormatter CreateTracer(XmlMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new XmlMediaTypeFormatterTracer(formatter, traceWriter, request);
        }

        [Fact]
        public void UseXmlSerializer_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.UseXmlSerializer = !innerFormatter.UseXmlSerializer;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.UseXmlSerializer, tracer.UseXmlSerializer);
        }

        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void Indent_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.Indent = !innerFormatter.Indent;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.Indent, tracer.Indent);
        }

        [Fact]
        public void Inner_Property_On_XmlMediaTypeFormatterTracer_Returns_XmlMediaTypeFormatter()
        {
            // Arrange
            XmlMediaTypeFormatter expectedInner = new XmlMediaTypeFormatter();
            XmlMediaTypeFormatterTracer productUnderTest = new XmlMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            XmlMediaTypeFormatter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_XmlMediaTypeFormatterTracer_Returns_XmlMediaTypeFormatter()
        {
            // Arrange
            XmlMediaTypeFormatter expectedInner = new XmlMediaTypeFormatter();
            XmlMediaTypeFormatterTracer productUnderTest = new XmlMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            XmlMediaTypeFormatter actualInner = Decorator.GetInner(productUnderTest as XmlMediaTypeFormatter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void CreateXmlReader_Calls_CreateXmlReaderOnInner()
        {
            // Arrange
            Stream stream = new Mock<Stream>().Object;
            HttpContent content = new StringContent("");
            Mock<XmlMediaTypeFormatter> formatter = new Mock<XmlMediaTypeFormatter>();
            XmlMediaTypeFormatterTracer tracer = CreateTracer(formatter.Object);

            // Act
            tracer.InvokeCreateXmlReader(stream, content);

            // Assert
            formatter.Protected().Verify("CreateXmlReader", Times.Once(), stream, content);
        }

        [Fact]
        public void CreateXmlWriter_Calls_CreateXmlWriterOnInner()
        {
            // Arrange
            Stream stream = new Mock<Stream>().Object;
            HttpContent content = new StringContent("");
            Mock<XmlMediaTypeFormatter> formatter = new Mock<XmlMediaTypeFormatter>();
            XmlMediaTypeFormatterTracer tracer = CreateTracer(formatter.Object);

            // Act
            tracer.InvokeCreateXmlWriter(stream, content);

            // Assert
            formatter.Protected().Verify("CreateXmlWriter", Times.Once(), stream, content);
        }

        [Fact]
        public void GetDeserializer_Calls_GetDeserializerOnInner()
        {
            // Arrange
            Type type = new Mock<Type>().Object;
            HttpContent content = new StringContent("");
            Mock<XmlMediaTypeFormatter> formatter = new Mock<XmlMediaTypeFormatter>();
            XmlMediaTypeFormatterTracer tracer = CreateTracer(formatter.Object);

            // Act
            tracer.InvokeGetDeserializer(type, content);

            // Assert
            formatter.Protected().Verify("GetDeserializer", Times.Once(), type, content);
        }

        [Fact]
        public void GetSerializer_Calls_GetSerializerOnInner()
        {
            // Arrange
            Type type = new Mock<Type>().Object;
            object value = new object();
            HttpContent content = new StringContent("");
            Mock<XmlMediaTypeFormatter> formatter = new Mock<XmlMediaTypeFormatter>();
            XmlMediaTypeFormatterTracer tracer = CreateTracer(formatter.Object);

            // Act
            tracer.InvokeGetSerializer(type, value, content);

            // Assert
            formatter.Protected().Verify("GetSerializer", Times.Once(), type, value, content);
        }

        private static XmlMediaTypeFormatterTracer CreateTracer(XmlMediaTypeFormatter inner)
        {
            ITraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            return new XmlMediaTypeFormatterTracer(inner, traceWriter, request);
        }
    }
}

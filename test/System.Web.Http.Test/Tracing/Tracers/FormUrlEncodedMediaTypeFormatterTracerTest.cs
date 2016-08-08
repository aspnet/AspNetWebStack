// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Services;
using Microsoft.TestCommon;

namespace System.Web.Http.Tracing.Tracers
{
    public class FormUrlEncodedMediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<FormUrlEncodedMediaTypeFormatter>
    {
        public override MediaTypeFormatter CreateTracer(FormUrlEncodedMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new FormUrlEncodedMediaTypeFormatterTracer(formatter, traceWriter, request);
        }

        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            FormUrlEncodedMediaTypeFormatter innerFormatter = new FormUrlEncodedMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            FormUrlEncodedMediaTypeFormatterTracer tracer = new FormUrlEncodedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void ReadBufferSize_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            FormUrlEncodedMediaTypeFormatter innerFormatter = new FormUrlEncodedMediaTypeFormatter();
            innerFormatter.ReadBufferSize = innerFormatter.ReadBufferSize + 1;
            FormUrlEncodedMediaTypeFormatterTracer tracer = new FormUrlEncodedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.ReadBufferSize, tracer.ReadBufferSize);
        }

        [Fact]
        public void Inner_Property_On_FormUrlEncodedMediaTypeFormatterTracer_Returns_FormUrlEncodedMediaTypeFormatter()
        {
            // Arrange
            FormUrlEncodedMediaTypeFormatter expectedInner = new FormUrlEncodedMediaTypeFormatter();
            FormUrlEncodedMediaTypeFormatterTracer productUnderTest = new FormUrlEncodedMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            FormUrlEncodedMediaTypeFormatter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_FormUrlEncodedMediaTypeFormatterTracer_Returns_FormUrlEncodedMediaTypeFormatter()
        {
            // Arrange
            FormUrlEncodedMediaTypeFormatter expectedInner = new FormUrlEncodedMediaTypeFormatter();
            FormUrlEncodedMediaTypeFormatterTracer productUnderTest = new FormUrlEncodedMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            FormUrlEncodedMediaTypeFormatter actualInner = Decorator.GetInner(productUnderTest as FormUrlEncodedMediaTypeFormatter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}

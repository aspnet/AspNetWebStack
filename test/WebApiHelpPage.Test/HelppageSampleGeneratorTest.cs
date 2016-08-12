// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.TestCommon;
using Moq;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;
using WebApiHelpPageWebHost.UnitTest.Helpers;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class HelpPageSampleGeneratorTest
    {
        [Fact]
        public void Constructor()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            Assert.NotNull(sampleGenerator.SampleObjects);
            Assert.NotNull(sampleGenerator.ActionSamples);
            Assert.NotNull(sampleGenerator.ActualHttpMessageTypes);
        }

        [Fact]
        public void GetSampleRequests_Empty()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Get");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleRequests(apiDescription);
            Assert.Empty(samples);
        }

        [Fact]
        public void GetSampleRequests_FromSampleObjects()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            sampleGenerator.SampleObjects.Add(typeof(string), "sample value");
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Post", "value");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleRequests(apiDescription);
            Assert.NotEmpty(samples);
            foreach (var samplePair in samples)
            {
                Assert.Contains("sample value", ((TextSample)samplePair.Value).Text);
            }
        }

        [Fact]
        public void GetSampleRequests_FromSampleObjects_AndSettingActualRequestTypes()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            sampleGenerator.ActualHttpMessageTypes.Add(new HelpPageSampleKey(SampleDirection.Request, "values", "options", new[] { "request" }), typeof(string));
            sampleGenerator.SampleObjects.Add(typeof(string), "sample value");
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "values", "options", "request");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleRequests(apiDescription);
            Assert.NotEmpty(samples);
            foreach (var samplePair in samples)
            {
                Assert.Contains("sample value", ((TextSample)samplePair.Value).Text);
            }
        }

        [Fact]
        public void GetSampleRequests_FromActionSamples_BasedOnMediaTypeAndType()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            var sample = new TextSample("test");
            sampleGenerator.ActionSamples.Add(new HelpPageSampleKey(new MediaTypeHeaderValue("application/json"), typeof(Tuple<int, string>)), sample);
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Patch", "valuePair");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleRequests(apiDescription);
            Assert.NotEmpty(samples);
            object result;
            samples.TryGetValue(new MediaTypeHeaderValue("application/json"), out result);
            Assert.Same(sample, result);
            samples.TryGetValue(new MediaTypeHeaderValue("application/xml"), out result);
            Assert.NotSame(sample, result);
        }

        [Fact]
        public void GetSampleRequests_FromActionSamples_BasedOnMediaTypeAndNames()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            var sample = new TextSample("test");
            sampleGenerator.ActionSamples.Add(new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Request, "Values", "Put", new[] { "valuePairCollection" }), sample);
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Put", "valuePairCollection");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleRequests(apiDescription);
            Assert.NotEmpty(samples);
            object result;
            samples.TryGetValue(new MediaTypeHeaderValue("application/xml"), out result);
            Assert.Same(sample, result);
            samples.TryGetValue(new MediaTypeHeaderValue("application/json"), out result);
            Assert.NotSame(sample, result);
        }

        [Fact]
        public void GetSampleRequests_FromActionSamples_WhenTheParameterIsHttpRequestMessage()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            var sample = new TextSample("test");
            sampleGenerator.ActionSamples.Add(new HelpPageSampleKey(new MediaTypeHeaderValue("plain/text"), SampleDirection.Request, "Values", "Options", new[] { "request" }), sample);
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Options", "request");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleRequests(apiDescription);
            Assert.NotEmpty(samples);
            object result;
            samples.TryGetValue(new MediaTypeHeaderValue("plain/text"), out result);
            Assert.Same(sample, result);
            samples.TryGetValue(new MediaTypeHeaderValue("application/json"), out result);
            Assert.Null(result);
        }

        [Fact]
        public void GetSampleResponses_Empty()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Delete", "id");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleResponses(apiDescription);
            Assert.Empty(samples);
        }

        [Fact]
        public void GetSampleResponses_FromSampleObjects()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            sampleGenerator.SampleObjects.Add(typeof(string), "sample value");
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Get", "id");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleResponses(apiDescription);
            Assert.NotEmpty(samples);
            foreach (var samplePair in samples)
            {
                Assert.Contains("sample value", ((TextSample)samplePair.Value).Text);
            }
        }

        [Fact]
        public void GetSampleResponses_FromSampleObjects_AndSettingActualResponseTypes()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            sampleGenerator.ActualHttpMessageTypes.Add(new HelpPageSampleKey(SampleDirection.Response, "values", "post", new[] { "value" }), typeof(string));
            sampleGenerator.SampleObjects.Add(typeof(string), "sample value");
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "values", "post", "value");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleResponses(apiDescription);
            Assert.NotEmpty(samples);
            foreach (var samplePair in samples)
            {
                Assert.Contains("sample value", ((TextSample)samplePair.Value).Text);
            }
        }

        [Fact]
        public void GetSampleResponses_FromActionSamples_BasedOnMediaTypeAndType()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            var sample = new TextSample("test");
            sampleGenerator.ActionSamples.Add(new HelpPageSampleKey(new MediaTypeHeaderValue("application/json"), typeof(IEnumerable<string>)), sample);
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Get", new string[0]);
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleResponses(apiDescription);
            Assert.NotEmpty(samples);
            object result;
            samples.TryGetValue(new MediaTypeHeaderValue("application/json"), out result);
            Assert.Same(sample, result);
            samples.TryGetValue(new MediaTypeHeaderValue("application/xml"), out result);
            Assert.NotSame(sample, result);
        }

        [Fact]
        public void GetSampleResponses_FromActionSamples_BasedOnMediaTypeAndNames()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            var sample = new TextSample("test");
            sampleGenerator.ActionSamples.Add(new HelpPageSampleKey(new MediaTypeHeaderValue("application/xml"), SampleDirection.Response, "Values", "Get", new[] { "id" }), sample);
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Get", "id");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleResponses(apiDescription);
            Assert.NotEmpty(samples);
            object result;
            samples.TryGetValue(new MediaTypeHeaderValue("application/xml"), out result);
            Assert.Same(sample, result);
            samples.TryGetValue(new MediaTypeHeaderValue("application/json"), out result);
            Assert.NotSame(sample, result);
        }

        [Fact]
        public void GetSampleResponses_FromActionSamples_WhenTheReturnTypeIsHttpResponseMessage()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            var sample = new TextSample("test");
            sampleGenerator.ActionSamples.Add(new HelpPageSampleKey(new MediaTypeHeaderValue("plain/text"), SampleDirection.Response, "Values", "Post", new[] { "value" }), sample);
            ApiDescription apiDescription = ApiDescriptionHelpers.GetApiDescription(null, "Values", "Post", "value");
            IDictionary<MediaTypeHeaderValue, object> samples = sampleGenerator.GetSampleResponses(apiDescription);
            Assert.NotEmpty(samples);
            object result;
            samples.TryGetValue(new MediaTypeHeaderValue("plain/text"), out result);
            Assert.Same(sample, result);
            samples.TryGetValue(new MediaTypeHeaderValue("application/json"), out result);
            Assert.Null(result);
        }

        [Fact]
        public void GetActionSample_ReturnNullWhenSampleNotProvided()
        {
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            Assert.Null(
                sampleGenerator.GetActionSample(
                    "a",
                    "b",
                    new string[0],
                    typeof(string),
                    new XmlMediaTypeFormatter(),
                    new MediaTypeHeaderValue("text/xml"),
                    SampleDirection.Response
                ));
        }

        [Fact]
        public void WriteSampleObjectUsingFormatter_ReturnsFormatterError()
        {
            Mock<MediaTypeFormatter> bogusFormatter = new Mock<MediaTypeFormatter>();
            bogusFormatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            bogusFormatter.Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<TransportContext>())).Returns(() =>
            {
                throw new ApplicationException("formatter failed.");
            });
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            InvalidSample sampleNotProvided = Assert.IsType<InvalidSample>(
                sampleGenerator.WriteSampleObjectUsingFormatter(
                    bogusFormatter.Object,
                    "hello world",
                    typeof(string),
                    new MediaTypeHeaderValue("text/json")
                ));
            Assert.Equal("An exception has occurred while using the formatter 'MediaTypeFormatterProxy' to generate sample for media type 'text/json'. Exception message: formatter failed.",
                sampleNotProvided.ErrorMessage);
        }

        [Fact]
        public void WriteSampleObjectUsingFormatter_TryFormattingNonXmlSamples_DoesNotThrow()
        {
            Mock<MediaTypeFormatter> customFormatter = new Mock<MediaTypeFormatter>();
            customFormatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            customFormatter.Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<TransportContext>())).Returns(
            (Type type, object obj, Stream stream, HttpContent content, TransportContext context) =>
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write("some\r\nnon xml string");
                writer.Flush();
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                return tcs.Task;
            });
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            TextSample sample = Assert.IsType<TextSample>(
                sampleGenerator.WriteSampleObjectUsingFormatter(
                    customFormatter.Object,
                    "hello world",
                    typeof(string),
                    new MediaTypeHeaderValue("text/xml")
                ));
            Assert.Equal("some\r\nnon xml string", sample.Text);
        }

        [Fact]
        public void WriteSampleObjectUsingFormatter_TryFormattingNonJsonSamples_DoesNotThrow()
        {
            Mock<MediaTypeFormatter> customFormatter = new Mock<MediaTypeFormatter>();
            customFormatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            customFormatter.Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<TransportContext>())).Returns(
            (Type type, object obj, Stream stream, HttpContent content, TransportContext context) =>
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write("some\r\nnon <json> string");
                writer.Flush();
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                return tcs.Task;
            });
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            TextSample sample = Assert.IsType<TextSample>(
                sampleGenerator.WriteSampleObjectUsingFormatter(
                    customFormatter.Object,
                    "hello world",
                    typeof(string),
                    new MediaTypeHeaderValue("text/json")
                ));
            Assert.Equal("some\r\nnon <json> string", sample.Text);
        }

        [Fact]
        public void WriteSampleObjectUsingFormatter_UnwrapsAggregateException()
        {
            Mock<MediaTypeFormatter> bogusFormatter = new Mock<MediaTypeFormatter>();
            bogusFormatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            bogusFormatter.Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<TransportContext>())).Returns(() =>
            {
                throw new AggregateException(new FormatException("Invalid format."));
            });
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
            InvalidSample sampleNotProvided = Assert.IsType<InvalidSample>(
                sampleGenerator.WriteSampleObjectUsingFormatter(
                    bogusFormatter.Object,
                    "hello world",
                    typeof(string),
                    new MediaTypeHeaderValue("text/json")
                ));
            Assert.Equal("An exception has occurred while using the formatter 'MediaTypeFormatterProxy' to generate sample for media type 'text/json'. Exception message: Invalid format.",
                sampleNotProvided.ErrorMessage);
        }

        [Fact]
        public void ResolveType_ThrowsInvalidEnumArgumentException()
        {
            Assert.Throws(typeof(InvalidEnumArgumentException), () =>
            {
                Collection<MediaTypeFormatter> formatters;
                HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
                sampleGenerator.ResolveType(new ApiDescription(), "c", "a", new[] { "p" }, (SampleDirection)78, out formatters);
            });
        }

        public static IEnumerable<object[]> GetSample_ThrowsArgumentNullException_PropertyData
        {
            get
            {
                yield return new Action[] { () =>
                {
                    HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
                    sampleGenerator.GetSample(null, SampleDirection.Request);
                }};
            }
        }

        public static IEnumerable<object[]> ResolveType_ThrowsArgumentNullException_PropertyData
        {
            get
            {
                yield return new Action[] { () =>
                {
                    HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
                    Collection<MediaTypeFormatter> formatters;
                    sampleGenerator.ResolveType(null, "a", "c", new string[0], SampleDirection.Request, out formatters);
                }};
            }
        }

        public static IEnumerable<object[]> WriteSampleObjectUsingFormatter_ThrowsArgumentNullException_PropertyData
        {
            get
            {
                yield return new Action[] { () =>
                {
                    HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
                    sampleGenerator.WriteSampleObjectUsingFormatter(null, "sample", typeof(string), new MediaTypeHeaderValue("text/xml"));
                }};
                yield return new Action[] { () =>
                {
                    HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();
                    sampleGenerator.WriteSampleObjectUsingFormatter(new XmlMediaTypeFormatter(), "sample", typeof(string), null);
                }};
            }
        }

        [Theory]
        [PropertyData("GetSample_ThrowsArgumentNullException_PropertyData")]
        [PropertyData("ResolveType_ThrowsArgumentNullException_PropertyData")]
        [PropertyData("WriteSampleObjectUsingFormatter_ThrowsArgumentNullException_PropertyData")]
        public void Method_ThrowsArgumentNullException(Action constructorDelegate)
        {
            Assert.Throws(typeof(ArgumentNullException), constructorDelegate);
        }
    }
}

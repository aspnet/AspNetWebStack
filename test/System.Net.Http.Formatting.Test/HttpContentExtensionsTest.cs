// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class HttpContentExtensionsTest
    {
        private static readonly IEnumerable<MediaTypeFormatter> _emptyFormatterList = Enumerable.Empty<MediaTypeFormatter>();
        private readonly Mock<MediaTypeFormatter> _formatterMock = new Mock<MediaTypeFormatter> { CallBase = true };
        private readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue("foo/bar");
        private readonly MediaTypeFormatter[] _formatters;

        public HttpContentExtensionsTest()
        {
            _formatterMock.Object.SupportedMediaTypes.Add(_mediaType);
            _formatters = new[] { _formatterMock.Object };
        }

        [Fact]
        public void ReadAsAsync_WhenContentParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync(null, typeof(string), _emptyFormatterList), "content");
        }

        [Fact]
        public void ReadAsAsync_WhenTypeParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync(new StringContent(""), null, _emptyFormatterList), "type");
        }

        [Fact]
        public void ReadAsAsync_WhenFormattersParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync(new StringContent(""), typeof(string), null), "formatters");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenContentParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync<string>(null, _emptyFormatterList), "content");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenFormattersParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => HttpContentExtensions.ReadAsAsync<string>(new StringContent(""), null), "formatters");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenContentIsObjectContent_GoesThroughSerializationCycleToConvertTypes()
        {
            var content = new ObjectContent<int[]>(new int[] { 10, 20, 30, 40 }, new JsonMediaTypeFormatter());

            byte[] result = content.ReadAsAsync<byte[]>().Result;

            Assert.Equal(new byte[] { 10, 20, 30, 40 }, result);
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenNoMatchingFormatterFound_Throws()
        {
            var content = new StringContent("{}");
            content.Headers.ContentType = _mediaType;
            content.Headers.ContentType.CharSet = "utf-16";
            var formatters = new MediaTypeFormatter[] { new JsonMediaTypeFormatter() };

            Assert.Throws<UnsupportedMediaTypeException>(() => content.ReadAsAsync<List<string>>(formatters),
                "No MediaTypeFormatter is available to read an object of type 'List`1' from content with media type 'foo/bar'.");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenTypeIsReferenceTypeAndNoMediaType_Throws()
        {
            var content = new StringContent("{}");
            content.Headers.ContentType = null;
            var formatters = new MediaTypeFormatter[] { new JsonMediaTypeFormatter() };

            Assert.Throws<UnsupportedMediaTypeException>(() => content.ReadAsAsync<List<string>>(formatters),
                "No MediaTypeFormatter is available to read an object of type 'List`1' from content with media type 'application/octet-stream'.");
        }

        [Fact]
        public void ReadAsAsyncOfT_WhenTypeIsValueTypeAndNoMediaType_Throws()
        {
            var content = new StringContent("123456");
            content.Headers.ContentType = null;
            var formatters = new MediaTypeFormatter[] { new JsonMediaTypeFormatter() };

            Assert.Throws<UnsupportedMediaTypeException>(() => content.ReadAsAsync<int>(formatters),
                "No MediaTypeFormatter is available to read an object of type 'Int32' from content with media type 'application/octet-stream'.");
        }

        [Fact]
        public void ReadAsAsyncOfT_ReadsFromContent_ThenInvokesFormattersReadFromStreamMethod()
        {
            Stream contentStream = null;
            string value = "42";
            var contentMock = new Mock<TestableHttpContent> { CallBase = true };
            contentMock.Setup(c => c.SerializeToStreamAsyncPublic(It.IsAny<Stream>(), It.IsAny<TransportContext>()))
                .Returns(TaskHelpers.Completed)
                .Callback((Stream s, TransportContext _) => contentStream = s)
                .Verifiable();
            HttpContent content = contentMock.Object;
            content.Headers.ContentType = _mediaType;
            _formatterMock
                .Setup(f => f.ReadFromStreamAsync(typeof(string), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<IFormatterLogger>()))
                .Returns(Task.FromResult<object>(value));
            _formatterMock.Setup(f => f.CanReadType(typeof(string))).Returns(true);

            var result = content.ReadAsAsync<string>(_formatters);

            var resultValue = result.Result;
            Assert.Same(value, resultValue);
            contentMock.Verify();
            _formatterMock.Verify(f => f.ReadFromStreamAsync(typeof(string), contentStream, content, null), Times.Once());
        }

        [Fact]
        public void ReadAsAsyncOfT_InvokesFormatterEvenIfContentLengthIsZero()
        {
            var content = new StringContent("");
            _formatterMock.Setup(f => f.CanReadType(typeof(string))).Returns(true);
            _formatterMock.Object.SupportedMediaTypes.Add(content.Headers.ContentType);

            var result = content.ReadAsAsync<string>(_formatters);

            result.WaitUntilCompleted();
            _formatterMock.Verify(f => f.ReadFromStreamAsync(typeof(string), It.IsAny<Stream>(), content, It.IsAny<IFormatterLogger>()), Times.Once());
        }

        [Fact]
        public void ReadAsAsync_WhenContentIsObjectContentAndValueIsCompatibleType_ReadsValueFromObjectContent()
        {
            _formatterMock.Setup(f => f.CanWriteType(typeof(TestClass))).Returns(true);
            var value = new TestClass();
            var content = new ObjectContent<TestClass>(value, _formatterMock.Object);

            Assert.Same(value, content.ReadAsAsync<object>(_formatters).Result);
            Assert.Same(value, content.ReadAsAsync<TestClass>(_formatters).Result);
            Assert.Same(value, content.ReadAsAsync(typeof(object), _formatters).Result);
            Assert.Same(value, content.ReadAsAsync(typeof(TestClass), _formatters).Result);

            _formatterMock.Verify(f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), content, It.IsAny<IFormatterLogger>()), Times.Never());
        }

        [Fact]
        public void ReadAsAsync_WhenContentIsObjectContentAndValueIsNull_IfTypeIsNullable_SerializesAndDeserializesValue()
        {
            _formatterMock.Setup(f => f.CanWriteType(typeof(object))).Returns(true);
            _formatterMock.Setup(f => f.CanReadType(It.IsAny<Type>())).Returns(true);
            var content = new ObjectContent<object>(null, _formatterMock.Object);
            SetupUpRoundTripSerialization(type => null);

            Assert.Null(content.ReadAsAsync<object>(_formatters).Result);
            Assert.Null(content.ReadAsAsync<TestClass>(_formatters).Result);
            Assert.Null(content.ReadAsAsync<Nullable<int>>(_formatters).Result);
            Assert.Null(content.ReadAsAsync(typeof(object), _formatters).Result);
            Assert.Null(content.ReadAsAsync(typeof(TestClass), _formatters).Result);
            Assert.Null(content.ReadAsAsync(typeof(Nullable<int>), _formatters).Result);

            _formatterMock.Verify(f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), content, It.IsAny<IFormatterLogger>()), Times.Exactly(6));
        }

        [Fact]
        public void ReadAsAsync_WhenContentIsObjectContentAndValueIsNull_IfTypeIsNotNullable_SerializesAndDeserializesValue()
        {
            _formatterMock.Setup(f => f.CanWriteType(typeof(object))).Returns(true);
            _formatterMock.Setup(f => f.CanReadType(typeof(Int32))).Returns(true);
            var content = new ObjectContent<object>(null, _formatterMock.Object, _mediaType);
            SetupUpRoundTripSerialization();

            Assert.IsType<Int32>(content.ReadAsAsync<Int32>(_formatters).Result);
            Assert.IsType<Int32>(content.ReadAsAsync(typeof(Int32), _formatters).Result);

            _formatterMock.Verify(f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), content, It.IsAny<IFormatterLogger>()), Times.Exactly(2));
        }

        [Fact]
        public void ReadAsAsync_WhenContentIsObjectContentAndValueIsNotCompatibleType_SerializesAndDeserializesValue()
        {
            _formatterMock.Setup(f => f.CanWriteType(typeof(TestClass))).Returns(true);
            _formatterMock.Setup(f => f.CanReadType(typeof(string))).Returns(true);
            var value = new TestClass();
            var content = new ObjectContent<TestClass>(value, _formatterMock.Object, _mediaType);
            SetupUpRoundTripSerialization(type => new TestClass());

            Assert.Throws<InvalidCastException>(() => content.ReadAsAsync<string>(_formatters).RethrowFaultedTaskException());

            Assert.IsNotType<string>(content.ReadAsAsync(typeof(string), _formatters).Result);

            _formatterMock.Verify(f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), content, It.IsAny<IFormatterLogger>()), Times.Exactly(2));
        }

        [Fact]
        public void ReadAsAsync_WhenContentIsMultipartContentAndFormatterCanReadFromTheContent()
        {
            MultipartContent mimeContent = new MultipartContent();
            mimeContent.Add(new StringContent("multipartContent"));

            _formatterMock.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            _formatterMock.Setup(f => f.CanReadType(It.IsAny<Type>())).Returns(true);
            _formatterMock.Setup(f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<IFormatterLogger>()))
                .Returns<Type, Stream, HttpContent, IFormatterLogger>((type, stream, content, logger) =>
                    {
                        MultipartMemoryStreamProvider provider = content.ReadAsMultipartAsync().Result;
                        Assert.Equal(1, provider.Contents.Count);
                        return Task.FromResult<object>(provider.Contents[0].ReadAsStringAsync().Result);
                    });
            MediaTypeFormatter formatter = _formatterMock.Object;
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/mixed"));

            Assert.Equal("multipartContent", mimeContent.ReadAsAsync<string>(new[] { formatter }).Result);
        }

        [Fact]
        public void ReadAsAsync_type_cancellationToken_PassesCancellationTokenFurther()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = new StringContent("42", Encoding.Default, "application/json");

            Assert.Throws<TaskCanceledException>(() => content.ReadAsAsync(typeof(int), cts.Token).Wait());
        }

        [Fact]
        public void ReadAsAsync_type_formatters_cancellationToken_PassesCancellationTokenFurther()
        {
            // Arrange
            Stream stream = new MemoryStream();
            HttpContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/test");
            CancellationToken token = new CancellationToken();
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>(MockBehavior.Strict);
            formatter.Object.SupportedMediaTypes.Add(content.Headers.ContentType);
            formatter.Setup(f => f.CanReadType(typeof(int))).Returns(true);
            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(int), It.IsAny<Stream>(), content, null, token))
                .Returns(Task.FromResult<object>(42))
                .Verifiable();

            // Act
            content.ReadAsAsync(typeof(int), new[] { formatter.Object }, token).Wait();

            // Assert
            formatter.Verify();
        }

        [Fact]
        public void ReadAsAsync_type_formatters_formatterLogger_cancellationToken_PassesCancellationTokenFurther()
        {
            // Arrange
            Stream stream = new MemoryStream();
            HttpContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/test");
            CancellationToken token = new CancellationToken();
            IFormatterLogger formatterLogger = new Mock<IFormatterLogger>().Object;

            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>(MockBehavior.Strict);
            formatter.Object.SupportedMediaTypes.Add(content.Headers.ContentType);
            formatter.Setup(f => f.CanReadType(typeof(int))).Returns(true);
            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(int), It.IsAny<Stream>(), content, formatterLogger, token))
                .Returns(Task.FromResult<object>(42))
                .Verifiable();

            // Act
            content.ReadAsAsync(typeof(int), new[] { formatter.Object }, formatterLogger, token).Wait();

            // Assert
            formatter.Verify();
        }

        [Fact]
        public void ReadAsAsyncOfT_cancellationToken_PassesCancellationTokenFurther()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = new StringContent("42", Encoding.Default, "application/json");

            Assert.Throws<TaskCanceledException>(() => content.ReadAsAsync<int>(cts.Token).Wait());
        }

        [Fact]
        public void ReadAsAsyncOfT_formatters_cancellationToken_PassesCancellationTokenFurther()
        {
            // Arrange
            Stream stream = new MemoryStream();
            HttpContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/test");
            CancellationToken token = new CancellationToken();
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>(MockBehavior.Strict);
            formatter.Object.SupportedMediaTypes.Add(content.Headers.ContentType);
            formatter.Setup(f => f.CanReadType(typeof(int))).Returns(true);
            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(int), It.IsAny<Stream>(), content, null, token))
                .Returns(Task.FromResult<object>(42))
                .Verifiable();

            // Act
            content.ReadAsAsync<int>(new[] { formatter.Object }, token).Wait();

            // Assert
            formatter.Verify();
        }

        [Fact]
        public void ReadAsAsyncOfT_formatters_formatterLogger_cancellationToken_PassesCancellationTokenFurther()
        {
            // Arrange
            Stream stream = new MemoryStream();
            HttpContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/test");
            CancellationToken token = new CancellationToken();
            IFormatterLogger formatterLogger = new Mock<IFormatterLogger>().Object;

            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>(MockBehavior.Strict);
            formatter.Object.SupportedMediaTypes.Add(content.Headers.ContentType);
            formatter.Setup(f => f.CanReadType(typeof(int))).Returns(true);
            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(int), It.IsAny<Stream>(), content, formatterLogger, token))
                .Returns(Task.FromResult<object>(42))
                .Verifiable();

            // Act
            content.ReadAsAsync<int>(new[] { formatter.Object }, formatterLogger, token).Wait();

            // Assert
            formatter.Verify();
        }

        private void SetupUpRoundTripSerialization(Func<Type, object> factory = null)
        {
            factory = factory ?? Activator.CreateInstance;
            _formatterMock.Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<TransportContext>()))
                .Returns(TaskHelpers.Completed());
            _formatterMock.Setup(f => f.ReadFromStreamAsync(It.IsAny<Type>(), It.IsAny<Stream>(), It.IsAny<HttpContent>(), It.IsAny<IFormatterLogger>()))
                .Returns<Type, Stream, HttpContent, IFormatterLogger>((type, stream, content, logger) => Task.FromResult<object>(factory(type)));
        }

        public class TestClass { }

        public abstract class TestableHttpContent : HttpContent
        {
            protected override Task<Stream> CreateContentReadStreamAsync()
            {
                return CreateContentReadStreamAsyncPublic();
            }

            public virtual Task<Stream> CreateContentReadStreamAsyncPublic()
            {
                return base.CreateContentReadStreamAsync();
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return SerializeToStreamAsyncPublic(stream, context);
            }

            public abstract Task SerializeToStreamAsyncPublic(Stream stream, TransportContext context);

            protected override bool TryComputeLength(out long length)
            {
                return TryComputeLengthPublic(out length);
            }

            public abstract bool TryComputeLengthPublic(out long length);
        }
    }
}

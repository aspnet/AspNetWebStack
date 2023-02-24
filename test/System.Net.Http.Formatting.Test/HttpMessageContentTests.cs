// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpMessageContentTests
    {
        private static readonly int iterations = 5;

        private static void AddMessageHeaders(HttpHeaders headers)
        {
            headers.Add("N1", new string[] { "V1a", "V1b", "V1c", "V1d", "V1e" });
            headers.Add("N2", "V2");
        }

        private static HttpRequestMessage CreateRequest(Uri requestUri, bool containsEntity)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod(ParserData.HttpMethod);
            httpRequest.RequestUri = requestUri;
            httpRequest.Version = new Version("1.2");
            AddMessageHeaders(httpRequest.Headers);
            if (containsEntity)
            {
                httpRequest.Content = new StringContent(ParserData.HttpMessageEntity);
            }

            return httpRequest;
        }

        private static HttpResponseMessage CreateResponse(bool containsEntity)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            httpResponse.StatusCode = ParserData.HttpStatus;
            httpResponse.ReasonPhrase = ParserData.HttpReasonPhrase;
            httpResponse.Version = new Version("1.2");
            AddMessageHeaders(httpResponse.Headers);
            httpResponse.Content =
                containsEntity ? new StringContent(ParserData.HttpMessageEntity) : new StreamContent(Stream.Null);

            return httpResponse;
        }

        private static async Task<string> ReadContentAsync(HttpContent content, bool unBuffered = false)
        {
            if (unBuffered)
            {
                var stream = new MemoryStream();
                await content.CopyToAsync(stream);
                stream.Position = 0L;

                // StreamReader will dispose of the Stream.
                using var reader = new StreamReader(stream);

                return await reader.ReadToEndAsync();
            }
            else
            {
                await content.LoadIntoBufferAsync();

                return await content.ReadAsStringAsync();
            }
        }

        private static async Task ValidateRequest(HttpContent content, bool containsEntity, bool unBuffered = false)
        {
            Assert.Equal(ParserData.HttpRequestMediaType, content.Headers.ContentType);
            long? length = content.Headers.ContentLength;
            Assert.NotNull(length);

            string message = await ReadContentAsync(content, unBuffered);
            if (containsEntity)
            {
                Assert.Equal(ParserData.HttpRequestWithEntity.Length, length);
                Assert.Equal(ParserData.HttpRequestWithEntity, message);
            }
            else
            {
                Assert.Equal(ParserData.HttpRequest.Length, length);
                Assert.Equal(ParserData.HttpRequest, message);
            }
        }

        private static async Task ValidateResponse(HttpContent content, bool containsEntity, bool unBuffered = false)
        {
            Assert.Equal(ParserData.HttpResponseMediaType, content.Headers.ContentType);

            long? length = content.Headers.ContentLength;
            Assert.NotNull(length);

            string message = await ReadContentAsync(content, unBuffered);
            if (containsEntity)
            {
                Assert.Equal(ParserData.HttpResponseWithEntity.Length, length);
                Assert.Equal(ParserData.HttpResponseWithEntity, message);
            }
            else
            {
                Assert.Equal(ParserData.HttpResponse.Length, length);
                Assert.Equal(ParserData.HttpResponse, message);
            }
        }

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpMessageContent, HttpContent>(TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsDisposable);
        }

        [Fact]
        public void RequestConstructor()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpMessageContent instance = new HttpMessageContent(request);
            Assert.NotNull(instance);
            Assert.Same(request, instance.HttpRequestMessage);
            Assert.Null(instance.HttpResponseMessage);
        }

        [Fact]
        public void RequestConstructorThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => { new HttpMessageContent((HttpRequestMessage)null); }, "httpRequest");
        }

        [Fact]
        public void ResponseConstructor()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            HttpMessageContent instance = new HttpMessageContent(response);
            Assert.NotNull(instance);
            Assert.Same(response, instance.HttpResponseMessage);
            Assert.Null(instance.HttpRequestMessage);
        }

        [Fact]
        public void ResponseConstructorThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => { new HttpMessageContent((HttpResponseMessage)null); }, "httpResponse");
        }


        [Fact]
        public async Task SerializeRequest()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, false);
                HttpMessageContent instance = new HttpMessageContent(request);
                await ValidateRequest(instance, false);
            }
        }

        [Fact]
        public async Task SerializeRequestWithExistingHostHeader()
        {
            HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, false);
            string host = ParserData.HttpHostName;
            request.Headers.Host = host;
            HttpMessageContent instance = new HttpMessageContent(request);
            string message = await ReadContentAsync(instance);
            Assert.Equal(ParserData.HttpRequestWithHost, message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SerializeRequestMultipleTimes(bool unBuffered)
        {
            HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, containsEntity: false);
            HttpMessageContent instance = new(request);
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                await ValidateRequest(instance, containsEntity: false, unBuffered);
            }
        }

        [Fact]
        public async Task SerializeResponse()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpResponseMessage response = CreateResponse(false);
                HttpMessageContent instance = new HttpMessageContent(response);
                await ValidateResponse(instance, false);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SerializeResponseMultipleTimes(bool unBuffered)
        {
            HttpResponseMessage response = CreateResponse(containsEntity: false);
            HttpMessageContent instance = new(response);
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                await ValidateResponse(instance, containsEntity: false, unBuffered);
            }
        }

        [Fact]
        public async Task SerializeRequestWithEntity()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, true);
                HttpMessageContent instance = new HttpMessageContent(request);
                await ValidateRequest(instance, true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SerializeRequestWithEntityMultipleTimes(bool unBuffered)
        {
            HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, containsEntity: true);
            HttpMessageContent instance = new(request);
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                await ValidateRequest(instance, containsEntity: true, unBuffered);
            }
        }

        [Fact]
        public async Task SerializeResponseWithEntity()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpResponseMessage response = CreateResponse(true);
                HttpMessageContent instance = new HttpMessageContent(response);
                await ValidateResponse(instance, true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SerializeResponseWithEntityMultipleTimes(bool unBuffered)
        {
            HttpResponseMessage response = CreateResponse(containsEntity: true);
            HttpMessageContent instance = new(response);
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                await ValidateResponse(instance, containsEntity: true, unBuffered);
            }
        }

        [Fact]
        public async Task SerializeRequestAsync()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, false);
                HttpMessageContent instance = new HttpMessageContent(request);
                await ValidateRequest(instance, false);
            }
        }

        [Fact]
        public async Task SerializeResponseAsync()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpResponseMessage response = CreateResponse(false);
                HttpMessageContent instance = new HttpMessageContent(response);
                await ValidateResponse(instance, false);
            }
        }

        [Fact]
        public async Task SerializeRequestWithPortAndQueryAsync()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUriWithPortAndQuery, false);
                HttpMessageContent instance = new HttpMessageContent(request);
                string message = await ReadContentAsync(instance);
                Assert.Equal(ParserData.HttpRequestWithPortAndQuery, message);
            }
        }

        [Fact]
        public async Task SerializeRequestWithEntityAsync()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, true);
                HttpMessageContent instance = new HttpMessageContent(request);
                await ValidateRequest(instance, true);
            }
        }

        [Fact]
        public async Task SerializeResponseWithEntityAsync()
        {
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                HttpResponseMessage response = CreateResponse(true);
                HttpMessageContent instance = new HttpMessageContent(response);
                await ValidateResponse(instance, true);
            }
        }

        [Fact]
        public void DisposeInnerHttpRequestMessage()
        {
            HttpRequestMessage request = CreateRequest(ParserData.HttpRequestUri, false);
            HttpMessageContent instance = new HttpMessageContent(request);
            instance.Dispose();
            Assert.ThrowsObjectDisposed(() => { request.Method = HttpMethod.Get; }, typeof(HttpRequestMessage).FullName);
        }

        [Fact]
        public void DisposeInnerHttpResponseMessage()
        {
            HttpResponseMessage response = CreateResponse(false);
            HttpMessageContent instance = new HttpMessageContent(response);
            instance.Dispose();
            Assert.ThrowsObjectDisposed(() => { response.StatusCode = HttpStatusCode.OK; }, typeof(HttpResponseMessage).FullName);
        }

        [Fact]
        public void Request_ContentLengthNull_IfReadOnlyStream()
        {
            var request = CreateRequest(ParserData.HttpRequestUri, containsEntity: false);
            request.Content = new StreamContent(new ReadOnlyStream());
            var instance = new HttpMessageContent(request);

            var length = instance.Headers.ContentLength;

            Assert.Null(length);
        }

        [Fact]
        public void Response_ContentLengthNull_IfReadOnlyStream()
        {
            var response = CreateResponse(containsEntity: false);
            response.Content = new StreamContent(new ReadOnlyStream());
            var instance = new HttpMessageContent(response);

            var length = instance.Headers.ContentLength;

            Assert.Null(length);
        }

        // Also confirms content can be serialized multiple times if either buffered or involves a seekable Stream.
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task Request_NoContentLength_IfNotRequested(bool readOnlyStream, bool unBuffered)
        {
            var request = CreateRequest(ParserData.HttpRequestUri, containsEntity: false);
            if (readOnlyStream)
            {
                request.Content = new StreamContent(new ReadOnlyStream());
            }
            var instance = new HttpMessageContent(request);

            for (int cnt = 0; cnt < iterations; cnt++)
            {
                var contentString = await ReadContentAsync(instance, unBuffered);

                Assert.Equal(ParserData.HttpRequest.Replace("Content-Length: 0\r\n", ""), contentString);
            }
        }

        // Also confirms content can be serialized multiple times if either buffered or involves a seekable Stream.
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task Response_NoContentLength_IfNotRequested(bool readOnlyStream, bool unBuffered)
        {
            var response = CreateResponse(containsEntity: false);
            if (readOnlyStream)
            {
                response.Content = new StreamContent(new ReadOnlyStream());
            }
            var instance = new HttpMessageContent(response);

            for (int cnt = 0; cnt < iterations; cnt++)
            {
                var contentString = await ReadContentAsync(instance, unBuffered);

                Assert.Equal(ParserData.HttpResponse.Replace("Content-Length: 0\r\n", ""), contentString);
            }
        }

        // Covers the false, false case of Request_NoContentLength_IfNotRequested(...).
        [Fact]
        public async Task Request_HasContentLength_IfBuffered_EvenIfNotRequested()
        {
            var request = CreateRequest(ParserData.HttpRequestUri, containsEntity: false);
            var instance = new HttpMessageContent(request);
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                var contentString = await ReadContentAsync(instance, unBuffered: false);

                Assert.Equal(ParserData.HttpRequest, contentString);
            }
        }

        // Covers the false, false case of Response_NoContentLength_IfNotRequested(...).
        [Fact]
        public async Task Response_HasContentLength_IfBuffered_EvenIfNotRequested()
        {
            var response = CreateResponse(containsEntity: false);
            var instance = new HttpMessageContent(response);
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                var contentString = await ReadContentAsync(instance, unBuffered: false);

                Assert.Equal(ParserData.HttpResponse, contentString);
            }
        }

        // Covers the true, true case of Request_NoContentLength_IfNotRequested(...).
        [Fact]
        public async Task Request_CannotSerializeMultipleTimes_IfNotBufferedAndNotSeekable()
        {
            var request = CreateRequest(ParserData.HttpRequestUri, containsEntity: false);
            request.Content = new StreamContent(new ReadOnlyStream());
            var instance = new HttpMessageContent(request);

            // Act #1
            var contentString = await ReadContentAsync(instance, unBuffered: true);

            // Assert #1
            Assert.Equal(ParserData.HttpRequest.Replace("Content-Length: 0\r\n", ""), contentString);

            // Act #2
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => ReadContentAsync(instance, unBuffered: true),
                "The 'HttpContent' of the 'HttpRequestMessage' has already been read.");
        }

        // Covers the true, true case of Response_NoContentLength_IfNotRequested(...).
        [Fact]
        public async Task Response_CannotSerializeMultipleTimes_IfNotBufferedAndNotSeekable()
        {
            var response = CreateResponse(containsEntity: false);
            response.Content = new StreamContent(new ReadOnlyStream());
            var instance = new HttpMessageContent(response);

            // Act #1
            var contentString = await ReadContentAsync(instance, unBuffered: true);

            // Assert #1
            Assert.Equal(ParserData.HttpResponse.Replace("Content-Length: 0\r\n", ""), contentString);

            // Act #2
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => ReadContentAsync(instance, unBuffered: true),
                "The 'HttpContent' of the 'HttpResponseMessage' has already been read.");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Request_CannotSerialize_IfWriteOnlyStream(bool unBuffered)
        {
            var request = CreateRequest(ParserData.HttpRequestUri, containsEntity: false);
            request.Content = new StreamContent(new WriteOnlyStream());
            var instance = new HttpMessageContent(request);

            await Assert.ThrowsAsync<NotSupportedException>(
                () => ReadContentAsync(instance, unBuffered),
                "Stream does not support reading.");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Response_CannotSerialize_IfWriteOnlyStream(bool unBuffered)
        {
            var response = CreateResponse(containsEntity: false);
            response.Content = new StreamContent(new WriteOnlyStream());
            var instance = new HttpMessageContent(response);

            await Assert.ThrowsAsync<NotSupportedException>(
                () => ReadContentAsync(instance, unBuffered),
                "Stream does not support reading.");
        }

        // Unlike Stream.Null, this stream does not support seeking. Bit more like (say) a network stream or
        // the EmptyReadStream introduced in .NET 5. Note: EmptyReadStream should never be visible to our code
        // because HttpContentMessageExtensions and HttpRequestMessageExtensions overwrite
        // HttpResponseMessage.Content (or HttpRequestMessage.Content in one case) when creating an instance.
        private class ReadOnlyStream : Stream
        {
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotImplementedException();
            public override long Position
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public override void Flush()
            {
                // Do nothing.
            }

            public override int Read(byte[] buffer, int offset, int count) => 0;

            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

            public override void SetLength(long value) => throw new NotImplementedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        }

        // Unlike Stream.Null, this stream does not support seeking. Bit more like (say) a network stream.
        private class WriteOnlyStream : Stream
        {
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotImplementedException();
            public override long Position
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public override void Flush()
            {
                // Do nothing.
            }

            public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

            public override void SetLength(long value) => throw new NotImplementedException();

            public override void Write(byte[] buffer, int offset, int count)
            {
                // Ignore all parameters and do nothing.
            }
        }
    }
}

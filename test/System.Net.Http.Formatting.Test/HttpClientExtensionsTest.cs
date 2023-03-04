// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Net.Http.Mocks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class HttpClientExtensionsTest
    {
        private const string InvalidUriMessage =
#if NET6_0_OR_GREATER
            "An invalid request URI was provided. Either the request URI must be an absolute URI or BaseAddress must be set.";
#else
            "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.";
#endif
        private readonly MediaTypeFormatter _formatter = new MockMediaTypeFormatter { CallBase = true };
        private readonly HttpClient _client;
        private readonly MediaTypeHeaderValue _mediaTypeHeader = MediaTypeHeaderValue.Parse("foo/bar; charset=utf-16");

        public HttpClientExtensionsTest()
        {
            Mock<TestableHttpMessageHandler> handlerMock = new Mock<TestableHttpMessageHandler> { CallBase = true };
            handlerMock
                .Setup(h => h.SendAsyncPublic(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns((HttpRequestMessage request, CancellationToken _) => Task.FromResult(new HttpResponseMessage() { RequestMessage = request }));

            _client = new HttpClient(handlerMock.Object);
        }

        [Fact]
        public void PostAsJsonAsync_String_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsJsonAsync("http://www.example.com", new object()), "client");
        }

        [Fact]
        public void PostAsJsonAsync_String_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsJsonAsync((string)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PostAsJsonAsync_String_UsesJsonMediaTypeFormatter()
        {
            var response = await _client.PostAsJsonAsync("http://example.com", new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<JsonMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PostAsXmlAsync_String_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsXmlAsync("http://www.example.com", new object()), "client");
        }

#if !Testing_NetStandard1_3 // Avoid "The configured formatter 'System.Net.Http.Formatting.XmlMediaTypeFormatter' cannot write an object of type 'Object'."
        [Fact]
        public void PostAsXmlAsync_String_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsXmlAsync((string)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PostAsXmlAsync_String_UsesXmlMediaTypeFormatter()
        {
            var response = await _client.PostAsXmlAsync("http://example.com", new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<XmlMediaTypeFormatter>(content.Formatter);
        }
#endif

        [Fact]
        public void PostAsync_String_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsync("http://www.example.com", new object(), new JsonMediaTypeFormatter(), "text/json"), "client");
        }

        [Fact]
        public void PostAsync_String_WhenRequestUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsync((string)null, new object(), new JsonMediaTypeFormatter(), "text/json"),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PostAsync_String_WhenRequestUriIsSet_CreatesRequestWithAppropriateUri()
        {
            _client.BaseAddress = new Uri("http://example.com/");

            var response = await _client.PostAsync("myapi/", new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Equal("http://example.com/myapi/", request.RequestUri.ToString());
        }

        [Fact]
        public async Task PostAsync_String_WhenAuthoritativeMediaTypeIsSet_CreatesRequestWithAppropriateContentType()
        {
            var response = await _client.PostAsync("http://example.com/myapi/", new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-16", request.Content.Headers.ContentType.CharSet);
        }

        [Fact]
        public async Task PostAsync_String_WhenAuthoritativeMediaTypeStringIsSet_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PostAsync("http://example.com/myapi/", new object(), _formatter, mediaType, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PostAsync_String_WhenAuthoritativeMediaTypeStringIsSetWithoutCT_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PostAsync("http://example.com/myapi/", new object(), _formatter, mediaType);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PostAsync_String_WhenFormatterIsSet_CreatesRequestWithObjectContentAndCorrectFormatter()
        {
            var response = await _client.PostAsync("http://example.com/myapi/", new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            var content = Assert.IsType<ObjectContent<object>>(request.Content);
            Assert.Same(_formatter, content.Formatter);
        }

        [Fact]
        public async Task PostAsync_String_IssuesPostRequest()
        {
            var response = await _client.PostAsync("http://example.com/myapi/", new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Same(HttpMethod.Post, request.Method);
        }

        [Fact]
        public void PostAsync_String_WhenMediaTypeFormatterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _client.PostAsync("http://example.com", new object(), formatter: null), "formatter");
        }

        [Fact]
        public void PutAsJsonAsync_String_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsJsonAsync("http://www.example.com", new object()), "client");
        }

        [Fact]
        public void PutAsJsonAsync_String_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsJsonAsync((string)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PutAsJsonAsync_String_UsesJsonMediaTypeFormatter()
        {
            var response = await _client.PutAsJsonAsync("http://example.com", new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<JsonMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PutAsXmlAsync_String_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsXmlAsync("http://www.example.com", new object()), "client");
        }

#if !Testing_NetStandard1_3 // Avoid "The configured formatter 'System.Net.Http.Formatting.XmlMediaTypeFormatter' cannot write an object of type 'Object'."
        [Fact]
        public void PutAsXmlAsync_String_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsXmlAsync((string)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PutAsXmlAsync_String_UsesXmlMediaTypeFormatter()
        {
            var response = await _client.PutAsXmlAsync("http://example.com", new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<XmlMediaTypeFormatter>(content.Formatter);
        }
#endif

        [Fact]
        public void PutAsync_String_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsync("http://www.example.com", new object(), new JsonMediaTypeFormatter(), "text/json"), "client");
        }

        [Fact]
        public void PutAsync_String_WhenRequestUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsync((string)null, new object(), new JsonMediaTypeFormatter(), "text/json"),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PutAsync_String_WhenRequestUriIsSet_CreatesRequestWithAppropriateUri()
        {
            _client.BaseAddress = new Uri("http://example.com/");

            var response = await _client.PutAsync("myapi/", new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Equal("http://example.com/myapi/", request.RequestUri.ToString());
        }

        [Fact]
        public async Task PutAsync_String_WhenAuthoritativeMediaTypeIsSet_CreatesRequestWithAppropriateContentType()
        {
            var response = await _client.PutAsync("http://example.com/myapi/", new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-16", request.Content.Headers.ContentType.CharSet);
        }

        [Fact]
        public async Task PutAsync_String_WhenFormatterIsSet_CreatesRequestWithObjectContentAndCorrectFormatter()
        {
            var response = await _client.PutAsync("http://example.com/myapi/", new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            var content = Assert.IsType<ObjectContent<object>>(request.Content);
            Assert.Same(_formatter, content.Formatter);
        }

        [Fact]
        public async Task PutAsync_String_WhenAuthoritativeMediaTypeStringIsSet_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PutAsync("http://example.com/myapi/", new object(), _formatter, mediaType, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PutAsync_String_WhenAuthoritativeMediaTypeStringIsSetWithoutCT_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PutAsync("http://example.com/myapi/", new object(), _formatter, mediaType);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PutAsync_String_IssuesPutRequest()
        {
            var response = await _client.PutAsync("http://example.com/myapi/", new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Same(HttpMethod.Put, request.Method);
        }

        [Fact]
        public void PutAsync_String_WhenMediaTypeFormatterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _client.PutAsync("http://example.com", new object(), formatter: null), "formatter");
        }

        [Fact]
        public void PostAsJsonAsync_Uri_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsJsonAsync(new Uri("http://www.example.com"), new object()), "client");
        }

        [Fact]
        public void PostAsJsonAsync_Uri_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsJsonAsync((Uri)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PostAsJsonAsync_Uri_UsesJsonMediaTypeFormatter()
        {
            var response = await _client.PostAsJsonAsync(new Uri("http://example.com"), new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<JsonMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PostAsXmlAsync_Uri_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsXmlAsync(new Uri("http://www.example.com"), new object()), "client");
        }

#if !Testing_NetStandard1_3 // Avoid "The configured formatter 'System.Net.Http.Formatting.XmlMediaTypeFormatter' cannot write an object of type 'Object'."
        [Fact]
        public void PostAsXmlAsync_Uri_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsXmlAsync((Uri)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PostAsXmlAsync_Uri_UsesXmlMediaTypeFormatter()
        {
            var response = await _client.PostAsXmlAsync(new Uri("http://example.com"), new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<XmlMediaTypeFormatter>(content.Formatter);
        }
#endif

        [Fact]
        public void PostAsync_Uri_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PostAsync(new Uri("http://www.example.com"), new object(), new JsonMediaTypeFormatter(), "text/json"), "client");
        }

        [Fact]
        public void PostAsync_Uri_WhenRequestUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PostAsync((Uri)null, new object(), new JsonMediaTypeFormatter(), "text/json"),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PostAsync_Uri_WhenRequestUriIsSet_CreatesRequestWithAppropriateUri()
        {
            _client.BaseAddress = new Uri("http://example.com/");

            var response = await _client.PostAsync(new Uri("myapi/", UriKind.Relative), new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Equal("http://example.com/myapi/", request.RequestUri.ToString());
        }

        [Fact]
        public async Task PostAsync_Uri_WhenAuthoritativeMediaTypeIsSet_CreatesRequestWithAppropriateContentType()
        {
            var response = await _client.PostAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-16", request.Content.Headers.ContentType.CharSet);
        }

        [Fact]
        public async Task PostAsync_Uri_WhenFormatterIsSet_CreatesRequestWithObjectContentAndCorrectFormatter()
        {
            var response = await _client.PostAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            var content = Assert.IsType<ObjectContent<object>>(request.Content);
            Assert.Same(_formatter, content.Formatter);
        }

        [Fact]
        public async Task PostAsync_Uri_WhenAuthoritativeMediaTypeStringIsSet_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PostAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, mediaType, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PostAsync_Uri_WhenAuthoritativeMediaTypeStringIsSetWithoutCT_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PostAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, mediaType);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PostAsync_Uri_IssuesPostRequest()
        {
            var response = await _client.PostAsync(new Uri("http://example.com/myapi/"), new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Same(HttpMethod.Post, request.Method);
        }

        [Fact]
        public void PostAsync_Uri_WhenMediaTypeFormatterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _client.PostAsync(new Uri("http://example.com"), new object(), formatter: null), "formatter");
        }

        [Fact]
        public void PutAsJsonAsync_Uri_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsJsonAsync(new Uri("http://www.example.com"), new object()), "client");
        }

        [Fact]
        public void PutAsJsonAsync_Uri_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsJsonAsync((Uri)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PutAsJsonAsync_Uri_UsesJsonMediaTypeFormatter()
        {
            var response = await _client.PutAsJsonAsync(new Uri("http://example.com"), new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<JsonMediaTypeFormatter>(content.Formatter);
        }

        [Fact]
        public void PutAsXmlAsync_Uri_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsXmlAsync(new Uri("http://www.example.com"), new object()), "client");
        }

#if !Testing_NetStandard1_3 // Avoid "The configured formatter 'System.Net.Http.Formatting.XmlMediaTypeFormatter' cannot write an object of type 'Object'."
        [Fact]
        public void PutAsXmlAsync_Uri_WhenUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsXmlAsync((Uri)null, new object()),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PutAsXmlAsync_Uri_UsesXmlMediaTypeFormatter()
        {
            var response = await _client.PutAsXmlAsync(new Uri("http://example.com"), new object());

            var content = Assert.IsType<ObjectContent<object>>(response.RequestMessage.Content);
            Assert.IsType<XmlMediaTypeFormatter>(content.Formatter);
        }
#endif

        [Fact]
        public void PutAsync_Uri_WhenClientIsNull_ThrowsException()
        {
            HttpClient client = null;

            Assert.ThrowsArgumentNull(() => client.PutAsync(new Uri("http://www.example.com"), new object(), new JsonMediaTypeFormatter(), "text/json"), "client");
        }

        [Fact]
        public void PutAsync_Uri_WhenRequestUriIsNull_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _client.PutAsync((Uri)null, new object(), new JsonMediaTypeFormatter(), "text/json"),
                InvalidUriMessage);
        }

        [Fact]
        public async Task PutAsync_Uri_WhenRequestUriIsSet_CreatesRequestWithAppropriateUri()
        {
            _client.BaseAddress = new Uri("http://example.com/");

            var response = await _client.PutAsync(new Uri("myapi/", UriKind.Relative), new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Equal("http://example.com/myapi/", request.RequestUri.ToString());
        }

        [Fact]
        public async Task PutAsync_Uri_WhenAuthoritativeMediaTypeIsSet_CreatesRequestWithAppropriateContentType()
        {
            var response = await _client.PutAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-16", request.Content.Headers.ContentType.CharSet);
        }

        [Fact]
        public async Task PutAsync_Uri_WhenFormatterIsSet_CreatesRequestWithObjectContentAndCorrectFormatter()
        {
            var response = await _client.PutAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, _mediaTypeHeader, CancellationToken.None);

            var request = response.RequestMessage;
            var content = Assert.IsType<ObjectContent<object>>(request.Content);
            Assert.Same(_formatter, content.Formatter);
        }

        [Fact]
        public async Task PutAsync_Uri_WhenAuthoritativeMediaTypeStringIsSet_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PutAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, mediaType, CancellationToken.None);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PutAsync_Uri_WhenAuthoritativeMediaTypeStringIsSetWithoutCT_CreatesRequestWithAppropriateContentType()
        {
            string mediaType = _mediaTypeHeader.MediaType;
            var response = await _client.PutAsync(new Uri("http://example.com/myapi/"), new object(), _formatter, mediaType);

            var request = response.RequestMessage;
            Assert.Equal("foo/bar", request.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PutAsync_Uri_IssuesPutRequest()
        {
            var response = await _client.PutAsync(new Uri("http://example.com/myapi/"), new object(), _formatter);

            var request = response.RequestMessage;
            Assert.Same(HttpMethod.Put, request.Method);
        }

        [Fact]
        public void PutAsync_Uri_WhenMediaTypeFormatterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _client.PutAsync(new Uri("http://example.com"), new object(), formatter: null), "formatter");
        }
    }
}

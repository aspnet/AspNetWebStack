// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.WebHost
{
    public class HttpControllerHandlerTest
    {
        public static TheoryDataSet<HttpMethod> AllHttpMethods
        {
            get
            {
                return new TheoryDataSet<HttpMethod>
                {
                    HttpMethod.Get,
                    HttpMethod.Post,
                    HttpMethod.Put,
                    HttpMethod.Delete,
                    HttpMethod.Head,
                    HttpMethod.Options,
                    HttpMethod.Trace
                };
            }
        }

        public static TheoryDataSet<HttpMethod> HttpMethodsWithContent
        {
            get
            {
                return new TheoryDataSet<HttpMethod>
                {
                    HttpMethod.Post,
                    HttpMethod.Put,
                    HttpMethod.Delete,
                };
            }
        }

        [Theory]
        [PropertyData("AllHttpMethods")]
        public void ConvertRequest_Creates_HttpRequestMessage_For_All_HttpMethods(HttpMethod httpMethod)
        {
            // Arrange
            HttpContextBase contextBase = CreateStubContextBase(httpMethod.Method, new MemoryStream());

            // Act
            HttpRequestMessage request = HttpControllerHandler.ConvertRequest(contextBase);

            // Assert
            Assert.Equal(httpMethod, request.Method);
        }

        [Fact]
        public void ConvertRequest_Copies_Headers_And_Content_Headers()
        {
            // Arrange
            HttpContextBase contextBase = CreateStubContextBase("Get", new MemoryStream());
            HttpRequestBase requestBase = contextBase.Request;
            NameValueCollection nameValues = requestBase.Headers;
            nameValues["myHeader"] = "myValue";
            nameValues["Content-Type"] = "application/mine";

            // Act
            HttpRequestMessage request = HttpControllerHandler.ConvertRequest(contextBase);
            string[] headerValues = request.Headers.GetValues("myHeader").ToArray();

            // Assert
            Assert.Equal("myValue", headerValues[0]);
            Assert.Equal("application/mine", request.Content.Headers.ContentType.MediaType);
        }

        [Theory]
        [PropertyData("HttpMethodsWithContent")]
        public void ConvertRequest_Creates_Request_With_Content_For_Content_Methods(HttpMethod httpMethod)
        {
            // Arrange
            HttpContextBase contextBase = CreateStubContextBase(httpMethod.Method, new MemoryStream());

            // Act
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(contextBase);

            // Assert
            Assert.NotNull(actualRequest.Content);
        }

        [Fact]
        public async Task ConvertRequest_Uses_HostBufferPolicySelector_To_Select_Buffered_Stream()
        {
            // Arrange
            HttpContextBase contextMock = CreateStubContextBase("Post", new MemoryStream(new byte[] { 5 }));
            MemoryStream memoryStream = new MemoryStream();

            // Act
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(contextMock);
            await actualRequest.Content.CopyToAsync(memoryStream);
            byte[] actualBuffer = memoryStream.GetBuffer();

            // Assert
            Assert.Equal(5, actualBuffer[0]);
        }

        [Fact]
        public void ConvertRequest_AddsOwinEnvironment_WhenPresentInHttpContext()
        {
            // Arrange
            using (MemoryStream ignoreStream = new MemoryStream())
            {
                HttpRequestBase stubRequest = CreateStubRequestBase("IgnoreMethod", ignoreStream);
                IDictionary<string, object> expectedEnvironment = new Dictionary<string, object>();
                IDictionary items = new Hashtable();
                items.Add(HttpControllerHandler.OwinEnvironmentHttpContextKey, expectedEnvironment);
                HttpContextBase context = CreateStubContextBase(stubRequest, items);

                // Act
                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    IDictionary<string, object> environment = actualRequest.GetOwinEnvironment();

                    // Assert
                    Assert.Same(expectedEnvironment, environment);
                }
            }
        }

        [Fact]
        public void ConvertRequest_DoesNotAddOwinEnvironment_WhenNotPresentInHttpContext()
        {
            // Arrange
            using (MemoryStream ignoreStream = new MemoryStream())
            {
                HttpRequestBase stubRequest = CreateStubRequestBase("IgnoreMethod", ignoreStream);
                IDictionary items = new Hashtable();
                HttpContextBase context = CreateStubContextBase(stubRequest, items);

                // Act
                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Assert
                    object ignore;
                    bool found = actualRequest.Properties.TryGetValue(HttpControllerHandler.OwinEnvironmentKey,
                        out ignore);
                    Assert.False(found);
                }
            }
        }

        [Fact]
        public void ConvertRequest_DoesNotAddOwinEnvironment_WhenItemsIsNull()
        {
            // Arrange
            using (MemoryStream ignoreStream = new MemoryStream())
            {
                HttpRequestBase stubRequest = CreateStubRequestBase("IgnoreMethod", ignoreStream);
                IDictionary items = null;
                HttpContextBase context = CreateStubContextBase(stubRequest, items);

                // Act
                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Assert
                    object ignore;
                    bool found = actualRequest.Properties.TryGetValue(HttpControllerHandler.OwinEnvironmentKey,
                        out ignore);
                    Assert.False(found);
                }
            }
        }

        [Fact]
        public async Task ConvertRequest_DoesLazyGetInputStream()
        {
            bool inputStreamCalled = false;
            HttpRequestBase stubRequest = CreateFakeRequestBase(() =>
            {
                inputStreamCalled = true;
                return new MemoryStream();
            }, buffered: true);
            HttpContextBase context = CreateStubContextBase(request: stubRequest, items: null);

            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context);

            Assert.False(inputStreamCalled);
            var contentStream = await actualRequest.Content.ReadAsStreamAsync();
            Assert.True(inputStreamCalled);
        }

        [Fact]
        public async Task ConvertRequest_UsesRequestInputStream_InClassicMode()
        {
            // Arrange
            string input = "Hello world";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            HttpRequestBase fakeRequest = CreateFakeRequestBase(() => stream, buffered: true);
            HttpContextBase context = CreateStubContextBase(request: fakeRequest, items: null);
            Mock<HttpRequestBase> mockRequest = Mock.Get<HttpRequestBase>(fakeRequest);

            // Act
            fakeRequest.InputStream.Position = 10;
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context);
            string result = await actualRequest.Content.ReadAsStringAsync();

            // Assert
            // Verify that the InputStream was reset when reading the content.
            Assert.Equal(input, result);
            mockRequest.Verify(r => r.InputStream, Times.AtLeastOnce());
            mockRequest.Verify(r => r.GetBufferedInputStream(), Times.Never());
        }

        [Fact]
        public async Task ConvertRequest_UsesBufferedInputStream_IfReadEntityBodyModeIsNone()
        {
            // Arrange
            HttpRequestBase stubRequest = CreateFakeRequestBase(() => new MemoryStream(), buffered: true);
            HttpContextBase context = CreateStubContextBase(request: stubRequest, items: null);
            Mock<HttpRequestBase> mockRequest = Mock.Get<HttpRequestBase>(stubRequest);

            // Act
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context);
            await actualRequest.Content.ReadAsStreamAsync();

            // Assert
            mockRequest.Verify(r => r.InputStream, Times.Never());
            mockRequest.Verify(r => r.GetBufferedInputStream(), Times.AtLeastOnce());
        }

        [Fact]
        public Task ConvertRequest_WithBufferedPolicy_ThrowsIfRequestHasBeenPartiallyRead()
        {
            // Arrange
            var stream = new MemoryStream(new byte[16]);
            HttpRequestBase fakeRequest = CreateFakeRequestBase(() => stream, buffered: true);
            HttpContextBase context = CreateStubContextBase(request: fakeRequest, items: null);

            // Act
            fakeRequest.GetBufferedInputStream().Position = 4;
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => actualRequest.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ConvertRequest_WithBufferedPolicy_ReturnsInputStreamIfBufferedStreamWasFullyRead()
        {
            // Arrange
            string inputStreamMessage = "This is from input stream";
            var bufferedStream = new MemoryStream(new byte[16]);
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(inputStreamMessage));
            HttpRequestBase fakeRequest = CreateFakeRequestBase(() => bufferedStream, buffered: true);
            HttpContextBase context = CreateStubContextBase(request: fakeRequest, items: null);
            Mock<HttpRequestBase> mockRequest = Mock.Get<HttpRequestBase>(fakeRequest);
            mockRequest.SetupGet(f => f.InputStream)
                       .Returns(inputStream)
                       .Verifiable();

            // Act
            bufferedStream.Seek(0, SeekOrigin.End);
            new StreamReader(fakeRequest.GetBufferedInputStream()).ReadToEnd();
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context);
            string result = await actualRequest.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(inputStreamMessage, result);
        }

        [Fact]
        public async Task ConvertRequest_DoesLazyGetBufferlessInputStream_IfRequestStreamHasNotBeenRead()
        {
            // Arrange
            bool inputStreamCalled = false;
            var hostBufferPolicy = new Mock<IHostBufferPolicySelector>();
            hostBufferPolicy.Setup(c => c.UseBufferedInputStream(It.IsAny<object>()))
                                          .Returns(false);
            hostBufferPolicy.Setup(c => c.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>()))
                                          .Returns(true);

            HttpRequestBase stubRequest = CreateFakeRequestBase(() =>
                {
                    inputStreamCalled = true;
                    return new MemoryStream();
                }, buffered: false);
            HttpContextBase context = HttpControllerHandlerTest.CreateStubContextBase(request: stubRequest, items: null);

            // Act
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context, hostBufferPolicy.Object);

            // Assert
            Assert.False(inputStreamCalled);
            Stream contentStream = await actualRequest.Content.ReadAsStreamAsync();
            Assert.True(inputStreamCalled);
        }

        [Fact]
        public Task ConvertRequest_WithBufferlessPolicy_ThrowsIfRequestStreamHasBeenReadInClassicMode()
        {
            // Arrange
            var hostBufferPolicy = new Mock<IHostBufferPolicySelector>();
            hostBufferPolicy.Setup(c => c.UseBufferedInputStream(It.IsAny<object>()))
                                          .Returns(false);
            hostBufferPolicy.Setup(c => c.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>()))
                                          .Returns(true);

            var stream = new MemoryStream(8);
            HttpRequestBase stubRequest = CreateFakeRequestBase(() => stream, buffered: false);
            HttpContextBase context = HttpControllerHandlerTest.CreateStubContextBase(request: stubRequest, items: null);

            // Act
            context.Request.InputStream.Position = 2;
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context, hostBufferPolicy.Object);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => actualRequest.Content.ReadAsStreamAsync(),
                 "Unable to read the entity body in Bufferless mode. The request stream has already been buffered.");
        }

        [Fact]
        public Task ConvertRequest_WithBufferlessPolicy_ThrowsIfRequestStreamHasBeenReadInBufferedMode()
        {
            // Arrange
            var hostBufferPolicy = new Mock<IHostBufferPolicySelector>();
            hostBufferPolicy.Setup(c => c.UseBufferedInputStream(It.IsAny<object>()))
                                          .Returns(false);
            hostBufferPolicy.Setup(c => c.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>()))
                                          .Returns(true);

            var stream = new MemoryStream(8);
            HttpRequestBase stubRequest = CreateFakeRequestBase(() => stream, buffered: false);
            HttpContextBase context = HttpControllerHandlerTest.CreateStubContextBase(request: stubRequest, items: null);

            // Act
            context.Request.GetBufferedInputStream();
            HttpRequestMessage message = HttpControllerHandler.ConvertRequest(context, hostBufferPolicy.Object);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => message.Content.ReadAsStringAsync(),
                 "Unable to read the entity body. The request stream has already been read in 'Buffered' mode.");
        }

        [Fact]
        public Task ConvertRequest_WithBufferlessPolicy_ThrowsIfRequestStreamHasBeenRead()
        {
            // Arrange
            var hostBufferPolicy = new Mock<IHostBufferPolicySelector>();
            hostBufferPolicy.Setup(c => c.UseBufferedInputStream(It.IsAny<object>()))
                                          .Returns(false);
            hostBufferPolicy.Setup(c => c.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>()))
                                          .Returns(true);

            var stream = new MemoryStream(new byte[16]);
            HttpRequestBase stubRequest = CreateFakeRequestBase(() => stream, buffered: false);
            HttpContextBase context = HttpControllerHandlerTest.CreateStubContextBase(request: stubRequest, items: null);

            // Act
            context.Request.GetBufferlessInputStream().Position = 4;
            HttpRequestMessage message = HttpControllerHandler.ConvertRequest(context, hostBufferPolicy.Object);

            // Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => message.Content.ReadAsStringAsync(),
                 "Unable to read the entity body. A portion of the request stream has already been read.");
        }

        [Fact]
        public void ConvertRequest_AddsWebHostHttpRequestContext()
        {
            // Arrange
            Mock<HttpRequestBase> requestBaseMock = new Mock<HttpRequestBase>(MockBehavior.Strict);
            requestBaseMock.Setup(r => r.HttpMethod).Returns("IGNORED");
            requestBaseMock.Setup(r => r.Url).Returns(new Uri("http://ignore"));
            requestBaseMock.Setup(r => r.Headers).Returns(new NameValueCollection());
            requestBaseMock.Setup(r => r.ReadEntityBodyMode).Returns(ReadEntityBodyMode.None);
            HttpRequestBase requestBase = requestBaseMock.Object;
            Mock<HttpContextBase> contextBaseMock = new Mock<HttpContextBase>(MockBehavior.Strict);
            contextBaseMock.Setup(c => c.Request).Returns(requestBase);
            contextBaseMock.Setup(c => c.Items).Returns((IDictionary)null);
            HttpContextBase contextBase = contextBaseMock.Object;

            // Act
            using (HttpRequestMessage expectedRequest = HttpControllerHandler.ConvertRequest(contextBase))
            {
                // Assert
                HttpRequestContext context = expectedRequest.GetRequestContext();
                WebHostHttpRequestContext typedContext = Assert.IsType<WebHostHttpRequestContext>(context);
                Assert.Same(contextBase, typedContext.Context);
                Assert.Same(requestBase, typedContext.WebRequest);
                Assert.Same(expectedRequest, typedContext.Request);
            }
        }

        [Fact]
        public async Task CopyResponseAsync_IfResponseHasNoCacheControlDefined_SetsNoCacheCacheabilityOnAspNetResponse()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();

            // Act
            await CopyResponseAsync(contextMock.Object, request, response);

            // Assert
            contextMock.Verify(c => c.Response.Cache.SetCacheability(HttpCacheability.NoCache));
        }

        [Fact]
        public async Task CopyResponseAsync_IfResponseHasCacheControlDefined_DoesNotSetCacheCacheabilityOnAspNetResponse()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { Public = true };

            // Act
            await CopyResponseAsync(contextMock.Object, request, response);

            // Assert
            contextMock.Verify(c => c.Response.Cache.SetCacheability(HttpCacheability.NoCache), Times.Never());
        }

        [Fact]
        public async Task ProcessRequestAsync_DisposesRequestAndResponse()
        {
            // Arrange
            Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>() { DefaultValue = DefaultValue.Mock };
            responseMock.SetupGet(r => r.OutputStream).Returns(Stream.Null);
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(hcb => hcb.Response).Returns(responseMock.Object);
            IDictionary items = new Dictionary<object, object>();
            contextMock.SetupGet((hcb) => hcb.Items).Returns(items);
            HttpContextBase context = contextMock.Object;

            HttpRequestMessage request = new HttpRequestMessage();
            context.SetHttpRequestMessage(request);
            SpyDisposable spy = new SpyDisposable();
            request.RegisterForDispose(spy);
            HttpResponseMessage response = new HttpResponseMessage();

            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, c) =>
                Task.FromResult(response);

            using (HttpMessageHandler handler = new LambdaHttpMessageHandler(sendAsync))
            {
                HttpControllerHandler product = new HttpControllerHandler(
                    new Mock<RouteData>(MockBehavior.Strict).Object, handler);

                // Act
                await product.ProcessRequestAsyncCore(context);

                // Assert
                Assert.True(spy.Disposed);
                Assert.ThrowsObjectDisposed(() => request.Method = HttpMethod.Get, typeof(HttpRequestMessage).FullName);
                Assert.ThrowsObjectDisposed(() => response.StatusCode = HttpStatusCode.OK, typeof(HttpResponseMessage).FullName);
            }
        }

        [Fact]
        public async Task ProcessRequestAsync_DisposesRequestAndResponseWithContent()
        {
            // Arrange
            Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>() { DefaultValue = DefaultValue.Mock };
            responseMock.SetupGet(r => r.OutputStream).Returns(Stream.Null);
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(hcb => hcb.Response).Returns(responseMock.Object);
            IDictionary items = new Dictionary<object, object>();
            contextMock.SetupGet((hcb) => hcb.Items).Returns(items);
            HttpContextBase context = contextMock.Object;

            HttpRequestMessage request = new HttpRequestMessage() { Content = new StringContent("request") };
            context.SetHttpRequestMessage(request);
            SpyDisposable spy = new SpyDisposable();
            request.RegisterForDispose(spy);
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent("response") };

            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, c) =>
                Task.FromResult(response);

            using (HttpMessageHandler handler = new LambdaHttpMessageHandler(sendAsync))
            {
                HttpControllerHandler product = new HttpControllerHandler(
                    new Mock<RouteData>(MockBehavior.Strict).Object, handler);

                // Act
                await product.ProcessRequestAsyncCore(context);

                // Assert
                Assert.True(spy.Disposed);
                Assert.ThrowsObjectDisposed(() => request.Method = HttpMethod.Get, typeof(HttpRequestMessage).FullName);
                Assert.ThrowsObjectDisposed(() => response.StatusCode = HttpStatusCode.OK, typeof(HttpResponseMessage).FullName);
            }
        }

        [Fact]
        public async Task ProcessRequestAsync_IfHandlerFaults_DisposesRequest()
        {
            // Arrange
            Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(r => r.OutputStream).Returns(Stream.Null);
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(hcb => hcb.Response).Returns(responseMock.Object);
            IDictionary items = new Dictionary<object, object>();
            contextMock.SetupGet((hcb) => hcb.Items).Returns(items);
            HttpContextBase context = contextMock.Object;

            HttpRequestMessage request = new HttpRequestMessage();
            context.SetHttpRequestMessage(request);
            SpyDisposable spy = new SpyDisposable();
            request.RegisterForDispose(spy);

            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, c) =>
                CreateFaultedTask<HttpResponseMessage>(CreateException());

            using (HttpMessageHandler handler = new LambdaHttpMessageHandler(sendAsync))
            {
                HttpControllerHandler product = new HttpControllerHandler(
                    new Mock<RouteData>(MockBehavior.Strict).Object, handler);

                // Act & Assert
                await Assert.ThrowsAsync<EncoderFallbackException>(() => product.ProcessRequestAsyncCore(context));

                Assert.True(spy.Disposed);
                Assert.ThrowsObjectDisposed(() => request.Method = HttpMethod.Get, typeof(HttpRequestMessage).FullName);
            }
        }

        [Fact]
        public void SuppressFormsAuthenticationRedirect_DoesntRequireSuppressRedirect()
        {
            // Arrange
            Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(r => r.StatusCode).Returns(200);
            responseMock.SetupSet(r => r.SuppressFormsAuthenticationRedirect = It.IsAny<bool>()).Verifiable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            IDictionary contextItems = new Hashtable();
            contextMock.SetupGet(hcb => hcb.Response).Returns(responseMock.Object);
            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);

            // Act
            HttpControllerHandler.EnsureSuppressFormsAuthenticationRedirect(contextMock.Object);

            // Assert
            responseMock.VerifySet(r => r.SuppressFormsAuthenticationRedirect = It.IsAny<bool>(), Times.Never);
        }

        [Fact]
        public void SuppressFormsAuthenticationRedirect_RequireSuppressRedirect()
        {
            // Arrange
            bool suppressFormsAuthenticationRedirect = false;
            Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>();
            responseMock.SetupGet(r => r.StatusCode).Returns(401);
            responseMock.SetupSet<bool>(r => r.SuppressFormsAuthenticationRedirect = It.IsAny<bool>())
                .Callback(value => suppressFormsAuthenticationRedirect = value)
                .Verifiable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            IDictionary contextItems = new Hashtable();
            contextMock.SetupGet(hcb => hcb.Response).Returns(responseMock.Object);
            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);

            // Act
            HttpControllerHandler.EnsureSuppressFormsAuthenticationRedirect(contextMock.Object);

            // Assert
            responseMock.VerifySet(r => r.SuppressFormsAuthenticationRedirect = It.IsAny<bool>(), Times.Once);
            Assert.True(suppressFormsAuthenticationRedirect);
        }

        [Fact]
        public async Task CopyResponseAsync_Creates_Correct_HttpResponseBase()
        {
            // Arrange
            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", new JsonMediaTypeFormatter());

            // Act
            await CopyResponseAsync(contextMock.Object, request, response);

            // Assert preparation -- deserialize the response
            memoryStream.Seek(0L, SeekOrigin.Begin);
            string responseString = null;
            using (var streamReader = new StreamReader(memoryStream))
            {
                responseString = streamReader.ReadToEnd();
            }

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, responseBase.StatusCode);
            Assert.StartsWith(JsonMediaTypeFormatter.DefaultMediaType.MediaType, responseBase.Headers["Content-Type"]);
            Assert.Equal("\"hello\"", responseString);
        }

        [Fact]
        public async Task CopyResponseAsync_IfTransferEncodingChunkedAndContentLengthAreBothSet_IgnoresContentLength()
        {
            // Arrange
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(Stream.Null).Object;
            HttpContextBase contextBase = CreateStubContextBase(responseBase);

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request })
            {
                response.Headers.TransferEncodingChunked = true;
                response.Content = new StringContent("SomeContent");
                Assert.NotNull(response.Content.Headers.ContentLength); // Guard; added by System.Net.Http.

                // Act
                await CopyResponseAsync(contextBase, request, response);

                // Assert
                Assert.DoesNotContain("Content-Length", responseBase.Headers.OfType<string>());
            }
        }

        [Fact]
        public async Task CopyResponseAsync_IfTransferEncodingIsJustChunked_DoesNotCopyHeaderToHost()
        {
            // Arrange
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(Stream.Null).Object;
            HttpContextBase contextBase = CreateStubContextBase(responseBase);

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request })
            {
                response.Headers.TransferEncodingChunked = true;

                // Act
                await CopyResponseAsync(contextBase, request, response);

                // Assert
                Assert.DoesNotContain("Transfer-Encoding", responseBase.Headers.OfType<string>());
            }
        }

        [Fact]
        public async Task CopyResponseAsync_IfTransferEncodingIsIdentity_CopiesHeaderToHost()
        {
            // Arrange
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(Stream.Null).Object;
            HttpContextBase contextBase = CreateStubContextBase(responseBase);

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request })
            {
                response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("identity"));

                // Act
                await CopyResponseAsync(contextBase, request, response);

                // Assert
                Assert.Contains("Transfer-Encoding", responseBase.Headers.OfType<string>());
                Assert.Equal(new string[] { "identity" }, responseBase.Headers.GetValues("Transfer-Encoding"));
            }
        }

        [Fact]
        public async Task CopyResponseAsync_IfTransferEncodingIsIdentityChunked_CopiesHeaderToHost()
        {
            // Arrange
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(Stream.Null).Object;
            HttpContextBase contextBase = CreateStubContextBase(responseBase);

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request })
            {
                response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("identity"));
                response.Headers.TransferEncodingChunked = true;
                Assert.Equal("identity, chunked", response.Headers.TransferEncoding.ToString()); // Guard

                // Act
                await CopyResponseAsync(contextBase, request, response);

                // Assert
                Assert.Contains("Transfer-Encoding", responseBase.Headers.OfType<string>());
                Assert.Equal(new string[] { "identity", "chunked" },
                    responseBase.Headers.GetValues("Transfer-Encoding"));
            }
        }

        [Fact]
        public async Task CopyResponseAsync_IfTransferEncodingIsChunked_DisablesResponseBuffering()
        {
            // Arrange
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(Stream.Null).Object;
            HttpContextBase contextBase = CreateStubContextBase(responseBase);

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request })
            {
                response.Headers.TransferEncodingChunked = true;
                response.Content = new ObjectContent(typeof(string), String.Empty, new JsonMediaTypeFormatter());

                // Act
                await CopyResponseAsync(contextBase, request, response);

                // Assert
                Assert.False(responseBase.BufferOutput);
            }
        }

        [Fact]
        public async Task CopyResponseAsync_IfHandlerIsDefault_Returns_Error_Response_When_Formatter_Write_Task_Faults()
        {
            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            try
            {
                // to capture stack trace inside this method
                throw new NotSupportedException("Expected error");
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                          It.IsAny<object>(),
                                                          It.IsAny<Stream>(),
                                                          It.IsAny<HttpContent>(),
                                                          It.IsAny<TransportContext>())).Returns(tcs.Task);

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(GlobalConfiguration.Configuration);

            // Act
            await HttpControllerHandler.CopyResponseAsync(contextMock.Object, request, response, exceptionLogger,
                exceptionHandler, CancellationToken.None);

            // Assert preparation -- deserialize the HttpError response
            HttpError httpError = null;
            memoryStream.Seek(0L, SeekOrigin.Begin);
            using (StreamContent content = new StreamContent(memoryStream))
            {
                content.Headers.ContentType = JsonMediaTypeFormatter.DefaultMediaType;
                httpError = await content.ReadAsAsync<HttpError>();
            }

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.StartsWith(JsonMediaTypeFormatter.DefaultMediaType.MediaType, responseBase.Headers["Content-Type"]);
            Assert.Equal("An error has occurred.", httpError["Message"]);
            Assert.Equal("The 'ObjectContent`1' type failed to serialize the response body for content type 'application/json; charset=utf-8'.", httpError["ExceptionMessage"]);
            Assert.Equal(typeof(InvalidOperationException).FullName, httpError["ExceptionType"]);
            Assert.True(httpError.ContainsKey("StackTrace"));

            HttpError innerError = (httpError["InnerException"] as JObject).ToObject<HttpError>();
            Assert.NotNull(innerError);
            Assert.Equal(typeof(NotSupportedException).FullName, innerError["ExceptionType"].ToString());
            Assert.Equal("Expected error", innerError["ExceptionMessage"]);
            Assert.Contains(MethodInfo.GetCurrentMethod().Name, innerError["StackTrace"].ToString());
        }

        [Fact]
        public async Task CopyResponseAsync_IfHandlerIsDefault_Returns_Error_Response_When_Formatter_Write_Throws_Immediately()
        {
            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                          It.IsAny<object>(),
                                                          It.IsAny<Stream>(),
                                                          It.IsAny<HttpContent>(),
                                                          It.IsAny<TransportContext>())).Throws(new NotSupportedException("Expected error"));

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(GlobalConfiguration.Configuration);

            // Act
            await HttpControllerHandler.CopyResponseAsync(contextMock.Object, request, response, exceptionLogger, exceptionHandler, CancellationToken.None);

            // Assert preparation -- deserialize the HttpError response
            HttpError httpError = null;
            memoryStream.Seek(0L, SeekOrigin.Begin);
            using (StreamContent content = new StreamContent(memoryStream))
            {
                content.Headers.ContentType = JsonMediaTypeFormatter.DefaultMediaType;
                httpError = await content.ReadAsAsync<HttpError>();
            }

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.StartsWith(JsonMediaTypeFormatter.DefaultMediaType.MediaType, responseBase.Headers["Content-Type"]);
            Assert.Equal("An error has occurred.", httpError["Message"]);
            Assert.Equal("The 'ObjectContent`1' type failed to serialize the response body for content type 'application/json; charset=utf-8'.", httpError["ExceptionMessage"]);
            Assert.Equal(typeof(InvalidOperationException).FullName, httpError["ExceptionType"]);
            Assert.True(httpError.ContainsKey("StackTrace"));

            HttpError innerError = (httpError["InnerException"] as JObject).ToObject<HttpError>();
            Assert.NotNull(innerError);
            Assert.Equal(typeof(NotSupportedException).FullName, innerError["ExceptionType"].ToString());
            Assert.Equal("Expected error", innerError["ExceptionMessage"]);
            Assert.Contains("System.Net.Http.HttpContent.CopyToAsync", innerError["StackTrace"].ToString());
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_User_Response_When_Formatter_Write_Throws_HttpResponseException_With_No_Content()
        {
            // Arrange
            HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            errorResponse.Headers.Add("myHeader", "myValue");

            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                          It.IsAny<object>(),
                                                          It.IsAny<Stream>(),
                                                          It.IsAny<HttpContent>(),
                                                          It.IsAny<TransportContext>())).Throws(new HttpResponseException(errorResponse));

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);

            // Act
            await CopyResponseAsync(contextMock.Object, request, response);
            memoryStream.Seek(0L, SeekOrigin.Begin);

            // Assert
            Assert.Equal((int)errorResponse.StatusCode, responseBase.StatusCode);
            Assert.Equal(0, memoryStream.Length);
            Assert.Equal("myValue", responseBase.Headers["myHeader"]);
            Assert.Null(responseBase.Headers["Content-Type"]);
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_User_Response_When_Formatter_Write_Throws_HttpResponseException_With_Content()
        {
            // Arrange
            HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            errorResponse.Headers.Add("myHeader", "myValue");
            errorResponse.Content = new StringContent("user message", Encoding.UTF8, "application/fake");

            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                          It.IsAny<object>(),
                                                          It.IsAny<Stream>(),
                                                          It.IsAny<HttpContent>(),
                                                          It.IsAny<TransportContext>())).Throws(new HttpResponseException(errorResponse));

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);

            // Act
            await CopyResponseAsync(contextMock.Object, request, response);

            // Assert preparation -- deserialize the response

            memoryStream.Seek(0L, SeekOrigin.Begin);
            string responseContent = null;
            using (var streamReader = new StreamReader(memoryStream))
            {
                responseContent = streamReader.ReadToEnd();
            }

            // Assert
            Assert.Equal((int)errorResponse.StatusCode, responseBase.StatusCode);
            Assert.StartsWith("application/fake", responseBase.Headers["Content-Type"]);
            Assert.Equal("user message", responseContent);
            Assert.Equal("myValue", responseBase.Headers["myHeader"]);
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_InternalServerError_And_No_Content_When_Formatter_Write_Task_Faults_During_Error_Response()
        {
            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetException(new NotSupportedException("Expected error"));

            // This formatter throws on any write attempt
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                            It.IsAny<object>(),
                                                            It.IsAny<Stream>(),
                                                            It.IsAny<HttpContent>(),
                                                            It.IsAny<TransportContext>())).Returns(tcs.Task);

            // Create a local config to hook to the request to condition
            // the formatter selection for the error response
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(formatterMock.Object);

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            request.SetConfiguration(config);
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(GlobalConfiguration.Configuration);

            // Act
            await HttpControllerHandler.CopyResponseAsync(contextMock.Object, request, response, exceptionLogger,
                exceptionHandler, CancellationToken.None);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.Equal(0, memoryStream.Length);
            Assert.Null(responseBase.Headers["Content-Type"]);
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_InternalServerError_And_No_Content_When_Formatter_Write_Throws_Immediately_During_Error_Response()
        {
            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };

            // This formatter throws on any write attempt
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                            It.IsAny<object>(),
                                                            It.IsAny<Stream>(),
                                                            It.IsAny<HttpContent>(),
                                                            It.IsAny<TransportContext>())).Throws(new NotSupportedException("Expected error"));

            // Create a local config to hook to the request to condition
            // the formatter selection for the error response
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(formatterMock.Object);

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            request.SetConfiguration(config);
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(GlobalConfiguration.Configuration);

            // Act
            await HttpControllerHandler.CopyResponseAsync(contextMock.Object, request, response, exceptionLogger,
                exceptionHandler, CancellationToken.None);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.Equal(0, memoryStream.Length);
            Assert.Null(responseBase.Headers["Content-Type"]);
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_InternalServerError_And_No_Content_When_Content_Negotiation_Cannot_Find_Formatter_For_Error_Response()
        {
            // Create a content negotiator that works attempting a normal response but fails when creating the error response.
            Mock<IContentNegotiator> negotiatorMock = new Mock<IContentNegotiator>() { CallBase = true };
            negotiatorMock.Setup(m => m.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(), It.IsAny<IEnumerable<MediaTypeFormatter>>()))
                .Returns((Type t, HttpRequestMessage r, IEnumerable<MediaTypeFormatter> f) =>
                    {
                        ContentNegotiationResult result = t == typeof(HttpError)
                            ? null
                            : new ContentNegotiationResult(f.First(), JsonMediaTypeFormatter.DefaultMediaType);
                        return result;
                    });

            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };

            // This formatter throws on any write attempt
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                            It.IsAny<object>(),
                                                            It.IsAny<Stream>(),
                                                            It.IsAny<HttpContent>(),
                                                            It.IsAny<TransportContext>())).Throws(new NotSupportedException("Expected error"));

            // Create a local config to hook to the request to condition
            // the formatter selection for the error response
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(formatterMock.Object);
            config.Services.Replace(typeof(IContentNegotiator), negotiatorMock.Object);

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            request.SetConfiguration(config);
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(GlobalConfiguration.Configuration);

            // Act
            await HttpControllerHandler.CopyResponseAsync(contextMock.Object, request, response, exceptionLogger,
                exceptionHandler, CancellationToken.None);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.Equal(0, memoryStream.Length);
            Assert.Null(responseBase.Headers["Content-Type"]);
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_InternalServerError_And_No_Content_When_No_Content_Negotiator_For_Error_Response()
        {
            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };

            // This formatter throws on any write attempt
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                            It.IsAny<object>(),
                                                            It.IsAny<Stream>(),
                                                            It.IsAny<HttpContent>(),
                                                            It.IsAny<TransportContext>())).Throws(new NotSupportedException("Expected error"));

            // Create a local config to hook to the request to condition
            // the formatter selection for the error response
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(formatterMock.Object);
            config.Services.Replace(typeof(IContentNegotiator), null /*negotiatorMock.Object*/);

            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            request.SetConfiguration(config);
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(GlobalConfiguration.Configuration);

            // Act
            await HttpControllerHandler.CopyResponseAsync(contextMock.Object, request, response, exceptionLogger,
                exceptionHandler, CancellationToken.None);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.Equal(0, memoryStream.Length);
            Assert.Null(responseBase.Headers["Content-Type"]);
        }

        [Fact]
        public async Task CopyResponseAsync_Returns_InternalServerError_And_No_Content_For_Null_HttpResponseMessage()
        {
            // Arrange
            MemoryStream memoryStream = new MemoryStream();
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBaseForResponse(memoryStream);
            HttpResponseBase responseBase = contextMock.Object.Response;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            await CopyResponseAsync(contextMock.Object, request: new HttpRequestMessage(), response: null);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseBase.StatusCode);
            Assert.Equal(0, memoryStream.Length);
            Assert.Null(responseBase.Headers["Content-Type"]);
        }

        [Fact]
        public async Task WriteStreamedResponseContentAsync_Aborts_When_Formatter_Write_Throws_Immediately()
        {
            // Arrange
            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                          It.IsAny<object>(),
                                                          It.IsAny<Stream>(),
                                                          It.IsAny<HttpContent>(),
                                                          It.IsAny<TransportContext>())).Throws(new NotSupportedException("Expected error"));

            MemoryStream memoryStream = new MemoryStream();

            Mock<HttpRequestBase> requestBaseMock = new Mock<HttpRequestBase>();
            requestBaseMock.Setup(m => m.Abort()).Verifiable();
            HttpRequestBase requestBase = requestBaseMock.Object;
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(memoryStream).Object;
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(m => m.Response).Returns(responseBase);
            HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);

            // Act
            await WriteStreamedResponseContentAsync(contextBase, request, response);

            // Assert
            requestBaseMock.Verify();
        }

        [Fact]
        public async Task WriteStreamedResponseContentAsync_Aborts_When_Formatter_Write_Faults()
        {
            // Arrange
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(new NotSupportedException("Expected error"));

            Mock<JsonMediaTypeFormatter> formatterMock = new Mock<JsonMediaTypeFormatter>() { CallBase = true };
            formatterMock.Setup(m => m.WriteToStreamAsync(It.IsAny<Type>(),
                                                          It.IsAny<object>(),
                                                          It.IsAny<Stream>(),
                                                          It.IsAny<HttpContent>(),
                                                          It.IsAny<TransportContext>())).Returns(tcs.Task);

            MemoryStream memoryStream = new MemoryStream();

            Mock<HttpRequestBase> requestBaseMock = new Mock<HttpRequestBase>();
            requestBaseMock.Setup(m => m.Abort()).Verifiable();
            HttpRequestBase requestBase = requestBaseMock.Object;
            HttpResponseBase responseBase = CreateMockHttpResponseBaseForResponse(memoryStream).Object;
            HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            response.Content = new ObjectContent<string>("hello", formatterMock.Object);

            // Act
            await WriteStreamedResponseContentAsync(contextBase, request, response);

            // Assert
            requestBaseMock.Verify();
        }

        [Fact]
        public async Task WriteStreamedResponseContentAsync_IfCopyToAsyncThrows_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedException = CreateException();

            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            IExceptionLogger logger = mock.Object;

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedResponse.Content = CreateFaultingContent(expectedException);

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                await HttpControllerHandler.WriteStreamedResponseContentAsync(contextBase, expectedRequest,
                    expectedResponse, logger, expectedCancellationToken);

                // Assert
                mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedException
                    && c.ExceptionContext.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerStreamContent
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedResponse
                    ), expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task WriteStreamedResponseContentAsync_IfCopyToAsyncCancells_DoesNotCallExceptionLogger()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            IExceptionLogger logger = mock.Object;

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedResponse.Content = CreateFaultingContent(expectedException);

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act & Assert
                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => HttpControllerHandler.WriteStreamedResponseContentAsync(
                        contextBase,
                        expectedRequest,
                        expectedResponse,
                        logger,
                        expectedCancellationToken));
            }
        }

        [Fact]
        public async Task WriteBufferedResponseContentAsync_IfCopyToAsyncThrows_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();

            Mock<IExceptionLogger> loggerMock = CreateStubExceptionLoggerMock();
            IExceptionLogger logger = loggerMock.Object;
            Mock<IExceptionHandler> handlerMock = CreateStubExceptionHandlerMock();
            IExceptionHandler handler = handlerMock.Object;

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedResponse.Content = CreateFaultingContent(expectedException);

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act & Assert
                await Assert.ThrowsAsync<EncoderFallbackException>(
                    () => HttpControllerHandler.WriteBufferedResponseContentAsync(
                        contextBase,
                        expectedRequest,
                        expectedResponse,
                        logger,
                        handler,
                        expectedCancellationToken));

                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent
                    && c.Request == expectedRequest
                    && c.Response == expectedResponse;

                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    exceptionContextMatches(c.ExceptionContext)), expectedCancellationToken), Times.Once());
                handlerMock.Verify(l => l.HandleAsync(It.Is<ExceptionHandlerContext>(c =>
                    exceptionContextMatches(c.ExceptionContext)), expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task WriteBufferedResponseContentAsync_IfCopyToAsyncCancels_DoesNotCallExceptionServices()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            Mock<IExceptionLogger> loggerMock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            IExceptionLogger logger = loggerMock.Object;
            Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            IExceptionHandler handler = handlerMock.Object;

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedResponse.Content = CreateFaultingContent(expectedException);

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act & Assert
                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => HttpControllerHandler.WriteBufferedResponseContentAsync(
                        contextBase,
                        expectedRequest,
                        expectedResponse,
                        logger,
                        handler,
                        expectedCancellationToken));
            }
        }

        [Fact]
        public async Task WriteBufferedResponseContentAsync_IfCopyToAsyncThrowsAndHandlerHandles_ReturnsCompletedTask()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = HttpStatusCode.ExpectationFailed;

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage())
            {
                IExceptionLogger logger = CreateStubExceptionLogger();
                Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
                handlerMock
                    .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                    .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                    {
                        c.Result = new StatusCodeResult(expectedStatusCode, request);
                        return Task.FromResult(0);
                    });
                IExceptionHandler handler = handlerMock.Object;

                response.Content = CreateFaultingContent(CreateException());

                int statusCode = 0;
                HttpRequestBase requestBase = CreateStubRequestBase();
                Mock<HttpResponseBase> responseBaseMock = new Mock<HttpResponseBase>();
                responseBaseMock.Setup(r => r.OutputStream).Returns(Stream.Null);
                responseBaseMock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((c) => statusCode = c);
                HttpResponseBase responseBase = responseBaseMock.Object;
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = CancellationToken.None;

                // Act
                await HttpControllerHandler.WriteBufferedResponseContentAsync(contextBase, request,
                    response, logger, handler, expectedCancellationToken);

                // Assert
                Assert.Equal((int)expectedStatusCode, statusCode);
            }
        }

        [Fact]
        public async Task WriteBufferedResponseContentAsync_IfCopyToAsyncThrowsAndHandlerDoesNotHandle_PropagatesFault()
        {
            // Arrange
            Exception expectedException = CreateExceptionWithCallStack();
            string expectedStackTrace = expectedException.StackTrace;

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage())
            {
                IExceptionLogger logger = CreateStubExceptionLogger();
                IExceptionHandler handler = CreateStubExceptionHandler();

                response.Content = CreateFaultingContent(expectedException);

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = CancellationToken.None;

                // Act & Assert
                var exception = await Assert.ThrowsAsync<EncoderFallbackException>(
                    () => HttpControllerHandler.WriteBufferedResponseContentAsync(
                        contextBase,
                        request,
                        response,
                        logger,
                        handler,
                        expectedCancellationToken));

                Assert.Same(expectedException, exception);
                Assert.NotNull(exception.StackTrace);
                Assert.StartsWith(expectedStackTrace, exception.StackTrace);
            }
        }

        [Fact]
        public async Task WriteBufferedResponseContentAsync_IfCopyToAsyncOnErrorResponseThrows_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            Exception expectedErrorException = CreateException();

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedOriginalResponse = new HttpResponseMessage())
            using (HttpResponseMessage expectedErrorResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedOriginalResponse.Content = CreateFaultingContent(expectedOriginalException);
                expectedErrorResponse.Content = CreateFaultingContent(expectedErrorException);

                Mock<IExceptionLogger> loggerMock = CreateStubExceptionLoggerMock();
                IExceptionLogger logger = loggerMock.Object;
                Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
                handlerMock
                    .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                    .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                    {
                        c.Result = new ResponseMessageResult(expectedErrorResponse);
                        return Task.FromResult(0);
                    });
                IExceptionHandler handler = handlerMock.Object;

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                await HttpControllerHandler.WriteBufferedResponseContentAsync(contextBase, expectedRequest,
                    expectedOriginalResponse, logger, handler, expectedCancellationToken);

                // Assert
                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedOriginalException
                    && c.ExceptionContext.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedOriginalResponse),
                    expectedCancellationToken), Times.Once());
                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedErrorException
                    && c.ExceptionContext.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerBufferError
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedErrorResponse),
                    expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task WriteBufferedResponseContentAsync_IfCopyToAsyncOnErrorResponseCancels_DoesNotCallCallsExceptionLogger()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            Exception expectedErrorException = new OperationCanceledException();

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedOriginalResponse = new HttpResponseMessage())
            using (HttpResponseMessage expectedErrorResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedOriginalResponse.Content = CreateFaultingContent(expectedOriginalException);
                expectedErrorResponse.Content = CreateFaultingContent(expectedErrorException);

                Mock<IExceptionLogger> loggerMock = CreateStubExceptionLoggerMock();
                IExceptionLogger logger = loggerMock.Object;
                Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
                handlerMock
                    .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                    .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                    {
                        c.Result = new ResponseMessageResult(expectedErrorResponse);
                        return Task.FromResult(0);
                    });
                IExceptionHandler handler = handlerMock.Object;

                HttpRequestBase requestBase = CreateStubRequestBase();
                HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
                HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act & Assert
                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => HttpControllerHandler.WriteBufferedResponseContentAsync(
                        contextBase,
                        expectedRequest,
                        expectedOriginalResponse,
                        logger,
                        handler,
                        expectedCancellationToken));

                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedOriginalException
                    && c.ExceptionContext.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedOriginalResponse),
                    expectedCancellationToken), Times.Once());
                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedErrorException
                    && c.ExceptionContext.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerBufferError
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedErrorResponse),
                    expectedCancellationToken), Times.Never());
            }
        }

        [Fact]
        public async Task PrepareHeadersAsync_IfTryComputeLengthThrows_CallsExceptionLogger()
        {
            // Arrange
            HttpResponseBase responseBase = CreateStubResponseBase();
            Exception expectedException = CreateException();

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                expectedResponse.Content = CreateThrowingContent(expectedException);

                Mock<IExceptionLogger> loggerMock = CreateStubExceptionLoggerMock();
                IExceptionLogger logger = loggerMock.Object;

                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                await HttpControllerHandler.PrepareHeadersAsync(responseBase, expectedRequest, expectedResponse,
                    logger, expectedCancellationToken);

                // Assert
                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedException
                    && c.ExceptionContext.CatchBlock == WebHostExceptionCatchBlocks.HttpControllerHandlerComputeContentLength
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedResponse),
                    expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public async Task PrepareHeadersAsync_IfTryComputeLengthThrows_SetsEmptyErrorResponseAndReturnsFalse()
        {
            // Arrange
            Mock<HttpResponseBase> responseBaseMock = new Mock<HttpResponseBase>(MockBehavior.Strict);
            responseBaseMock.Setup(r => r.Clear());
            responseBaseMock.Setup(r => r.ClearHeaders());
            int statusCode = 0;
            responseBaseMock.SetupSet((r) => r.StatusCode = It.IsAny<int>()).Callback<int>((s) => statusCode = s);
            bool suppressContent = false;
            responseBaseMock.SetupSet((r) => r.SuppressContent = It.IsAny<bool>()).Callback<bool>((s) => suppressContent = s);
            HttpResponseBase responseBase = responseBaseMock.Object;

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpResponseMessage response = new HttpResponseMessage())
            {
                response.Content = CreateThrowingContent(CreateException());
                IExceptionLogger logger = CreateStubExceptionLogger();
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                bool result = await HttpControllerHandler.PrepareHeadersAsync(responseBase, request, response, logger,
                    cancellationToken);

                // Assert
                responseBaseMock.Verify(r => r.Clear(), Times.Once());
                responseBaseMock.Verify(r => r.ClearHeaders(), Times.Once());
                Assert.Equal(500, statusCode);
                Assert.True(suppressContent);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task GetBufferedStream_ReadAsString_GetsSeekableInputStream()
        {
            // Arrange
            using (MemoryStream nonSeekable = new MemoryStream())
            using (MemoryStream seekable = new MemoryStream())
            {
                var request = CreateStubRequestBaseMock("IgnoreMethod", nonSeekable, seekable);
                var context = CreateStubContextBase(request.Object);

                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Guard
                    request.Verify(r => r.GetBufferedInputStream(), Times.Never());
                    request.Verify(r => r.InputStream, Times.Never());

                    // Act
                    var content = await actualRequest.Content.ReadAsStringAsync();

                    // Assert
                    request.Verify(r => r.GetBufferedInputStream(), Times.Once());
                    request.Verify(r => r.InputStream, Times.Once());
                }
            }
        }

        [Fact]
        public async Task GetBufferedStream_ReadAsStream_DoesNotGetSeekableInputStream()
        {
            // Arrange
            using (MemoryStream nonSeekable = new MemoryStream())
            using (MemoryStream seekable = new MemoryStream())
            {
                var request = CreateStubRequestBaseMock("IgnoreMethod", nonSeekable, seekable);
                var context = CreateStubContextBase(request.Object);

                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Guard
                    request.Verify(r => r.GetBufferedInputStream(), Times.Never());
                    request.Verify(r => r.InputStream, Times.Never());

                    // Act
                    var stream = await actualRequest.Content.ReadAsStreamAsync();

                    // Assert
                    request.Verify(r => r.GetBufferedInputStream(), Times.Once());
                    request.Verify(r => r.InputStream, Times.Never());
                }
            }
        }

        [Fact]
        public async Task GetBufferedStream_ReadAsStream_ThenSeek_GetsSeekableInputStream()
        {
            // Arrange
            string content = "Hello, World!";
            using (MemoryStream nonSeekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream seekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var request = CreateStubRequestBaseMock("IgnoreMethod", nonSeekable, seekable);
                var context = CreateStubContextBase(request.Object);

                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    var stream = await actualRequest.Content.ReadAsStreamAsync();

                    // Guard
                    request.Verify(r => r.GetBufferedInputStream(), Times.Once());
                    request.Verify(r => r.InputStream, Times.Never());

                    // Act
                    stream.Seek(1L, SeekOrigin.Begin);

                    // Assert
                    request.Verify(r => r.GetBufferedInputStream(), Times.Once());
                    request.Verify(r => r.InputStream, Times.Once());
                }
            }
        }

        [Fact]
        public async Task GetBufferedStream_EndToEnd_ReadContentTwice()
        {
            // Arrange
            string content = "Hello, World!";
            using (MemoryStream nonSeekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream seekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var request = CreateStubRequestBaseMock("IgnoreMethod", nonSeekable, seekable);
                var context = CreateStubContextBase(request.Object);

                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Act
                    var actual1 = await actualRequest.Content.ReadAsStringAsync();
                    var actual2 = await actualRequest.Content.ReadAsStringAsync();

                    // Assert
                    Assert.Equal(content, actual1);
                    Assert.Equal(content, actual2);
                }
            }
        }

        [Fact]
        public async Task GetBufferedStream_EndToEnd_ReadContentThenSeekThenRead()
        {
            // Arrange
            string content = "Hello, World!";
            using (MemoryStream nonSeekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream seekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var request = CreateStubRequestBaseMock("IgnoreMethod", nonSeekable, seekable);
                var context = CreateStubContextBase(request.Object);

                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Act
                    var actual1 = await actualRequest.Content.ReadAsStringAsync();

                    var stream = await actualRequest.Content.ReadAsStreamAsync();
                    stream.Seek(0, SeekOrigin.Begin);

                    string actual2;
                    using (var reader = new StreamReader(stream))
                    {
                        actual2 = await reader.ReadToEndAsync();
                    }

                    // Assert
                    Assert.Equal(content, actual1);
                    Assert.Equal(content, actual2);
                }
            }
        }

        [Fact]
        public async Task GetBufferedStream_EndToEnd_SeekThenRead()
        {
            // Arrange
            string content = "Hello, World!";
            using (MemoryStream nonSeekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream seekable = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var request = CreateStubRequestBaseMock("IgnoreMethod", nonSeekable, seekable);
                var context = CreateStubContextBase(request.Object);

                using (HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context))
                {
                    // Act
                    var stream = await actualRequest.Content.ReadAsStreamAsync();
                    stream.Seek(1L, SeekOrigin.Begin);
                    stream.Seek(0L, SeekOrigin.Begin);

                    var actual = await actualRequest.Content.ReadAsStringAsync();

                    // Assert
                    Assert.Equal(content, actual);
                }
            }
        }

        [Fact]
        public async Task ProcessRequestAsync_Cancels_AbortsRequest()
        {
            // Arrange
            var request = CreateStubRequestBaseMock("Ignore", new MemoryStream(), new MemoryStream());
            request.Setup(r => r.Abort()).Verifiable();

            var context = CreateStubContextBase(request.Object);

            var messageHandler = new LambdaHttpMessageHandler((r, ct) => { throw new OperationCanceledException(); });

            var handler = new HttpControllerHandler(new RouteData(), messageHandler);

            // Act
            await handler.ProcessRequestAsyncCore(context);

            // Assert
            request.Verify(r => r.Abort(), Times.Once());
        }

        private static Task CopyResponseAsync(HttpContextBase contextBase, HttpRequestMessage request, HttpResponseMessage response)
        {
            IExceptionLogger exceptionLogger = CreateDummyExceptionLogger();
            IExceptionHandler exceptionHandler = CreateDummyExceptionHandler();
            CancellationToken cancellationToken = CancellationToken.None;

            return HttpControllerHandler.CopyResponseAsync(contextBase, request, response, exceptionLogger,
                exceptionHandler, cancellationToken);
        }

        private static IExceptionHandler CreateDummyExceptionHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IExceptionLogger CreateDummyExceptionLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private static Exception CreateException()
        {
            return new EncoderFallbackException();
        }

        private static Exception CreateExceptionWithCallStack()
        {
            try
            {
                throw CreateException();
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Task<T> CreateFaultedTask<T>(Exception exception)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            source.SetException(exception);
            return source.Task;
        }

        private static HttpContent CreateFaultingContent(Exception exception)
        {
            return new FaultingHttpContent(exception);
        }

        private static HttpContextBase CreateStubContextBase(string httpMethod, Stream bufferedStream)
        {
            HttpRequestBase request = CreateStubRequestBase(httpMethod, bufferedStream);
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(m => m.Request).Returns(request);
            return contextMock.Object;
        }

        internal static HttpContextBase CreateStubContextBase(HttpRequestBase request)
        {
            return CreateStubContextBase(request, new Hashtable());
        }

        internal static HttpContextBase CreateStubContextBase(HttpRequestBase request, IDictionary items)
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(m => m.Request).Returns(request);
            contextMock.SetupGet(m => m.Items).Returns(items);
            return contextMock.Object;
        }

        private static HttpContextBase CreateStubContextBase(HttpRequestBase request, HttpResponseBase response)
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.SetupGet(m => m.Request).Returns(request);
            contextMock.SetupGet(m => m.Response).Returns(response);
            return contextMock.Object;
        }

        private static HttpContextBase CreateStubContextBase(HttpResponseBase response)
        {
            Mock<HttpContextBase> mock = new Mock<HttpContextBase>(MockBehavior.Strict);
            mock.SetupGet(m => m.Response).Returns(response);
            return mock.Object;
        }

        private static IExceptionHandler CreateStubExceptionHandler()
        {
            return CreateStubExceptionHandlerMock().Object;
        }

        private static Mock<IExceptionHandler> CreateStubExceptionHandlerMock()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private static IExceptionLogger CreateStubExceptionLogger()
        {
            return CreateStubExceptionLoggerMock().Object;
        }

        private static Mock<IExceptionLogger> CreateStubExceptionLoggerMock()
        {
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private static HttpRequestBase CreateStubRequestBase()
        {
            return new Mock<HttpRequestBase>().Object;
        }

        private static HttpRequestBase CreateStubRequestBase(string httpMethod, Stream bufferedStream)
        {
            return CreateStubRequestBaseMock(httpMethod, bufferedStream, bufferedStream).Object;
        }

        private static Mock<HttpRequestBase> CreateStubRequestBaseMock(string httpMethod, Stream nonSeekableStream, Stream seekableStream)
        {
            Mock<HttpRequestBase> requestBaseMock = new Mock<HttpRequestBase>() { CallBase = true };
            requestBaseMock.SetupGet(m => m.HttpMethod).Returns(httpMethod);
            requestBaseMock.SetupGet(m => m.Url).Returns(new Uri("Http://localhost"));
            requestBaseMock.SetupGet(m => m.Headers).Returns(new NameValueCollection());
            requestBaseMock.SetupGet(m => m.ReadEntityBodyMode).Returns(ReadEntityBodyMode.None);

            requestBaseMock.Setup(m => m.GetBufferedInputStream()).Returns(nonSeekableStream).Verifiable();
            requestBaseMock.SetupGet(m => m.InputStream).Returns(seekableStream).Verifiable();

            requestBaseMock.Setup(m => m.GetBufferlessInputStream()).Throws<InvalidOperationException>();

            return requestBaseMock;
        }

        internal static HttpRequestBase CreateFakeRequestBase(Func<Stream> getStream, bool buffered)
        {
            var readEntityBodyMode = ReadEntityBodyMode.None;
            Mock<HttpRequestBase> requestBaseMock = new Mock<HttpRequestBase>() { CallBase = true };
            requestBaseMock.SetupGet(m => m.HttpMethod).Returns("GET");
            requestBaseMock.SetupGet(m => m.Url).Returns(new Uri("Http://localhost"));
            requestBaseMock.SetupGet(m => m.Headers).Returns(new NameValueCollection());
            requestBaseMock.Setup(r => r.ReadEntityBodyMode).Returns(() => readEntityBodyMode);

            requestBaseMock.Setup(m => m.GetBufferedInputStream()).Returns(() =>
            {
                if (readEntityBodyMode == ReadEntityBodyMode.None || readEntityBodyMode == ReadEntityBodyMode.Buffered)
                {
                    readEntityBodyMode = ReadEntityBodyMode.Buffered;
                    return getStream();
                }
                throw new InvalidOperationException();
            });

            requestBaseMock.SetupGet(m => m.InputStream).Returns(() =>
            {
                if (readEntityBodyMode == ReadEntityBodyMode.None || readEntityBodyMode == ReadEntityBodyMode.Classic)
                {
                    readEntityBodyMode = ReadEntityBodyMode.Classic;
                    return getStream();
                }
                else if (readEntityBodyMode == ReadEntityBodyMode.Buffered)
                {
                    Stream stream = getStream();
                    if (stream.Position == stream.Length)
                    {
                        return stream;
                    }
                }
                throw new InvalidOperationException();
            });

            requestBaseMock.Setup(m => m.GetBufferlessInputStream()).Returns(() =>
            {
                if (readEntityBodyMode == ReadEntityBodyMode.None || readEntityBodyMode == ReadEntityBodyMode.Bufferless)
                {
                    readEntityBodyMode = ReadEntityBodyMode.Bufferless;
                    return getStream();
                }
                throw new InvalidOperationException();
            });
            return requestBaseMock.Object;
        }

        private static HttpResponseBase CreateStubResponseBase()
        {
            return new Mock<HttpResponseBase>().Object;
        }

        private static HttpResponseBase CreateStubResponseBase(CancellationToken clientDisconnectedToken)
        {
            Mock<HttpResponseBase> mock = new Mock<HttpResponseBase>();
            mock.Setup(r => r.ClientDisconnectedToken).Returns(clientDisconnectedToken);
            return mock.Object;
        }

        private static HttpResponseBase CreateStubResponseBase(Stream outputStream)
        {
            Mock<HttpResponseBase> responseBaseMock = new Mock<HttpResponseBase>();
            responseBaseMock.Setup(r => r.OutputStream).Returns(outputStream);
            return responseBaseMock.Object;
        }

        private static Mock<HttpResponseBase> CreateMockHttpResponseBaseForResponse(Stream outputStream)
        {
            NameValueCollection testHeaders = new NameValueCollection();

            Mock<HttpResponseBase> responseBaseMock = new Mock<HttpResponseBase>() { DefaultValue = DefaultValue.Mock };
            responseBaseMock.Setup(m => m.OutputStream).Returns(outputStream);
            responseBaseMock.Setup(m => m.Headers).Returns(testHeaders);
            responseBaseMock.Setup(m => m.AppendHeader(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((s, v) => testHeaders.Add(s, v));
            responseBaseMock.Setup(m => m.ClearHeaders()).Callback(() => testHeaders.Clear());
            responseBaseMock.Setup(m => m.Clear()).Callback(() => testHeaders.Clear());
            responseBaseMock.SetupProperty(m => m.StatusCode);
            responseBaseMock.SetupProperty(m => m.BufferOutput);

            return responseBaseMock;
        }

        private static Mock<HttpContextBase> CreateMockHttpContextBaseForResponse(Stream outputStream)
        {
            NameValueCollection testHeaders = new NameValueCollection();

            Mock<HttpResponseBase> responseBaseMock = CreateMockHttpResponseBaseForResponse(outputStream);
            HttpResponseBase responseBase = responseBaseMock.Object;
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(m => m.Response).Returns(responseBase);

            return contextMock;
        }

        private static HttpContent CreateThrowingContent(Exception exception)
        {
            return new ThrowingHttpContent(exception);
        }

        private static Task WriteStreamedResponseContentAsync(HttpContextBase contextBase, HttpRequestMessage request,
            HttpResponseMessage response)
        {
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            CancellationToken cancellationToken = CancellationToken.None;

            return HttpControllerHandler.WriteStreamedResponseContentAsync(contextBase, request, response,
                exceptionLogger, cancellationToken);
        }

        private class FaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public FaultingHttpContent(Exception exception)
            {
                Contract.Assert(exception != null);
                _exception = exception;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask<object>(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                Contract.Assert(sendAsync != null);
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }

        private sealed class SpyDisposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private class ThrowingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public ThrowingHttpContent(Exception exception)
            {
                Contract.Assert(exception != null);
                _exception = exception;

            }
            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw _exception;
            }

            protected override bool TryComputeLength(out long length)
            {
                throw _exception;
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Owin.ExceptionHandling;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class HttpMessageHandlerAdapterTest
    {
        [Fact]
        public void ConstructorWithOptions_IfOptionsIsNull_Throws()
        {
            // Arrange
            HttpMessageHandlerOptions options = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(options), "options");
        }

        [Fact]
        public void ConstructorWithOptions_IfMessageHandlerIsNull_Throws()
        {
            // Arrange
            HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler: null);

            // Act & Assert.
            Assert.ThrowsArgument(() => CreateProductUnderTest(options), "options",
                "HttpMessageHandlerOptions.MessageHandler must not be null.");
        }

        [Fact]
        public void ConstructorWithOptions_IfBufferPolicySelectorIsNull_Throws()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.BufferPolicySelector = null;

                // Act & Assert.
                Assert.ThrowsArgument(() => CreateProductUnderTest(options), "options",
                    "HttpMessageHandlerOptions.BufferPolicySelector must not be null.");
            }
        }

        [Fact]
        public void ConstructorWithOptions_IfExceptionLoggerIsNull_Throws()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.ExceptionLogger = null;

                // Act & Assert.
                Assert.ThrowsArgument(() => CreateProductUnderTest(options), "options",
                    "HttpMessageHandlerOptions.ExceptionLogger must not be null.");
            }
        }

        [Fact]
        public void ConstructorWithOptions_IfExceptionHandlerIsNull_Throws()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.ExceptionHandler = null;

                // Act & Assert.
                Assert.ThrowsArgument(() => CreateProductUnderTest(options), "options",
                    "HttpMessageHandlerOptions.ExceptionHandler must not be null.");
            }
        }

        [Fact]
        public void ConstructorWithList_IfMessageHandlerIsNull_Throws()
        {
            // Arrange
            HttpMessageHandler messageHandler = null;
            IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();

            // Act & Assert.
#pragma warning disable 0618
            Assert.ThrowsArgumentNull(() => new HttpMessageHandlerAdapter(null, messageHandler, bufferPolicySelector),
                "messageHandler");
#pragma warning restore 0618
        }

        [Fact]
        public void ConstructorWithList_IfBufferPolicySelectorIsNull_Throws()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = null;

                // Act & Assert.
#pragma warning disable 0618
                Assert.ThrowsArgumentNull(() => new HttpMessageHandlerAdapter(null, messageHandler,
                    bufferPolicySelector), "bufferPolicySelector");
#pragma warning restore 0618
            }
        }

        [Fact]
        public void MessageHandler_IfUsingOptionsConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler expectedMessageHandler = CreateDummyMessageHandler())
            {
                HttpMessageHandlerOptions options = CreateDummyOptions(expectedMessageHandler);
                HttpMessageHandlerAdapter product = CreateProductUnderTest(options);

                // Act
                HttpMessageHandler messageHandler = product.MessageHandler;

                // Assert
                Assert.Same(expectedMessageHandler, messageHandler);
            }
        }

        [Fact]
        public void MessageHandler_IfUsingListConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler expectedMessageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();
#pragma warning disable 0618
                HttpMessageHandlerAdapter product = new HttpMessageHandlerAdapter(null, expectedMessageHandler,
                    bufferPolicySelector);
#pragma warning restore 0618

                // Act
                HttpMessageHandler messageHandler = product.MessageHandler;

                // Assert
                Assert.Same(expectedMessageHandler, messageHandler);
            }
        }

        [Fact]
        public void BufferPolicySelector_IfUsingOptionsConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector expectedBufferPolicySelector = CreateDummyBufferPolicy();
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.BufferPolicySelector = expectedBufferPolicySelector;
                HttpMessageHandlerAdapter product = CreateProductUnderTest(options);

                // Act
                IHostBufferPolicySelector bufferPolicySelector = product.BufferPolicySelector;

                // Assert
                Assert.Same(expectedBufferPolicySelector, bufferPolicySelector);
            }
        }

        [Fact]
        public void BufferPolicySelector_IfUsingListConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector expectedBufferPolicySelector = CreateDummyBufferPolicy();
#pragma warning disable 0618
                HttpMessageHandlerAdapter product = new HttpMessageHandlerAdapter(null, messageHandler,
                    expectedBufferPolicySelector);
#pragma warning restore 0618

                // Act
                IHostBufferPolicySelector bufferPolicySelector = product.BufferPolicySelector;

                // Assert
                Assert.Same(expectedBufferPolicySelector, bufferPolicySelector);
            }
        }

        [Fact]
        public void ExceptionLogger_IfUsingOptionsConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IExceptionLogger expectedExceptionLogger = CreateDummyExceptionLogger();
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.ExceptionLogger = expectedExceptionLogger;
                HttpMessageHandlerAdapter product = CreateProductUnderTest(options);

                // Act
                IExceptionLogger exceptionLogger = product.ExceptionLogger;

                // Assert
                Assert.Same(expectedExceptionLogger, exceptionLogger);
            }
        }

        [Fact]
        public void ExceptionLogger_IfUsingListConstructor_ReturnsEmptyExceptionLogger()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();
#pragma warning disable 0618
                HttpMessageHandlerAdapter product = new HttpMessageHandlerAdapter(null, messageHandler,
                    bufferPolicySelector);
#pragma warning restore 0618

                // Act
                IExceptionLogger exceptionLogger = product.ExceptionLogger;

                // Assert
                Assert.IsType<EmptyExceptionLogger>(exceptionLogger);
            }
        }

        [Fact]
        public void ExceptionHandler_IfUsingOptionsConstructor_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IExceptionHandler expectedExceptionHandler = CreateDummyExceptionHandler();
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.ExceptionHandler = expectedExceptionHandler;
                HttpMessageHandlerAdapter product = CreateProductUnderTest(options);

                // Act
                IExceptionHandler exceptionHandler = product.ExceptionHandler;

                // Assert
                Assert.Same(expectedExceptionHandler, exceptionHandler);
            }
        }

        [Fact]
        public void ExceptionHandler_IfUsingListConstructor_ReturnsDefaultExceptionHandler()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();
#pragma warning disable 0618
                HttpMessageHandlerAdapter product = new HttpMessageHandlerAdapter(null, messageHandler,
                    bufferPolicySelector);
#pragma warning restore 0618

                // Act
                IExceptionHandler exceptionHandler = product.ExceptionHandler;

                // Assert
                Assert.IsType<DefaultExceptionHandler>(exceptionHandler);
            }
        }

        [Fact]
        public void AppDisposing_IfUsingOptionsConstructor_ReturnsSpecifiedValue()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                CancellationToken expectedAppDisposing = tokenSource.Token;
                HttpMessageHandlerOptions options = CreateDummyOptions(messageHandler);
                options.AppDisposing = expectedAppDisposing;
                HttpMessageHandlerAdapter product = CreateProductUnderTest(options);

                // Act
                CancellationToken appDisposing = product.AppDisposing;

                // Assert
                Assert.Equal(expectedAppDisposing, appDisposing);
            }
        }

        [Fact]
        public void AppDisposing_IfUsingListConstructor_ReturnsDefaultExceptionHandler()
        {
            // Arrange
            using (HttpMessageHandler messageHandler = CreateDummyMessageHandler())
            {
                IHostBufferPolicySelector bufferPolicySelector = CreateDummyBufferPolicy();
#pragma warning disable 0618
                HttpMessageHandlerAdapter product = new HttpMessageHandlerAdapter(null, messageHandler,
                    bufferPolicySelector);
#pragma warning restore 0618

                // Act
                CancellationToken appDisposing = product.AppDisposing;

                // Assert
                Assert.Equal(CancellationToken.None, appDisposing);
            }
        }

        [Fact]
        public void TriggeringHostOnAppDisposing_DisposesMessageHandler()
        {
            // Arrange
            using (SpyDisposeHttpMessageHandler spy = new SpyDisposeHttpMessageHandler())
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                HttpMessageHandlerOptions options = CreateDummyOptions(spy);
                options.AppDisposing = tokenSource.Token;
                HttpMessageHandlerAdapter product = CreateProductUnderTest(options);

                // Act
                tokenSource.Cancel(throwOnFirstException: true);
                CancellationToken appDisposing = product.AppDisposing;

                // Assert
                Assert.True(spy.Disposed);
            }
        }

        [Fact]
        public async Task Invoke_ThrowsOnNullRequest()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);
            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(context => context.Response).Returns(new OwinResponse());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => adapter.Invoke(mockContext.Object),
                "The OWIN context's Request property must not be null.");
        }

        [Fact]
        public async Task Invoke_ThrowsOnNullResponse()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);
            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(context => context.Request).Returns(new OwinRequest());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => adapter.Invoke(mockContext.Object),
                "The OWIN context's Response property must not be null.");
        }

        [Fact]
        public async Task Invoke_BuildsAppropriateRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Invoke_BuildsUriWithQueryStringIfPresent()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            environment["owin.RequestQueryString"] = "id=45";
            var adapter = CreateProductUnderTest(new HttpMessageHandlerOptions
            {
                MessageHandler = handler,
                BufferPolicySelector = bufferPolicySelector,
                ExceptionLogger = new EmptyExceptionLogger(),
                ExceptionHandler = new DefaultExceptionHandler()
            });

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers?id=45", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Invoke_BuildsUriWithHostAndPort()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost:12345", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost:12345/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Theory]
        [InlineData("a b")]
        // reserved characters
        [InlineData("!*'();:@&=$,[]")]
        // common unreserved characters
        [InlineData(@"-_.~+""<>^`{|}")]
        // random unicode characters
        [InlineData("激光這")]
        [InlineData("%24")]
        [InlineData("?#")]
        public async Task Invoke_CreatesUri_ThatGeneratesCorrectlyDecodedStrings(string decodedId)
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers/" + decodedId);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);
            var route = new HttpRoute("api/customers/{id}");

            await adapter.Invoke(new OwinContext(environment));
            IHttpRouteData routeData = route.GetRouteData("/vroot", handler.Request);

            Assert.NotNull(routeData);
            Assert.Equal(decodedId, routeData.Values["id"]);
        }

        [Fact]
        public async Task Invoke_AddsRequestHeadersToRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var requestHeaders = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            requestHeaders["Accept"] = new string[] { "application/json", "application/xml" };
            requestHeaders["Content-Length"] = new string[] { "45" };
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Equal(2, request.Headers.Count());
            Assert.Equal(new string[] { "application/json", "application/xml" }, request.Headers.Accept.Select(mediaType => mediaType.ToString()).ToArray());
            Assert.Equal("localhost", request.Headers.Host);
            Assert.Single(request.Content.Headers);
            Assert.Equal(45, request.Content.Headers.ContentLength);
        }

        [Fact]
        public async Task Invoke_SetsRequestBodyOnRequestMessage()
        {
            string body = null;
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = async (r, i) =>
            {
                body = await r.Content.ReadAsStringAsync();
                return new HttpResponseMessage();
            };
            var handler = CreateLambdaMessageHandler(sendAsync);
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var expectedBody = "This is the request body.";
            environment["owin.RequestBody"] = new MemoryStream(Encoding.UTF8.GetBytes(expectedBody));
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            Assert.Equal(expectedBody, body);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Invoke_RespectsInputBufferingSetting(bool bufferInput)
        {
            var expectedBody = "This is the request body.";
            var requestBodyMock = new Mock<MemoryStream>(Encoding.UTF8.GetBytes(expectedBody));
            requestBodyMock.CallBase = true;
            requestBodyMock.Setup(s => s.CanSeek).Returns(false);
            MemoryStream requestBody = requestBodyMock.Object;

            string body = null;
            bool originalStreamDisposed = false;
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = async (r, i) =>
            {
                body = await r.Content.ReadAsStringAsync();
                originalStreamDisposed = !requestBody.CanRead;
                return new HttpResponseMessage();
            };
            var handler = CreateLambdaMessageHandler(sendAsync);
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: bufferInput, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            environment["owin.RequestBody"] = requestBody;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            // Assert that Web API gets the right body
            Assert.Equal(expectedBody, body);
            // The original stream should have been fully read and then disposed only when buffering.
            Assert.Equal(bufferInput, originalStreamDisposed);

            if (bufferInput)
            {
                // Assert that the OWIN environment still has a request body that can be read
                var owinRequestBody = environment["owin.RequestBody"] as Stream;
                byte[] bodyBytes = new byte[25];
                int charsRead = owinRequestBody.Read(bodyBytes, 0, 25);
                Assert.Equal(expectedBody, Encoding.UTF8.GetString(bodyBytes));
            }
        }

        [Fact]
        public async Task Invoke_IfBufferPolicyDisablesInputBuffering_DisablesRequestBuffering()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: false);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector)))
            {
                bool bufferingDisabled = false;
                Action disableBuffering = () => bufferingDisabled = true;
                IOwinRequest owinRequest = CreateFakeOwinRequest(disableBuffering);
                IOwinResponse owinResponse = CreateFakeOwinResponse();
                IOwinContext expectedContext = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(expectedContext);

                // Assert
                Assert.True(bufferingDisabled);
            }
        }

        [Fact]
        public async Task Invoke_IfBufferPolicyEnablesInputBuffering_DoesNotDisableRequestBuffering()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: true,
                bufferOutput: false);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector)))
            {
                bool bufferingDisabled = false;
                Action disableBuffering = () => bufferingDisabled = true;
                IOwinRequest owinRequest = CreateFakeOwinRequest(disableBuffering);
                IOwinResponse owinResponse = CreateFakeOwinResponse();
                IOwinContext expectedContext = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(expectedContext);

                // Assert
                Assert.False(bufferingDisabled);
            }
        }

        [Fact]
        public async Task Invoke_SetsOwinEnvironment()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Same(environment, request.GetOwinEnvironment());
        }

        [Fact]
        public async Task Invoke_SetsOwinRequestContext()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpResponseMessage response = CreateResponse())
            {
                HttpRequestMessage request = null;
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, c) =>
                {
                    request = r;
                    return Task.FromResult(response);
                };

                using (HttpMessageHandler messageHandler = CreateLambdaMessageHandler(sendAsync))
                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                    bufferPolicySelector)))
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();
                    IOwinResponse owinResponse = CreateFakeOwinResponse();
                    IOwinContext expectedContext = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    await product.Invoke(expectedContext);

                    // Assert
                    HttpRequestContext requestContext = request.GetRequestContext();
                    OwinHttpRequestContext typedContext = Assert.IsType<OwinHttpRequestContext>(requestContext);
                    Assert.Same(expectedContext, typedContext.Context);
                    Assert.Same(request, typedContext.Request);
                }
            }
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public async Task Invoke_SetsRequestIsLocalProperty(bool? isLocal, bool expectedRequestLocal)
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            if (isLocal.HasValue)
            {
                environment["server.IsLocal"] = isLocal.Value;
            }
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Equal(expectedRequestLocal, request.IsLocal());
        }

        [Fact]
        public async Task Invoke_SetsClientCertificate()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var clientCert = new Mock<X509Certificate2>().Object;
            environment["ssl.ClientCertificate"] = clientCert;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var request = handler.Request;
            Assert.Equal(clientCert, request.GetClientCertificate());
        }

        [Fact]
        public async Task Invoke_CallsMessageHandler_WithEnvironmentCancellationToken()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var cancellationToken = new CancellationToken();
            environment["owin.CallCancelled"] = cancellationToken;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            Assert.Equal(cancellationToken, handler.CancellationToken);
        }

        [Fact]
        public async Task Invoke_CallsMessageHandler_WithEnvironmentUser()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var user = new Mock<IPrincipal>().Object;
            environment["server.User"] = user;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            Assert.Equal(user, handler.User);
        }

        [Fact]
        public async Task Invoke_Throws_IfMessageHandlerReturnsNull()
        {
            HttpResponseMessage response = null;
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => adapter.Invoke(new OwinContext(environment)),
                "The message handler did not return a response message.");
        }

        [Fact]
        public async Task Invoke_DoesNotCallNext_IfMessageHandlerDoesNotReturn404()
        {
            var mockNext = new Mock<OwinMiddleware>(MockBehavior.Strict, null);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(mockNext.Object, options);

            // Does not throw.
            await adapter.Invoke(new OwinContext(environment));
        }

        [Fact]
        public async Task Invoke_DoesNotCallNext_IfMessageHandlerDoesNotAddNoRouteMatchedProperty()
        {
            var mockNext = new Mock<OwinMiddleware>(MockBehavior.Strict, null);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = false };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(mockNext.Object, options);

            // Does not throw.
            await adapter.Invoke(new OwinContext(environment));
        }

        [Fact]
        public async Task Invoke_CallsNext_IfMessageHandlerReturns404WithNoRouteMatched()
        {
            var nextMock = new Mock<OwinMiddleware>(null);
            nextMock.Setup(middleware => middleware.Invoke(It.IsAny<OwinContext>())).Returns(TaskHelpers.Completed()).Verifiable();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(nextMock.Object, options);

            await adapter.Invoke(new OwinContext(environment));

            nextMock.Verify();
        }

        [Fact]
        public async Task Invoke_SetsResponseStatusCodeAndReasonPhrase()
        {
            var expectedReasonPhrase = "OH NO!";
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = expectedReasonPhrase };
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            Assert.Equal(503, environment["owin.ResponseStatusCode"]);
            Assert.Equal(expectedReasonPhrase, environment["owin.ResponseReasonPhrase"]);
        }

        [Fact]
        public async Task Invoke_SetsResponseHeaders()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Location = new Uri("http://www.location.com/");
            response.Content = new StringContent(@"{""x"":""y""}", Encoding.UTF8, "application/json");
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.Equal(3, responseHeaders.Count);
            Assert.Equal("http://www.location.com/", Assert.Single(responseHeaders["Location"]));
            Assert.Equal("9", Assert.Single(responseHeaders["Content-Length"]));
            Assert.Equal("application/json; charset=utf-8", Assert.Single(responseHeaders["Content-Type"]));
        }

        [Fact]
        public async Task Invoke_SetsResponseBody()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expectedBody = @"{""x"":""y""}";
            response.Content = new StringContent(expectedBody, Encoding.UTF8, "application/json");
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var responseStream = new MemoryStream();
            environment["owin.ResponseBody"] = responseStream;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            responseStream.Seek(0, SeekOrigin.Begin);
            byte[] bodyBytes = new byte[9];
            int charsRead = responseStream.Read(bodyBytes, 0, 9);
            // Assert that we can read 9 characters and no more characters after that
            Assert.Equal(9, charsRead);
            Assert.Equal(-1, responseStream.ReadByte());
            Assert.Equal(expectedBody, Encoding.UTF8.GetString(bodyBytes));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Invoke_RespectsOutputBufferingSetting(bool bufferOutput)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: bufferOutput);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            if (bufferOutput)
            {
                Assert.True(responseHeaders.ContainsKey("Content-Length"));
            }
            else
            {
                Assert.False(responseHeaders.ContainsKey("Content-Length"));
            }
        }

        [Fact]
        public async Task Invoke_IfBufferPolicyDisablesOutputBuffering_DisablesResponseBuffering()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: false);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector)))
            {
                IOwinRequest owinRequest = CreateFakeOwinRequest();
                bool bufferingDisabled = false;
                Action disableBuffering = () => bufferingDisabled = true;
                IOwinResponse owinResponse = CreateFakeOwinResponse(disableBuffering);
                IOwinContext expectedContext = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(expectedContext);

                // Assert
                Assert.True(bufferingDisabled);
            }
        }

        [Fact]
        public async Task Invoke_IfBufferPolicyEnablesOutputBuffering_DoesNotDisableResponseBuffering()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector)))
            {
                IOwinRequest owinRequest = CreateFakeOwinRequest();
                bool bufferingDisabled = false;
                Action disableBuffering = () => bufferingDisabled = true;
                IOwinResponse owinResponse = CreateFakeOwinResponse(disableBuffering);
                IOwinContext expectedContext = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(expectedContext);

                // Assert
                Assert.False(bufferingDisabled);
            }
        }

        [Fact]
        public async Task Invoke_AddsZeroContentLengthHeader_WhenThereIsNoContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.Equal("0", responseHeaders["Content-Length"][0]);
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingChunkedAndContentLengthAreBothSet_IgnoresContentLength()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Hello world")));
            response.Headers.TransferEncodingChunked = true;
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var environment = CreateOwinEnvironment("GET", "http", "localhost", "/vroot", "/api/customers");
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new OwinContext(environment));

            var responseHeaders = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            Assert.False(responseHeaders.ContainsKey("Content-Length"));
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingIsJustChunked_DoesNotCopyHeader()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicy = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicy)))
            {
                response.Headers.TransferEncodingChunked = true;

                IOwinRequest owinRequest = CreateFakeOwinRequest();
                IOwinResponse owinResponse = CreateFakeOwinResponseWithHeaders();
                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(context);

                // Assert
                Assert.DoesNotContain("Transfer-Encoding", owinResponse.Headers.Keys);
            }
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingIsIdentity_DoesCopyHeader()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicy = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicy)))
            {
                response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("identity"));

                IOwinRequest owinRequest = CreateFakeOwinRequest();
                IOwinResponse owinResponse = CreateFakeOwinResponseWithHeaders();
                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(context);

                // Assert
                Assert.Contains("Transfer-Encoding", owinResponse.Headers.Keys);
                Assert.Equal("identity", owinResponse.Headers["Transfer-Encoding"]);
            }
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingIsIdentityChunked_DoesCopyHeader()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicy = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);

            using (HttpResponseMessage response = CreateResponse())
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicy)))
            {
                response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("identity"));
                response.Headers.TransferEncodingChunked = true;
                Assert.Equal("identity, chunked", response.Headers.TransferEncoding.ToString()); // Guard

                IOwinRequest owinRequest = CreateFakeOwinRequest();
                IOwinResponse owinResponse = CreateFakeOwinResponseWithHeaders();
                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                // Act
                await product.Invoke(context);

                // Assert
                Assert.Contains("Transfer-Encoding", owinResponse.Headers.Keys);
                Assert.Equal("identity,chunked", owinResponse.Headers["Transfer-Encoding"]);
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingFaults_DisposesOriginalResponse()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpContent content = CreateFaultingContent())
            using (SpyDisposeHttpResponseMessage spy = new SpyDisposeHttpResponseMessage(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(spy))
            {
                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (MemoryStream output = new MemoryStream())
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);
                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    Assert.True(spy.Disposed);
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingFaults_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage expectedResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse))
            {
                Mock<IExceptionLogger> exceptionLoggerMock = CreateStubExceptionLoggerMock();
                IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

                Mock<IExceptionHandler> exceptionHandlerMock = CreateStubExceptionHandlerMock();
                IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionLogger = exceptionLogger;
                options.ExceptionHandler = exceptionHandler;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act & Assert
                    await Assert.ThrowsAsync<Exception>(() => product.Invoke(context));

                    Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                        c != null
                        && c.Exception == expectedException
                        && c.CatchBlock == OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent
                        && c.Request != null
                        && c.Response == expectedResponse;

                    exceptionLoggerMock.Verify(l => l.LogAsync(
                        It.Is<ExceptionLoggerContext>(c => exceptionContextMatches(c.ExceptionContext)),
                        expectedCancellationToken), Times.Once());
                    exceptionHandlerMock.Verify(h => h.HandleAsync(
                        It.Is<ExceptionHandlerContext>((c) => exceptionContextMatches(c.ExceptionContext)),
                        expectedCancellationToken), Times.Once());
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingCancels_DoesNotCallExceptionServices()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage expectedResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse))
            {
                Mock<IExceptionLogger> exceptionLoggerMock = new Mock<IExceptionLogger>(MockBehavior.Strict);
                IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

                Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
                IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionLogger = exceptionLogger;
                options.ExceptionHandler = exceptionHandler;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act & Assert
                    await Assert.ThrowsAsync<OperationCanceledException>(() => product.Invoke(context));
                }
            }
        }

        [Fact]
        public async Task Invoke_IfExceptionHandlerSetsNullResult_PropogatesFaultedTaskException()
        {
            // Arrange
            Exception expectedException = CreateExceptionWithCallStack();
            string expectedStackTrace = expectedException.StackTrace;

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            {
                IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
                IExceptionHandler exceptionHandler = CreateExceptionHandler(result: null);

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionLogger = exceptionLogger;
                options.ExceptionHandler = exceptionHandler;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act & Assert
                    var exception = await Assert.ThrowsAsync<Exception>(() => product.Invoke(context));
                    Assert.Same(expectedException, exception);
                    Assert.NotNull(exception.StackTrace);
                    Assert.StartsWith(expectedStackTrace, exception.StackTrace);
                }
            }
        }

        [Fact]
        public async Task Invoke_IfExceptionHandlerHandlesException_SendsResponse()
        {
            // Arrange
            Exception expectedException = CreateException();
            string expectedErrorContents = "Sorry";
            HttpStatusCode expectedErrorStatusCode = HttpStatusCode.BadRequest;

            using (HttpContent content = CreateFaultingContent())
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (StringContent errorContent = new StringContent(expectedErrorContents))
            using (HttpResponseMessage errorResponse = CreateResponse(errorContent))
            {
                errorResponse.StatusCode = expectedErrorStatusCode;
                errorResponse.Content = errorContent;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionLogger = CreateStubExceptionLogger();
                options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(errorResponse));

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (MemoryStream output = new MemoryStream())
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();

                    int statusCode = 0;
                    Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
                    mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                    mock.SetupGet(r => r.Body).Returns(output);
                    mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((v) => statusCode = v);
                    IOwinResponse owinResponse = mock.Object;

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    Assert.Equal((int)expectedErrorStatusCode, statusCode);
                    using (HttpRequestMessage request = CreateRequest(includeErrorDetail: true))
                    {
                        Assert.Equal(expectedErrorContents, Encoding.UTF8.GetString(output.ToArray()));
                    }
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingFaultsAndUsingListConstructor_SendsErrorResponse()
        {
            // Arrange
            Exception expectedException = CreateException();
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector)))
            using (MemoryStream output = new MemoryStream())
            {
                IOwinRequest owinRequest = CreateFakeOwinRequest();

                int statusCode = 0;
                Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
                mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                mock.SetupGet(r => r.Body).Returns(output);
                mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((v) => statusCode = v);
                IOwinResponse owinResponse = mock.Object;

                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                // Act
                await product.Invoke(context);

                // Assert
                Assert.Equal(500, statusCode);
                using (HttpRequestMessage request = CreateRequest(includeErrorDetail: true))
                using (HttpResponseMessage expectedResponse = request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError, expectedException))
                {
                    string expectedContents = await expectedResponse.Content.ReadAsStringAsync();
                    Assert.Equal(expectedContents, Encoding.UTF8.GetString(output.ToArray()));
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingErrorFaults_DisposesErrorResponse()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            Exception expectedErrorException = CreateException();

            using (HttpContent content = CreateFaultingContent(expectedOriginalException))
            using (HttpResponseMessage expectedOriginalResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedOriginalResponse))
            using (HttpContent errorContent = CreateFaultingContent(expectedErrorException))
            using (SpyDisposeHttpResponseMessage spy = new SpyDisposeHttpResponseMessage(errorContent))
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(spy));

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    Assert.True(spy.Disposed);
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingErrorFaults_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            Exception expectedErrorException = CreateException();

            using (HttpContent content = CreateFaultingContent(expectedOriginalException))
            using (HttpResponseMessage expectedOriginalResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedOriginalResponse))
            using (HttpContent errorContent = CreateFaultingContent(expectedErrorException))
            using (HttpResponseMessage expectedErrorResponse = CreateResponse(errorContent))
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                Mock<IExceptionLogger> mock = CreateStubExceptionLoggerMock();
                IExceptionLogger exceptionLogger = mock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionLogger = exceptionLogger;
                options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(expectedErrorResponse));

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                        c.ExceptionContext != null
                        && c.ExceptionContext.Exception == expectedOriginalException
                        && c.ExceptionContext.CatchBlock ==
                            OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent
                        && c.ExceptionContext.Request != null
                        && c.ExceptionContext.Response == expectedOriginalResponse),
                        expectedCancellationToken), Times.Once());
                    mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                        c.ExceptionContext != null
                        && c.ExceptionContext.Exception == expectedErrorException
                        && c.ExceptionContext.CatchBlock ==
                            OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferError
                        && c.ExceptionContext.Request != null
                        && c.ExceptionContext.Response == expectedErrorResponse),
                        expectedCancellationToken), Times.Once());
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingErrorCancels_DoesNotCallExceptionLogger()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            Exception expectedErrorException = new OperationCanceledException();

            using (HttpContent content = CreateFaultingContent(expectedOriginalException))
            using (HttpResponseMessage expectedOriginalResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedOriginalResponse))
            using (HttpContent errorContent = CreateFaultingContent(expectedErrorException))
            using (HttpResponseMessage expectedErrorResponse = CreateResponse(errorContent))
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                Mock<IExceptionLogger> mock = CreateStubExceptionLoggerMock();
                IExceptionLogger exceptionLogger = mock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionLogger = exceptionLogger;
                options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(expectedErrorResponse));

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act & Assert
                    await Assert.ThrowsAsync<OperationCanceledException>(() => product.Invoke(context));

                    mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                        c.ExceptionContext != null
                        && c.ExceptionContext.Exception == expectedOriginalException
                        && c.ExceptionContext.CatchBlock ==
                            OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent
                        && c.ExceptionContext.Request != null
                        && c.ExceptionContext.Response == expectedOriginalResponse),
                        expectedCancellationToken), Times.Once());

                    // This shouldn't be called for 'HttpMessageHandlerAdapterBufferError' because that's what's being
                    // cancelled.
                    mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                        c.ExceptionContext != null
                        && c.ExceptionContext.Exception == expectedErrorException
                        && c.ExceptionContext.CatchBlock ==
                            OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferError
                        && c.ExceptionContext.Request != null
                        && c.ExceptionContext.Response == expectedErrorResponse),
                        expectedCancellationToken), Times.Never());
                }
            }
        }

        [Fact]
        public async Task Invoke_IfBufferingErrorFaults_SendsEmptyErrorResponse()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using (HttpContent content = CreateFaultingContent())
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpContent errorContent = CreateFaultingContent())
            using (HttpResponseMessage errorResponse = CreateResponse(errorContent))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            {
                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
                options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(errorResponse));

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (MemoryStream output = new MemoryStream())
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();

                    int statusCode = 0;
                    Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
                    mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                    mock.SetupGet(r => r.Body).Returns(output);
                    mock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((v) => statusCode = v);
                    IOwinResponse owinResponse = mock.Object;

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse, isLocal: true);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    Assert.Equal(500, statusCode);
                    Assert.Equal(new byte[0], output.ToArray());
                }
            }
        }

        [Fact]
        public async Task Invoke_IfStreamingFaults_ReturnsCanceledTask()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: false);

            using (HttpContent content = CreateFaultingContent())
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            using (HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector)))
            {
                IOwinRequest owinRequest = CreateFakeOwinRequest();
                IOwinResponse owinResponse = CreateFakeOwinResponse();

                IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                // Act & Assert
                await Assert.ThrowsAsync<TaskCanceledException>(() => product.Invoke(context));
            }
        }

        [Fact]
        public async Task Invoke_IfStreamingFaults_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedException = CreateException();

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage expectedResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse))
            {
                Mock<IExceptionLogger> mock = CreateStubExceptionLoggerMock();
                IExceptionLogger exceptionLogger = mock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
                options.ExceptionLogger = exceptionLogger;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);

                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act & Assert
                    await Assert.ThrowsAsync<TaskCanceledException>(() => product.Invoke(context));

                    mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>((c) =>
                        c != null
                        && c.ExceptionContext != null
                        && c.ExceptionContext.Exception == expectedException
                        && c.ExceptionContext.CatchBlock ==
                            OwinExceptionCatchBlocks.HttpMessageHandlerAdapterStreamContent
                        && c.ExceptionContext.Request != null
                        && c.ExceptionContext.Response == expectedResponse),
                        expectedCancellationToken), Times.Once());
                }
            }
        }

        [Fact]
        public async Task Invoke_IfStreamingCancels_DoesNotCallExceptionLogger()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            using (HttpContent content = CreateFaultingContent(expectedException))
            using (HttpResponseMessage expectedResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse))
            {
                Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
                IExceptionLogger exceptionLogger = mock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
                options.ExceptionLogger = exceptionLogger;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);

                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    await Assert.ThrowsAsync<OperationCanceledException>(() => product.Invoke(context));
                }
            }
        }

        [Fact]
        public async Task Invoke_IfTryComputeLengthThrows_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedException = CreateException();

            using (HttpContent content = CreateThrowingContent(expectedException))
            using (HttpResponseMessage expectedResponse = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse))
            {
                Mock<IExceptionLogger> mock = CreateStubExceptionLoggerMock();
                IExceptionLogger exceptionLogger = mock.Object;

                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
                options.ExceptionLogger = exceptionLogger;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
                {
                    CancellationToken expectedCancellationToken = tokenSource.Token;
                    IOwinRequest owinRequest = CreateFakeOwinRequest(expectedCancellationToken);

                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>((c) =>
                        c != null
                        && c.ExceptionContext != null
                        && c.ExceptionContext.Exception == expectedException
                        && c.ExceptionContext.CatchBlock ==
                            OwinExceptionCatchBlocks.HttpMessageHandlerAdapterComputeContentLength
                        && c.ExceptionContext.Request != null
                        && c.ExceptionContext.Response == expectedResponse),
                        expectedCancellationToken), Times.Once());
                }
            }
        }

        [Fact]
        public async Task Invoke_IfTryComputeLengthThrows_SendsEmptyErrorResponse()
        {
            // Arrange
            Exception expectedException = CreateException();

            using (HttpContent content = CreateThrowingContent(expectedException))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            {
                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();

                    Mock<IOwinResponse> owinResponseMock = new Mock<IOwinResponse>(MockBehavior.Strict);
                    int statusCode = 0;
                    owinResponseMock.SetupGet(r => r.Environment).Returns((IDictionary<string, object>)null);
                    owinResponseMock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>(s => statusCode = s);
                    int? contentLength = null;
                    Mock<IHeaderDictionary> headersMock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
                    headersMock
                        .As<IDictionary<string, string[]>>()
                        .SetupSet(h => h["Content-Length"] = It.IsAny<string[]>())
                        .Callback<string, string[]>((i, v) => contentLength = int.Parse(v[0]));
                    IHeaderDictionary headers = headersMock.Object;
                    owinResponseMock.SetupGet(r => r.Headers).Returns(headers);
                    IOwinResponse owinResponse = owinResponseMock.Object;

                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    Assert.Equal(500, statusCode);
                    Assert.Equal(0, contentLength);
                }
            }
        }

        [Fact]
        public async Task Invoke_IfTryComputeLengthThrows_DoesNotEvaluateOutputBufferPolicy()
        {
            // Arrange
            using (HttpContent content = CreateThrowingContent(CreateException()))
            using (HttpResponseMessage response = CreateResponse(content))
            using (HttpMessageHandler messageHandler = CreateStubMessageHandler(response))
            {
                HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);

                Mock<IHostBufferPolicySelector> bufferPolicyMock =
                    new Mock<IHostBufferPolicySelector>(MockBehavior.Strict);
                bufferPolicyMock
                    .Setup(p => p.UseBufferedInputStream(It.IsAny<object>()))
                    .Returns(false);
                bool calledUseBufferedOutputStream = false;
                bufferPolicyMock
                    .Setup(p => p.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>()))
                    .Callback(() => calledUseBufferedOutputStream = true)
                    .Returns(false);
                options.BufferPolicySelector = bufferPolicyMock.Object;

                using (HttpMessageHandlerAdapter product = CreateProductUnderTest(options))
                {
                    IOwinRequest owinRequest = CreateFakeOwinRequest();
                    IOwinResponse owinResponse = CreateFakeOwinResponse(Stream.Null);
                    IOwinContext context = CreateStubOwinContext(owinRequest, owinResponse);

                    // Act
                    await product.Invoke(context);

                    // Assert
                    Assert.False(calledUseBufferedOutputStream);
                }
            }
        }

        private static IHostBufferPolicySelector CreateBufferPolicySelector(bool bufferInput, bool bufferOutput)
        {
            var mock = new Mock<IHostBufferPolicySelector>();
            mock.Setup(bps => bps.UseBufferedInputStream(It.IsAny<object>())).Returns(bufferInput);
            mock.Setup(bps => bps.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>())).Returns(bufferOutput);
            return mock.Object;
        }

        private static CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static IHostBufferPolicySelector CreateDummyBufferPolicy()
        {
            return new Mock<IHostBufferPolicySelector>(MockBehavior.Strict).Object;
        }

        private static IExceptionHandler CreateDummyExceptionHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IExceptionLogger CreateDummyExceptionLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private static HttpMessageHandler CreateDummyMessageHandler()
        {
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock.As<IDisposable>().Setup(c => c.Dispose());
            return mock.Object;
        }

        private static HttpMessageHandlerOptions CreateDummyOptions(HttpMessageHandler messageHandler)
        {
            return new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = CreateDummyBufferPolicy(),
                ExceptionLogger = CreateDummyExceptionLogger(),
                ExceptionHandler = CreateDummyExceptionHandler()
            };
        }

        private static Exception CreateException()
        {
            return new Exception();
        }

        private static IExceptionHandler CreateExceptionHandler(IHttpActionResult result)
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Callback<ExceptionHandlerContext, CancellationToken>((c, i) => c.Result = result)
                .Returns(Task.FromResult(0));
            return mock.Object;
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

        private static IHeaderDictionary CreateFakeHeaders()
        {
            return new HeaderDictionary(new Dictionary<string, string[]>());
        }

        private static IOwinRequest CreateFakeOwinRequest()
        {
            return CreateFakeOwinRequest(CancellationToken.None);
        }

        private static IOwinRequest CreateFakeOwinRequest(CancellationToken cancellationToken)
        {
            return CreateFakeOwinRequestMock(cancellationToken).Object;
        }

        private static IOwinRequest CreateFakeOwinRequest(Action disableBuffering)
        {
            Mock<IDictionary<string, object>> environmentMock = new Mock<IDictionary<string, object>>(MockBehavior.Strict);
            object disableBufferingValue = disableBuffering;
            environmentMock.Setup(d => d.TryGetValue("server.DisableRequestBuffering", out disableBufferingValue)).Returns(true);
            IDictionary<string, object> environment = environmentMock.Object;

            Mock<IOwinRequest> mock = CreateFakeOwinRequestMock(CancellationToken.None);
            mock.SetupGet(r => r.Environment).Returns(environment);
            return mock.Object;
        }

        private static Mock<IOwinRequest> CreateFakeOwinRequestMock(CancellationToken cancellationToken)
        {
            Mock<IHeaderDictionary> headersMock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            headersMock.Setup(h => h.GetEnumerator()).Returns(
                new Mock<IEnumerator<KeyValuePair<string, string[]>>>().Object);

            Mock<IOwinRequest> mock = new Mock<IOwinRequest>(MockBehavior.Strict);
            mock.SetupGet(r => r.Method).Returns("GET");
            mock.SetupGet(r => r.Uri).Returns(new Uri("http://ignore"));
            mock.SetupGet(r => r.Body).Returns(Stream.Null);
            mock.SetupGet(r => r.Headers).Returns(headersMock.Object);
            mock.SetupGet(r => r.User).Returns((IPrincipal)null);
            mock.SetupGet(r => r.CallCancelled).Returns(cancellationToken);
            mock.SetupGet(r => r.Environment).Returns((IDictionary<string, object>)null);
            return mock;
        }

        private static IOwinResponse CreateFakeOwinResponse()
        {
            return CreateFakeOwinResponseMock().Object;
        }

        private static IOwinResponse CreateFakeOwinResponse(Stream body)
        {
            Mock<IOwinResponse> mock = CreateFakeOwinResponseMock();
            mock.SetupGet(r => r.Body).Returns(body);
            return mock.Object;
        }

        private static IOwinResponse CreateFakeOwinResponse(Action disableBuffering)
        {
            Mock<IDictionary<string, object>> environmentMock = new Mock<IDictionary<string, object>>(MockBehavior.Strict);
            object disableBufferingValue = disableBuffering;
            environmentMock.Setup(d => d.TryGetValue("server.DisableResponseBuffering", out disableBufferingValue)).Returns(true);
            IDictionary<string, object> environment = environmentMock.Object;

            Mock<IOwinResponse> mock = CreateFakeOwinResponseMock();
            mock.SetupGet(r => r.Environment).Returns(environment);
            return mock.Object;
        }

        private static Mock<IOwinResponse> CreateFakeOwinResponseMock()
        {
            Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
            mock.Setup(r => r.Headers).Returns(new Mock<IHeaderDictionary>().Object);
            return mock;
        }

        private static IOwinResponse CreateFakeOwinResponseWithHeaders()
        {
            Mock<IOwinResponse> mock = new Mock<IOwinResponse>();
            IHeaderDictionary headers = CreateFakeHeaders();
            mock.Setup(r => r.Headers).Returns(headers);
            return mock.Object;
        }

        private static Task CreateFaultedTask(Exception exception)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            source.SetException(exception);
            return source.Task;
        }

        private static FaultingHttpContent CreateFaultingContent()
        {
            return CreateFaultingContent(CreateException());
        }

        private static FaultingHttpContent CreateFaultingContent(Exception exception)
        {
            return new FaultingHttpContent(exception);
        }

        private static MediaTypeFormatter CreateFaultingFormatter(Exception exception)
        {
            Mock<MediaTypeFormatter> mock = new Mock<MediaTypeFormatter>();
            mock.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            mock.Setup(f => f.GetPerRequestFormatterInstance(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(),
                It.IsAny<MediaTypeHeaderValue>())).Returns(mock.Object);
            mock
                .Setup(f => f.WriteToStreamAsync(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<Stream>(),
                    It.IsAny<HttpContent>(), It.IsAny<TransportContext>()))
                .Returns(CreateFaultedTask(exception));
            return mock.Object;
        }

        private static HttpMessageHandler CreateLambdaMessageHandler(
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            return new LambdaHttpMessageHandler(sendAsync);
        }

        private static HandlerStub CreateOKHandlerStub()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return new HandlerStub() { Response = response };
        }

        private static Dictionary<string, object> CreateOwinEnvironment(string method, string scheme, string hostHeaderValue, string pathBase, string path)
        {
            var environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            environment["owin.RequestMethod"] = method;
            environment["owin.RequestScheme"] = scheme;
            environment["owin.RequestHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { { "Host", new string[] { hostHeaderValue } } };
            environment["owin.RequestPathBase"] = pathBase;
            environment["owin.RequestPath"] = path;
            environment["owin.RequestQueryString"] = "";
            environment["owin.RequestBody"] = new MemoryStream();
            environment["owin.CallCancelled"] = new CancellationToken();
            environment["owin.ResponseHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment["owin.ResponseBody"] = new MemoryStream();
            return environment;
        }

        private static HttpMessageHandlerAdapter CreateProductUnderTest(HttpMessageHandlerOptions options)
        {
            return CreateProductUnderTest(next: null, options: options);
        }

        private static HttpMessageHandlerAdapter CreateProductUnderTest(OwinMiddleware next, HttpMessageHandlerOptions options)
        {
            return new HttpMessageHandlerAdapter(next, options);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestMessage CreateRequest(bool includeErrorDetail)
        {
            HttpRequestMessage request = CreateRequest();
            request.SetRequestContext(new HttpRequestContext
            {
                IncludeErrorDetail = includeErrorDetail
            });
            return request;
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static HttpResponseMessage CreateResponse(HttpContent content)
        {
            return new HttpResponseMessage
            {
                Content = content
            };
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

        private static HttpMessageHandler CreateStubMessageHandler(HttpResponseMessage response)
        {
            return new LambdaHttpMessageHandler((r, c) => Task.FromResult(response));
        }

        private static IOwinContext CreateStubOwinContext(IOwinRequest request, IOwinResponse response)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.Setup(c => c.Request).Returns(request);
            mock.Setup(c => c.Response).Returns(response);
            return mock.Object;
        }

        private static IOwinContext CreateStubOwinContext(IOwinRequest request, IOwinResponse response, bool isLocal)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.Setup(c => c.Request).Returns(request);
            mock.Setup(c => c.Response).Returns(response);
            mock.Setup(c => c.Get<bool>("server.IsLocal")).Returns(isLocal);
            return mock.Object;
        }

        private static ThrowingHttpContent CreateThrowingContent(Exception exception)
        {
            return new ThrowingHttpContent(exception);
        }

        private static HttpMessageHandlerOptions CreateValidOptions(HttpMessageHandler messageHandler)
        {
            IHostBufferPolicySelector bufferPolicy =
                CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            return CreateValidOptions(messageHandler, bufferPolicy);
        }

        private static HttpMessageHandlerOptions CreateValidOptions(HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
        {
            return new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = bufferPolicySelector,
                ExceptionLogger = new EmptyExceptionLogger(),
                ExceptionHandler = new DefaultExceptionHandler()
            };
        }

        public class HandlerStub : HttpMessageHandler
        {
            public HttpRequestMessage Request { get; private set; }
            public CancellationToken CancellationToken { get; private set; }
            public HttpResponseMessage Response { get; set; }
            public IPrincipal User { get; set; }
            public bool AddNoRouteMatchedKey { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                CancellationToken = cancellationToken;
                User = Thread.CurrentPrincipal;

                if (AddNoRouteMatchedKey)
                {
                    request.Properties[HttpPropertyKeys.NoRouteMatched] = true;
                }

                return Task.FromResult<HttpResponseMessage>(Response);
            }
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }

        private class FaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public FaultingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        private class SpyDisposeFaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public SpyDisposeFaultingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            public bool Disposed { get; private set; }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class SpyDisposeHttpMessageHandler : HttpMessageHandler
        {
            public bool Disposed { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
            }
        }

        private class SpyDisposeHttpResponseMessage : HttpResponseMessage
        {
            public SpyDisposeHttpResponseMessage(HttpContent content)
            {
                Content = content;
            }

            public bool Disposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class SpyDisposeStream : Stream
        {
            public bool Disposed { get; private set; }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class ThrowingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public ThrowingHttpContent(Exception exception)
            {
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

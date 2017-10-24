// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost
{
    public class WebHostExceptionHandlerTests
    {
        [Fact]
        public void InnerHandler_IsSpecifiedInstance()
        {
            // Arrange
            IExceptionHandler expectedHandler = CreateDummyHandler();
            WebHostExceptionHandler product = CreateProductUnderTest(expectedHandler);

            // Act
            IExceptionHandler handler = product.InnerHandler;

            // Assert
            Assert.Same(expectedHandler, handler);
        }

        [Fact]
        public void HandleAsync_DelegatesToInnerHandler()
        {
            // Arrange
            Task expectedTask = CreateTask();
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>();
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(expectedTask);
            IExceptionHandler innerHandler = mock.Object;

            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            ExceptionHandlerContext expectedContext = CreateContext();

            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                Task task = product.HandleAsync(expectedContext, expectedCancellationToken);

                // Assert
                mock.Verify(h => h.HandleAsync(expectedContext, expectedCancellationToken), Times.Once());
                Assert.Same(expectedTask, task);
            }
        }

        [Fact]
        public void HandleAsync_IfContextIsNull_Throws()
        {
            // Arrange
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            ExceptionHandlerContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.HandleAsync(context, cancellationToken), "context");
        }

        [Fact]
        public async Task HandleAsync_IfCatchBlockIsWebHostBufferedContent_HandlesWithCustomException()
        {
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            {
                originalResponse.Content = new StringContent("Error");
                originalResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent);
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                IHttpActionResult result = context.Result;
                ResponseMessageResult typedResult = Assert.IsType<ResponseMessageResult>(result);

                using (HttpResponseMessage response = typedResult.Response)
                using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                    HttpStatusCode.InternalServerError, new InvalidOperationException("The 'StringContent' type " +
                        "failed to serialize the response body for content type 'text/plain'.",
                        context.ExceptionContext.Exception)))
                {
                    AssertErrorResponse(expectedResponse, response);
                }
            }
        }

        [Fact]
        public async Task HandleAsync_IfCatchBlockIsWebHostBufferedContent_WithoutContentType_HandlesWithCustomException()
        {
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            {
                originalResponse.Content = new StringContent("Error");
                originalResponse.Content.Headers.ContentType = null;
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent);
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                IHttpActionResult result = context.Result;
                ResponseMessageResult typedResult = Assert.IsType<ResponseMessageResult>(result);

                using (HttpResponseMessage response = typedResult.Response)
                using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                    HttpStatusCode.InternalServerError, new InvalidOperationException(
                        "The 'StringContent' type failed to serialize the response body.",
                        context.ExceptionContext.Exception)))
                {
                    AssertErrorResponse(expectedResponse, response);
                }
            }
        }

        [Fact]
        public async Task HandleAsync_IfCatchBlockIsWebHostBufferedContent_WithFailedNegotiation_HandlesWithCustomException()
        {
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.Formatters.Clear();

                originalResponse.Content = new StringContent("Error");
                expectedRequest.SetRequestContext(new HttpRequestContext
                {
                    IncludeErrorDetail = true,
                    Configuration = configuration
                });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent);
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                IHttpActionResult result = context.Result;
                ResponseMessageResult typedResult = Assert.IsType<ResponseMessageResult>(result);
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Null(response.Content);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task HandleAsync_IfCatchBlockIsWebHostBufferedContent_WithCreateException_HandlesWithCustomException()
        {
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.Services.Clear(typeof(IContentNegotiator));

                originalResponse.Content = new StringContent("Error");
                expectedRequest.SetRequestContext(new HttpRequestContext
                {
                    IncludeErrorDetail = true,
                    Configuration = configuration
                });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent);
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                IHttpActionResult result = context.Result;
                ResponseMessageResult typedResult = Assert.IsType<ResponseMessageResult>(result);
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Null(response.Content);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_AndRequestIsNull_Throws()
        {
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            // Arrange
            using (HttpResponseMessage response = CreateResponse())
            {
                ExceptionHandlerContext context = CreateContext(
                    CreateMinimalValidExceptionContext(WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent));

                Assert.Null(context.ExceptionContext.Request); // Guard
                CancellationToken cancellationToken = CancellationToken.None;

                // Act & Assert
                Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                    "ExceptionContext.Request must not be null.");
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_AndResponseIsNull_Throws()
        {
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionHandlerContext context = CreateValidContext(request,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent);
                Assert.Null(context.ExceptionContext.Response); // Guard
                CancellationToken cancellationToken = CancellationToken.None;

                // Act & Assert
                Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                    "ExceptionContext.Response must not be null.");
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_AndResponseContentIsNull_Throws()
        {
            // Arrange
            IExceptionHandler innerHandler = CreateDummyHandler();
            IExceptionHandler product = CreateProductUnderTest(innerHandler);

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                ExceptionHandlerContext context = CreateValidContext(request,
                    WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent);
                context.ExceptionContext.Response = response;
                Assert.Null(context.ExceptionContext.Request.Content); // Guard
                CancellationToken cancellationToken = CancellationToken.None;

                // Act & Assert
                Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                    "HttpResponseMessage.Content must not be null.");
            }
        }

        private static void AssertErrorResponse(HttpResponseMessage expected, HttpResponseMessage actual)
        {
            Assert.NotNull(expected); // Guard
            ObjectContent<HttpError> expectedContent = Assert.IsType<ObjectContent<HttpError>>(expected.Content); // Guard
            Assert.NotNull(expectedContent.Formatter); // Guard

            Assert.NotNull(actual);
            Assert.Equal(expected.StatusCode, actual.StatusCode);
            ObjectContent<HttpError> actualContent = Assert.IsType<ObjectContent<HttpError>>(actual.Content);
            Assert.NotNull(actualContent.Formatter);
            Assert.Same(expectedContent.Formatter.GetType(), actualContent.Formatter.GetType());
            Assert.Equal(Flatten(expectedContent.Value), Flatten(actualContent.Value));
            Assert.Same(expected.RequestMessage, actual.RequestMessage);
        }

        private static object Flatten(object obj)
        {
            IDictionary<string, object> dictionary = obj as IDictionary<string, object>;

            if (dictionary == null)
            {
                return obj;
            }

            IDictionary<string, object> flattened = new Dictionary<string, object>();
            AddValues(dictionary, null, flattened);
            return flattened;
        }

        private static void AddValues(IDictionary<string, object> source, string prefix,
            IDictionary<string, object> destination)
        {
            foreach (string key in source.Keys)
            {
                object value = source[key];
                IDictionary<string, object> dictionaryValue = value as IDictionary<string, object>;

                string prefixedKey = prefix != null ? prefix + "." + key : key;

                if (dictionaryValue != null)
                {
                    destination.Add(prefixedKey, "<Flattened>");
                    AddValues(dictionaryValue, prefixedKey, destination);
                }
                else
                {
                    destination.Add(prefixedKey, value);
                }
            }
        }

        private static CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static ExceptionHandlerContext CreateContext()
        {
            return CreateContext(new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer));
        }

        private static ExceptionHandlerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }

        private static IExceptionHandler CreateDummyHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IHttpActionResult CreateDummyResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static WebHostExceptionHandler CreateProductUnderTest(IExceptionHandler innerHandler)
        {
            return new WebHostExceptionHandler(innerHandler);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static Task CreateTask()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            return source.Task;
        }

        private static ExceptionHandlerContext CreateValidContext(HttpRequestMessage request,
            ExceptionContextCatchBlock catchBlock)
        {
            return CreateContext(CreateMinimalValidExceptionContext(catchBlock, request));
        }

        private static ExceptionContext CreateMinimalValidExceptionContext(ExceptionContextCatchBlock catchBlock, HttpRequestMessage request = null)
        {
            return new ExceptionContext(new InvalidOperationException(), catchBlock)
                        {
                            Request = request,
                        };
        }
    }
}

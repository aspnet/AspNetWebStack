// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class DefaultExceptionHandlerTests
    {
        [Fact]
        public void HandleAsync_IfContextIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();
            ExceptionHandlerContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.HandleAsync(context, cancellationToken), "context");
        }

        [Fact]
        public void HandleAsync_IfRequestIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();
            ExceptionHandlerContext context = new ExceptionHandlerContext(new ExceptionContext(CreateException(), ExceptionCatchBlocks.HttpServer));
            Assert.Null(context.ExceptionContext.Request); // Guard
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                "ExceptionContext.Request must not be null.");
        }

        [Fact]
        public async Task HandleAsync_HandlesExceptionViaCreateErrorResponse()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest);
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                IHttpActionResult result = context.Result;
                ResponseMessageResult typedResult = Assert.IsType<ResponseMessageResult>(result);
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);

                    using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                        HttpStatusCode.InternalServerError, context.ExceptionContext.Exception))
                    {
                        AssertErrorResponse(expectedResponse, response);
                    }

                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task HandleAsync_IfCatchBlockIsIExceptionFilter_LeavesExceptionUnhandled()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionHandlerContext context = CreateValidContext(request, ExceptionCatchBlocks.IExceptionFilter);
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                Assert.Null(context.Result);
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
            Assert.Equal(expectedContent.Value, actualContent.Value);
            Assert.Same(expected.RequestMessage, actual.RequestMessage);
        }

        private static ExceptionHandlerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }

        private static Exception CreateException()
        {
            return new NotSupportedException();
        }

        private static DefaultExceptionHandler CreateProductUnderTest()
        {
            return new DefaultExceptionHandler();
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static ExceptionHandlerContext CreateValidContext(HttpRequestMessage request)
        {
            return CreateContext(new ExceptionContext(CreateException(), ExceptionCatchBlocks.HttpServer, request));
        }

        private static ExceptionHandlerContext CreateValidContext(HttpRequestMessage request,
            ExceptionContextCatchBlock catchBlock)
        {
            return CreateContext(new ExceptionContext(CreateException(), catchBlock, request));
        }
    }
}

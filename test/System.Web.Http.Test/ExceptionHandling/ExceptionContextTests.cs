// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionContextTests
    {
        [Fact]
        public void ConstructorWithoutArguments_SetsPropertiesToSpecifiedValues()
        {
            // Act
            ExceptionContext product = CreateProductUnderTest();

            // Assert
            Assert.NotNull(product.Exception);
            Assert.NotNull(product.CatchBlock);

            Assert.Null(product.ActionContext);
            Assert.Null(product.ControllerContext);
            Assert.Null(product.RequestContext);
            Assert.Null(product.Request);
            Assert.Null(product.Response);
        }

        [Fact]
        public void ConstructorWithActionContext_SetsPropertiesToSpecifiedValues()
        {
            // Arrange
            Exception expectedException = CreateException();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpControllerContext expectedControllerContext = CreateControllerContext(expectedRequestContext,
                    expectedRequest);
                HttpActionContext expectedActionContext = CreateActionContext(expectedControllerContext);
                ExceptionContextCatchBlock expectedCatchBlock = CreateCatchBlock();

                // Act
                ExceptionContext product = CreateProductUnderTest(expectedException, expectedCatchBlock,
                    expectedActionContext);

                // Assert
                Assert.Same(expectedException, product.Exception);
                Assert.Same(expectedCatchBlock, product.CatchBlock);
                Assert.Same(expectedActionContext, product.ActionContext);
                Assert.Same(expectedControllerContext, product.ControllerContext);
                Assert.Same(expectedRequestContext, product.RequestContext);
                Assert.Same(expectedRequest, product.Request);
                Assert.Null(product.Response);
            }
        }

        [Fact]
        public void ConstructorWithActionContext_IfExceptionIsNull_Throws()
        {
            // Arrange
            Exception exception = null;
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpControllerContext controllerContext = CreateControllerContext(requestContext, request);
                HttpActionContext actionContext = CreateActionContext(controllerContext);

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, actionContext),
                    "exception");
            }
        }

        [Fact]
        public void ConstructorWithActionContext_IfCatchBlockIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = null;
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpControllerContext controllerContext = CreateControllerContext(requestContext, request);
                HttpActionContext actionContext = CreateActionContext(controllerContext);

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, actionContext),
                    "catchBlock");
            }
        }

        [Fact]
        public void ConstructorWithActionContext_IfActionContextIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpActionContext actionContext = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, actionContext),
                "actionContext");
        }

        [Fact]
        public void ConstructorWithActionContext_IfControllerContextIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpActionContext actionContext = new HttpActionContext();
            Assert.Null(actionContext.ControllerContext); // Guard

            // Act & Assert
            Assert.ThrowsArgument(() => CreateProductUnderTest(exception, catchBlock, actionContext), "actionContext",
                "HttpActionContext.ControllerContext must not be null.");
        }

        [Fact]
        public void ConstructorWithActionContext_IfRequestIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpRequestContext requestContext = CreateRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                RequestContext = requestContext
            };
            Assert.Null(controllerContext.Request); // Guard
            HttpActionContext actionContext = CreateActionContext(controllerContext);

            // Act & Assert
            Assert.ThrowsArgument(() => CreateProductUnderTest(exception, catchBlock, actionContext), "actionContext",
                "HttpControllerContext.Request must not be null");
        }

        [Fact]
        public void ConstructorWithRequest_SetsPropertiesToSpecifiedValues()
        {
            // Arrange
            Exception expectedException = CreateException();
            ExceptionContextCatchBlock expectedCatchBlock = CreateCatchBlock();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                expectedRequest.SetRequestContext(expectedRequestContext);

                // Act
                ExceptionContext product = CreateProductUnderTest(expectedException, expectedCatchBlock,
                    expectedRequest);

                // Assert
                Assert.Same(expectedException, product.Exception);
                Assert.Same(expectedCatchBlock, product.CatchBlock);
                Assert.Same(expectedRequest, product.Request);
                Assert.Same(expectedRequestContext, product.RequestContext);
                Assert.Null(product.ControllerContext);
                Assert.Null(product.ActionContext);
                Assert.Null(product.Response);
            }
        }

        [Fact]
        public void ConstructorWithRequest_IfExceptionIsNull_Throws()
        {
            // Arrange
            Exception exception = null;
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(requestContext);

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request), "exception");
            }
        }

        [Fact]
        public void ConstructorWithRequest_IfCatchBlockIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = null;
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(requestContext);

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request), "catchBlock");
            }
        }

        [Fact]
        public void ConstructorWithRequest_IfRequestIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request), "request");
        }

        [Fact]
        public void ConstructorWithRequest_IfRequestContextIsNull_IgnoresRequestContext()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();

            using (HttpRequestMessage request = CreateRequest())
            {
                Assert.Null(request.GetRequestContext()); // Guard

                // Act
                ExceptionContext product = CreateProductUnderTest(exception, catchBlock, request);

                // Assert
                Assert.Null(product.RequestContext);
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_SetsPropertiesToSpecifiedValues()
        {
            // Arrange
            Exception expectedException = CreateException();
            ExceptionContextCatchBlock expectedCatchBlock = CreateCatchBlock();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                expectedRequest.SetRequestContext(expectedRequestContext);

                // Act
                ExceptionContext product = CreateProductUnderTest(expectedException, expectedCatchBlock,
                    expectedRequest, expectedResponse);

                // Assert
                Assert.Same(expectedException, product.Exception);
                Assert.Same(expectedCatchBlock, product.CatchBlock);
                Assert.Same(expectedRequest, product.Request);
                Assert.Same(expectedRequestContext, product.RequestContext);
                Assert.Same(expectedResponse, product.Response);
                Assert.Null(product.ControllerContext);
                Assert.Null(product.ActionContext);
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfExceptionIsNull_Throws()
        {
            // Arrange
            Exception exception = null;
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                request.SetRequestContext(requestContext);

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request, response),
                    "exception");
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfCatchBlockIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = null;
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                request.SetRequestContext(requestContext);

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request, response),
                    "catchBlock");
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfRequestIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();
            HttpRequestMessage request = null;

            using (HttpResponseMessage response = CreateResponse())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request, response),
                    "request");
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfRequestContextIsNull_IgnoresRequestContext()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                Assert.Null(request.GetRequestContext()); // Guard

                // Act
                ExceptionContext product = CreateProductUnderTest(exception, catchBlock, request, response);

                // Assert
                Assert.Null(product.RequestContext);
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfResponseIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            ExceptionContextCatchBlock catchBlock = CreateCatchBlock();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpResponseMessage response = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, catchBlock, request, response),
                    "response");
            }
        }

        private static HttpActionContext CreateActionContext()
        {
            return new HttpActionContext();
        }

        private static HttpActionContext CreateActionContext(HttpControllerContext controllerContext)
        {
            return new HttpActionContext
            {
                ControllerContext = controllerContext
            };
        }

        private static ExceptionContextCatchBlock CreateCatchBlock()
        {
            return new ExceptionContextCatchBlock("IgnoreCaughtAt", isTopLevel: false, callsHandler: false);
        }

        private static HttpControllerContext CreateControllerContext()
        {
            return new HttpControllerContext();
        }

        private static HttpControllerContext CreateControllerContext(HttpRequestContext requestContext,
            HttpRequestMessage request)
        {
            return new HttpControllerContext
            {
                RequestContext = requestContext,
                Request = request
            };
        }

        private static Exception CreateException()
        {
            return new DivideByZeroException();
        }

        private static ExceptionContext CreateProductUnderTest()
        {
            return new ExceptionContext(CreateException(), CreateCatchBlock());
        }

        private static ExceptionContext CreateProductUnderTest(Exception exception,
            ExceptionContextCatchBlock catchBlock, HttpActionContext actionContext)
        {
            return new ExceptionContext(exception, catchBlock, actionContext);
        }

        private static ExceptionContext CreateProductUnderTest(Exception exception,
            ExceptionContextCatchBlock catchBlock, HttpRequestMessage request)
        {
            return new ExceptionContext(exception, catchBlock, request);
        }

        private static ExceptionContext CreateProductUnderTest(Exception exception,
            ExceptionContextCatchBlock catchBlock, HttpRequestMessage request, HttpResponseMessage response)
        {
            return new ExceptionContext(exception, catchBlock, request, response);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestContext CreateRequestContext()
        {
            return new HttpRequestContext();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }
    }
}

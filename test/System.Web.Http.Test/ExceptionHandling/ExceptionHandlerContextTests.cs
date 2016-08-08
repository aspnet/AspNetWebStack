// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerContextTests
    {
        [Fact]
        public void Constructor_IfExceptionContextIsNull_Throws()
        {
            // Arrange
            ExceptionContext context = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(context), "exceptionContext");
        }

        [Fact]
        public void ExceptionContextGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            ExceptionContext expectedContext = CreateMinimalContext();
            ExceptionHandlerContext product = CreateProductUnderTest(expectedContext);

            // Act
            ExceptionContext context = product.ExceptionContext;

            // Assert
            Assert.Same(expectedContext, context);
        }

        [Fact]
        public void ResultSet_UpdatesValue()
        {
            // Arrange
            ExceptionHandlerContext product = CreateProductUnderTest(CreateMinimalContext());
            IHttpActionResult expectedResult = CreateDummyResult();

            // Act
            product.Result = expectedResult;

            // Assert
            IHttpActionResult result = product.Result;
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ExceptionGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            Exception expectedException = new InvalidOperationException();
            ExceptionContext context = new ExceptionContext(expectedException, ExceptionCatchBlocks.HttpServer);
            ExceptionHandlerContext product = CreateProductUnderTest(context);

            // Act
            Exception exception = product.Exception;

            // Assert
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void CatchBlockGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            ExceptionContextCatchBlock expectedCatchBlock = new ExceptionContextCatchBlock("IgnoreName", false, false);
            ExceptionContext context = new ExceptionContext(new Exception(), expectedCatchBlock);
            ExceptionHandlerContext product = CreateProductUnderTest(context);

            // Act
            ExceptionContextCatchBlock catchBlock = product.CatchBlock;

            // Assert
            Assert.Same(expectedCatchBlock, catchBlock);
        }

        [Fact]
        public void RequestGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            {
                ExceptionContext context = CreateMinimalContext(expectedRequest);
                ExceptionHandlerContext product = CreateProductUnderTest(context);

                // Act
                HttpRequestMessage request = product.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void RequestContextGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpRequestContext expectedRequestContext = new HttpRequestContext();
            ExceptionContext context = CreateMinimalContext(expectedRequestContext);
            ExceptionHandlerContext product = CreateProductUnderTest(context);

            // Act
            HttpRequestContext requestContext = product.RequestContext;

            // Assert
            Assert.Same(expectedRequestContext, requestContext);
        }

        private static ExceptionContext CreateMinimalContext(HttpRequestContext context = null)
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer)
            {
                RequestContext = context,
            };
        }

        private static ExceptionContext CreateMinimalContext(HttpRequestMessage request)
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer, request);
        }

        private static IHttpActionResult CreateDummyResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static ExceptionHandlerContext CreateProductUnderTest(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionLoggerContextTests
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
            ExceptionContext expectedContext = CreateMinimalValidContext();
            ExceptionLoggerContext product = CreateProductUnderTest(expectedContext);

            // Act
            ExceptionContext context = product.ExceptionContext;

            // Assert
            Assert.Same(expectedContext, context);
        }

        [Fact]
        public void ExceptionGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            Exception expectedException = new InvalidOperationException();
            ExceptionContext context = new ExceptionContext(expectedException, ExceptionCatchBlocks.HttpServer);
            ExceptionLoggerContext product = CreateProductUnderTest(context);

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
            ExceptionLoggerContext product = CreateProductUnderTest(context);

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
                ExceptionContext context = new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer, expectedRequest);

                ExceptionLoggerContext product = CreateProductUnderTest(context);

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
            ExceptionContext context = CreateMinimalValidContext(expectedRequestContext);
            ExceptionLoggerContext product = CreateProductUnderTest(context);

            // Act
            HttpRequestContext requestContext = product.RequestContext;

            // Assert
            Assert.Same(expectedRequestContext, requestContext);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CallsHandlerGet_ReturnsCatchBlockCallsHandler(bool expectedCallsHandler)
        {
            // Arrange
            ExceptionContext context = new ExceptionContext(new Exception(),
                new ExceptionContextCatchBlock("IgnoreName", isTopLevel: false,
                    callsHandler: expectedCallsHandler));

            ExceptionLoggerContext product = CreateProductUnderTest(context);

            // Act
            bool callsHandler = product.CallsHandler;

            // Assert
            Assert.Equal(expectedCallsHandler, callsHandler);
        }

        private static ExceptionContext CreateMinimalValidContext(HttpRequestContext context = null)
        {
            return new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer)
            {
                RequestContext = context,
            };
        }

        private static ExceptionLoggerContext CreateProductUnderTest(ExceptionContext exceptionContext)
        {
            return new ExceptionLoggerContext(exceptionContext);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.ExceptionServices;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.WebHost.Routing
{
    public class HttpRouteExceptionRouteHandlerTests
    {
        [Fact]
        public void ExceptionInfo_ReturnsSpecifiedInstance()
        {
            // Arrange
            ExceptionDispatchInfo expectedExceptionInfo = CreateExceptionInfo();
            HttpRouteExceptionRouteHandler product = CreateProductUnderTest(expectedExceptionInfo);

            // Act
            ExceptionDispatchInfo exceptionInfo = product.ExceptionInfo;

            // Assert
            Assert.Same(exceptionInfo, expectedExceptionInfo);
        }

        [Fact]
        public void GetHttpHandler_ReturnsExceptionHandlerWithExceptionInfo()
        {
            // Arrange
            ExceptionDispatchInfo expectedExceptionInfo = CreateExceptionInfo();
            IRouteHandler product = CreateProductUnderTest(expectedExceptionInfo);
            RequestContext requestContext = null;

            // Act
            IHttpHandler handler = product.GetHttpHandler(requestContext);

            // Assert
            HttpRouteExceptionHandler typedHandler = Assert.IsType<HttpRouteExceptionHandler>(handler);
            Assert.Same(expectedExceptionInfo, typedHandler.ExceptionInfo);
        }

        private static ExceptionDispatchInfo CreateExceptionInfo()
        {
            return ExceptionDispatchInfo.Capture(new Exception());
        }

        private static HttpRouteExceptionRouteHandler CreateProductUnderTest(ExceptionDispatchInfo exceptionInfo)
        {
            return new HttpRouteExceptionRouteHandler(exceptionInfo);
        }
    }
}

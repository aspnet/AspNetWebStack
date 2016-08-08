// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Hosting;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpHandlerUtilTest
    {
        [Fact]
        public void WrapForServerExecute_BeginProcessRequest_DelegatesCorrectly()
        {
            // Arrange
            IAsyncResult expectedResult = new Mock<IAsyncResult>().Object;
            AsyncCallback cb = delegate { };

            HttpContext httpContext = GetHttpContext();
            Mock<IHttpAsyncHandler> mockHttpHandler = new Mock<IHttpAsyncHandler>();
            mockHttpHandler.Setup(o => o.BeginProcessRequest(httpContext, cb, "extraData")).Returns(expectedResult);

            IHttpAsyncHandler wrapper = (IHttpAsyncHandler)HttpHandlerUtil.WrapForServerExecute(mockHttpHandler.Object);

            // Act
            IAsyncResult actualResult = wrapper.BeginProcessRequest(httpContext, cb, "extraData");

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void WrapForServerExecute_EndProcessRequest_DelegatesCorrectly()
        {
            // Arrange
            IAsyncResult asyncResult = new Mock<IAsyncResult>().Object;

            HttpContext httpContext = GetHttpContext();
            Mock<IHttpAsyncHandler> mockHttpHandler = new Mock<IHttpAsyncHandler>();
            mockHttpHandler.Setup(o => o.EndProcessRequest(asyncResult)).Verifiable();

            IHttpAsyncHandler wrapper = (IHttpAsyncHandler)HttpHandlerUtil.WrapForServerExecute(mockHttpHandler.Object);

            // Act
            wrapper.EndProcessRequest(asyncResult);

            // Assert
            mockHttpHandler.Verify();
        }

        [Fact]
        public void WrapForServerExecute_ProcessRequest_DelegatesCorrectly()
        {
            // Arrange
            HttpContext httpContext = GetHttpContext();
            Mock<IHttpHandler> mockHttpHandler = new Mock<IHttpHandler>();
            mockHttpHandler.Setup(o => o.ProcessRequest(httpContext)).Verifiable();

            IHttpHandler wrapper = HttpHandlerUtil.WrapForServerExecute(mockHttpHandler.Object);

            // Act
            wrapper.ProcessRequest(httpContext);

            // Assert
            mockHttpHandler.Verify();
        }

        [Fact]
        public void WrapForServerExecute_ProcessRequest_PropagatesExceptionsIfNotHttpException()
        {
            // Arrange
            HttpContext httpContext = GetHttpContext();
            Mock<IHttpHandler> mockHttpHandler = new Mock<IHttpHandler>();
            mockHttpHandler.Setup(o => o.ProcessRequest(httpContext)).Throws(new InvalidOperationException("Some exception."));

            IHttpHandler wrapper = HttpHandlerUtil.WrapForServerExecute(mockHttpHandler.Object);

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { wrapper.ProcessRequest(httpContext); },
                @"Some exception.");
        }

        [Fact]
        public void WrapForServerExecute_ProcessRequest_PropagatesHttpExceptionIfStatusCode500()
        {
            // Arrange
            HttpContext httpContext = GetHttpContext();
            Mock<IHttpHandler> mockHttpHandler = new Mock<IHttpHandler>();
            mockHttpHandler.Setup(o => o.ProcessRequest(httpContext)).Throws(new HttpException(500, "Some exception."));

            IHttpHandler wrapper = HttpHandlerUtil.WrapForServerExecute(mockHttpHandler.Object);

            // Act & assert
            Assert.ThrowsHttpException(
                delegate { wrapper.ProcessRequest(httpContext); },
                @"Some exception.",
                500);
        }

        [Fact]
        public void WrapForServerExecute_ProcessRequest_WrapsHttpExceptionIfStatusCodeNot500()
        {
            // Arrange
            HttpContext httpContext = GetHttpContext();
            Mock<IHttpHandler> mockHttpHandler = new Mock<IHttpHandler>();
            mockHttpHandler.Setup(o => o.ProcessRequest(httpContext)).Throws(new HttpException(404, "Some exception."));

            IHttpHandler wrapper = HttpHandlerUtil.WrapForServerExecute(mockHttpHandler.Object);

            // Act & assert
            HttpException outerException = Assert.ThrowsHttpException(
                delegate { wrapper.ProcessRequest(httpContext); },
                @"Execution of the child request failed. Please examine the InnerException for more information.",
                500);

            HttpException innerException = outerException.InnerException as HttpException;
            Assert.NotNull(innerException);
            Assert.Equal(404, innerException.GetHttpCode());
            Assert.Equal("Some exception.", innerException.Message);
        }

        [Fact]
        public void WrapForServerExecute_ReturnsIHttpAsyncHandler()
        {
            // Arrange
            IHttpAsyncHandler httpHandler = new Mock<IHttpAsyncHandler>().Object;

            // Act
            IHttpHandler wrapper = HttpHandlerUtil.WrapForServerExecute(httpHandler);

            // Assert
            Assert.True(wrapper is IHttpAsyncHandler);
        }

        [Fact]
        public void WrapForServerExecute_ReturnsIHttpHandler()
        {
            // Arrange
            IHttpHandler httpHandler = new Mock<IHttpHandler>().Object;

            // Act
            IHttpHandler wrapper = HttpHandlerUtil.WrapForServerExecute(httpHandler);

            // Assert
            Assert.False(wrapper is IHttpAsyncHandler);
        }

        private static HttpContext GetHttpContext()
        {
            return new HttpContext(new SimpleWorkerRequest("/", "/", "Page", "Query", TextWriter.Null));
        }
    }
}

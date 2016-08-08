// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AjaxRequestExtensionsTest
    {
        [Fact]
        public void IsAjaxRequestWithNullRequestThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { AjaxRequestExtensions.IsAjaxRequest(null); }, "request");
        }

        [Fact]
        public void IsAjaxRequestWithKeyIsTrue()
        {
            // Arrange
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(r => r["X-Requested-With"]).Returns("XMLHttpRequest").Verifiable();
            HttpRequestBase request = mockRequest.Object;

            // Act
            bool retVal = AjaxRequestExtensions.IsAjaxRequest(request);

            // Assert
            Assert.True(retVal);
            mockRequest.Verify();
        }

        [Fact]
        public void IsAjaxRequestWithoutKeyOrHeaderIsFalse()
        {
            // Arrange
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            NameValueCollection headerCollection = new NameValueCollection();
            mockRequest.Setup(r => r.Headers).Returns(headerCollection).Verifiable();
            mockRequest.Setup(r => r["X-Requested-With"]).Returns((string)null).Verifiable();
            HttpRequestBase request = mockRequest.Object;

            // Act
            bool retVal = AjaxRequestExtensions.IsAjaxRequest(request);

            // Assert
            Assert.False(retVal);
            mockRequest.Verify();
        }

        [Fact]
        public void IsAjaxRequestReturnsTrueIfHeaderSet()
        {
            // Arrange
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            NameValueCollection headerCollection = new NameValueCollection();
            headerCollection["X-Requested-With"] = "XMLHttpRequest";
            mockRequest.Setup(r => r.Headers).Returns(headerCollection).Verifiable();
            HttpRequestBase request = mockRequest.Object;

            // Act
            bool retVal = AjaxRequestExtensions.IsAjaxRequest(request);

            // Assert
            Assert.True(retVal);
            mockRequest.Verify();
        }
    }
}

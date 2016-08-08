// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpStatusCodeResultTest
    {
        [Fact]
        public void ExecuteResult()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 666).Verifiable();

            HttpStatusCodeResult result = new HttpStatusCodeResult(666);

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultWithDescription()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 666).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusDescription = "Foo Bar").Verifiable();
            HttpStatusCodeResult result = new HttpStatusCodeResult(666, "Foo Bar");

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultWithNullContextThrows()
        {
            // Act and Assert
            Assert.ThrowsArgumentNull(delegate { new HttpStatusCodeResult(1).ExecuteResult(context: null); }, "context");
        }

        [Fact]
        public void StatusCode()
        {
            // Assert
            Assert.Equal(123, new HttpStatusCodeResult(123).StatusCode);
            Assert.Equal(234, new HttpStatusCodeResult(234, "foobar").StatusCode);
        }

        [Fact]
        public void StatusDescription()
        {
            // Assert
            Assert.Null(new HttpStatusCodeResult(123).StatusDescription);
            Assert.Equal("foobar", new HttpStatusCodeResult(234, "foobar").StatusDescription);
        }

        [Fact]
        public void HttpStatusCodeAndStatusDescription()
        {
            // Arrange
            int unusedStatusCode = 306;

            // Act
            HttpStatusCodeResult result = new HttpStatusCodeResult(HttpStatusCode.Unused, "foobar");

            // Assert
            Assert.Equal(unusedStatusCode, result.StatusCode);
            Assert.Equal("foobar", result.StatusDescription);
        }

        [Fact]
        public void ExecuteResultWithHttpStatusCode()
        {
            // Arrange
            int unusedStatusCode = 306;
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = unusedStatusCode).Verifiable();

            HttpStatusCodeResult result = new HttpStatusCodeResult(HttpStatusCode.Unused);

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }
    }
}

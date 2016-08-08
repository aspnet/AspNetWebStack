// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ContentResultTest
    {
        [Fact]
        public void AllPropertiesDefaultToNull()
        {
            // Act
            ContentResult result = new ContentResult();

            // Assert
            Assert.Null(result.Content);
            Assert.Null(result.ContentEncoding);
            Assert.Null(result.ContentType);
        }

        [Fact]
        public void EmptyContentTypeIsNotOutput()
        {
            // Arrange
            string content = "Some content.";
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(content)).Verifiable();

            ContentResult result = new ContentResult
            {
                Content = content,
                ContentType = String.Empty,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResult()
        {
            // Arrange
            string content = "Some content.";
            string contentType = "Some content type.";
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(content)).Verifiable();

            ContentResult result = new ContentResult
            {
                Content = content,
                ContentType = contentType,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultWithNullContextThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new ContentResult().ExecuteResult(null /* context */); }, "context");
        }

        [Fact]
        public void NullContentIsNotOutput()
        {
            // Arrange
            string contentType = "Some content type.";
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();

            ContentResult result = new ContentResult
            {
                ContentType = contentType,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void NullContentEncodingIsNotOutput()
        {
            // Arrange
            string content = "Some content.";
            string contentType = "Some content type.";

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(content)).Verifiable();

            ContentResult result = new ContentResult
            {
                Content = content,
                ContentType = contentType,
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void NullContentTypeIsNotOutput()
        {
            // Arrange
            string content = "Some content.";
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(content)).Verifiable();

            ContentResult result = new ContentResult
            {
                Content = content,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }
    }
}

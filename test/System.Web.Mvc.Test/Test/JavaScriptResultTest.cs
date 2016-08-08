// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class JavaScriptResultTest
    {
        [Fact]
        public void AllPropertiesDefaultToNull()
        {
            // Act
            JavaScriptResult result = new JavaScriptResult();

            // Assert
            Assert.Null(result.Script);
        }

        [Fact]
        public void ExecuteResult()
        {
            // Arrange
            string script = "alert('foo');";
            string contentType = "application/x-javascript";

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(script)).Verifiable();

            JavaScriptResult result = new JavaScriptResult
            {
                Script = script
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
                delegate { new JavaScriptResult().ExecuteResult(null /* context */); }, "context");
        }

        [Fact]
        public void NullScriptIsNotOutput()
        {
            // Arrange
            string contentType = "application/x-javascript";

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();

            JavaScriptResult result = new JavaScriptResult();

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }
    }
}

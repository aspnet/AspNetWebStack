// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpNotFoundResultTest
    {
        [Fact]
        public void ExecuteResult()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 404).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusDescription = "Some description").Verifiable();

            HttpNotFoundResult result = new HttpNotFoundResult("Some description");

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void StatusCode()
        {
            Assert.Equal(404, new HttpNotFoundResult().StatusCode);
        }
    }
}

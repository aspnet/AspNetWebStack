// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FilePathResultTest
    {
        [Fact]
        public void ConstructorSetsFileNameProperty()
        {
            // Act
            FilePathResult result = new FilePathResult("someFile", "contentType");

            // Assert
            Assert.Equal("someFile", result.FileName);
        }

        [Fact]
        public void ConstructorThrowsIfFileNameIsEmpty()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new FilePathResult(String.Empty, "contentType"); }, "fileName");
        }

        [Fact]
        public void ConstructorThrowsIfFileNameIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new FilePathResult(null, "contentType"); }, "fileName");
        }

        [Fact]
        public void WriteFileTransmitsFileToOutputStream()
        {
            // Arrange
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.TransmitFile("someFile")).Verifiable();

            FilePathResultHelper helper = new FilePathResultHelper("someFile", "application/octet-stream");

            // Act
            helper.PublicWriteFile(mockResponse.Object);

            // Assert
            mockResponse.Verify();
        }

        private class FilePathResultHelper : FilePathResult
        {
            public FilePathResultHelper(string fileName, string contentType)
                : base(fileName, contentType)
            {
            }

            public void PublicWriteFile(HttpResponseBase response)
            {
                WriteFile(response);
            }
        }
    }
}

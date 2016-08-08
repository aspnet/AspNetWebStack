// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FileContentResultTest
    {
        [Fact]
        public void ConstructorSetsFileContentsProperty()
        {
            // Arrange
            byte[] fileContents = new byte[0];

            // Act
            FileContentResult result = new FileContentResult(fileContents, "contentType");

            // Assert
            Assert.Same(fileContents, result.FileContents);
        }

        [Fact]
        public void ConstructorThrowsIfFileContentsIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new FileContentResult(null, "contentType"); }, "fileContents");
        }

        [Fact]
        public void WriteFileCopiesBufferToOutputStream()
        {
            // Arrange
            byte[] buffer = new byte[] { 1, 2, 3, 4, 5 };

            Mock<Stream> mockOutputStream = new Mock<Stream>();
            mockOutputStream.Setup(s => s.Write(buffer, 0, buffer.Length)).Verifiable();
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.OutputStream).Returns(mockOutputStream.Object);

            FileContentResultHelper helper = new FileContentResultHelper(buffer, "application/octet-stream");

            // Act
            helper.PublicWriteFile(mockResponse.Object);

            // Assert
            mockOutputStream.Verify();
            mockResponse.Verify();
        }

        private class FileContentResultHelper : FileContentResult
        {
            public FileContentResultHelper(byte[] fileContents, string contentType)
                : base(fileContents, contentType)
            {
            }

            public void PublicWriteFile(HttpResponseBase response)
            {
                WriteFile(response);
            }
        }
    }
}

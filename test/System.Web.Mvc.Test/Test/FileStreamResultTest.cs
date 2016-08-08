// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FileStreamResultTest
    {
        private static readonly Random _random = new Random();

        [Fact]
        public void ConstructorSetsFileStreamProperty()
        {
            // Arrange
            Stream stream = new MemoryStream();

            // Act
            FileStreamResult result = new FileStreamResult(stream, "contentType");

            // Assert
            Assert.Same(stream, result.FileStream);
        }

        [Fact]
        public void ConstructorThrowsIfFileStreamIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new FileStreamResult(null, "contentType"); }, "fileStream");
        }

        [Fact]
        public void WriteFileCopiesProvidedStreamToOutputStream()
        {
            // Arrange
            int byteLen = 0x1234;
            byte[] originalBytes = GetRandomByteArray(byteLen);
            MemoryStream originalStream = new MemoryStream(originalBytes);
            MemoryStream outStream = new MemoryStream();

            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.OutputStream).Returns(outStream);

            FileStreamResultHelper helper = new FileStreamResultHelper(originalStream, "application/octet-stream");

            // Act
            helper.PublicWriteFile(mockResponse.Object);

            // Assert
            byte[] outBytes = outStream.ToArray();
            Assert.True(originalBytes.SequenceEqual(outBytes));
            mockResponse.Verify();
        }

        private static byte[] GetRandomByteArray(int length)
        {
            byte[] bytes = new byte[length];
            _random.NextBytes(bytes);
            return bytes;
        }

        private class FileStreamResultHelper : FileStreamResult
        {
            public FileStreamResultHelper(Stream fileStream, string contentType)
                : base(fileStream, contentType)
            {
            }

            public void PublicWriteFile(HttpResponseBase response)
            {
                WriteFile(response);
            }
        }
    }
}

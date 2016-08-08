// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class MultipartFileDataTest
    {
        [Fact]
        public void Constructor_ThrowsOnNullHeaders()
        {
            Assert.ThrowsArgumentNull(() => new MultipartFileData(null, "file"), "headers");
        }

        [Fact]
        public void Constructor_ThrowsOnNullLocalFileName()
        {
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();
            Assert.ThrowsArgumentNull(() => new MultipartFileData(headers, null), "localFileName");
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();
            string fileName = "filename";

            // Act
            MultipartFileData fileData = new MultipartFileData(headers, fileName);


            Assert.Same(headers, fileData.Headers);
            Assert.Same(fileName, fileData.LocalFileName);
        }
    }
}

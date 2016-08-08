// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class MultipartRemoteFileDataTests
    {
        [Fact]
        public void Constructor_ThrowsOnNullHeaders()
        {
            // Arrange, Act & Assert
            Assert.ThrowsArgumentNull(
                () => new MultipartRemoteFileData(null, "http://some/path/to", "Name"),
                "headers");
        }

        [Fact]
        public void Constructor_ThrowsOnNullLocation()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();
            
            // Act and Assert
            Assert.ThrowsArgumentNull(() => new MultipartRemoteFileData(headers, null, "Name"), "location");
        }

        [Fact]
        public void Constructor_ThrowsOnNullFileName()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            // Act and Assert
            Assert.ThrowsArgumentNull(() => new MultipartRemoteFileData(headers, "http://some/path/to", null),
                "fileName");
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();
            string remoteFileURL = "http://some/path/to";
            string fileName = "Name";

            // Act
            MultipartRemoteFileData fileData = new MultipartRemoteFileData(headers, remoteFileURL, fileName);

            // Assert
            Assert.Same(headers, fileData.Headers);
            Assert.Same(remoteFileURL, fileData.Location);
            Assert.Same(fileName, fileData.FileName);
        }
    }
}

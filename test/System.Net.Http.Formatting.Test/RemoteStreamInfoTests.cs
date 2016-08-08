// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class RemoteStreamInfoTests
    {
        [Fact]
        public void Constructor_ThrowsOnNullStream()
        {
            // Arrange, Act & Assert
            Assert.ThrowsArgumentNull(
                () => new RemoteStreamInfo(null,  "http://some/path/to", "Name"),
                "remoteStream");
        }

        [Fact]
        public void Constructor_ThrowsOnNullLocation()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => new RemoteStreamInfo(new MemoryStream(), null, "Name"), "location");
        }

        [Fact]
        public void Constructor_ThrowsOnNullFileName()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => new RemoteStreamInfo(new MemoryStream(), "http://some/path/to", null),
                "fileName");
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();
            string remoteFileURL = "http://some/path/to";
            string fileName = "Name";
            Stream stream = new MemoryStream();

            // Act
            RemoteStreamInfo fileData = new RemoteStreamInfo(stream, remoteFileURL, fileName);

            // Assert
            Assert.Same(stream, fileData.RemoteStream);
            Assert.Same(remoteFileURL, fileData.Location);
            Assert.Same(fileName, fileData.FileName);
        }
    }
}

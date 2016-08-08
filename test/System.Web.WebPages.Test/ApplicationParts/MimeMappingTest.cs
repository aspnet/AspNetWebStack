// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Internal.Web.Utils;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class MimeMappingTest
    {
        [Fact]
        public void MimeMappingThrowsForNullFileName()
        {
            // Arrange
            string fileName = null;

            // Act and Assert
            Assert.ThrowsArgumentNull(() => MimeMapping.GetMimeMapping(fileName), "fileName");
        }

        [Fact]
        public void MimeMappingReturnsGenericTypeForUnknownExtensions()
        {
            // Arrange
            string fileName = "file.does-not-exist";

            // Act
            string mimeType = MimeMapping.GetMimeMapping(fileName);

            // Assert
            Assert.Equal("application/octet-stream", mimeType);
        }

        [Fact]
        public void MimeMappingReturnsGenericTypeForNoExtensions()
        {
            // Arrange
            string fileName = "file";

            // Act
            string mimeType = MimeMapping.GetMimeMapping(fileName);

            // Assert
            Assert.Equal("application/octet-stream", mimeType);
        }

        [Fact]
        public void MimeMappingPerformsCaseInsensitiveSearches()
        {
            // Arrange
            string fileName1 = "file.doc";
            string fileName2 = "file.dOC";

            // Act
            string mimeType1 = MimeMapping.GetMimeMapping(fileName1);
            string mimeType2 = MimeMapping.GetMimeMapping(fileName2);

            // Assert
            Assert.Equal("application/msword", mimeType1);
            Assert.Equal("application/msword", mimeType2);
        }
    }
}

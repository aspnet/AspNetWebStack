// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class DisplayInfoTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => new DisplayInfo(filePath: null, displayMode: new Mock<IDisplayMode>().Object), "filePath");
            Assert.ThrowsArgumentNull(() => new DisplayInfo("testPath", displayMode: null), "displayMode");
        }

        [Fact]
        public void ConstructorSetsDisplayInfoProperties()
        {
            // Arrange
            string path = "testPath";
            IDisplayMode displayMode = new Mock<IDisplayMode>().Object;

            // Act
            DisplayInfo info = new DisplayInfo(path, displayMode);

            // Assert
            Assert.Equal(path, info.FilePath);
            Assert.Equal(displayMode, info.DisplayMode);
        }

        [Fact]
        public void ConstructorSetsEmptyFilePath()
        {
            // Act & Assert
            Assert.Equal(String.Empty, new DisplayInfo(String.Empty, new Mock<IDisplayMode>().Object).FilePath);
        }
    }
}

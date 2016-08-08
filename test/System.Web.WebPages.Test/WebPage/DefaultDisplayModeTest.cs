// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class DefaultDisplayModeTest
    {
        [Fact]
        public void DefaultDisplayModeWithEmptySuffix()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode();

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz.aspx", virtualPath => true);

            // Assert
            Assert.Equal(String.Empty, displayMode.DisplayModeId);
            Assert.Equal("/bar/baz.aspx", info.FilePath);
        }

        [Fact]
        public void DefaultDisplayModeWithNullSuffix()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode(null);

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz.aspx", virtualPath => true);

            // Assert
            Assert.Equal(String.Empty, displayMode.DisplayModeId);
            Assert.Equal("/bar/baz.aspx", info.FilePath);
        }

        [Fact]
        public void DefaultDisplayModeSetSuffixAsId()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act & Assert
            Assert.Equal("foo", displayMode.DisplayModeId);
        }

        [Fact]
        public void GetDisplayInfoWithNullOrEmptySuffixReturnsPathThatExists()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz.aspx", virtualPath => true);

            // Assert
            Assert.IsType<DefaultDisplayMode>(info.DisplayMode);
            Assert.Equal("/bar/baz.foo.aspx", info.FilePath);
        }

        [Fact]
        public void GetDisplayInfoInsertsSuffixIntoVirtualPathThatExists()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz.aspx", virtualPath => true);

            // Assert
            Assert.IsType<DefaultDisplayMode>(info.DisplayMode);
            Assert.Equal("/bar/baz.foo.aspx", info.FilePath);
        }

        [Fact]
        public void GetDisplayInfoInsertsSuffixBeforeLastSectionOfExtension()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz.txt.aspx", virtualPath => true);

            // Assert
            Assert.Equal("/bar/baz.txt.foo.aspx", info.FilePath);
        }

        [Fact]
        public void GetDisplayInfoSuffixesPathWithNoExtension()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz", virtualPath => true);

            // Assert
            Assert.Equal("/bar/baz.foo", info.FilePath);
        }

        [Fact]
        public void GetDisplayInfoWithNullVirtualPath()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, virtualPath: null, virtualPathExists: virtualPath => true);

            // Assert
            Assert.Null(info);
        }

        [Fact]
        public void GetDisplayInfoSuffixesPathWithEmptyVirtualPath()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, String.Empty, virtualPath => true);

            // Assert
            Assert.Equal(String.Empty, info.FilePath);
        }

        [Fact]
        public void GetDisplayInfoReturnsNullIfPathDoesNotExist()
        {
            // Arrange
            DefaultDisplayMode displayMode = new DefaultDisplayMode("foo");

            // Act
            DisplayInfo info = displayMode.GetDisplayInfo(new Mock<HttpContextBase>(MockBehavior.Strict).Object, "/bar/baz", virtualPath => false);

            // Assert
            Assert.Null(info);
        }
    }
}

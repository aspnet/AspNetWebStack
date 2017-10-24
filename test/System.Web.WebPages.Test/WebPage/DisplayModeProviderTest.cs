// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class DisplayModesTest
    {
        [Fact]
        public void DefaultInstanceHasDefaultModes()
        {
            // Act
            IList<IDisplayMode> displayModes = DisplayModeProvider.Instance.Modes;

            // Assert
            Assert.Equal(2, displayModes.Count);

            Assert.IsType<DefaultDisplayMode>(displayModes[0]);
            Assert.Equal(displayModes[0].DisplayModeId, DisplayModeProvider.MobileDisplayModeId);

            Assert.IsType<DefaultDisplayMode>(displayModes[1]);
            Assert.Equal(displayModes[1].DisplayModeId, DisplayModeProvider.DefaultDisplayModeId);
        }

        [Fact]
        public void GetDisplayInfoForVirtualPathReturnsDisplayInfoFromFirstDisplayModeToHandleRequest()
        {
            // Arrange
            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();
            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode3.Object);

            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);

            var expected = new DisplayInfo("Foo", displayMode3.Object);
            Func<string, bool> fileExists = path => true;
            displayMode3.Setup(d => d.GetDisplayInfo(httpContext.Object, "path", fileExists)).Returns(expected);

            // Act
            DisplayInfo result = displayModeProvider.GetDisplayInfoForVirtualPath("path", httpContext.Object, fileExists, currentDisplayMode: null);

            // Assert
            Assert.Same(expected, result);
        }

        [Fact]
        public void GetDisplayInfoForVirtualPathWithConsistentDisplayModeBeginsSearchAtCurrentDisplayMode()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);

            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();
            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode3.Object);

            var displayInfo = new DisplayInfo("Foo", displayMode3.Object);
            Func<string, bool> fileExists = path => true;
            displayMode3.Setup(d => d.GetDisplayInfo(httpContext.Object, "path", fileExists)).Returns(displayInfo);

            // Act
            DisplayInfo result = displayModeProvider.GetDisplayInfoForVirtualPath("path", httpContext.Object, fileExists, currentDisplayMode: displayMode2.Object,
                requireConsistentDisplayMode: true);

            // Assert
            Assert.Same(displayInfo, result);
        }

        [Fact]
        public void GetDisplayInfoForVirtualPathWithoutConsistentDisplayModeIgnoresCurrentDisplayMode()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();
            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode3.Object);

            var displayInfo = new DisplayInfo("Foo", displayMode3.Object);
            Func<string, bool> fileExists = path => true;
            displayMode1.Setup(d => d.GetDisplayInfo(httpContext.Object, "path", fileExists)).Returns(displayInfo);

            // Act
            DisplayInfo result = displayModeProvider.GetDisplayInfoForVirtualPath("path", httpContext.Object, fileExists, currentDisplayMode: displayMode1.Object,
                requireConsistentDisplayMode: false);

            // Assert
            Assert.Same(displayInfo, result);
        }

        [Fact]
        public void GetDisplayModesForRequestReturnsNullIfNoDisplayModesHandleRequest()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();
            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode3.Object);

            // Act
            DisplayInfo displayModeForRequest = displayModeProvider.GetDisplayInfoForVirtualPath("path", httpContext.Object, path => false, currentDisplayMode: null);

            // Assert
            Assert.Null(displayModeForRequest);
        }

        [Fact]
        public void GetAvailableDisplayModesForContextWithRestrictingPageElements()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();
            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode3.Object);

            // Act
            var availableDisplayModes = displayModeProvider.GetAvailableDisplayModesForContext(httpContext.Object, displayMode2.Object, requireConsistentDisplayMode: true).ToList();

            // Assert
            var availableDisplayMode = Assert.Single(availableDisplayModes);
            Assert.Equal(displayMode3.Object, availableDisplayMode);
        }

        [Fact]
        public void GetAvailableDisplayModesForContextWithoutRestrictingPageElements()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();

            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode3.Object);

            // Act
            var availableDisplayModes = displayModeProvider.GetAvailableDisplayModesForContext(httpContext.Object, displayMode2.Object, requireConsistentDisplayMode: false).ToList();

            // Assert
            Assert.Equal(2, availableDisplayModes.Count);
            Assert.Same(displayMode1.Object, availableDisplayModes[0]);
            Assert.Same(displayMode3.Object, availableDisplayModes[1]);
        }

        [Fact]
        public void GetAvailableDisplayModesReturnsOnlyModesThatCanHandleContext()
        {
            // Arrange
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var displayModeProvider = new DisplayModeProvider();
            displayModeProvider.Modes.Clear();
            var displayMode1 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode1.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode1.Object);

            var displayMode2 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode2.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(true);
            displayModeProvider.Modes.Add(displayMode2.Object);

            var displayMode3 = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode3.Setup(d => d.CanHandleContext(It.IsAny<HttpContextBase>())).Returns(false);
            displayModeProvider.Modes.Add(displayMode3.Object);

            // Act
            var availableDisplayModes = displayModeProvider.GetAvailableDisplayModesForContext(httpContext.Object, displayMode1.Object, requireConsistentDisplayMode: false).ToList();

            // Assert
            IDisplayMode availableDisplayMode = Assert.Single(availableDisplayModes);
            Assert.Equal(displayMode2.Object, availableDisplayMode);
        }
    }
}

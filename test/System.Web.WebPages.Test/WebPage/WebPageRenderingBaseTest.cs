// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class WebPageRenderingBaseTest
    {
        [Fact]
        public void SetCultureThrowsIfValueIsNull()
        {
            // Arrange
            string value = null;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => webPageRenderingBase.Culture = value, "value");
        }

        [Fact]
        public void SetCultureThrowsIfValueIsEmpty()
        {
            // Arrange
            string value = String.Empty;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => webPageRenderingBase.Culture = value, "value");
        }

        [Fact]
        public void SetUICultureThrowsIfValueIsNull()
        {
            // Arrange
            string value = null;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => webPageRenderingBase.UICulture = value, "value");
        }

        [Fact]
        public void SetUICultureThrowsIfValueIsEmpty()
        {
            // Arrange
            string value = String.Empty;
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true }.Object;

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => webPageRenderingBase.UICulture = value, "value");
        }

        [Fact]
        public void DisplayModePropertyWithNullContext()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            var displayMode = new DefaultDisplayMode("test");
            var webPageRenderingBase = new Mock<WebPageRenderingBase>() { CallBase = true };

            // Act & Assert
            Assert.Null(webPageRenderingBase.Object.DisplayMode);
        }
    }
}

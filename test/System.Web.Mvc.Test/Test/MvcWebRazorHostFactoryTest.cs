// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Razor;
using System.Web.WebPages.Razor;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class MvcWebRazorHostFactoryTest
    {
        [Fact]
        public void Constructor()
        {
            new MvcWebRazorHostFactory();

            // All is cool
        }

        [Fact]
        public void CreateHost_ReplacesRegularHostWithMvcSpecificOne()
        {
            // Arrange
            MvcWebRazorHostFactory factory = new MvcWebRazorHostFactory();

            // Act
            WebPageRazorHost result = factory.CreateHost("foo.cshtml", null);

            // Assert
            Assert.IsType<MvcWebPageRazorHost>(result);
        }

        [Fact]
        public void CreateHost_DoesNotChangeAppStartFileHost()
        {
            // Arrange
            MvcWebRazorHostFactory factory = new MvcWebRazorHostFactory();

            // Act
            WebPageRazorHost result = factory.CreateHost("_appstart.cshtml", null);

            // Assert
            Assert.IsNotType<MvcWebPageRazorHost>(result);
        }

        [Fact]
        public void CreateHost_DoesNotChangePageStartFileHost()
        {
            // Arrange
            MvcWebRazorHostFactory factory = new MvcWebRazorHostFactory();

            // Act
            WebPageRazorHost result = factory.CreateHost("_pagestart.cshtml", null);

            // Assert
            Assert.IsNotType<MvcWebPageRazorHost>(result);
        }
    }
}

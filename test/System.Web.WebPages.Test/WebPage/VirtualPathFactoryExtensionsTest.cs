// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class VirtualPathFactoryExtensionsTest
    {
        [Fact]
        public void VirtualPathFactoryExtensionsSpecialCasesVirtualPathFactoryManager()
        {
            // Arrange
            var virtualPath = "~/index.cshtml";
            var mockPage = Utils.CreatePage(_ => { }, virtualPath);
            var factory = new Mock<IVirtualPathFactory>();
            factory.Setup(c => c.Exists(virtualPath)).Returns(true).Verifiable();
            factory.Setup(c => c.CreateInstance(virtualPath)).Returns(mockPage);

            // Act
            var factoryManager = new VirtualPathFactoryManager(factory.Object);
            var page = factoryManager.CreateInstance<WebPageBase>(virtualPath);

            // Assert
            Assert.Equal(mockPage, page);
            factory.Verify();
        }

        [Fact]
        public void GenericCreateInstanceLoopsOverAllRegisteredFactories()
        {
            // Arrange
            var virtualPath = "~/index.cshtml";
            var mockPage = Utils.CreatePage(_ => { }, virtualPath);
            var factory1 = new HashVirtualPathFactory(mockPage);
            var factory2 = new HashVirtualPathFactory(Utils.CreatePage(null, "~/_admin/index.cshtml"));

            // Act
            var factoryManager = new VirtualPathFactoryManager(factory2);
            factoryManager.RegisterVirtualPathFactoryInternal(factory1);
            var page = factoryManager.CreateInstance<WebPageBase>(virtualPath);

            // Assert
            Assert.Equal(mockPage, page);
        }

        [Fact]
        public void GenericCreateInstanceReturnsNullIfNoFactoryCanCreateVirtualPath()
        {
            // Arrange
            var factory1 = new HashVirtualPathFactory(Utils.CreatePage(_ => { }, "~/index.cshtml"));
            var factory2 = new HashVirtualPathFactory(Utils.CreatePage(null, "~/_admin/index.cshtml"));

            // Act
            var factoryManager = new VirtualPathFactoryManager(factory2);
            factoryManager.RegisterVirtualPathFactoryInternal(factory1);
            var page = factoryManager.CreateInstance<WebPageBase>("~/does-not-exist.cshtml");

            // Assert
            Assert.Null(page);
        }
    }
}

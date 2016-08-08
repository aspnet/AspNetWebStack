// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class VirtualPathFactoryManagerTest
    {
        [Fact]
        public void DefaultFactoryIsListedInRegisteredFactories()
        {
            // Arrange
            var factory = new HashVirtualPathFactory();

            // Act
            var factoryManager = new VirtualPathFactoryManager(factory);

            // Assert
            Assert.Equal(factory, factoryManager.RegisteredFactories.Single());
        }

        [Fact]
        public void RegisterFactoryEnsuresDefaultFactoryRemainsTheLast()
        {
            // Arrange
            var defaultFactory = new HashVirtualPathFactory();
            var factory1 = new HashVirtualPathFactory();
            var factory2 = new HashVirtualPathFactory();
            var factory3 = new HashVirtualPathFactory();

            // Act
            var factoryManager = new VirtualPathFactoryManager(defaultFactory);
            factoryManager.RegisterVirtualPathFactoryInternal(factory1);
            factoryManager.RegisterVirtualPathFactoryInternal(factory2);
            factoryManager.RegisterVirtualPathFactoryInternal(factory3);

            // Assert
            Assert.Equal(factory1, factoryManager.RegisteredFactories.ElementAt(0));
            Assert.Equal(factory2, factoryManager.RegisteredFactories.ElementAt(1));
            Assert.Equal(factory3, factoryManager.RegisteredFactories.ElementAt(2));
            Assert.Equal(defaultFactory, factoryManager.RegisteredFactories.Last());
        }

        [Fact]
        public void CreateInstanceUsesRegisteredFactoriesForExistence()
        {
            // Arrange
            var path = "~/index.cshtml";
            var factory1 = new Mock<IVirtualPathFactory>();
            factory1.Setup(c => c.Exists(path)).Returns(false).Verifiable();
            var factory2 = new Mock<IVirtualPathFactory>();
            factory2.Setup(c => c.Exists(path)).Returns(true).Verifiable();
            var factory3 = new Mock<IVirtualPathFactory>();
            factory3.Setup(c => c.Exists(path)).Throws(new Exception("This factory should not be called since the page has already been found in 2"));
            var defaultFactory = new Mock<IVirtualPathFactory>();
            defaultFactory.Setup(c => c.Exists(path)).Throws(new Exception("This factory should not be called since it always called last"));

            var vpfm = new VirtualPathFactoryManager(defaultFactory.Object);
            vpfm.RegisterVirtualPathFactoryInternal(factory1.Object);
            vpfm.RegisterVirtualPathFactoryInternal(factory2.Object);
            vpfm.RegisterVirtualPathFactoryInternal(factory3.Object);

            // Act
            var result = vpfm.Exists(path);

            // Assert
            Assert.True(result);

            factory1.Verify();
            factory2.Verify();
        }

        [Fact]
        public void CreateInstanceLooksThroughAllRegisteredFactoriesForExistence()
        {
            // Arrange
            var page = Utils.CreatePage(null);
            var factory1 = new Mock<IVirtualPathFactory>();
            factory1.Setup(c => c.Exists(page.VirtualPath)).Returns(false).Verifiable();
            var factory2 = new Mock<IVirtualPathFactory>();
            factory2.Setup(c => c.Exists(page.VirtualPath)).Returns(true).Verifiable();
            factory2.Setup(c => c.CreateInstance(page.VirtualPath)).Returns(page).Verifiable();
            var factory3 = new Mock<IVirtualPathFactory>();
            factory3.Setup(c => c.Exists(page.VirtualPath)).Throws(new Exception("This factory should not be called since the page has already been found in 2"));
            var defaultFactory = new Mock<IVirtualPathFactory>();
            defaultFactory.Setup(c => c.Exists(page.VirtualPath)).Throws(new Exception("This factory should not be called since it always called last"));

            var vpfm = new VirtualPathFactoryManager(defaultFactory.Object);
            vpfm.RegisterVirtualPathFactoryInternal(factory1.Object);
            vpfm.RegisterVirtualPathFactoryInternal(factory2.Object);
            vpfm.RegisterVirtualPathFactoryInternal(factory3.Object);

            // Act
            var result = vpfm.CreateInstance(page.VirtualPath);

            // Assert
            Assert.Equal(page, result);

            factory1.Verify();
            factory2.Verify();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ValueProviderFactoryCollectionTest
    {
        [Fact]
        public void ListWrappingConstructor()
        {
            // Arrange
            List<ValueProviderFactory> list = new List<ValueProviderFactory>()
            {
                new FormValueProviderFactory()
            };

            // Act
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection(list);

            // Assert
            Assert.Equal(list, collection.ToList());
        }

        [Fact]
        public void ListWrappingConstructorThrowsIfListIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ValueProviderFactoryCollection(null); },
                "list");
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Act
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void AddNullValueProviderFactoryThrows()
        {
            // Arrange
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.Add(null); },
                "item");
        }

        [Fact]
        public void ValueProviderFactoryCollectionCombinedItemsCaches()
        {
            // Arrange
            var factories = new ValueProviderFactory[]
            {
                new Mock<ValueProviderFactory>(MockBehavior.Strict).Object,
                new Mock<ValueProviderFactory>(MockBehavior.Strict).Object
            };
            var collection = new ValueProviderFactoryCollection(factories);

            // Act
            var combined1 = collection.CombinedItems;
            var combined2 = collection.CombinedItems;

            // Assert
            Assert.Equal(factories, combined1);
            Assert.Same(combined1, combined2);
        }

        [Fact]
        public void ValueProviderFactoryCollectionCombinedItemsClearResetsCache()
        {
            TestCacheReset((collection) => collection.Clear());
        }

        [Fact]
        public void ValueProviderFactoryCollectionCombinedItemsInsertResetsCache()
        {
            TestCacheReset((collection) => collection.Insert(0, new Mock<ValueProviderFactory>(MockBehavior.Strict).Object));
        }

        [Fact]
        public void ValueProviderFactoryCollectionCombinedItemsRemoveResetsCache()
        {
            TestCacheReset((collection) => collection.RemoveAt(0));
        }

        [Fact]
        public void ValueProviderFactoryCollectionCombinedItemsSetResetsCache()
        {
            TestCacheReset((collection) => collection[0] = new Mock<ValueProviderFactory>(MockBehavior.Strict).Object);
        }

        private static void TestCacheReset(Action<ValueProviderFactoryCollection> mutatingAction)
        {
            // Arrange
            var providers = new List<ValueProviderFactory>()
            {
                new Mock<ValueProviderFactory>(MockBehavior.Strict).Object,
                new Mock<ValueProviderFactory>(MockBehavior.Strict).Object
            };
            var collection = new ValueProviderFactoryCollection(providers);

            // Act
            mutatingAction(collection);

            ValueProviderFactory[] combined = collection.CombinedItems;

            // Assert
            Assert.Equal(providers, combined);
        }

        [Fact]
        public void ValueProviderFactoryCollectionCombinedItemsDelegatesToResolver()
        {
            // Arrange
            var firstFactory = new Mock<ValueProviderFactory>();
            var secondFactory = new Mock<ValueProviderFactory>();
            var thirdFactory = new Mock<ValueProviderFactory>();
            var dependencyFactories = new ValueProviderFactory[] { firstFactory.Object, secondFactory.Object };
            var collectionFactories = new ValueProviderFactory[] { thirdFactory.Object };
            var expectedFactories = new ValueProviderFactory[] { firstFactory.Object, secondFactory.Object, thirdFactory.Object };

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(ValueProviderFactory))).Returns(dependencyFactories);

            var factories = new ValueProviderFactoryCollection(collectionFactories, resolver.Object);

            // Act
            ValueProviderFactory[] combined = factories.CombinedItems;

            // Assert
            Assert.Equal(expectedFactories, combined);
        }

        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            IValueProvider[] expectedValueProviders = new[]
            {
                new Mock<IValueProvider>().Object,
                new Mock<IValueProvider>().Object
            };

            Mock<ValueProviderFactory> mockFactory1 = new Mock<ValueProviderFactory>();
            mockFactory1.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[0]);
            Mock<ValueProviderFactory> mockFactory2 = new Mock<ValueProviderFactory>();
            mockFactory2.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[1]);

            ValueProviderFactoryCollection factories = new ValueProviderFactoryCollection()
            {
                mockFactory1.Object,
                mockFactory2.Object
            };

            // Act
            ValueProviderCollection valueProviders = (ValueProviderCollection)factories.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(expectedValueProviders, valueProviders.ToArray());
        }

        [Fact]
        public void GetValueProviderDelegatesToResolver()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            IValueProvider[] expectedValueProviders = new[]
            {
                new Mock<IValueProvider>().Object,
                new Mock<IValueProvider>().Object
            };

            Mock<ValueProviderFactory> mockFactory1 = new Mock<ValueProviderFactory>();
            mockFactory1.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[0]);
            Mock<ValueProviderFactory> mockFactory2 = new Mock<ValueProviderFactory>();
            mockFactory2.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[1]);

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(ValueProviderFactory))).Returns(new[] { mockFactory1.Object, mockFactory2.Object });
            ValueProviderFactoryCollection factories = new ValueProviderFactoryCollection(new ValueProviderFactory[0], resolver.Object);

            // Act
            ValueProviderCollection valueProviders = (ValueProviderCollection)factories.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(expectedValueProviders, valueProviders.ToArray());
        }

        [Fact]
        public void SetItem()
        {
            // Arrange
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();
            collection.Add(new Mock<ValueProviderFactory>().Object);

            ValueProviderFactory newFactory = new Mock<ValueProviderFactory>().Object;

            // Act
            collection[0] = newFactory;

            // Assert
            ValueProviderFactory factory = Assert.Single(collection);
            Assert.Equal(newFactory, factory);
        }

        [Fact]
        public void SetNullValueProviderFactoryThrows()
        {
            // Arrange
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();
            collection.Add(new Mock<ValueProviderFactory>().Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection[0] = null; },
                "item");
        }
    }
}

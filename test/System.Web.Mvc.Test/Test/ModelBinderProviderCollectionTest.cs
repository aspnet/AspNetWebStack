// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ModelBinderProviderCollectionTest
    {
        [Fact]
        public void GuardClause()
        {
            // Arrange
            var collection = new ModelBinderProviderCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => collection.GetBinder(null),
                "modelType"
                );
        }

        [Fact]
        public void ModelBinderProviderCollectionCombinedItemsCaches()
        {
            // Arrange
            var providers = new IModelBinderProvider[] 
            {
                new Mock<IModelBinderProvider>(MockBehavior.Strict).Object, 
                new Mock<IModelBinderProvider>(MockBehavior.Strict).Object
            };
            var collection = new ModelBinderProviderCollection(providers);

            // Act
            var combined1 = collection.CombinedItems;
            var combined2 = collection.CombinedItems;

            // Assert
            Assert.Equal(providers, combined1);
            Assert.Same(combined1, combined2);
        }

        [Fact]
        public void ModelBinderProviderCollectionCombinedItemsClearResetsCache()
        {
            TestCacheReset((collection) => collection.Clear());
        }

        [Fact]
        public void ModelBinderProviderCollectionCombinedItemsInsertResetsCache()
        {
            TestCacheReset((collection) => collection.Insert(0, new Mock<IModelBinderProvider>(MockBehavior.Strict).Object));
        }

        [Fact]
        public void ModelBinderProviderCollectionCombinedItemsRemoveResetsCache()
        {
            TestCacheReset((collection) => collection.RemoveAt(0));
        }

        [Fact]
        public void ModelBinderProviderCollectionCombinedItemsSetResetsCache()
        {
            TestCacheReset((collection) => collection[0] = new Mock<IModelBinderProvider>(MockBehavior.Strict).Object);
        }

        private static void TestCacheReset(Action<ModelBinderProviderCollection> mutatingAction)
        {
            // Arrange
            var providers = new List<IModelBinderProvider>() 
            {
                new Mock<IModelBinderProvider>(MockBehavior.Strict).Object, 
                new Mock<IModelBinderProvider>(MockBehavior.Strict).Object
            };
            var collection = new ModelBinderProviderCollection(providers);

            // Act
            mutatingAction(collection);

            IModelBinderProvider[] combined = collection.CombinedItems;

            // Assert
            Assert.Equal(providers, combined);
        }

        [Fact]
        public void ModelBinderProviderCollectionCombinedItemsDelegatesToResolver()
        {
            // Arrange
            var firstProvider = new Mock<IModelBinderProvider>();
            var secondProvider = new Mock<IModelBinderProvider>();
            var thirdProvider = new Mock<IModelBinderProvider>();
            var dependencyProviders = new IModelBinderProvider[] { firstProvider.Object, secondProvider.Object };
            var collectionProviders = new IModelBinderProvider[] { thirdProvider.Object };
            var expectedProviders = new IModelBinderProvider[] { firstProvider.Object, secondProvider.Object, thirdProvider.Object };

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IModelBinderProvider))).Returns(dependencyProviders);

            var providers = new ModelBinderProviderCollection(collectionProviders, resolver.Object);

            // Act
            IModelBinderProvider[] combined = providers.CombinedItems;

            // Assert
            Assert.Equal(expectedProviders, combined);
        }

        [Fact]
        public void GetBinderUsesRegisteredProviders()
        {
            // Arrange
            var testType = typeof(string);
            var expectedBinder = new Mock<IModelBinder>().Object;

            var provider = new Mock<IModelBinderProvider>(MockBehavior.Strict);
            provider.Setup(p => p.GetBinder(testType)).Returns(expectedBinder);
            var collection = new ModelBinderProviderCollection(new[] { provider.Object });

            // Act
            IModelBinder returnedBinder = collection.GetBinder(testType);

            // Assert
            Assert.Same(expectedBinder, returnedBinder);
        }

        [Fact]
        public void GetBinderReturnsValueFromFirstSuccessfulBinderProvider()
        {
            // Arrange
            var testType = typeof(string);
            IModelBinder nullModelBinder = null;
            IModelBinder expectedBinder = new Mock<IModelBinder>().Object;
            IModelBinder secondMatchingBinder = new Mock<IModelBinder>().Object;

            var provider1 = new Mock<IModelBinderProvider>();
            provider1.Setup(p => p.GetBinder(testType)).Returns(nullModelBinder);

            var provider2 = new Mock<IModelBinderProvider>(MockBehavior.Strict);
            provider2.Setup(p => p.GetBinder(testType)).Returns(expectedBinder);

            var provider3 = new Mock<IModelBinderProvider>(MockBehavior.Strict);
            provider3.Setup(p => p.GetBinder(testType)).Returns(secondMatchingBinder);

            var collection = new ModelBinderProviderCollection(new[] { provider1.Object, provider2.Object, provider3.Object });

            // Act
            IModelBinder returnedBinder = collection.GetBinder(testType);

            // Assert
            Assert.Same(expectedBinder, returnedBinder);
        }

        [Fact]
        public void GetBinderReturnsNullWhenNoSuccessfulBinderProviders()
        {
            // Arrange
            var testType = typeof(string);
            IModelBinder nullModelBinder = null;

            var provider1 = new Mock<IModelBinderProvider>();
            provider1.Setup(p => p.GetBinder(testType)).Returns(nullModelBinder);

            var provider2 = new Mock<IModelBinderProvider>(MockBehavior.Strict);
            provider2.Setup(p => p.GetBinder(testType)).Returns(nullModelBinder);

            var collection = new ModelBinderProviderCollection(new[] { provider1.Object, provider2.Object });

            // Act
            IModelBinder returnedBinder = collection.GetBinder(testType);

            // Assert
            Assert.Null(returnedBinder);
        }

        [Fact]
        public void GetBinderDelegatesToResolver()
        {
            // Arrange
            Type modelType = typeof(string);
            IModelBinder expectedBinder = new Mock<IModelBinder>().Object;

            Mock<IModelBinderProvider> locatedProvider = new Mock<IModelBinderProvider>();
            locatedProvider.Setup(p => p.GetBinder(modelType))
                .Returns(expectedBinder);

            Mock<IModelBinderProvider> secondProvider = new Mock<IModelBinderProvider>();
            Mock<IModelBinderProvider> thirdProvider = new Mock<IModelBinderProvider>();
            IModelBinderProvider[] dependencyProviders = new IModelBinderProvider[] { locatedProvider.Object, secondProvider.Object };
            IModelBinderProvider[] collectionProviders = new IModelBinderProvider[] { thirdProvider.Object };

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IModelBinderProvider))).Returns(dependencyProviders);

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection(collectionProviders, resolver.Object);

            // Act
            IModelBinder returnedBinder = providers.GetBinder(modelType);

            // Assert
            Assert.Same(expectedBinder, returnedBinder);
        }
    }
}

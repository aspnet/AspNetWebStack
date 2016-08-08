// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FilterProviderCollectionTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var collection = new FilterProviderCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => collection.GetFilters(null, descriptor),
                "controllerContext"
                );
            Assert.ThrowsArgumentNull(
                () => collection.GetFilters(context, null),
                "actionDescriptor"
                );
        }

        [Fact]
        public void FilterProviderCollectionCombinedItemsCaches()
        {
            // Arrange
            var providers = new IFilterProvider[] 
            {
                new Mock<IFilterProvider>(MockBehavior.Strict).Object, 
                new Mock<IFilterProvider>(MockBehavior.Strict).Object
            };
            var collection = new FilterProviderCollection(providers);

            // Act
            IFilterProvider[] combined1 = collection.CombinedItems;
            IFilterProvider[] combined2 = collection.CombinedItems;

            // Assert
            Assert.Equal(providers, combined1);
            Assert.Same(combined1, combined2);
        }

        [Fact]
        public void FilterProviderCollectionCombinedItemsClearResetsCache()
        {
            TestCacheReset((collection) => collection.Clear());
        }

        [Fact]
        public void FilterProviderCollectionCombinedItemsInsertResetsCache()
        {
            TestCacheReset((collection) => collection.Insert(0, new Mock<IFilterProvider>(MockBehavior.Strict).Object));
        }

        [Fact]
        public void FilterProviderCollectionCombinedItemsRemoveResetsCache()
        {
            TestCacheReset((collection) => collection.RemoveAt(0));
        }

        [Fact]
        public void FilterProviderCollectionCombinedItemsSetResetsCache()
        {
            TestCacheReset((collection) => collection[0] = new Mock<IFilterProvider>(MockBehavior.Strict).Object);
        }

        private static void TestCacheReset(Action<FilterProviderCollection> mutatingAction)
        {
            // Arrange
            var providers = new List<IFilterProvider>() 
            {
                new Mock<IFilterProvider>(MockBehavior.Strict).Object, 
                new Mock<IFilterProvider>(MockBehavior.Strict).Object
            };
            var collection = new FilterProviderCollection(providers);

            // Act
            mutatingAction(collection);

            IFilterProvider[] combined = collection.CombinedItems;

            // Assert
            Assert.Equal(providers, combined);
        }

        [Fact]
        public void FilterProviderCollectionCombinedItemsDelegatesToResolver()
        {
            // Arrange
            var firstProvider = new Mock<IFilterProvider>();
            var secondProvider = new Mock<IFilterProvider>();
            var thirdProvider = new Mock<IFilterProvider>();
            var dependencyProviders = new IFilterProvider[] { firstProvider.Object, secondProvider.Object };
            var collectionProviders = new IFilterProvider[] { thirdProvider.Object };
            var expectedProviders = new IFilterProvider[] { firstProvider.Object, secondProvider.Object, thirdProvider.Object };

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(dependencyProviders);

            var providers = new FilterProviderCollection(collectionProviders, resolver.Object);

            // Act
            IFilterProvider[] combined = providers.CombinedItems;

            // Assert
            Assert.Equal(expectedProviders, combined);
        }

        [Fact]
        public void GetFiltersUsesRegisteredProviders()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var filter = new Filter(new Object(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor)).Returns(new[] { filter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.Same(filter, result.Single());
        }

        [Fact]
        public void GetFiltersDelegatesToResolver()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var filter = new Filter(new Object(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            provider.Setup(p => p.GetFilters(context, descriptor)).Returns(new[] { filter });

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { provider.Object });


            var collection = new FilterProviderCollection(new IFilterProvider[0], resolver.Object);

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.Same(filter, result.Single());
        }

        [Fact]
        public void GetFiltersSortsFiltersByOrderFirstThenScope()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var actionFilter = new Filter(new Object(), FilterScope.Action, null);
            var controllerFilter = new Filter(new Object(), FilterScope.Controller, null);
            var globalFilter = new Filter(new Object(), FilterScope.Global, null);
            var earlyActionFilter = new Filter(new Object(), FilterScope.Action, -100);
            var lateGlobalFilter = new Filter(new Object(), FilterScope.Global, 100);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                .Returns(new[] { actionFilter, controllerFilter, globalFilter, earlyActionFilter, lateGlobalFilter });

            // Act
            Filter[] result = collection.GetFilters(context, descriptor).ToArray();

            // Assert
            Assert.Equal(5, result.Length);
            Assert.Same(earlyActionFilter, result[0]);
            Assert.Same(globalFilter, result[1]);
            Assert.Same(controllerFilter, result[2]);
            Assert.Same(actionFilter, result[3]);
            Assert.Same(lateGlobalFilter, result[4]);
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private class AllowMultipleFalseAttribute : FilterAttribute
        {
        }

        [Fact]
        public void GetFiltersIncludesLastFilterOnlyWithAttributeUsageAllowMultipleFalse()
        { // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleFalseAttribute(), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleFalseAttribute(), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleFalseAttribute(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.Same(actionFilter, result.Single());
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class AllowMultipleTrueAttribute : FilterAttribute
        {
        }

        [Fact]
        public void GetFiltersIncludesAllFiltersWithAttributeUsageAllowMultipleTrue()
        { // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleTrueAttribute(), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleTrueAttribute(), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleTrueAttribute(), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            List<Filter> result = collection.GetFilters(context, descriptor).ToList();

            // Assert
            Assert.Same(globalFilter, result[0]);
            Assert.Same(controllerFilter, result[1]);
            Assert.Same(actionFilter, result[2]);
        }

        private class AllowMultipleCustomFilter : MvcFilter
        {
            public AllowMultipleCustomFilter(bool allowMultiple)
                : base(allowMultiple, -1)
            {
            }
        }

        [Fact]
        public void GetFiltersIncludesLastFilterOnlyWithCustomFilterAllowMultipleFalse()
        { // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleCustomFilter(false), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleCustomFilter(false), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleCustomFilter(false), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            IEnumerable<Filter> result = collection.GetFilters(context, descriptor);

            // Assert
            Assert.Same(actionFilter, result.Single());
        }

        [Fact]
        public void GetFiltersIncludesAllFiltersWithCustomFilterAllowMultipleTrue()
        { // DDB #222988
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var globalFilter = new Filter(new AllowMultipleCustomFilter(true), FilterScope.Global, null);
            var controllerFilter = new Filter(new AllowMultipleCustomFilter(true), FilterScope.Controller, null);
            var actionFilter = new Filter(new AllowMultipleCustomFilter(true), FilterScope.Action, null);
            var provider = new Mock<IFilterProvider>(MockBehavior.Strict);
            var collection = new FilterProviderCollection(new[] { provider.Object });
            provider.Setup(p => p.GetFilters(context, descriptor))
                .Returns(new[] { controllerFilter, actionFilter, globalFilter });

            // Act
            List<Filter> result = collection.GetFilters(context, descriptor).ToList();

            // Assert
            Assert.Same(globalFilter, result[0]);
            Assert.Same(controllerFilter, result[1]);
            Assert.Same(actionFilter, result[2]);
        }
    }
}

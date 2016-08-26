// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class MultiServiceResolverTest
    {
        [Fact]
        public void GetCombinedPrependsFromResolver()
        {
            // Arrange
            IEnumerable<TestProvider> providersFromServiceLocation = GetProvidersFromService();
            IList<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk().ToList();
            IEnumerable<TestProvider> expectedProviders = providersFromServiceLocation.Concat(providersFromItemsThunk);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                .Returns(providersFromServiceLocation);

            // Act
            IEnumerable<TestProvider> returnedProviders = MultiServiceResolver.GetCombined<TestProvider>(providersFromItemsThunk, resolver.Object);

            // Assert
            Assert.Equal(expectedProviders.ToList(), returnedProviders.ToList());
        }

        [Fact]
        public void CurrentReturnsCurrentItemsWhenResolverReturnsNoInstances()
        {
            // Arrange
            IList<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk().ToList();

            // Act
            IEnumerable<TestProvider> returnedProviders = MultiServiceResolver.GetCombined<TestProvider>(providersFromItemsThunk, null);

            // Assert
            Assert.Equal(providersFromItemsThunk.ToList(), returnedProviders.ToList());
        }

        [Fact]
        public void CurrentPropagatesExceptionWhenResolverThrowsNonActivationException()
        {
            // Arrange
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);

            // Act & Assert
            var ex = Assert.Throws<MockException>(
                () => MultiServiceResolver.GetCombined<TestProvider>(null, resolver.Object));
            Assert.Equal(
                "IDependencyResolver.GetServices(System.Web.Mvc.Test.MultiServiceResolverTest+TestProvider) invocation failed with mock behavior Strict." +
                Environment.NewLine +
                "All invocations on the mock must have a corresponding setup.",
                ex.Message,
                ignoreLineEndingDifferences: true);
        }

        private class TestProvider
        {
        }

        private IEnumerable<TestProvider> GetProvidersFromService()
        {
            return new TestProvider[]
            {
                new TestProvider(),
                new TestProvider()
            };
        }

        private IEnumerable<TestProvider> GetProvidersFromItemsThunk()
        {
            return new TestProvider[]
            {
                new TestProvider(),
                new TestProvider()
            };
        }
    }
}

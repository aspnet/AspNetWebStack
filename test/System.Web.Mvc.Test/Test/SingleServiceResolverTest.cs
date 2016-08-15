// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class SingleServiceResolverTest
    {
        [Fact]
        public void ConstructorWithNullThunkArgumentThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new SingleServiceResolver<TestProvider>(null, null, "TestProvider.Current"); },
                "currentValueThunk");

            Assert.ThrowsArgumentNull(
                delegate { new SingleServiceResolver<TestProvider>(null, null, "TestProvider.Current"); },
                "currentValueThunk");

            Assert.ThrowsArgumentNull(
                delegate { new SingleServiceResolver<TestProvider>(() => null, null, "TestProvider.Current"); },
                "defaultValue");
        }

        [Fact]
        public void CurrentConsultsResolver()
        {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromServiceLocation = new TestProvider();

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetService(typeof(TestProvider)))
                .Returns(providerFromServiceLocation);

            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            TestProvider returnedProvider = singleResolver.Current;

            // Assert
            Assert.Equal(providerFromServiceLocation, returnedProvider);
        }

        [Fact]
        public void CurrentReturnsCurrentProviderNotDefaultIfSet()
        {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromCurrentValueThunk = null;
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => providerFromCurrentValueThunk, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            providerFromCurrentValueThunk = new TestProvider();
            TestProvider returnedProvider = singleResolver.Current;

            // Assert
            Assert.Equal(providerFromCurrentValueThunk, returnedProvider);
            resolver.Verify(r => r.GetService(typeof(TestProvider)));
        }

        [Fact]
        public void CurrentCachesResolverResult()
        {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromServiceLocation = new TestProvider();

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetService(typeof(TestProvider)))
                .Returns(providerFromServiceLocation);

            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            TestProvider returnedProvider = singleResolver.Current;
            TestProvider cachedProvider = singleResolver.Current;

            // Assert
            Assert.Equal(providerFromServiceLocation, returnedProvider);
            Assert.Equal(providerFromServiceLocation, cachedProvider);
            resolver.Verify(r => r.GetService(typeof(TestProvider)), Times.Exactly(1));
        }

        [Fact]
        public void CurrentDoesNotQueryResolverAfterReceivingNull()
        {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            TestProvider providerFromCurrentValueThunk = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => providerFromCurrentValueThunk, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act
            TestProvider returnedProvider = singleResolver.Current;
            TestProvider cachedProvider = singleResolver.Current;

            // Assert
            Assert.Equal(providerFromCurrentValueThunk, returnedProvider);
            Assert.Equal(providerFromCurrentValueThunk, cachedProvider);
            resolver.Verify(r => r.GetService(typeof(TestProvider)), Times.Exactly(1));
        }

        [Fact]
        public void CurrentReturnsDefaultIfCurrentNotSet()
        {
            //Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            //Act
            TestProvider returnedProvider = singleResolver.Current;

            // Assert
            Assert.Equal(returnedProvider, providerFromDefaultValue);
            resolver.Verify(l => l.GetService(typeof(TestProvider)));
        }

        [Fact]
        public void CurrentThrowsIfCurrentSetThroughServiceAndSetter()
        {
            // Arrange
            TestProvider providerFromCurrentValueThunk = new TestProvider();
            TestProvider providerFromServiceLocation = new TestProvider();
            TestProvider providerFromDefaultValue = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();

            resolver.Setup(r => r.GetService(typeof(TestProvider)))
                .Returns(providerFromServiceLocation);

            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => providerFromCurrentValueThunk, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            //Act & assert
            Assert.Throws<InvalidOperationException>(
                () => singleResolver.Current,
                "An instance of TestProvider was found in the resolver as well as a custom registered provider in TestProvider.Current. Please set only one or the other."
                );
        }

        [Fact]
        public void CurrentPropagatesExceptionWhenResolverThrowsNonActivationException()
        {
            // Arrange
            TestProvider providerFromDefaultValue = new TestProvider();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            SingleServiceResolver<TestProvider> singleResolver = new SingleServiceResolver<TestProvider>(() => null, providerFromDefaultValue, resolver.Object, "TestProvider.Current");

            // Act & Assert
            var ex = Assert.Throws<MockException>(() => singleResolver.Current);
            Assert.Equal(
                "IDependencyResolver.GetService(System.Web.Mvc.Test.SingleServiceResolverTest+TestProvider) invocation failed with mock behavior Strict." +
                Environment.NewLine +
                "All invocations on the mock must have a corresponding setup.",
                ex.Message,
                ignoreLineEndingDifferences: true);
        }

        private class TestProvider
        {
        }
    }
}

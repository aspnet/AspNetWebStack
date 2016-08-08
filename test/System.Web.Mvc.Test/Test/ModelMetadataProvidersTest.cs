// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ModelMetadataProvidersTest
    {
        [Fact]
        public void DefaultModelMetadataProviderIsCachedDataAnnotations()
        {
            // Arrange
            ModelMetadataProviders providers = new ModelMetadataProviders();

            // Act
            ModelMetadataProvider provider = providers.CurrentInternal;

            // Assert
            Assert.IsType<CachedDataAnnotationsModelMetadataProvider>(provider);
        }

        [Fact]
        public void SettingModelMetadataProviderReturnsSetProvider()
        {
            // Arrange
            ModelMetadataProviders providers = new ModelMetadataProviders();
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act
            providers.CurrentInternal = provider.Object;

            // Assert
            Assert.Same(provider.Object, providers.CurrentInternal);
        }

        [Fact]
        public void SettingNullModelMetadataProviderUsesEmptyModelMetadataProvider()
        {
            // Arrange
            ModelMetadataProviders providers = new ModelMetadataProviders();

            // Act
            providers.CurrentInternal = null;

            // Assert
            Assert.IsType<EmptyModelMetadataProvider>(providers.CurrentInternal);
        }

        [Fact]
        public void ModelMetadataProvidersCurrentDelegatesToResolver()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Resolver<ModelMetadataProvider> resolver = new Resolver<ModelMetadataProvider> { Current = provider.Object };
            ModelMetadataProviders providers = new ModelMetadataProviders(resolver);

            // Act
            ModelMetadataProvider result = providers.CurrentInternal;

            // Assert
            Assert.Same(provider.Object, result);
        }

        private class Resolver<T> : IResolver<T>
        {
            public T Current { get; set; }
        }
    }
}

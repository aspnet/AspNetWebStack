// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Validation.Providers
{
    public class AssociatedValidatorProviderTest
    {
        private static DataAnnotationsModelMetadataProvider _metadataProvider = new DataAnnotationsModelMetadataProvider();
        private static IEnumerable<ModelValidatorProvider> _noValidatorProviders = Enumerable.Empty<ModelValidatorProvider>();

        [Fact]
        public void GetValidatorsGuardClauses()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForType(null, typeof(object));
            Mock<AssociatedValidatorProvider> provider = new Mock<AssociatedValidatorProvider> { CallBase = true };

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => provider.Object.GetValidators(metadata: null, validatorProviders: _noValidatorProviders),
                "metadata");
            Assert.ThrowsArgumentNull(
                () => provider.Object.GetValidators(metadata, validatorProviders: null),
                "validatorProviders");
        }

        [Fact]
        public void GetValidatorsForPropertyWithLocalAttributes()
        {
            // Arrange
            IEnumerable<Attribute> callbackAttributes = null;
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(null, typeof(PropertyModel), "LocalAttributes");
            Mock<TestableAssociatedValidatorProvider> provider = new Mock<TestableAssociatedValidatorProvider> { CallBase = true };
            provider.Setup(p => p.AbstractGetValidators(metadata, _noValidatorProviders, It.IsAny<IEnumerable<Attribute>>()))
                    .Callback<ModelMetadata, IEnumerable<ModelValidatorProvider>, IEnumerable<Attribute>>((m, c, attributes) => callbackAttributes = attributes)
                    .Returns(() => null)
                    .Verifiable();

            // Act
            provider.Object.GetValidators(metadata, _noValidatorProviders);

            // Assert
            provider.Verify();
            Assert.Contains(callbackAttributes, a => a is RequiredAttribute);
        }

        [Fact]
        public void GetValidatorsForPropertyWithMetadataAttributes()
        {
            // Arrange
            IEnumerable<Attribute> callbackAttributes = null;
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(null, typeof(PropertyModel), "MetadataAttributes");
            Mock<TestableAssociatedValidatorProvider> provider = new Mock<TestableAssociatedValidatorProvider> { CallBase = true };
            provider.Setup(p => p.AbstractGetValidators(metadata, _noValidatorProviders, It.IsAny<IEnumerable<Attribute>>()))
                    .Callback<ModelMetadata, IEnumerable<ModelValidatorProvider>, IEnumerable<Attribute>>((m, c, attributes) => callbackAttributes = attributes)
                    .Returns(() => null)
                    .Verifiable();

            // Act
            provider.Object.GetValidators(metadata, _noValidatorProviders);

            // Assert
            provider.Verify();
            Assert.Contains(callbackAttributes, a => a is RangeAttribute);
        }

        [Fact]
        public void GetValidatorsForPropertyWithMixedAttributes()
        {
            // Arrange
            IEnumerable<Attribute> callbackAttributes = null;
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(null, typeof(PropertyModel), "MixedAttributes");
            Mock<TestableAssociatedValidatorProvider> provider = new Mock<TestableAssociatedValidatorProvider> { CallBase = true };
            provider.Setup(p => p.AbstractGetValidators(metadata, _noValidatorProviders, It.IsAny<IEnumerable<Attribute>>()))
                    .Callback<ModelMetadata, IEnumerable<ModelValidatorProvider>, IEnumerable<Attribute>>((m, c, attributes) => callbackAttributes = attributes)
                    .Returns(() => null)
                    .Verifiable();

            // Act
            provider.Object.GetValidators(metadata, _noValidatorProviders);

            // Assert
            provider.Verify();
            Assert.Contains(callbackAttributes, a => a is RangeAttribute);
            Assert.Contains(callbackAttributes, a => a is RequiredAttribute);
        }

        [MetadataType(typeof(Metadata))]
        private class PropertyModel
        {
            [Required]
            public int LocalAttributes { get; set; }

            public string MetadataAttributes { get; set; }

            [Required]
            public double MixedAttributes { get; set; }

            private class Metadata
            {
                [Range(10, 100)]
                public object MetadataAttributes { get; set; }

                [Range(10, 100)]
                public object MixedAttributes { get; set; }
            }
        }

        public abstract class TestableAssociatedValidatorProvider : AssociatedValidatorProvider
        {
            protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
            {
                return AbstractGetValidators(metadata, validatorProviders, attributes);
            }

            public abstract IEnumerable<ModelValidator> AbstractGetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes);
        }
    }
}

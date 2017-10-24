// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.Metadata.Providers
{
    public class AssociatedMetadataProviderTest
    {
        // GetMetadataForProperties

        [Fact]
        public void GetMetadataForPropertiesNullContainerTypeThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => provider.GetMetadataForProperties(new Object(), containerType: null),
                "containerType");
        }

        [Fact]
        public void GetMetadataForPropertiesCreatesMetadataForAllPropertiesOnModelWithPropertyValues()
        {
            // Arrange
            PropertyModel model = new PropertyModel { LocalAttributes = 42, MetadataAttributes = "hello", MixedAttributes = 21.12 };
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act
            provider.GetMetadataForProperties(model, typeof(PropertyModel)).ToList(); // Call ToList() to force the lazy evaluation to evaluate

            // Assert
            CreateMetadataPrototypeParams local = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                m => m.ContainerType == typeof(PropertyModel) && m.PropertyName == "LocalAttributes");
            Assert.Equal(typeof(int), local.ModelType);
            Assert.Contains(local.Attributes, a => a is RequiredAttribute);

            CreateMetadataPrototypeParams metadata = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                m => m.ContainerType == typeof(PropertyModel) && m.PropertyName == "MetadataAttributes");
            Assert.Equal(typeof(string), metadata.ModelType);
            Assert.Contains(metadata.Attributes, a => a is RangeAttribute);

            CreateMetadataPrototypeParams mixed = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                m => m.ContainerType == typeof(PropertyModel) && m.PropertyName == "MixedAttributes");
            Assert.Equal(typeof(double), mixed.ModelType);
            Assert.Contains(mixed.Attributes, a => a is RequiredAttribute);
            Assert.Contains(mixed.Attributes, a => a is RangeAttribute);
        }

        [Fact]
        public void GetMetadataForPropertyWithNullContainerReturnsMetadataWithNullValuesForProperties()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act
            provider.GetMetadataForProperties(null, typeof(PropertyModel)).ToList(); // Call ToList() to force the lazy evaluation to evaluate

            // Assert
            Assert.True(provider.CreateMetadataFromPrototypeLog.Any());
            foreach (var parms in provider.CreateMetadataFromPrototypeLog)
            {
                Assert.Null(parms.Model);
            }
        }

        // GetMetadataForProperty

        [Fact]
        public void GetMetadataForPropertyNullContainerTypeThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => provider.GetMetadataForProperty(modelAccessor: null, containerType: null, propertyName: "propertyName"),
                "containerType");
        }

        [Fact]
        public void GetMetadataForPropertyNullOrEmptyPropertyNameThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgument(
                () => provider.GetMetadataForProperty(modelAccessor: null, containerType: typeof(object), propertyName: null),
                "propertyName",
                "The argument 'propertyName' is null or empty.");
            Assert.ThrowsArgument(
                () => provider.GetMetadataForProperty(modelAccessor: null, containerType: typeof(object), propertyName: String.Empty),
                "propertyName",
                "The argument 'propertyName' is null or empty.");
        }

        [Fact]
        public void GetMetadataForPropertyInvalidPropertyNameThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgument(
                () => provider.GetMetadataForProperty(modelAccessor: null, containerType: typeof(object), propertyName: "BadPropertyName"),
                "propertyName",
                "The property System.Object.BadPropertyName could not be found.");
        }

        [Fact]
        public void GetMetadataForPropertyWithLocalAttributes()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, typeof(PropertyModel), null, typeof(int), "LocalAttributes");
            provider.CreateMetadataFromPrototypeReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(PropertyModel), "LocalAttributes");

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataPrototypeParams metadataPrototypeParams = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                parameters => parameters.PropertyName == "LocalAttributes");
            Assert.Contains(metadataPrototypeParams.Attributes, a => a is RequiredAttribute);
        }

        [Fact]
        public void GetMetadataForPropertyWithMetadataAttributes()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, typeof(PropertyModel), null, typeof(string), "MetadataAttributes");
            provider.CreateMetadataFromPrototypeReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(PropertyModel), "MetadataAttributes");

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataPrototypeParams parms = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                parameters => parameters.PropertyName == "MetadataAttributes");
            Assert.Contains(parms.Attributes, a => a is RangeAttribute);
        }

        [Fact]
        public void GetMetadataForPropertyWithMixedAttributes()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, typeof(PropertyModel), null, typeof(double), "MixedAttributes");
            provider.CreateMetadataFromPrototypeReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(PropertyModel), "MixedAttributes");

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataPrototypeParams parms = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                p => p.PropertyName == "MixedAttributes");
            Assert.Contains(parms.Attributes, a => a is RequiredAttribute);
            Assert.Contains(parms.Attributes, a => a is RangeAttribute);
        }

        // GetMetadataForType

        [Fact]
        public void GetMetadataForTypeNullModelTypeThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => provider.GetMetadataForType(() => new Object(), modelType: null),
                "modelType");
        }

        [Fact]
        public void GetMetadataForTypeIncludesAttributesOnType()
        {
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, null, typeof(TypeModel), null);
            provider.CreateMetadataFromPrototypeReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForType(null, typeof(TypeModel));

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataPrototypeParams parms = Assert.Single(
                provider.CreateMetadataPrototypeLog,
                p => p.ModelType == typeof(TypeModel));
            Assert.Contains(parms.Attributes, a => a is ReadOnlyAttribute);
        }

        // Helpers

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

        private class ModelWithReadOnlyProperty
        {
            public int ReadOnlyProperty { get; private set; }
        }

        [ReadOnly(true)]
        private class TypeModel
        {
        }

        class TestableAssociatedMetadataProvider : AssociatedMetadataProvider<ModelMetadata>
        {
            public List<CreateMetadataPrototypeParams> CreateMetadataPrototypeLog = new List<CreateMetadataPrototypeParams>();
            public List<CreateMetadataFromPrototypeParams> CreateMetadataFromPrototypeLog = new List<CreateMetadataFromPrototypeParams>();
            public ModelMetadata CreateMetadataPrototypeReturnValue = null;
            public ModelMetadata CreateMetadataFromPrototypeReturnValue = null;

            protected override ModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
            {
                CreateMetadataPrototypeLog.Add(new CreateMetadataPrototypeParams
                {
                    Attributes = attributes,
                    ContainerType = containerType,
                    ModelType = modelType,
                    PropertyName = propertyName
                });

                return CreateMetadataPrototypeReturnValue;
            }

            protected override ModelMetadata CreateMetadataFromPrototype(ModelMetadata prototype, Func<object> modelAccessor)
            {
                CreateMetadataFromPrototypeLog.Add(new CreateMetadataFromPrototypeParams
                {
                    Prototype = prototype,
                    Model = modelAccessor == null ? null : modelAccessor()
                });

                return CreateMetadataFromPrototypeReturnValue;
            }
        }

        class CreateMetadataPrototypeParams
        {
            public IEnumerable<Attribute> Attributes { get; set; }
            public Type ContainerType { get; set; }
            public Type ModelType { get; set; }
            public string PropertyName { get; set; }
        }

        class CreateMetadataFromPrototypeParams
        {
            public ModelMetadata Prototype { get; set; }
            public object Model { get; set; }
        }
    }
}

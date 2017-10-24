// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class AssociatedMetadataProviderTest
    {
        // FilterAttributes

        [Fact]
        public void ReadOnlyAttributeIsFilteredOffWhenContainerTypeIsViewPage()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act
            provider.GetMetadataForProperty(() => null, typeof(ViewPage<PropertyModel>), "Model");

            // Assert
            CreateMetadataParams parms = provider.CreateMetadataLog.Single();
            Assert.DoesNotContain(parms.Attributes, a => a is ReadOnlyAttribute);
        }

        [Fact]
        public void ReadOnlyAttributeIsFilteredOffWhenContainerTypeIsViewUserControl()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act
            provider.GetMetadataForProperty(() => null, typeof(ViewUserControl<PropertyModel>), "Model");

            // Assert
            CreateMetadataParams parms = provider.CreateMetadataLog.Single();
            Assert.DoesNotContain(parms.Attributes, a => a is ReadOnlyAttribute);
        }

        [Fact]
        public void ReadOnlyAttributeIsPreservedForReadOnlyModelProperties()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act
            provider.GetMetadataForProperty(() => null, typeof(ModelWithReadOnlyProperty), "ReadOnlyProperty");

            // Assert
            CreateMetadataParams parms = provider.CreateMetadataLog.Single();
            Assert.Contains(parms.Attributes, a => a is ReadOnlyAttribute);
        }

        // GetMetadataForProperties

        [Fact]
        public void GetMetadataForPropertiesNullContainerTypeThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => provider.GetMetadataForProperties(new Object(), null),
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
            CreateMetadataParams local =
                provider.CreateMetadataLog.Single(m => m.ContainerType == typeof(PropertyModel) &&
                                                       m.PropertyName == "LocalAttributes");
            Assert.Equal(typeof(int), local.ModelType);
            Assert.Equal(42, local.Model);
            Assert.Contains(local.Attributes, a => a is RequiredAttribute);

            CreateMetadataParams metadata =
                provider.CreateMetadataLog.Single(m => m.ContainerType == typeof(PropertyModel) &&
                                                       m.PropertyName == "MetadataAttributes");
            Assert.Equal(typeof(string), metadata.ModelType);
            Assert.Equal("hello", metadata.Model);
            Assert.Contains(metadata.Attributes, a => a is RangeAttribute);

            CreateMetadataParams mixed =
                provider.CreateMetadataLog.Single(m => m.ContainerType == typeof(PropertyModel) &&
                                                       m.PropertyName == "MixedAttributes");
            Assert.Equal(typeof(double), mixed.ModelType);
            Assert.Equal(21.12, mixed.Model);
            Assert.Contains(mixed.Attributes, a => a is RequiredAttribute);
            Assert.Contains(mixed.Attributes, a => a is RangeAttribute);
        }

        [Fact]
        public void GetMetadataForPropertiesContainerTest()
        {
            // Arrange
            PropertyModel model = new PropertyModel { LocalAttributes = 42, MetadataAttributes = "hello", MixedAttributes = 21.12 };
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();

            // Act
            List<ModelMetadata> metadata = provider.GetMetadataForProperties(model, typeof(PropertyModel)).ToList();

            // Assert
            Assert.Equal(3, metadata.Count());
            Assert.Same(model, metadata[0].Container);
            Assert.Same(model, metadata[1].Container);
            Assert.Same(model, metadata[2].Container);
        }

        [Fact]
        public void GetMetadataForPropertyWithNullContainerReturnsMetadataWithNullValuesForProperties()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act
            provider.GetMetadataForProperties(null, typeof(PropertyModel)).ToList(); // Call ToList() to force the lazy evaluation to evaluate

            // Assert
            Assert.True(provider.CreateMetadataLog.Any());
            foreach (var parms in provider.CreateMetadataLog)
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
                () => provider.GetMetadataForProperty(null /* model */, null /* containerType */, "propertyName"),
                "containerType");
        }

        [Fact]
        public void GetMetadataForPropertyNullOrEmptyPropertyNameThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => provider.GetMetadataForProperty(null /* model */, typeof(object), null /* propertyName */),
                "propertyName");
            Assert.ThrowsArgumentNullOrEmpty(
                () => provider.GetMetadataForProperty(null, typeof(object), String.Empty),
                "propertyName");
        }

        [Fact]
        public void GetMetadataForPropertyInvalidPropertyNameThrows()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => provider.GetMetadataForProperty(null, typeof(object), "BadPropertyName"),
                "The property System.Object.BadPropertyName could not be found.");
        }

        [Fact]
        public void GetMetadataForPropertyWithLocalAttributes()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, typeof(PropertyModel), null, typeof(int), "LocalAttributes");
            provider.CreateMetadataReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(PropertyModel), "LocalAttributes");

            // Assert
            Assert.Same(metadata, result);
            Assert.Contains(provider.CreateMetadataLog.Single().Attributes, a => a is RequiredAttribute);
        }

        [Fact]
        public void GetMetadataForPropertyWithMetadataAttributes()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, typeof(PropertyModel), null, typeof(string), "MetadataAttributes");
            provider.CreateMetadataReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(PropertyModel), "MetadataAttributes");

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataParams parms = provider.CreateMetadataLog.Single(p => p.PropertyName == "MetadataAttributes");
            Assert.Contains(parms.Attributes, a => a is RangeAttribute);
        }

        [Fact]
        public void GetMetadataForPropertyWithMixedAttributes()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, typeof(PropertyModel), null, typeof(double), "MixedAttributes");
            provider.CreateMetadataReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(PropertyModel), "MixedAttributes");

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataParams parms = provider.CreateMetadataLog.Single(p => p.PropertyName == "MixedAttributes");
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
                () => provider.GetMetadataForType(() => new Object(), null),
                "modelType");
        }

        [Fact]
        public void GetMetadataForTypeIncludesAttributesOnType()
        {
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, null, typeof(TypeModel), null);
            provider.CreateMetadataReturnValue = metadata;

            // Act
            ModelMetadata result = provider.GetMetadataForType(null, typeof(TypeModel));

            // Assert
            Assert.Same(metadata, result);
            CreateMetadataParams parms = provider.CreateMetadataLog.Single(p => p.ModelType == typeof(TypeModel));
            Assert.Contains(parms.Attributes, a => a is ReadOnlyAttribute);
        }

        [AdditionalMetadata("ClassName", "ClassValue")]
        class ClassWithAdditionalMetadata
        {
            [AdditionalMetadata("PropertyName", "PropertyValue")]
            public int MyProperty { get; set; }
        }

        [Fact]
        public void MetadataAwareAttributeCanModifyTypeMetadata()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            provider.CreateMetadataReturnValue = new ModelMetadata(provider, null, null, typeof(ClassWithAdditionalMetadata), null);

            // Act
            ModelMetadata metadata = provider.GetMetadataForType(null, typeof(ClassWithAdditionalMetadata));

            // Assert
            var kvp = metadata.AdditionalValues.Single();
            Assert.Equal("ClassName", kvp.Key);
            Assert.Equal("ClassValue", kvp.Value);
        }

        [Fact]
        public void MetadataAwareAttributeCanModifyPropertyMetadata()
        {
            // Arrange
            TestableAssociatedMetadataProvider provider = new TestableAssociatedMetadataProvider();
            provider.CreateMetadataReturnValue = new ModelMetadata(provider, typeof(ClassWithAdditionalMetadata), null, typeof(int), "MyProperty");

            // Act
            ModelMetadata metadata = provider.GetMetadataForProperty(null, typeof(ClassWithAdditionalMetadata), "MyProperty");

            // Assert
            var kvp = metadata.AdditionalValues.Single();
            Assert.Equal("PropertyName", kvp.Key);
            Assert.Equal("PropertyValue", kvp.Value);
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

        class TestableAssociatedMetadataProvider : AssociatedMetadataProvider
        {
            public List<CreateMetadataParams> CreateMetadataLog = new List<CreateMetadataParams>();
            public ModelMetadata CreateMetadataReturnValue = null;

            protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType,
                                                            Func<object> modelAccessor, Type modelType,
                                                            string propertyName)
            {
                CreateMetadataLog.Add(new CreateMetadataParams
                {
                    Attributes = attributes,
                    ContainerType = containerType,
                    Model = modelAccessor == null ? null : modelAccessor(),
                    ModelType = modelType,
                    PropertyName = propertyName
                });

                return CreateMetadataReturnValue;
            }
        }

        class CreateMetadataParams
        {
            public IEnumerable<Attribute> Attributes { get; set; }
            public Type ContainerType { get; set; }
            public object Model { get; set; }
            public Type ModelType { get; set; }
            public string PropertyName { get; set; }
        }
    }
}

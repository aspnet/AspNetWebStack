// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Metadata
{
    public class ModelMetadataTest
    {
        // Guard clauses

        [Fact]
        public void NullProviderThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ModelMetadata(null /* provider */, null /* containerType */, null /* model */, typeof(object), null /* propertyName */),
                "provider");
        }

        [Fact]
        public void NullTypeThrows()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ModelMetadata(provider.Object, null /* containerType */, null /* model */, null /* modelType */, null /* propertyName */),
                "modelType");
        }

        // Constructor

        [Fact]
        public void DefaultValues()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(Exception), () => "model", typeof(string), "propertyName");

            // Assert
            Assert.Equal(typeof(Exception), metadata.ContainerType);
            Assert.True(metadata.ConvertEmptyStringToNull);
            Assert.Null(metadata.Description);
            Assert.Equal("model", metadata.Model);
            Assert.Equal(typeof(string), metadata.ModelType);
            Assert.Equal("propertyName", metadata.PropertyName);
            Assert.False(metadata.IsReadOnly);
        }

        // IsComplexType

        struct IsComplexTypeModel
        {
        }

        [Fact]
        public void IsComplexTypeTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Object), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(IsComplexTypeModel), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<IsComplexTypeModel>), null).IsComplexType);
        }

        // IsNullableValueType

        [Fact]
        public void IsNullableValueTypeTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsNullableValueType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsNullableValueType);
        }

        // Properties

        [Fact]
        public void PropertiesCallsProvider()
        {
            // Arrange
            Type modelType = typeof(string);
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>();
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                .Returns(propertyMetadata)
                .Verifiable();

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            Assert.Equal(propertyMetadata, result.ToList());
            provider.Verify();
        }

        [Fact]
        public void PropertiesListGetsResetWhenModelGetsReset()
        { // Dev10 Bug #923263
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();
            var metadata = new ModelMetadata(provider, null, () => new Class1(), typeof(Class1), null);

            // Act
            ModelMetadata[] originalProps = metadata.Properties.ToArray();
            metadata.Model = new Class2();
            ModelMetadata[] newProps = metadata.Properties.ToArray();

            // Assert
            ModelMetadata originalProp = Assert.Single(originalProps);
            Assert.Equal(typeof(string), originalProp.ModelType);
            Assert.Equal("Prop1", originalProp.PropertyName);
            ModelMetadata newProp = Assert.Single(newProps);
            Assert.Equal(typeof(int), newProp.ModelType);
            Assert.Equal("Prop2", newProp.PropertyName);
        }

        class Class1
        {
            public string Prop1 { get; set; }
        }

        class Class2
        {
            public int Prop2 { get; set; }
        }

        // GetDisplayName()

        [Fact]
        public void ReturnsPropertyNameWhenSetAndDisplayNameIsNull()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName");

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("PropertyName", result);
        }

        [Fact]
        public void ReturnsTypeNameWhenPropertyNameAndDisplayNameAreNull()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Object", result);
        }

        // Helpers

        private class DummyContactModel
        {
            public int IntField = 0;
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Nullable<int> NullableIntValue { get; set; }
            public int[] Array { get; set; }

            public string this[int index]
            {
                get { return "Indexed into " + index; }
            }
        }

        private class DummyModelContainer
        {
            public DummyContactModel Model { get; set; }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ComplexModelDtoTest
    {
        [Fact]
        public void ConstructorThrowsIfModelMetadataIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new ComplexModelDto(null, Enumerable.Empty<ModelMetadata>()),
                "modelMetadata");
        }

        [Fact]
        public void ConstructorThrowsIfPropertyMetadataIsNull()
        {
            // Arrange
            ModelMetadata modelMetadata = GetModelMetadata();

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new ComplexModelDto(modelMetadata, null),
                "propertyMetadata");
        }

        [Fact]
        public void ConstructorSetsProperties()
        {
            // Arrange
            ModelMetadata modelMetadata = GetModelMetadata();
            ModelMetadata[] propertyMetadata = new ModelMetadata[0];

            // Act
            ComplexModelDto dto = new ComplexModelDto(modelMetadata, propertyMetadata);

            // Assert
            Assert.Equal(modelMetadata, dto.ModelMetadata);
            Assert.Equal(propertyMetadata, dto.PropertyMetadata.ToArray());
            Assert.Empty(dto.Results);
        }

        private static ModelMetadata GetModelMetadata()
        {
            return new ModelMetadata(new EmptyModelMetadataProvider(), typeof(object), null, typeof(object), "PropertyName");
        }
    }
}

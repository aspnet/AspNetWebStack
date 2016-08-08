// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AdditionalMetadataAttributeTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new AdditionalMetadataAttribute(null, new object()),
                "name");

            AdditionalMetadataAttribute attr = new AdditionalMetadataAttribute("key", null);
            Assert.ThrowsArgumentNull(
                () => attr.OnMetadataCreated(null),
                "metadata");
        }

        [Fact]
        public void OnMetaDataCreatedSetsAdditionalValue()
        {
            // Arrange
            string name = "name";
            object value = new object();

            ModelMetadata modelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(object), null);
            AdditionalMetadataAttribute attr = new AdditionalMetadataAttribute(name, value);

            // Act
            attr.OnMetadataCreated(modelMetadata);

            // Assert
            Assert.Equal(modelMetadata.AdditionalValues[name], value);
            Assert.Equal(attr.Name, name);
            Assert.Equal(attr.Value, value);
        }

        [Fact]
        public void MultipleAttributesCanSetValuesOnMetadata()
        {
            // Arrange
            string name1 = "name1";
            string name2 = "name2";

            object value1 = new object();
            object value2 = new object();
            object value3 = new object();

            ModelMetadata modelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(object), null);
            AdditionalMetadataAttribute attr1 = new AdditionalMetadataAttribute(name1, value1);
            AdditionalMetadataAttribute attr2 = new AdditionalMetadataAttribute(name2, value2);
            AdditionalMetadataAttribute attr3 = new AdditionalMetadataAttribute(name1, value3);

            // Act
            attr1.OnMetadataCreated(modelMetadata);
            attr2.OnMetadataCreated(modelMetadata);
            attr3.OnMetadataCreated(modelMetadata);

            // Assert
            Assert.Equal(2, modelMetadata.AdditionalValues.Count);
            Assert.Equal(modelMetadata.AdditionalValues[name1], value3);
            Assert.Equal(modelMetadata.AdditionalValues[name2], value2);

            Assert.NotEqual(attr1.TypeId, attr2.TypeId);
            Assert.NotEqual(attr2.TypeId, attr3.TypeId);
            Assert.NotEqual(attr3.TypeId, attr1.TypeId);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AllowHtmlAttributeTest
    {
        [Fact]
        public void OnMetadataCreated_ThrowsIfMetadataIsNull()
        {
            // Arrange
            AllowHtmlAttribute attr = new AllowHtmlAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnMetadataCreated(null); }, "metadata");
        }

        [Fact]
        public void OnMetadataCreated()
        {
            // Arrange
            ModelMetadata modelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(object), "SomeProperty");
            AllowHtmlAttribute attr = new AllowHtmlAttribute();

            // Act
            bool originalValue = modelMetadata.RequestValidationEnabled;
            attr.OnMetadataCreated(modelMetadata);
            bool newValue = modelMetadata.RequestValidationEnabled;

            // Assert
            Assert.True(originalValue);
            Assert.False(newValue);
        }
    }
}

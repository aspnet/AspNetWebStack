// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class DeserializeAttributeTest
    {
        [Fact]
        public void BinderReturnsDeserializedValue()
        {
            // Arrange
            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Deserialize("some-value")).Returns(42);
            DeserializeAttribute attr = new DeserializeAttribute() { Serializer = mockSerializer.Object };

            IModelBinder binder = attr.GetBinder();
            ModelBindingContext mbContext = new ModelBindingContext
            {
                ModelName = "someKey",
                ValueProvider = new SimpleValueProvider
                {
                    { "someKey", "some-value" }
                }
            };

            // Act
            object retVal = binder.BindModel(null, mbContext);

            // Assert
            Assert.Equal(42, retVal);
        }

        [Fact]
        public void BinderReturnsNullIfValueProviderDoesNotContainKey()
        {
            // Arrange
            DeserializeAttribute attr = new DeserializeAttribute();
            IModelBinder binder = attr.GetBinder();
            ModelBindingContext mbContext = new ModelBindingContext
            {
                ModelName = "someKey",
                ValueProvider = new SimpleValueProvider()
            };

            // Act
            object retVal = binder.BindModel(null, mbContext);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void BinderThrowsIfBindingContextIsNull()
        {
            // Arrange
            DeserializeAttribute attr = new DeserializeAttribute();
            IModelBinder binder = attr.GetBinder();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(null, null); }, "bindingContext");
        }

        [Fact]
        public void BinderThrowsIfDataCorrupt()
        {
            // Arrange
            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Deserialize(It.IsAny<string>())).Throws(new SerializationException());
            DeserializeAttribute attr = new DeserializeAttribute { Serializer = mockSerializer.Object };

            IModelBinder binder = attr.GetBinder();
            ModelBindingContext mbContext = new ModelBindingContext
            {
                ModelName = "someKey",
                ValueProvider = new SimpleValueProvider
                {
                    { "someKey", "This data is corrupted." }
                }
            };

            // Act & assert
            Exception exception = Assert.Throws<SerializationException>(
                delegate { binder.BindModel(null, mbContext); });
        }
    }
}

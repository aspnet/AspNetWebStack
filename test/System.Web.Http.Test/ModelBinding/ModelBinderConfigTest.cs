// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class ModelBinderConfigTest
    {
        [Fact]
        public void GetUserResourceString_NullControllerContext_ReturnsNull()
        {
            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(null /* controllerContext */, "someResourceName", "someResourceClassKey");

            // Assert
            Assert.Null(customResourceString);
        }

        [Fact]
        public void TypeConversionErrorMessageProvider_DefaultValue()
        {
            // Arrange
            ModelMetadata metadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(int), "SomePropertyName");

            // Act
            string errorString = ModelBinderConfig.TypeConversionErrorMessageProvider(null, metadata, "some incoming value");

            // Assert
            Assert.Equal("The value 'some incoming value' is not valid for SomePropertyName.", errorString);
        }

        [Fact]
        public void TypeConversionErrorMessageProvider_Property()
        {
            // Arrange
            ModelBinderConfigWrapper wrapper = new ModelBinderConfigWrapper();

            // Act & assert
            try
            {
                MemberHelper.TestPropertyWithDefaultInstance(wrapper, "TypeConversionErrorMessageProvider", (ModelBinderErrorMessageProvider)DummyErrorSelector);
            }
            finally
            {
                wrapper.Reset();
            }
        }

        [Fact]
        public void ValueRequiredErrorMessageProvider_DefaultValue()
        {
            // Arrange
            ModelMetadata metadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(int), "SomePropertyName");

            // Act
            string errorString = ModelBinderConfig.ValueRequiredErrorMessageProvider(null, metadata, "some incoming value");

            // Assert
            Assert.Equal("A value is required.", errorString);
        }

        [Fact]
        public void ValueRequiredErrorMessageProvider_Property()
        {
            // Arrange
            ModelBinderConfigWrapper wrapper = new ModelBinderConfigWrapper();

            // Act & assert
            try
            {
                MemberHelper.TestPropertyWithDefaultInstance(wrapper, "ValueRequiredErrorMessageProvider", (ModelBinderErrorMessageProvider)DummyErrorSelector);
            }
            finally
            {
                wrapper.Reset();
            }
        }

        private string DummyErrorSelector(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue)
        {
            throw new NotImplementedException();
        }

        private sealed class ModelBinderConfigWrapper
        {
            public ModelBinderErrorMessageProvider TypeConversionErrorMessageProvider
            {
                get { return ModelBinderConfig.TypeConversionErrorMessageProvider; }
                set { ModelBinderConfig.TypeConversionErrorMessageProvider = value; }
            }

            public ModelBinderErrorMessageProvider ValueRequiredErrorMessageProvider
            {
                get { return ModelBinderConfig.ValueRequiredErrorMessageProvider; }
                set { ModelBinderConfig.ValueRequiredErrorMessageProvider = value; }
            }

            public void Reset()
            {
                ModelBinderConfig.TypeConversionErrorMessageProvider = null;
                ModelBinderConfig.ValueRequiredErrorMessageProvider = null;
            }
        }
    }
}

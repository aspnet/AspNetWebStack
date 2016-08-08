// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
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
        public void GetUserResourceString_NullHttpContext_ReturnsNull()
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext).Returns((HttpContextBase)null);

            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(mockControllerContext.Object, "someResourceName", "someResourceClassKey");

            // Assert
            Assert.Null(customResourceString);
        }

        [Fact]
        public void GetUserResourceString_NullResourceKey_ReturnsNull()
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();

            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(mockControllerContext.Object, "someResourceName", null /* resourceClassKey */);

            // Assert
            mockControllerContext.Verify(o => o.HttpContext, Times.Never());
            Assert.Null(customResourceString);
        }

        [Fact]
        public void GetUserResourceString_ValidResourceObject_ReturnsResourceString()
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.GetGlobalResourceObject("someResourceClassKey", "someResourceName", CultureInfo.CurrentUICulture)).Returns("My custom resource string");

            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(mockControllerContext.Object, "someResourceName", "someResourceClassKey");

            // Assert
            Assert.Equal("My custom resource string", customResourceString);
        }

        [Fact]
        public void Initialize_ReplacesOriginalCollection()
        {
            // Arrange
            ModelBinderDictionary oldBinders = new ModelBinderDictionary();
            oldBinders[typeof(int)] = new Mock<IModelBinder>().Object;
            ModelBinderProviderCollection newBinderProviders = new ModelBinderProviderCollection();

            // Act
            ModelBinderConfig.Initialize(oldBinders, newBinderProviders);

            // Assert
            Assert.Empty(oldBinders);

            var shimBinder = Assert.IsType<ExtensibleModelBinderAdapter>(oldBinders.DefaultBinder);
            Assert.Same(newBinderProviders, shimBinder.Providers);
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

        private string DummyErrorSelector(ControllerContext controllerContext, ModelMetadata modelMetadata, object incomingValue)
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

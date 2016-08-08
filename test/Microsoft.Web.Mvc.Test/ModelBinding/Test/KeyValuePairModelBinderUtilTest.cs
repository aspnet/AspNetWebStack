// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class KeyValuePairModelBinderUtilTest
    {
        [Fact]
        public void TryBindStrongModel_BinderExists_BinderReturnsCorrectlyTypedObject_ReturnsTrue()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Equal("someName.key", mbc.ModelName);
                        mbc.Model = 42;
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);

            // Act
            int model;
            bool retVal = KeyValuePairModelBinderUtil.TryBindStrongModel(controllerContext, bindingContext, "key", new EmptyModelMetadataProvider(), out model);

            // Assert
            Assert.True(retVal);
            Assert.Equal(42, model);
            Assert.Single(bindingContext.ValidationNode.ChildNodes);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void TryBindStrongModel_BinderExists_BinderReturnsIncorrectlyTypedObject_ReturnsTrue()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Equal("someName.key", mbc.ModelName);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);

            // Act
            int model;
            bool retVal = KeyValuePairModelBinderUtil.TryBindStrongModel(controllerContext, bindingContext, "key", new EmptyModelMetadataProvider(), out model);

            // Assert
            Assert.True(retVal);
            Assert.Equal(default(int), model);
            Assert.Single(bindingContext.ValidationNode.ChildNodes);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void TryBindStrongModel_NoBinder_ReturnsFalse()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            // Act
            int model;
            bool retVal = KeyValuePairModelBinderUtil.TryBindStrongModel(controllerContext, bindingContext, "key", new EmptyModelMetadataProvider(), out model);

            // Assert
            Assert.False(retVal);
            Assert.Equal(default(int), model);
            Assert.Empty(bindingContext.ValidationNode.ChildNodes);
            Assert.Empty(bindingContext.ModelState);
        }
    }
}

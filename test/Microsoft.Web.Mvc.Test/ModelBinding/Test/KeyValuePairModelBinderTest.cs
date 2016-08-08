// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class KeyValuePairModelBinderTest
    {
        [Fact]
        public void BindModel_MissingKey_ReturnsFalse()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            KeyValuePairModelBinder<int, string> binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.False(retVal);
            Assert.Null(bindingContext.Model);
            Assert.Empty(bindingContext.ValidationNode.ChildNodes);
        }

        [Fact]
        public void BindModel_MissingValue_ReturnsTrue()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = 42;
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);

            KeyValuePairModelBinder<int, string> binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Null(bindingContext.Model);
            Assert.Equal(new[] { "someName.key" }, bindingContext.ValidationNode.ChildNodes.Select(n => n.ModelStateKey).ToArray());
        }

        [Fact]
        public void BindModel_SubBindingSucceeds()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = 42;
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);
            Mock<IExtensibleModelBinder> mockStringBinder = new Mock<IExtensibleModelBinder>();
            mockStringBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = "forty-two";
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(string), mockStringBinder.Object, true /* suppressPrefixCheck */);

            KeyValuePairModelBinder<int, string> binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = binder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(new KeyValuePair<int, string>(42, "forty-two"), bindingContext.Model);
            Assert.Equal(new[] { "someName.key", "someName.value" }, bindingContext.ValidationNode.ChildNodes.Select(n => n.ModelStateKey).ToArray());
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding.Binders
{
    public class KeyValuePairModelBinderUtilTest
    {
        [Fact]
        public void TryBindStrongModel_BinderExists_BinderReturnsCorrectlyTypedObject_ReturnsTrue()
        {
            // Arrange
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider()
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object) { SuppressPrefixCheck = true });

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc) =>
                {
                    Assert.Equal("someName.key", mbc.ModelName);
                    mbc.Model = 42;
                    return true;
                });

            // Act
            int model;
            bool retVal = context.TryBindStrongModel(bindingContext, "key", new EmptyModelMetadataProvider(), out model);

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
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider()
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object) { SuppressPrefixCheck = true });

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc) =>
                {
                    Assert.Equal("someName.key", mbc.ModelName);
                    return true;
                });

            // Act
            int model;
            bool retVal = context.TryBindStrongModel(bindingContext, "key", new EmptyModelMetadataProvider(), out model);

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
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider()
            };

            // Act
            int model;
            bool retVal = context.TryBindStrongModel(bindingContext, "key", new EmptyModelMetadataProvider(), out model);

            // Assert
            Assert.False(retVal);
            Assert.Equal(default(int), model);
            Assert.Empty(bindingContext.ValidationNode.ChildNodes);
            Assert.Empty(bindingContext.ModelState);
        }
    }
}

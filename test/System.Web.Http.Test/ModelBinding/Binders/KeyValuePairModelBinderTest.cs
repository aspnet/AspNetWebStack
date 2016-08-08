// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding.Binders
{
    public class KeyValuePairModelBinderTest
    {
        [Fact]
        public void BindModel_MissingKey_ReturnsFalse()
        {
            // Arrange
            KeyValuePairModelBinder<int, string> binder = new KeyValuePairModelBinder<int, string>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider()
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(KeyValuePair<int, string>), binder));

            // Act
            bool retVal = binder.BindModel(context, bindingContext);

            // Assert
            Assert.False(retVal);
            Assert.Null(bindingContext.Model);
            Assert.Empty(bindingContext.ValidationNode.ChildNodes);
        }

        [Fact]
        public void BindModel_MissingValue_ReturnsTrue()
        {
            // Arrange
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider()
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object) { SuppressPrefixCheck = true });

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc) =>
                {
                    mbc.Model = 42;
                    return true;
                });
            KeyValuePairModelBinder<int, string> binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = binder.BindModel(context, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Null(bindingContext.Model);
            Assert.Equal(new[] { "someName.key" }, bindingContext.ValidationNode.ChildNodes.Select(n => n.ModelStateKey).ToArray());
        }

        [Fact]
        public void BindModel_SubBindingSucceeds()
        {
            // Arrange
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            Mock<IModelBinder> mockStringBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider()
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.ReplaceRange(typeof(ModelBinderProvider),
                new ModelBinderProvider[] {
                    new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object) { SuppressPrefixCheck = true },
                    new SimpleModelBinderProvider(typeof(string), mockStringBinder.Object) { SuppressPrefixCheck = true }
                });

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc) =>
                {
                    mbc.Model = 42;
                    return true;
                });
            mockStringBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc) =>
                {
                    mbc.Model = "forty-two";
                    return true;
                });
            KeyValuePairModelBinder<int, string> binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = binder.BindModel(context, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(new KeyValuePair<int, string>(42, "forty-two"), bindingContext.Model);
            Assert.Equal(new[] { "someName.key", "someName.value" }, bindingContext.ValidationNode.ChildNodes.Select(n => n.ModelStateKey).ToArray());
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ArrayModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int[])),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName[0]", "42" },
                    { "someName[1]", "84" }
                }
            };
            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            // Act
            bool retVal = new ArrayModelBinder<int>().BindModel(context, bindingContext);

            // Assert
            Assert.True(retVal);

            int[] array = bindingContext.Model as int[];
            Assert.Equal(new[] { 42, 84 }, array);
        }

        [Fact]
        public void GetBinder_ValueProviderDoesNotContainPrefix_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int[])),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider()
            };

            ArrayModelBinderProvider binderProvider = new ArrayModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);
            bool bound = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(bound);
        }

        [Fact]
        public void GetBinder_ModelMetadataReturnsReadOnly_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int[])),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo[0]", "42" },
                }
            };
            bindingContext.ModelMetadata.IsReadOnly = true;

            ArrayModelBinderProvider binderProvider = new ArrayModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);
            bool bound = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(bound);
        }
    }
}

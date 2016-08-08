// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ComplexModelDtoModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            MyModel model = new MyModel();
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => model, typeof(MyModel));
            ComplexModelDto dto = new ComplexModelDto(modelMetadata, modelMetadata.Properties);

            Mock<IExtensibleModelBinder> mockStringBinder = new Mock<IExtensibleModelBinder>();
            mockStringBinder
                .Setup(b => b.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Equal(typeof(string), mbc.ModelType);
                        Assert.Equal("theModel.StringProperty", mbc.ModelName);
                        mbc.ValidationNode = new ModelValidationNode(mbc.ModelMetadata, "theModel.StringProperty");
                        mbc.Model = "someStringValue";
                        return true;
                    });

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(b => b.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Equal(typeof(int), mbc.ModelType);
                        Assert.Equal("theModel.IntProperty", mbc.ModelName);
                        mbc.ValidationNode = new ModelValidationNode(mbc.ModelMetadata, "theModel.IntProperty");
                        mbc.Model = 42;
                        return true;
                    });

            Mock<IExtensibleModelBinder> mockDateTimeBinder = new Mock<IExtensibleModelBinder>();
            mockDateTimeBinder
                .Setup(b => b.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Equal(typeof(DateTime), mbc.ModelType);
                        Assert.Equal("theModel.DateTimeProperty", mbc.ModelName);
                        return false;
                    });

            ModelBinderProviderCollection binders = new ModelBinderProviderCollection();
            binders.RegisterBinderForType(typeof(string), mockStringBinder.Object, true /* suppressPrefixCheck */);
            binders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);
            binders.RegisterBinderForType(typeof(DateTime), mockDateTimeBinder.Object, true /* suppressPrefixCheck */);

            ExtensibleModelBindingContext parentBindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => dto, typeof(ComplexModelDto)),
                ModelName = "theModel",
                ModelBinderProviders = binders
            };

            ComplexModelDtoModelBinder binder = new ComplexModelDtoModelBinder();

            // Act
            bool retVal = binder.BindModel(controllerContext, parentBindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(dto, parentBindingContext.Model);

            ComplexModelDtoResult stringDtoResult = dto.Results[dto.PropertyMetadata.Where(m => m.ModelType == typeof(string)).First()];
            Assert.Equal("someStringValue", stringDtoResult.Model);
            Assert.Equal("theModel.StringProperty", stringDtoResult.ValidationNode.ModelStateKey);

            ComplexModelDtoResult intDtoResult = dto.Results[dto.PropertyMetadata.Where(m => m.ModelType == typeof(int)).First()];
            Assert.Equal(42, intDtoResult.Model);
            Assert.Equal("theModel.IntProperty", intDtoResult.ValidationNode.ModelStateKey);

            ComplexModelDtoResult dateTimeDtoResult = dto.Results[dto.PropertyMetadata.Where(m => m.ModelType == typeof(DateTime)).First()];
            Assert.Null(dateTimeDtoResult);
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, modelType, "SomeProperty")
            };
        }

        private sealed class MyModel
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public object ObjectProperty { get; set; } // no binding should happen since no registered binder
            public DateTime DateTimeProperty { get; set; } // registered binder returns false
        }
    }
}

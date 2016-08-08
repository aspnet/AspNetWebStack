// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Validation;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ComplexModelDtoModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            MyModel model = new MyModel();
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => model, typeof(MyModel));
            ComplexModelDto dto = new ComplexModelDto(modelMetadata, modelMetadata.Properties);
            Mock<IModelBinder> mockStringBinder = new Mock<IModelBinder>();
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            Mock<IModelBinder> mockDateTimeBinder = new Mock<IModelBinder>();
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.ReplaceRange(typeof(ModelBinderProvider),
                new ModelBinderProvider[] {
                    new SimpleModelBinderProvider(typeof(string), mockStringBinder.Object) { SuppressPrefixCheck = true },
                    new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object) { SuppressPrefixCheck = true },
                    new SimpleModelBinderProvider(typeof(DateTime), mockDateTimeBinder.Object) { SuppressPrefixCheck = true }
                });

            mockStringBinder
                .Setup(b => b.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    Assert.Equal(typeof(string), mbc.ModelType);
                    Assert.Equal("theModel.StringProperty", mbc.ModelName);
                    mbc.ValidationNode = new ModelValidationNode(mbc.ModelMetadata, "theModel.StringProperty");
                    mbc.Model = "someStringValue";
                    return true;
                });
            mockIntBinder
                .Setup(b => b.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    Assert.Equal(typeof(int), mbc.ModelType);
                    Assert.Equal("theModel.IntProperty", mbc.ModelName);
                    mbc.ValidationNode = new ModelValidationNode(mbc.ModelMetadata, "theModel.IntProperty");
                    mbc.Model = 42;
                    return true;
                });
            mockDateTimeBinder
                .Setup(b => b.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    Assert.Equal(typeof(DateTime), mbc.ModelType);
                    Assert.Equal("theModel.DateTimeProperty", mbc.ModelName);
                    return false;
                });
            ModelBindingContext parentBindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => dto, typeof(ComplexModelDto)),
                ModelName = "theModel",
            };
            ComplexModelDtoModelBinder binder = new ComplexModelDtoModelBinder();

            // Act
            bool retVal = binder.BindModel(context, parentBindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(dto, parentBindingContext.Model);

            ComplexModelDtoResult stringDtoResult = dto.Results[dto.PropertyMetadata.Where(m => m.ModelType == typeof(string)).First()];
            Assert.Equal("someStringValue", stringDtoResult.Model);
            Assert.Equal("theModel.StringProperty", stringDtoResult.ValidationNode.ModelStateKey);

            ComplexModelDtoResult intDtoResult = dto.Results[dto.PropertyMetadata.Where(m => m.ModelType == typeof(int)).First()];
            Assert.Equal(42, intDtoResult.Model);
            Assert.Equal("theModel.IntProperty", intDtoResult.ValidationNode.ModelStateKey);

            // Bind failed, so DateTime won't even be in the DTO dictionary
            bool containsMissingKey = dto.Results.ContainsKey(dto.PropertyMetadata.Where(m => m.ModelType == typeof(DateTime)).First());
            Assert.False(containsMissingKey);
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

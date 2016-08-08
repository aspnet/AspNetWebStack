// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding.Binders
{
    public class DictionaryModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            Mock<IModelBinder> mockKvpBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName[0]", new KeyValuePair<int, string>(42, "forty-two") },
                    { "someName[1]", new KeyValuePair<int, string>(84, "eighty-four") }
                }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), (new SimpleModelBinderProvider(typeof(KeyValuePair<int, string>), mockKvpBinder.Object)));

            mockKvpBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            // Act
            bool retVal = new DictionaryModelBinder<int, string>().BindModel(context, bindingContext);

            // Assert
            Assert.True(retVal);

            var dictionary = Assert.IsAssignableFrom<IDictionary<int, string>>(bindingContext.Model);
            Assert.NotNull(dictionary);
            Assert.Equal(2, dictionary.Count);
            Assert.Equal("forty-two", dictionary[42]);
            Assert.Equal("eighty-four", dictionary[84]);
        }
    }
}

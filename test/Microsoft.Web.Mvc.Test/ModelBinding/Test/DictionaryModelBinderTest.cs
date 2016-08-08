// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class DictionaryModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName[0]", new KeyValuePair<int, string>(42, "forty-two") },
                    { "someName[1]", new KeyValuePair<int, string>(84, "eighty-four") }
                }
            };

            Mock<IExtensibleModelBinder> mockKvpBinder = new Mock<IExtensibleModelBinder>();
            mockKvpBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(KeyValuePair<int, string>), mockKvpBinder.Object, false /* suppressPrefixCheck */);

            // Act
            bool retVal = new DictionaryModelBinder<int, string>().BindModel(controllerContext, bindingContext);

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

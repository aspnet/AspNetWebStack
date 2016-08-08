// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ArrayModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int[])),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName[0]", "42" },
                    { "someName[1]", "84" }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, false /* suppressPrefixCheck */);

            // Act
            bool retVal = new ArrayModelBinder<int>().BindModel(controllerContext, bindingContext);

            // Assert
            Assert.True(retVal);

            int[] array = bindingContext.Model as int[];
            Assert.Equal(new[] { 42, 84 }, array);
        }
    }
}

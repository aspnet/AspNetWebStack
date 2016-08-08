// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ComplexModelDtoModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_TypeDoesNotMatch_ReturnsNull()
        {
            // Arrange
            ComplexModelDtoModelBinderProvider provider = new ComplexModelDtoModelBinderProvider();
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(object));

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeMatches_ReturnsBinder()
        {
            // Arrange
            ComplexModelDtoModelBinderProvider provider = new ComplexModelDtoModelBinderProvider();
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(ComplexModelDto));

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<ComplexModelDtoModelBinder>(binder);
        }

        private static ExtensibleModelBindingContext GetBindingContext(Type modelType)
        {
            return new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => null, modelType)
            };
        }
    }
}

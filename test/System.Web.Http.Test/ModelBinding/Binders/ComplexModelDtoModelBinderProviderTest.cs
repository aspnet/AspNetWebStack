// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ComplexModelDtoModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_TypeDoesNotMatch_ReturnsNull()
        {
            // Arrange
            ComplexModelDtoModelBinderProvider provider = new ComplexModelDtoModelBinderProvider();
            ModelBindingContext bindingContext = GetBindingContext(typeof(object));

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => null, modelType)
            };
        }
    }
}

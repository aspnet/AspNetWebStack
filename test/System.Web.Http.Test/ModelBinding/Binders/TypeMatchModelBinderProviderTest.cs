// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class TypeMatchModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_InvalidValueProviderResult_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeMatchModelBinderProvider provider = new TypeMatchModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext.ModelType);
            bool bound = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(bound);
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", 42 }
            };

            TypeMatchModelBinderProvider provider = new TypeMatchModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.IsType<TypeMatchModelBinder>(binder);
        }

        private static ModelBindingContext GetBindingContext()
        {
            return GetBindingContext(typeof(int));
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName"
            };
        }
    }
}

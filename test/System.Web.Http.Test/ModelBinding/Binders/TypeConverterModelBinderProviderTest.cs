// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class TypeConverterModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_NoTypeConverterExistsFromString_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(void)); // no TypeConverter exists Void -> String

            TypeConverterModelBinderProvider provider = new TypeConverterModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_NullValueProviderResult_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(int));
            bindingContext.ValueProvider = new SimpleHttpValueProvider(); // clear the ValueProvider

            TypeConverterModelBinderProvider provider = new TypeConverterModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext.ModelType);
            bool bound = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(bound);
        }

        [Fact]
        public void GetBinder_TypeConverterExistsFromString_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(int)); // TypeConverter exists Int32 -> String

            TypeConverterModelBinderProvider provider = new TypeConverterModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.IsType<TypeConverterModelBinder>(binder);
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "theModelName", "someValue" }
                }
            };
        }
    }
}

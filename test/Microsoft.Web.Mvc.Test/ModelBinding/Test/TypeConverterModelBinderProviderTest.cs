// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class TypeConverterModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_NoTypeConverterExistsFromString_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(void)); // no TypeConverter exists Void -> String

            TypeConverterModelBinderProvider provider = new TypeConverterModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_NullValueProviderResult_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(int));
            bindingContext.ValueProvider = new SimpleValueProvider(); // clear the ValueProvider

            TypeConverterModelBinderProvider provider = new TypeConverterModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeConverterExistsFromString_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(int)); // TypeConverter exists Int32 -> String

            TypeConverterModelBinderProvider provider = new TypeConverterModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<TypeConverterModelBinder>(binder);
        }

        private static ExtensibleModelBindingContext GetBindingContext(Type modelType)
        {
            return new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName",
                ValueProvider = new SimpleValueProvider
                {
                    { "theModelName", "someValue" }
                }
            };
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class TypeMatchModelBinderProviderTest
    {
        [Fact]
        public void ProviderIsMarkedFrontOfList()
        {
            // Arrange
            Type t = typeof(TypeMatchModelBinderProvider);

            // Act & assert
            Assert.True(t.GetCustomAttributes(typeof(ModelBinderProviderOptionsAttribute), true /* inherit */).Cast<ModelBinderProviderOptionsAttribute>().Single().FrontOfList);
        }

        [Fact]
        public void GetBinder_InvalidValueProviderResult_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeMatchModelBinderProvider provider = new TypeMatchModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ReturnsBinder()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider
            {
                { "theModelName", 42 }
            };

            TypeMatchModelBinderProvider provider = new TypeMatchModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<TypeMatchModelBinder>(binder);
        }

        private static ExtensibleModelBindingContext GetBindingContext()
        {
            return GetBindingContext(typeof(int));
        }

        private static ExtensibleModelBindingContext GetBindingContext(Type modelType)
        {
            return new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName"
            };
        }
    }
}

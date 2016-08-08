// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class KeyValuePairModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_CorrectModelTypeAndValueProviderEntries_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.key", 42 },
                    { "foo.value", "someValue" }
                }
            };

            KeyValuePairModelBinderProvider binderProvider = new KeyValuePairModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.IsType<KeyValuePairModelBinder<int, string>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsIncorrect_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.key", 42 },
                    { "foo.value", "someValue" }
                }
            };

            KeyValuePairModelBinderProvider binderProvider = new KeyValuePairModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class KeyValuePairModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_CorrectModelTypeAndValueProviderEntries_ReturnsBinder()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.key", 42 },
                    { "foo.value", "someValue" }
                }
            };

            KeyValuePairModelBinderProvider binderProvider = new KeyValuePairModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<KeyValuePairModelBinder<int, string>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsIncorrect_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.key", 42 },
                    { "foo.value", "someValue" }
                }
            };

            KeyValuePairModelBinderProvider binderProvider = new KeyValuePairModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ValueProviderDoesNotContainKeyProperty_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.value", "someValue" }
                }
            };

            KeyValuePairModelBinderProvider binderProvider = new KeyValuePairModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ValueProviderDoesNotContainValueProperty_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.key", 42 }
                }
            };

            KeyValuePairModelBinderProvider binderProvider = new KeyValuePairModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }
    }
}

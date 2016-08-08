// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_CorrectModelTypeAndValueProviderEntries_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.IsType<DictionaryModelBinder<int, string>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsIncorrect_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsNullable_ReturnsNull()
        {
            // Arrange
            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, typeof(int?));

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsGeneric_ReturnsNull()
        {
            // Arrange
            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, typeof(Tuple<int>));

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ValueProviderDoesNotContainPrefix_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider()
            };

            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);
            bool bound = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(bound);
        }
    }
}

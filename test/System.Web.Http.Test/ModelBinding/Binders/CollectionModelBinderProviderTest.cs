// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    public class CollectionModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_CorrectModelTypeAndValueProviderEntries_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IEnumerable<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.IsType<CollectionModelBinder<int>>(binder);
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

            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsNullable_ReturnsNull()
        {
            // Arrange
            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, typeof(int?));

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsGeneric_ReturnsNull()
        {
            // Arrange
            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

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
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IEnumerable<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider()
            };

            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);
            bool bound = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(bound);
        }
    }
}

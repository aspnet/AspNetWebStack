// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class MutableObjectModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_NoPrefixInValueProvider_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => 42, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider()
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_PrefixInValueProvider_ComplexType_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new MutableTestType(), typeof(MutableTestType)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<MutableObjectModelBinder>(binder);
        }

        [Fact]
        public void GetBinder_PrefixInValueProvider_SimpleType_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => 42, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeIsComplexModelDto_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ComplexModelDto)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext.ModelType);

            // Assert
            Assert.Null(binder);
        }

        class MutableTestType
        {
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class CollectionModelBinderTest
    {
        [Fact]
        public void BindComplexCollectionFromIndexes_FiniteIndexes()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName[foo]", "42" },
                    { "someName[baz]", "200" }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, false /* suppressPrefixCheck */);

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindComplexCollectionFromIndexes(controllerContext, bindingContext, new[] { "foo", "bar", "baz" });

            // Assert
            Assert.Equal(new[] { 42, 0, 200 }, boundCollection.ToArray());
            Assert.Equal(new[] { "someName[foo]", "someName[baz]" }, bindingContext.ValidationNode.ChildNodes.Select(o => o.ModelStateKey).ToArray());
        }

        [Fact]
        public void BindComplexCollectionFromIndexes_InfiniteIndexes()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName[0]", "42" },
                    { "someName[1]", "100" },
                    { "someName[3]", "400" }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, false /* suppressPrefixCheck */);

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindComplexCollectionFromIndexes(controllerContext, bindingContext, null /* indexNames */);

            // Assert
            Assert.Equal(new[] { 42, 100 }, boundCollection.ToArray());
            Assert.Equal(new[] { "someName[0]", "someName[1]" }, bindingContext.ValidationNode.ChildNodes.Select(o => o.ModelStateKey).ToArray());
        }

        [Fact]
        public void BindModel_ComplexCollection()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName.index", new[] { "foo", "bar", "baz" } },
                    { "someName[foo]", "42" },
                    { "someName[bar]", "100" },
                    { "someName[baz]", "200" }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);

            CollectionModelBinder<int> modelBinder = new CollectionModelBinder<int>();

            // Act
            bool retVal = modelBinder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal(new[] { 42, 100, 200 }, ((List<int>)bindingContext.Model).ToArray());
        }

        [Fact]
        public void BindModel_SimpleCollection()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider
                {
                    { "someName", new[] { "42", "100", "200" } }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);

            CollectionModelBinder<int> modelBinder = new CollectionModelBinder<int>();

            // Act
            bool retVal = modelBinder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(new[] { 42, 100, 200 }, ((List<int>)bindingContext.Model).ToArray());
        }

        [Fact]
        public void BindSimpleCollection_RawValueIsEmptyCollection_ReturnsEmptyList()
        {
            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindSimpleCollection(null, null, new object[0], null);

            // Assert
            Assert.NotNull(boundCollection);
            Assert.Empty(boundCollection);
        }

        [Fact]
        public void BindSimpleCollection_RawValueIsNull_ReturnsNull()
        {
            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindSimpleCollection(null, null, null, null);

            // Assert
            Assert.Null(boundCollection);
        }

        [Fact]
        public void BindSimpleCollection_SubBinderDoesNotExist()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindSimpleCollection(controllerContext, bindingContext, new int[1], culture);

            // Assert
            Assert.Equal(new[] { 0 }, boundCollection.ToArray());
            Assert.Empty(bindingContext.ValidationNode.ChildNodes);
        }

        [Fact]
        public void BindSimpleCollection_SubBindingSucceeds()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ValueProvider = new SimpleValueProvider()
            };

            ModelValidationNode childValidationNode = null;
            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Equal("someName", mbc.ModelName);
                        childValidationNode = mbc.ValidationNode;
                        mbc.Model = 42;
                        return true;
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, true /* suppressPrefixCheck */);

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindSimpleCollection(controllerContext, bindingContext, new int[1], culture);

            // Assert
            Assert.Equal(new[] { 42 }, boundCollection.ToArray());
            Assert.Equal(new[] { childValidationNode }, bindingContext.ValidationNode.ChildNodes.ToArray());
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Util;
using System.Web.Http.Validation;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class CollectionModelBinderTest
    {
        [Fact]
        public void BindComplexCollectionFromIndexes_FiniteIndexes()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName[foo]", "42" },
                    { "someName[baz]", "200" }
                }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindComplexCollectionFromIndexes(context, bindingContext, new[] { "foo", "bar", "baz" });

            // Assert
            Assert.Equal(new[] { 42, 0, 200 }, boundCollection.ToArray());
            Assert.Equal(new[] { "someName[foo]", "someName[baz]" }, bindingContext.ValidationNode.ChildNodes.Select(o => o.ModelStateKey).ToArray());
        }

        [Fact]
        public void BindComplexCollectionFromIndexes_InfiniteIndexes()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName[0]", "42" },
                    { "someName[1]", "100" },
                    { "someName[3]", "400" }
                }
            };

            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindComplexCollectionFromIndexes(context, bindingContext, null /* indexNames */);

            // Assert
            Assert.Equal(new[] { 42, 100 }, boundCollection.ToArray());
            Assert.Equal(new[] { "someName[0]", "someName[1]" }, bindingContext.ValidationNode.ChildNodes.Select(o => o.ModelStateKey).ToArray());
        }

        [Fact]
        public void BindModel_ComplexCollection()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName.index", new[] { "foo", "bar", "baz" } },
                    { "someName[foo]", "42" },
                    { "someName[bar]", "100" },
                    { "someName[baz]", "200" }
                }
            };

            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            CollectionModelBinder<int> modelBinder = new CollectionModelBinder<int>();

            // Act
            bool retVal = modelBinder.BindModel(context, bindingContext);

            // Assert
            Assert.Equal(new[] { 42, 100, 200 }, ((List<int>)bindingContext.Model).ToArray());
        }

        [Fact]
        public void BindModel_SimpleCollection()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName", new[] { "42", "100", "200" } }
                }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            CollectionModelBinder<int> modelBinder = new CollectionModelBinder<int>();

            // Act
            bool retVal = modelBinder.BindModel(context, bindingContext);

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
        public void BindSimpleCollection_SubBindingSucceeds()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider()
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            ModelValidationNode childValidationNode = null;
            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    Assert.Equal("someName", mbc.ModelName);
                    childValidationNode = mbc.ValidationNode;
                    mbc.Model = 42;
                    return true;
                });

            // Act
            List<int> boundCollection = CollectionModelBinder<int>.BindSimpleCollection(context, bindingContext, new int[1], culture);

            // Assert
            Assert.Equal(new[] { 42 }, boundCollection.ToArray());
            Assert.Equal(new[] { childValidationNode }, bindingContext.ValidationNode.ChildNodes.ToArray());
        }

        [Fact]
        public void BindingUnboundedCollection_WhenNoValuesArePresent_ProducesSingleEntryCollection()
        {
            // Arrange
            string propertyName = "Addresses";
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(
                                                            modelAccessor: null,
                                                            containerType: typeof(UserWithAddress),
                                                            propertyName: propertyName);
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = propertyName,
                ValueProvider = new SimpleHttpValueProvider { { propertyName, "some value" } }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            CollectionModelBinder<UserWithAddress> binder = new CollectionModelBinder<UserWithAddress>();

            // Act
            bool result = binder.BindModel(context, bindingContext);

            // Assert
            Assert.True(result);
            List<UserWithAddress> boundModel = Assert.IsType<List<UserWithAddress>>(bindingContext.Model);
            UserWithAddress listModel = Assert.Single(boundModel);
            Assert.Null(listModel.Addresses);
        }

        [Fact]
        public void BindingNestedUnboundedCollection_WhenNoValuesArePresent_ProducesEmptyCollection()
        {
            // Arrange
            string propertyName = "Addresses";
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(
                                                            modelAccessor: null,
                                                            containerType: typeof(UserWithAddress),
                                                            propertyName: propertyName);
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = propertyName,
                ValueProvider = new SimpleHttpValueProvider { { "Addresses.AddressLines", "some value" } }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            CollectionModelBinder<UserWithAddress> binder = new CollectionModelBinder<UserWithAddress>();

            // Act
            bool result = binder.BindModel(context, bindingContext);

            // Assert
            Assert.True(result);
            List<UserWithAddress> boundModel = Assert.IsType<List<UserWithAddress>>(bindingContext.Model);
            Assert.Empty(boundModel);
        }

        [Fact]
        public void BindingListsWithIndex_ProducesSingleLengthCollection_WithNullValues()
        {
            // Arrange
            string propertyName = "Addresses";
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(
                                                            modelAccessor: null,
                                                            containerType: typeof(UserWithAddress),
                                                            propertyName: propertyName);
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = propertyName,
                ValueProvider = new SimpleHttpValueProvider { { "Addresses.index", "10000" } }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            CollectionModelBinder<UserWithAddress> binder = new CollectionModelBinder<UserWithAddress>();

            // Act
            bool result = binder.BindModel(context, bindingContext);

            // Assert
            Assert.True(result);
            List<UserWithAddress> boundModel = Assert.IsType<List<UserWithAddress>>(bindingContext.Model);
            UserWithAddress listModel = Assert.Single(boundModel);
            Assert.Null(listModel);
        }

        [Fact]
        public void BindingUnboundedCollection_WithRecursiveRelation_ProducesSingleLengthCollection()
        {
            // Arrange
            string propertyName = "People";
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(
                                                            modelAccessor: null,
                                                            containerType: typeof(PeopleModel),
                                                            propertyName: propertyName);
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = propertyName,
                ValueProvider = new SimpleHttpValueProvider { { propertyName, "test value" } }
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            CollectionModelBinder<Person> binder =
                new CollectionModelBinder<Person>();

            // Act
            bool result = binder.BindModel(context, bindingContext);

            // Assert
            Assert.True(result);
            List<Person> boundModel =
                Assert.IsType<List<Person>>(bindingContext.Model);
            Person type = Assert.Single(boundModel);
            Assert.Null(type.Name);
        }
    }
}

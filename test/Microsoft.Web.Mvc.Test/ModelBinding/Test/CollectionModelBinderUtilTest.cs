// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class CollectionModelBinderUtilTest
    {
        [Fact]
        public void CreateOrReplaceCollection_OriginalModelImmutable_CreatesNewInstance()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new ReadOnlyCollection<int>(new int[0]), typeof(ICollection<int>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceCollection(bindingContext, new[] { 10, 20, 30 }, () => new List<int>());

            // Assert
            int[] newModel = (bindingContext.Model as ICollection<int>).ToArray();
            Assert.Equal(new[] { 10, 20, 30 }, newModel);
        }

        [Fact]
        public void CreateOrReplaceCollection_OriginalModelMutable_UpdatesOriginalInstance()
        {
            // Arrange
            List<int> originalInstance = new List<int> { 10, 20, 30 };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => originalInstance, typeof(ICollection<int>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceCollection(bindingContext, new[] { 40, 50, 60 }, () => new List<int>());

            // Assert
            Assert.Same(originalInstance, bindingContext.Model);
            Assert.Equal(new[] { 40, 50, 60 }, originalInstance.ToArray());
        }

        [Fact]
        public void CreateOrReplaceCollection_OriginalModelNotCollection_CreatesNewInstance()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ICollection<int>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceCollection(bindingContext, new[] { 10, 20, 30 }, () => new List<int>());

            // Assert
            int[] newModel = (bindingContext.Model as ICollection<int>).ToArray();
            Assert.Equal(new[] { 10, 20, 30 }, newModel);
        }

        [Fact]
        public void CreateOrReplaceDictionary_DisallowsDuplicateKeys()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(Dictionary<string, int>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceDictionary(
                bindingContext,
                new[]
                {
                    new KeyValuePair<string, int>("forty-two", 40),
                    new KeyValuePair<string, int>("forty-two", 2),
                    new KeyValuePair<string, int>("forty-two", 42)
                },
                () => new Dictionary<string, int>());

            // Assert
            IDictionary<string, int> newModel = bindingContext.Model as IDictionary<string, int>;
            Assert.Equal(new[] { "forty-two" }, newModel.Keys.ToArray());
            Assert.Equal(42, newModel["forty-two"]);
        }

        [Fact]
        public void CreateOrReplaceDictionary_DisallowsNullKeys()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(Dictionary<string, int>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceDictionary(
                bindingContext,
                new[]
                {
                    new KeyValuePair<string, int>("forty-two", 42),
                    new KeyValuePair<string, int>(null, 84)
                },
                () => new Dictionary<string, int>());

            // Assert
            IDictionary<string, int> newModel = bindingContext.Model as IDictionary<string, int>;
            Assert.Equal(new[] { "forty-two" }, newModel.Keys.ToArray());
            Assert.Equal(42, newModel["forty-two"]);
        }

        [Fact]
        public void CreateOrReplaceDictionary_OriginalModelImmutable_CreatesNewInstance()
        {
            // Arrange
            ReadOnlyDictionary<string, string> originalModel = new ReadOnlyDictionary<string, string>();

            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => originalModel, typeof(IDictionary<string, string>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceDictionary(
                bindingContext,
                new Dictionary<string, string>
                {
                    { "Hello", "World" }
                },
                () => new Dictionary<string, string>());

            // Assert
            IDictionary<string, string> newModel = bindingContext.Model as IDictionary<string, string>;
            Assert.NotSame(originalModel, newModel);
            Assert.Equal(new[] { "Hello" }, newModel.Keys.ToArray());
            Assert.Equal("World", newModel["Hello"]);
        }

        [Fact]
        public void CreateOrReplaceDictionary_OriginalModelMutable_UpdatesOriginalInstance()
        {
            // Arrange
            Dictionary<string, string> originalInstance = new Dictionary<string, string>
            {
                { "dog", "Canidae" },
                { "cat", "Felidae" }
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => originalInstance, typeof(IDictionary<string, string>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceDictionary(
                bindingContext,
                new Dictionary<string, string>
                {
                    { "horse", "Equidae" },
                    { "bear", "Ursidae" }
                },
                () => new Dictionary<string, string>());

            // Assert
            Assert.Same(originalInstance, bindingContext.Model);
            Assert.Equal(new[] { "horse", "bear" }, originalInstance.Keys.ToArray());
            Assert.Equal("Equidae", originalInstance["horse"]);
            Assert.Equal("Ursidae", originalInstance["bear"]);
        }

        [Fact]
        public void CreateOrReplaceDictionary_OriginalModelNotDictionary_CreatesNewInstance()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<string, string>))
            };

            // Act
            CollectionModelBinderUtil.CreateOrReplaceDictionary(
                bindingContext,
                new Dictionary<string, string>
                {
                    { "horse", "Equidae" },
                    { "bear", "Ursidae" }
                },
                () => new Dictionary<string, string>());

            // Assert
            IDictionary<string, string> newModel = bindingContext.Model as IDictionary<string, string>;
            Assert.Equal(new[] { "horse", "bear" }, newModel.Keys.ToArray());
            Assert.Equal("Equidae", newModel["horse"]);
            Assert.Equal("Ursidae", newModel["bear"]);
        }

        [Fact]
        public void GetIndexNamesFromValueProviderResult_ValueProviderResultIsNull_ReturnsNull()
        {
            // Act
            IEnumerable<string> indexNames = CollectionModelBinderUtil.GetIndexNamesFromValueProviderResult(null);

            // Assert
            Assert.Null(indexNames);
        }

        [Fact]
        public void GetIndexNamesFromValueProviderResult_ValueProviderResultReturnsEmptyArray_ReturnsNull()
        {
            // Arrange
            ValueProviderResult vpResult = new ValueProviderResult(new string[0], "", null);

            // Act
            IEnumerable<string> indexNames = CollectionModelBinderUtil.GetIndexNamesFromValueProviderResult(vpResult);

            // Assert
            Assert.Null(indexNames);
        }

        [Fact]
        public void GetIndexNamesFromValueProviderResult_ValueProviderResultReturnsNonEmptyArray_ReturnsArray()
        {
            // Arrange
            ValueProviderResult vpResult = new ValueProviderResult(new[] { "foo", "bar", "baz" }, "foo,bar,baz", null);

            // Act
            IEnumerable<string> indexNames = CollectionModelBinderUtil.GetIndexNamesFromValueProviderResult(vpResult);

            // Assert
            Assert.NotNull(indexNames);
            Assert.Equal(new[] { "foo", "bar", "baz" }, indexNames.ToArray());
        }

        [Fact]
        public void GetIndexNamesFromValueProviderResult_ValueProviderResultReturnsNull_ReturnsNull()
        {
            // Arrange
            ValueProviderResult vpResult = new ValueProviderResult(null, null, null);

            // Act
            IEnumerable<string> indexNames = CollectionModelBinderUtil.GetIndexNamesFromValueProviderResult(vpResult);

            // Assert
            Assert.Null(indexNames);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ModelTypeNotGeneric_Fail()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int));

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(null, null, modelMetadata);

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ModelTypeOpenGeneric_Fail()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IList<>));

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(null, null, modelMetadata);

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ModelTypeWrongNumberOfGenericArguments_Fail()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(KeyValuePair<int, string>));

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(ICollection<>), null, modelMetadata);

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ReadOnlyReference_ModelInstanceImmutable_Valid()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new int[0], typeof(IList<int>));
            modelMetadata.IsReadOnly = true;

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(IList<>), typeof(List<>), modelMetadata);

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ReadOnlyReference_ModelInstanceMutable_Valid()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new List<int>(), typeof(IList<int>));
            modelMetadata.IsReadOnly = true;

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(IList<>), typeof(List<>), modelMetadata);

            // Assert
            Assert.Equal(new[] { typeof(int) }, typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ReadOnlyReference_ModelInstanceOfWrongType_Fail()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new HashSet<int>(), typeof(ICollection<int>));
            modelMetadata.IsReadOnly = true;

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(IList<>), typeof(List<>), modelMetadata);

            // Assert
            // HashSet<> is not an IList<>, so we can't update
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ReadOnlyReference_ModelIsNull_Fail()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IList<int>));
            modelMetadata.IsReadOnly = true;

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(ICollection<>), typeof(List<>), modelMetadata);

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ReadWriteReference_NewInstanceAssignableToModelType_Success()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IList<int>));
            modelMetadata.IsReadOnly = false;

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(ICollection<>), typeof(List<>), modelMetadata);

            // Assert
            Assert.Equal(new[] { typeof(int) }, typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsForUpdatableGenericCollection_ReadWriteReference_NewInstanceNotAssignableToModelType_MutableInstance_Success()
        {
            // Arrange
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new Collection<int>(), typeof(Collection<int>));
            modelMetadata.IsReadOnly = false;

            // Act
            Type[] typeArguments = CollectionModelBinderUtil.GetTypeArgumentsForUpdatableGenericCollection(typeof(ICollection<>), typeof(List<>), modelMetadata);

            // Assert
            Assert.Equal(new[] { typeof(int) }, typeArguments);
        }

        [Fact]
        public void GetZeroBasedIndexes()
        {
            // Act
            string[] indexes = CollectionModelBinderUtil.GetZeroBasedIndexes().Take(5).ToArray();

            // Assert
            Assert.Equal(new[] { "0", "1", "2", "3", "4" }, indexes);
        }

        private class ReadOnlyDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>
        {
            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
            {
                get { return true; }
            }
        }
    }
}

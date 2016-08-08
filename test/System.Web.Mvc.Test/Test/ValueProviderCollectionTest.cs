// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ValueProviderCollectionTest
    {
        [Fact]
        public void ListWrappingConstructor()
        {
            // Arrange
            List<IValueProvider> list = new List<IValueProvider>()
            {
                new Mock<IValueProvider>().Object, new Mock<IValueProvider>().Object
            };

            // Act
            ValueProviderCollection collection = new ValueProviderCollection(list);

            // Assert
            Assert.Equal(list, collection.ToList());
        }

        [Fact]
        public void ListWrappingConstructorThrowsIfListIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ValueProviderCollection(null); },
                "list");
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Act
            ValueProviderCollection collection = new ValueProviderCollection();

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void AddNullValueProviderThrows()
        {
            // Arrange
            ValueProviderCollection collection = new ValueProviderCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.Add(null); },
                "item");
        }

        [Fact]
        public void SetItem()
        {
            // Arrange
            ValueProviderCollection collection = new ValueProviderCollection();
            collection.Add(new Mock<IValueProvider>().Object);

            IValueProvider newProvider = new Mock<IValueProvider>().Object;

            // Act
            collection[0] = newProvider;

            // Assert
            IValueProvider provider = Assert.Single(collection);
            Assert.Equal(newProvider, provider);
        }

        [Fact]
        public void SetNullValueProviderThrows()
        {
            // Arrange
            ValueProviderCollection collection = new ValueProviderCollection();
            collection.Add(new Mock<IValueProvider>().Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection[0] = null; },
                "item");
        }

        [Fact]
        public void ContainsPrefix()
        {
            // Arrange
            string prefix = "somePrefix";

            Mock<IValueProvider> mockProvider1 = new Mock<IValueProvider>();
            mockProvider1.Setup(p => p.ContainsPrefix(prefix)).Returns(false);
            Mock<IValueProvider> mockProvider2 = new Mock<IValueProvider>();
            mockProvider2.Setup(p => p.ContainsPrefix(prefix)).Returns(true);
            Mock<IValueProvider> mockProvider3 = new Mock<IValueProvider>();
            mockProvider3.Setup(p => p.ContainsPrefix(prefix)).Returns(false);

            ValueProviderCollection collection = new ValueProviderCollection()
            {
                mockProvider1.Object, mockProvider2.Object, mockProvider3.Object
            };

            // Act
            bool retVal = collection.ContainsPrefix(prefix);

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void GetValue()
        {
            // Arrange
            string key = "someKey";

            Mock<IValueProvider> mockProvider1 = new Mock<IValueProvider>();
            mockProvider1.Setup(p => p.GetValue(key)).Returns((ValueProviderResult)null);
            Mock<IValueProvider> mockProvider2 = new Mock<IValueProvider>();
            mockProvider2.Setup(p => p.GetValue(key)).Returns(new ValueProviderResult("2", "2", null));
            Mock<IValueProvider> mockProvider3 = new Mock<IValueProvider>();
            mockProvider3.Setup(p => p.GetValue(key)).Returns(new ValueProviderResult("3", "3", null));

            ValueProviderCollection collection = new ValueProviderCollection()
            {
                mockProvider1.Object, mockProvider2.Object, mockProvider3.Object
            };

            // Act
            ValueProviderResult result = collection.GetValue(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ConvertTo(typeof(int)));
        }

        [Fact]
        public void GetValueFromProvider_NormalProvider_DoNotSkipValidation()
        {
            // Arrange
            ValueProviderResult expectedResult = new ValueProviderResult("Success", "Success", null);

            Mock<IValueProvider> mockProvider = new Mock<IValueProvider>();
            mockProvider.Setup(o => o.GetValue("key")).Returns(expectedResult);

            // Act
            ValueProviderResult actualResult = ValueProviderCollection.GetValueFromProvider(mockProvider.Object, "key", skipValidation: false);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetValueFromProvider_NormalProvider_SkipValidation()
        {
            // Arrange
            ValueProviderResult expectedResult = new ValueProviderResult("Success", "Success", null);

            Mock<IValueProvider> mockProvider = new Mock<IValueProvider>();
            mockProvider.Setup(o => o.GetValue("key")).Returns(expectedResult);

            // Act
            ValueProviderResult actualResult = ValueProviderCollection.GetValueFromProvider(mockProvider.Object, "key", skipValidation: true);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetValueFromProvider_UnvalidatedProvider_DoNotSkipValidation()
        {
            // Arrange
            ValueProviderResult expectedResult = new ValueProviderResult("Success", "Success", null);

            Mock<IUnvalidatedValueProvider> mockProvider = new Mock<IUnvalidatedValueProvider>();
            mockProvider.Setup(o => o.GetValue("key", false)).Returns(expectedResult);

            // Act
            ValueProviderResult actualResult = ValueProviderCollection.GetValueFromProvider(mockProvider.Object, "key", skipValidation: false);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetValueFromProvider_UnvalidatedProvider_SkipValidation()
        {
            // Arrange
            ValueProviderResult expectedResult = new ValueProviderResult("Success", "Success", null);

            Mock<IUnvalidatedValueProvider> mockProvider = new Mock<IUnvalidatedValueProvider>();
            mockProvider.Setup(o => o.GetValue("key", true)).Returns(expectedResult);

            // Act
            ValueProviderResult actualResult = ValueProviderCollection.GetValueFromProvider(mockProvider.Object, "key", skipValidation: true);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetValueFromProvider_EnumeratedProvider_GoodPrefix()
        {
            // Arrange
            IDictionary<string, string> expectedResult = new Dictionary<string, string>()
            {
                { "random", "random.hello" }
            };

            Mock<IEnumerableValueProvider> mockProvider = new Mock<IEnumerableValueProvider>();
            mockProvider.Setup(o => o.GetKeysFromPrefix("prefix")).Returns(expectedResult);

            ValueProviderCollection providerCollection = new ValueProviderCollection(new List<IValueProvider>() { mockProvider.Object });

            // Act
            IDictionary<string, string> actualResult = providerCollection.GetKeysFromPrefix("prefix");

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetValueFromProvider_EnumeratedProvider_NotFound_DoNotReturnNull()
        {
            // Arrange
            IDictionary<string, string> expectedResult = new Dictionary<string, string>()
            {
                { "random", "random.hello" }
            };

            Mock<IEnumerableValueProvider> mockProvider = new Mock<IEnumerableValueProvider>();
            mockProvider.Setup(o => o.GetKeysFromPrefix("notfound")).Returns(expectedResult);

            ValueProviderCollection providerCollection = new ValueProviderCollection(new List<IValueProvider>() { mockProvider.Object });

            // Act
            IDictionary<string, string> actualResult = providerCollection.GetKeysFromPrefix("prefix");

            // Assert
            Assert.NotNull(actualResult);
            Assert.Empty(actualResult);
        }
    }
}

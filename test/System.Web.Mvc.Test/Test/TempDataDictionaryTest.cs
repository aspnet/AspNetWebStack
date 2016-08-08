// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class TempDataDictionaryTest
    {
        [Fact]
        public void CompareIsOrdinalIgnoreCase()
        {
            // Arrange
            TempDataDictionary tempData = new TempDataDictionary();
            object item = new object();

            // Act
            tempData["Foo"] = item;
            object value = tempData["FOO"];

            // Assert
            Assert.Same(item, value);
        }

        [Fact]
        public void EnumeratingDictionaryMarksValuesForDeletion()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            IEnumerator<KeyValuePair<string, object>> enumerator = tempData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object value = enumerator.Current;
            }
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.False(tempData.ContainsKey("Foo"));
            Assert.False(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void EnumeratingTempDataAsIEnmerableMarksValuesForDeletion()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            IEnumerator enumerator = ((IEnumerable)tempData).GetEnumerator();
            while (enumerator.MoveNext())
            {
                object value = enumerator.Current;
            }
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.False(tempData.ContainsKey("Foo"));
            Assert.False(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void KeepRetainsAllKeysWhenSavingDictionary()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(c => c.HttpContext.Request).Returns(new Mock<HttpRequestBase>().Object);
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            tempData.Keep();
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.True(tempData.ContainsKey("Foo"));
            Assert.True(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void KeepRetainsSpecificKeysWhenSavingDictionary()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(c => c.HttpContext.Request).Returns(new Mock<HttpRequestBase>().Object);
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            tempData.Keep("Foo");
            object value = tempData["Bar"];
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.True(tempData.ContainsKey("Foo"));
            Assert.False(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void LoadAndSaveAreCaseInsensitive()
        {
            // Arrange
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Foo"] = "Foo";
            data["Bar"] = "Bar";
            TestTempDataProvider provider = new TestTempDataProvider(data);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            TempDataDictionary tempData = new TempDataDictionary();

            // Act
            tempData.Load(controllerContext.Object, provider);
            object value = tempData["FOO"];
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.False(tempData.ContainsKey("foo"));
            Assert.True(tempData.ContainsKey("bar"));
        }

        [Fact]
        public void PeekDoesNotMarkKeyAsRead()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            tempData["Bar"] = "barValue";

            // Act
            object value = tempData.Peek("bar");
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.Equal("barValue", value);
            Assert.True(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void RemovalOfKeysAreCaseInsensitive()
        {
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            object fooValue;
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            tempData.TryGetValue("foo", out fooValue);
            object barValue = tempData["bar"];
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.False(tempData.ContainsKey("Foo"));
            Assert.False(tempData.ContainsKey("Boo"));
        }

        [Fact]
        public void SaveRetainsAllKeys()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.True(tempData.ContainsKey("Foo"));
            Assert.True(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void SaveRemovesKeysThatWereRead()
        {
            // Arrange
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            tempData["Foo"] = "Foo";
            tempData["Bar"] = "Bar";

            // Act
            object value = tempData["Foo"];
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.False(tempData.ContainsKey("Foo"));
            Assert.True(tempData.ContainsKey("Bar"));
        }

        [Fact]
        public void TempDataIsADictionary()
        {
            // Arrange
            TempDataDictionary tempData = new TempDataDictionary();

            // Act
            tempData["Key1"] = "Value1";
            tempData.Add("Key2", "Value2");
            ((ICollection<KeyValuePair<string, object>>)tempData).Add(new KeyValuePair<string, object>("Key3", "Value3"));

            // Assert (IDictionary)
            Assert.Equal(3, tempData.Count);
            Assert.True(tempData.Remove("Key1"));
            Assert.False(tempData.Remove("Key4"));
            Assert.True(tempData.ContainsValue("Value2"));
            Assert.False(tempData.ContainsValue("Value1"));
            Assert.Null(tempData["Key6"]);

            IEnumerator tempDataEnumerator = tempData.GetEnumerator();
            tempDataEnumerator.Reset();
            while (tempDataEnumerator.MoveNext())
            {
                KeyValuePair<string, object> pair = (KeyValuePair<string, object>)tempDataEnumerator.Current;
                Assert.True(((ICollection<KeyValuePair<string, object>>)tempData).Contains(pair));
            }

            // Assert (ICollection)
            foreach (string key in tempData.Keys)
            {
                Assert.True(((ICollection<KeyValuePair<string, object>>)tempData).Contains(new KeyValuePair<string, object>(key, tempData[key])));
            }

            foreach (string value in tempData.Values)
            {
                Assert.True(tempData.ContainsValue(value));
            }

            foreach (string key in ((IDictionary<string, object>)tempData).Keys)
            {
                Assert.True(tempData.ContainsKey(key));
            }

            foreach (string value in ((IDictionary<string, object>)tempData).Values)
            {
                Assert.True(tempData.ContainsValue(value));
            }

            KeyValuePair<string, object>[] keyValuePairArray = new KeyValuePair<string, object>[tempData.Count];
            ((ICollection<KeyValuePair<string, object>>)tempData).CopyTo(keyValuePairArray, 0);

            Assert.False(((ICollection<KeyValuePair<string, object>>)tempData).IsReadOnly);

            Assert.False(((ICollection<KeyValuePair<string, object>>)tempData).Remove(new KeyValuePair<string, object>("Key5", "Value5")));

            IEnumerator<KeyValuePair<string, object>> keyValuePairEnumerator = ((ICollection<KeyValuePair<string, object>>)tempData).GetEnumerator();
            keyValuePairEnumerator.Reset();
            while (keyValuePairEnumerator.MoveNext())
            {
                KeyValuePair<string, object> pair = keyValuePairEnumerator.Current;
                Assert.True(((ICollection<KeyValuePair<string, object>>)tempData).Contains(pair));
            }

            // Act
            tempData.Clear();

            // Assert
            Assert.Empty(tempData);
        }

        [Fact]
        public void TempDataDictionaryCreatesEmptyDictionaryIfProviderReturnsNull()
        {
            // Arrange
            TempDataDictionary tempDataDictionary = new TempDataDictionary();
            NullTempDataProvider provider = new NullTempDataProvider();

            // Act
            tempDataDictionary.Load(null /* controllerContext */, provider);

            // Assert
            Assert.Empty(tempDataDictionary);
        }

        [Fact]
        public void TryGetValueMarksKeyForDeletion()
        {
            NullTempDataProvider provider = new NullTempDataProvider();
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            object value;
            tempData["Foo"] = "Foo";

            // Act
            tempData.TryGetValue("Foo", out value);
            tempData.Save(controllerContext.Object, provider);

            // Assert
            Assert.False(tempData.ContainsKey("Foo"));
        }

        internal class NullTempDataProvider : ITempDataProvider
        {
            public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
            {
            }

            public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
            {
                return null;
            }
        }

        internal class TestTempDataProvider : ITempDataProvider
        {
            private IDictionary<string, object> _data;

            public TestTempDataProvider(IDictionary<string, object> data)
            {
                _data = data;
            }

            public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
            {
            }

            public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
            {
                return _data;
            }
        }
    }
}

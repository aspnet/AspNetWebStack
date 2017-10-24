// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class PageDataDictionaryTest
    {
        [Fact]
        public void PageDataDictionaryConstructorTest()
        {
            var d = new PageDataDictionary<dynamic>();
            Assert.NotNull(d.Data);
        }

        [Fact]
        public void AddTest()
        {
            var d = new PageDataDictionary<dynamic>();
            var item = new KeyValuePair<object, object>("x", 2);
            d.Add(item);
            Assert.True(d.Data.Contains(item));
        }

        [Fact]
        public void AddTest1()
        {
            var d = new PageDataDictionary<dynamic>();
            object key = "x";
            object value = 1;
            d.Add(key, value);
            Assert.True(d.Data.ContainsKey(key));
            Assert.Equal(1, d.Data[key]);
            // Use uppercase string key
            Assert.Equal(1, d.Data["X"]);
        }

        [Fact]
        public void ClearTest()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 2);
            d.Clear();
            Assert.Equal(0, d.Data.Count);
        }

        [Fact]
        public void ContainsTest()
        {
            var d = new PageDataDictionary<dynamic>();
            var item = new KeyValuePair<object, object>("x", 1);
            d.Add(item);
            Assert.Contains(item, d);
            var item2 = new KeyValuePair<object, object>("y", 2);
            Assert.DoesNotContain(item2, d);
        }

        [Fact]
        public void ContainsKeyTest()
        {
            var d = new PageDataDictionary<dynamic>();
            object key = "x";
            Assert.False(d.ContainsKey(key));
            d.Add(key, 1);
            Assert.True(d.ContainsKey(key));
            Assert.True(d.ContainsKey("X"));
        }

        [Fact]
        public void CopyToTest()
        {
            var d = new PageDataDictionary<dynamic>();
            KeyValuePair<object, object>[] array = new KeyValuePair<object, object>[1];
            d.Add("x", 1);
            d.CopyTo(array, 0);
            Assert.Equal(new KeyValuePair<object, object>("x", 1), array[0]);
        }

        [Fact]
        public void GetEnumeratorTest()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            var e = d.GetEnumerator();
            e.MoveNext();
            Assert.Equal<object>(new KeyValuePair<object, object>("x", 1), e.Current);
        }

        [Fact]
        public void RemoveTest()
        {
            var d = new PageDataDictionary<dynamic>();
            var key = "x";
            d.Add(key, 1);
            d.Remove(key);
            Assert.False(d.Data.ContainsKey(key));
        }

        [Fact]
        public void RemoveTest1()
        {
            var d = new PageDataDictionary<dynamic>();
            var item = new KeyValuePair<object, object>("x", 2);
            d.Add(item);
            Assert.Contains(item, d);
            d.Remove(item);
            Assert.DoesNotContain(item, d);
        }

        [Fact]
        public void GetEnumeratorTest1()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            var e = ((IEnumerable)d).GetEnumerator();
            e.MoveNext();
            Assert.Equal(new KeyValuePair<object, object>("x", 1), e.Current);
        }

        [Fact]
        public void TryGetValueTest()
        {
            var d = new PageDataDictionary<dynamic>();
            object key = "x";
            d.Add(key, 1);
            object value = null;
            Assert.True(d.TryGetValue(key, out value));
            Assert.Equal(1, value);
        }

        [Fact]
        public void CountTest()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            Assert.Single(d);
            d.Add("y", 2);
            Assert.Equal(2, d.Count);
        }

        [Fact]
        public void IsReadOnlyTest()
        {
            var d = new PageDataDictionary<dynamic>();
            Assert.Equal(d.Data.IsReadOnly, d.IsReadOnly);
        }

        [Fact]
        public void ItemTest()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.Equal(1, d["x"]);
            Assert.Equal(2, d["y"]);
        }

        [Fact]
        public void KeysTest()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.True(d.Keys.Contains("x"));
            Assert.True(d.Keys.Contains("y"));
            Assert.Equal(2, d.Keys.Count);
        }

        [Fact]
        public void ValuesTest()
        {
            var d = new PageDataDictionary<dynamic>();
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.True(d.Values.Contains(1));
            Assert.True(d.Values.Contains(2));
            Assert.Equal(2, d.Values.Count);
        }

        [Fact]
        public void KeysReturnsNumericKeysIfPresent()
        {
            // Arrange
            var d = new PageDataDictionary<string>();

            // Act
            d[100] = "foo";
            d[200] = "bar";

            // Assert
            Assert.Equal(2, d.Keys.Count);
            Assert.Equal(100, d.Keys.ElementAt(0));
            Assert.Equal(200, d.Keys.ElementAt(1));
        }

        [Fact]
        public void KeysReturnsUniqueSetOfValues()
        {
            // Act
            var innerDict = new Dictionary<string, object>()
            {
                { "my-key", "value" },
                { "test", "test-val" }
            };
            var dict = PageDataDictionary<dynamic>.CreatePageDataFromParameters(new PageDataDictionary<dynamic>(), innerDict);

            // Act
            dict.Add("my-key", "added-value");
            dict["foo"] = "bar";

            // Assert
            Assert.Equal(4, dict.Keys.Count);
            Assert.Equal("my-key", dict.Keys.ElementAt(0));
            Assert.Equal("test", dict.Keys.ElementAt(1));
            Assert.Equal(0, dict.Keys.ElementAt(2));
            Assert.Equal("foo", dict.Keys.ElementAt(3));
            Assert.Equal(dict[0], innerDict);
        }

        [Fact]
        public void AddValueOverwritesIndexDictionaryIfKeyExists()
        {
            // Act
            var dict = PageDataDictionary<dynamic>.CreatePageDataFromParameters(new PageDataDictionary<dynamic>(), new[] { "index-0-orig", "index-1" });

            // Act
            dict[0] = "index-0-new";

            // Assert
            Assert.Equal(2, dict.Keys.Count);
            Assert.Equal("index-0-new", dict[0]);
            Assert.Equal("index-1", dict[1]);
        }
    }
}

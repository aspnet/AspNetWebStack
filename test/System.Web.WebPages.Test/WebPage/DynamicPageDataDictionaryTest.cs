// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Web.WebPages.Resources;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class DynamicPageDataDictionaryTest
    {
        [Fact]
        public void DynamicTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            d["x"] = "y";
            Assert.Equal("y", d.x);
            d.a = "b";
            Assert.Equal("b", d["a"]);
            d[0] = "zero";
            Assert.Equal("zero", d[0]);
            d.Foo = "bar";
            Assert.Equal("bar", d.Foo);
            var a = d.Baz = 42;
            Assert.Equal(42, a);
            var b = d[new object()] = 666;
            Assert.Equal(666, b);
        }

        [Fact]
        public void CastToDictionaryTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            var typeCheckCast = d as IDictionary<object, dynamic>;
            var directCast = (IDictionary<object, dynamic>)d;

            Assert.NotNull(typeCheckCast);
            Assert.NotNull(directCast);
        }

        [Fact]
        public void AddTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            var item = new KeyValuePair<object, object>("x", 2);
            d.Add(item);
            Assert.True(d.Contains(item));
            Assert.Equal(2, d.x);
            Assert.Equal(2, d["x"]);
        }

        [Fact]
        public void AddTest1()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            object key = "x";
            object value = 1;
            d.Add(key, value);
            Assert.True(d.ContainsKey(key));
            Assert.Equal(1, d[key]);
            Assert.Equal(1, d.x);
        }

        [Fact]
        public void ClearTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            d.x = 2;
            d.Clear();
            Assert.Equal(0, d.Count);
        }

        [Fact]
        public void ContainsTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            var item = new KeyValuePair<object, object>("x", 1);
            d.Add(item);
            Assert.True(d.Contains(item));
            var item2 = new KeyValuePair<object, object>("y", 2);
            Assert.False(d.Contains(item2));
        }

        [Fact]
        public void ContainsKeyTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            object key = "x";
            Assert.False(d.ContainsKey(key));
            d.Add(key, 1);
            Assert.True(d.ContainsKey(key));
            Assert.True(d.ContainsKey("x"));
        }

        [Fact]
        public void CopyToTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            KeyValuePair<object, object>[] array = new KeyValuePair<object, object>[1];
            d.Add("x", 1);
            d.CopyTo(array, 0);
            Assert.Equal(new KeyValuePair<object, object>("x", 1), array[0]);
        }

        [Fact]
        public void GetEnumeratorTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            d.Add("x", 1);
            var e = d.GetEnumerator();
            e.MoveNext();
            Assert.Equal(new KeyValuePair<object, object>("x", 1), e.Current);
        }

        [Fact]
        public void RemoveTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            var key = "x";
            d.Add(key, 1);
            d.Remove(key);
            Assert.False(d.ContainsKey(key));
        }

        [Fact]
        public void RemoveTest1()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            var item = new KeyValuePair<object, object>("x", 2);
            d.Add(item);
            Assert.True(d.Contains(item));
            d.Remove(item);
            Assert.False(d.Contains(item));
        }

        [Fact]
        public void GetEnumeratorTest1()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            d.Add("x", 1);
            var e = ((IEnumerable)d).GetEnumerator();
            e.MoveNext();
            Assert.Equal(new KeyValuePair<object, object>("x", 1), e.Current);
        }

        [Fact]
        public void TryGetValueTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            object key = "x";
            d.Add(key, 1);
            object value = null;
            Assert.True(d.TryGetValue(key, out value));
            Assert.Equal(1, value);
        }

        [Fact]
        public void CountTest()
        {
            var d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            dynamic dyn = d;
            Assert.IsType<int>(dyn.Count);
            d.Add("x", 1);
            Assert.Single(d);
            d.Add("y", 2);
            Assert.Equal(2, d.Count);
            dyn.Count = "foo";
            Assert.Equal("foo", d["Count"]);
            Assert.Equal(3, d.Count);
        }

        [Fact]
        public void IsReadOnlyTest()
        {
            PageDataDictionary<dynamic> dict = new PageDataDictionary<dynamic>();
            var d = new DynamicPageDataDictionary<dynamic>(dict);
            dynamic dyn = d;
            Assert.IsType<bool>(dyn.IsReadOnly);
            Assert.Equal(dict.IsReadOnly, d.IsReadOnly);
            dyn.IsReadOnly = "foo";
            Assert.Equal("foo", d["IsReadOnly"]);
            Assert.Equal(dict.IsReadOnly, d.IsReadOnly);
            Assert.Equal(dict.IsReadOnly, dyn.IsReadOnly);
        }

        [Fact]
        public void ItemTest()
        {
            dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.Equal(1, d["x"]);
            Assert.Equal(2, d["y"]);
        }

        [Fact]
        public void KeysTest()
        {
            var d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            dynamic dyn = d;
            Assert.IsAssignableFrom<ICollection<object>>(dyn.Keys);
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.True(d.Keys.Contains("x"));
            Assert.True(d.Keys.Contains("y"));
            Assert.Equal(2, d.Keys.Count);
            dyn.Keys = "foo";
            Assert.Equal("foo", dyn["Keys"]);
            Assert.Equal(3, d.Count);
        }

        [Fact]
        public void ValuesTest()
        {
            var d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
            dynamic dyn = d;
            Assert.IsAssignableFrom<ICollection<dynamic>>(dyn.Values);
            d.Add("x", 1);
            d.Add("y", 2);
            Assert.True(d.Values.Contains(1));
            Assert.True(d.Values.Contains(2));
            Assert.Equal(2, d.Values.Count);
            dyn.Values = "foo";
            Assert.Equal("foo", dyn["Values"]);
            Assert.Equal(3, d.Count);
        }

        [Fact]
        public void InvalidNumberOfIndexes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
                d[1, 2] = 3;
            }, WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);

            Assert.Throws<ArgumentException>(() =>
            {
                dynamic d = new DynamicPageDataDictionary<dynamic>(new PageDataDictionary<dynamic>());
                var x = d[1, 2];
            }, WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
        }
    }
}

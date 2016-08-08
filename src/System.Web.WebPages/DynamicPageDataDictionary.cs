// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages
{
    /// <summary>
    /// This is a wrapper around PageDataDictionary[[dynamic]] which allows dynamic
    /// access (e.g. dict.Foo). Like PageDataDictionary, it returns null if the key is not found, 
    /// instead of throwing an exception.
    /// This class is intended to be used as DynamicPageDataDictionary[[dynamic]]
    /// </summary>
    // This is a generic type because C# does not allow implementing an interface 
    // involving dynamic types (implementing IDictionary<object, dynamic> causes
    // a compile error 
    // http://blogs.msdn.com/cburrows/archive/2009/02/04/c-dynamic-part-vii.aspx).
    internal class DynamicPageDataDictionary<TValue> : DynamicObject, IDictionary<object, TValue>
    {
        private PageDataDictionary<TValue> _data;

        public DynamicPageDataDictionary(PageDataDictionary<TValue> dictionary)
        {
            _data = dictionary;
        }

        public ICollection<object> Keys
        {
            get { return _data.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return _data.Values; }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return _data.IsReadOnly; }
        }

        public TValue this[object key]
        {
            get { return _data[key]; }
            set { _data[key] = value; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _data[binder.Name];
            // We return true here because PageDataDictionary returns null if the key is not
            // in the dictionary, so we simply pass on the returned value.
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // This cast should always succeed assuming TValue is dynamic.
            TValue v = (TValue)value;
            _data[binder.Name] = v;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes == null || indexes.Length != 1)
            {
                throw new ArgumentException(WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
            }

            result = _data[indexes[0]];
            // We return true here because PageDataDictionary returns null if the key is not
            // in the dictionary, so we simply pass on the returned value.
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes == null || indexes.Length != 1)
            {
                throw new ArgumentException(WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
            }

            // This cast should always succeed assuming TValue is dynamic.
            _data[indexes[0]] = (TValue)value;
            return true;
        }

        public void Add(object key, TValue value)
        {
            _data.Add(key, value);
        }

        public bool ContainsKey(object key)
        {
            return _data.ContainsKey(key);
        }

        public bool Remove(object key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue(object key, out TValue value)
        {
            return _data.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<object, TValue> item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<object, TValue> item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(KeyValuePair<object, TValue>[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<object, TValue> item)
        {
            return _data.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<object, TValue>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}

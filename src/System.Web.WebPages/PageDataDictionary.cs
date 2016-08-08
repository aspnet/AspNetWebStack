// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace System.Web.WebPages
{
    /// <summary>
    /// This is a wrapper around Dictionary so that using PageData[key] returns null
    /// if the key is not found, instead of throwing an exception.
    /// </summary>
    // This is a generic type because C# does not allow implementing an interface 
    // involving dynamic types (implementing IDictionary<object, dynamic> causes
    // a compile error 
    // http://blogs.msdn.com/cburrows/archive/2009/02/04/c-dynamic-part-vii.aspx).
    internal class PageDataDictionary<TValue> : IDictionary<object, TValue>
    {
        private IDictionary<object, TValue> _data = new Dictionary<object, TValue>(new PageDataComparer());

        private IDictionary<string, TValue> _stringDictionary = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

        private IList<TValue> _indexedValues = new List<TValue>();

        internal IDictionary<object, TValue> Data
        {
            get { return _data; }
        }

        internal IDictionary<string, TValue> StringDictionary
        {
            get { return _stringDictionary; }
        }

        internal IList<TValue> IndexedValues
        {
            get { return _indexedValues; }
        }

        public ICollection<object> Keys
        {
            get
            {
                List<object> keys = new List<object>();
                keys.AddRange(_stringDictionary.Keys);
                for (int i = 0; i < _indexedValues.Count; i++)
                {
                    keys.Add(i);
                }
                foreach (var key in _data.Keys)
                {
                    if (!ContainsIndex(key) && !ContainsStringKey(key))
                    {
                        keys.Add(key);
                    }
                }
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>();
                foreach (var key in Keys)
                {
                    values.Add(this[key]);
                }
                return values;
            }
        }

        internal ICollection<KeyValuePair<object, TValue>> Items
        {
            get
            {
                var items = new List<KeyValuePair<object, TValue>>();
                foreach (var key in Keys)
                {
                    var value = this[key];
                    var kvp = new KeyValuePair<object, TValue>(key, value);
                    items.Add(kvp);
                }
                return items;
            }
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public TValue this[object key]
        {
            get
            {
                TValue v = default(TValue);
                TryGetValue(key, out v);
                return v;
            }

            // Note that this affects and updates the string dictionary and indexed list
            // only for existing keys found in these collections. 
            // Otherwise, the key/value goes into the _data dictionary.
            set
            {
                if (ContainsStringKey(key))
                {
                    _stringDictionary[(string)key] = value;
                }
                else if (ContainsIndex(key))
                {
                    _indexedValues[(int)key] = value;
                }
                else
                {
                    _data[key] = value;
                }
            }
        }

        public void Add(object key, TValue value)
        {
            _data.Add(key, value);
        }

        internal bool ContainsIndex(object o)
        {
            if (o is int)
            {
                return ContainsIndex((int)o);
            }
            else
            {
                return false;
            }
        }

        internal bool ContainsIndex(int index)
        {
            return _indexedValues.Count > index && index >= 0;
        }

        internal bool ContainsStringKey(object o)
        {
            string s = o as string;
            if (s != null)
            {
                return ContainsStringKey(s);
            }
            else
            {
                return false;
            }
        }

        internal bool ContainsStringKey(string key)
        {
            return _stringDictionary.ContainsKey(key);
        }

        public bool ContainsKey(object key)
        {
            if (ContainsIndex(key))
            {
                return true;
            }
            else if (ContainsStringKey(key))
            {
                return true;
            }
            else if (_data.ContainsKey(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Note that if the key exists in more than one place, then only
        // the string dictionary and the indexed list will be updated.
        public bool Remove(object key)
        {
            if (ContainsStringKey(key))
            {
                return _stringDictionary.Remove((string)key);
            }
            else if (ContainsIndex(key))
            {
                return _indexedValues.Remove(_indexedValues[(int)key]);
            }
            else
            {
                return _data.Remove(key);
            }
        }

        public bool TryGetValue(object key, out TValue value)
        {
            if (ContainsStringKey(key))
            {
                return _stringDictionary.TryGetValue((string)key, out value);
            }
            else if (ContainsIndex(key))
            {
                value = _indexedValues[(int)key];
                return true;
            }
            else
            {
                return _data.TryGetValue(key, out value);
            }
        }

        public void Add(KeyValuePair<object, TValue> item)
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            _stringDictionary.Clear();
            _indexedValues.Clear();
            _data.Clear();
        }

        public bool Contains(KeyValuePair<object, TValue> item)
        {
            return ContainsKey(item.Key) && Values.Contains(item.Value);
        }

        public void CopyTo(KeyValuePair<object, TValue>[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<object, TValue> item)
        {
            if (Contains(item))
            {
                return Remove((object)item.Key);
            }
            return false;
        }

        public IEnumerator<KeyValuePair<object, TValue>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        // Creates a new PageData dictionary using only the original items from the pageData (excluding the string dictionary and indexed list),
        // and adding the parameters.
        internal static IDictionary<object, dynamic> CreatePageDataFromParameters(IDictionary<object, dynamic> previousPageData, params object[] data)
        {
            var oldPageData = previousPageData as PageDataDictionary<dynamic>;

            // Add the original items
            var pageData = new PageDataDictionary<dynamic>();
            foreach (var kvp in oldPageData.Data)
            {
                pageData.Data.Add(kvp);
            }

            if (data != null && data.Length > 0)
            {
                // Add items to the indexed list
                for (int i = 0; i < data.Length; i++)
                {
                    pageData.IndexedValues.Add(data[i]);
                }

                // Check for anonymous types, and add to the string dictionary
                object first = data[0];
                Type type = first.GetType();
                if (TypeHelper.IsAnonymousType(type))
                {
                    // Anonymous type
                    TypeHelper.AddAnonymousObjectToDictionary(pageData.StringDictionary, first);
                }

                // Check if the first element is of type IDictionary<string, object>
                if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
                {
                    // Dictionary
                    var stringDictionary = first as IDictionary<string, object>;
                    foreach (var kvp in stringDictionary)
                    {
                        pageData.StringDictionary.Add(kvp);
                    }
                }
            }

            return pageData;
        }

        // This comparer treats only strings as case-insensitive, but still handles objects
        // of other types as well.
        private sealed class PageDataComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                var s1 = x as string;
                var s2 = y as string;
                if (s1 != null && s2 != null)
                {
                    return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
                }
                return Equals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                var s = obj as string;
                if (s != null)
                {
                    return s.ToUpperInvariant().GetHashCode();
                }
                return obj.GetHashCode();
            }
        }
    }
}

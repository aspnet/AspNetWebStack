// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETFX_CORE // This file should only be included by the NetCore version of the formatting project, but adding a guard here just in case.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Internal
{
    // TODO: Remove this class after BCL makes their portable library version.
    internal sealed class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private object _lock = new object();

        public ICollection<TKey> Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary)_dictionary).IsReadOnly;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        // ConcurrentDictionary members
        public bool TryRemove(TKey key, out TValue removedValue)
        {
            lock (_lock)
            {
                if (_dictionary.TryGetValue(key, out removedValue))
                {
                    return _dictionary.Remove(key);
                }

                return false;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> addValueFactory)
        {
            lock (_lock)
            {
                TValue value;

                if (!_dictionary.TryGetValue(key, out value))
                {
                    value = addValueFactory.Invoke(key);
                    _dictionary.Add(key, value);
                }

                return value;
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_dictionary.ContainsKey(key))
                {
                    return false;
                }

                _dictionary.Add(key, value);
                return true;
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock (_lock)
            {
                TValue value;

                // update
                if (_dictionary.TryGetValue(key, out value))
                {
                    value = updateValueFactory.Invoke(key, value);
                    _dictionary[key] = value;
                    return value;
                }

                // add
                _dictionary.Add(key, addValue);
                return addValue;
            }
        }
    }
}
#endif
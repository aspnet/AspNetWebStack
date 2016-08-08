// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    public class TempDataDictionary : IDictionary<string, object>
    {
        internal const string TempDataSerializationKey = "__tempData";

        private Dictionary<string, object> _data;
        private HashSet<string> _initialKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _retainedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TempDataDictionary()
        {
            _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public ICollection<string> Keys
        {
            get { return _data.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _data.Values; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, object>>)_data).IsReadOnly; }
        }

        public object this[string key]
        {
            get
            {
                object value;
                if (TryGetValue(key, out value))
                {
                    _initialKeys.Remove(key);
                    return value;
                }
                return null;
            }
            set
            {
                _data[key] = value;
                _initialKeys.Add(key);
            }
        }

        public void Keep()
        {
            _retainedKeys.Clear();
            _retainedKeys.UnionWith(_data.Keys);
        }

        public void Keep(string key)
        {
            _retainedKeys.Add(key);
        }

        public void Load(ControllerContext controllerContext, ITempDataProvider tempDataProvider)
        {
            IDictionary<string, object> providerDictionary = tempDataProvider.LoadTempData(controllerContext);
            _data = (providerDictionary != null)
                ? new Dictionary<string, object>(providerDictionary, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _initialKeys = new HashSet<string>(_data.Keys, StringComparer.OrdinalIgnoreCase);
            _retainedKeys.Clear();
        }

        public object Peek(string key)
        {
            object value;
            _data.TryGetValue(key, out value);
            return value;
        }

        public void Save(ControllerContext controllerContext, ITempDataProvider tempDataProvider)
        {
            // Frequently called so ensure delegate is stateless
            _data.RemoveFromDictionary((KeyValuePair<string, object> entry, TempDataDictionary tempData) =>
                {
                    string key = entry.Key;
                    return !tempData._initialKeys.Contains(key) 
                        && !tempData._retainedKeys.Contains(key);
                }, this);

            tempDataProvider.SaveTempData(controllerContext, _data);
        }

        public void Add(string key, object value)
        {
            _data.Add(key, value);
            _initialKeys.Add(key);
        }

        public void Clear()
        {
            _data.Clear();
            _retainedKeys.Clear();
            _initialKeys.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public bool ContainsValue(object value)
        {
            return _data.ContainsValue(value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new TempDataDictionaryEnumerator(this);
        }

        public bool Remove(string key)
        {
            _retainedKeys.Remove(key);
            _initialKeys.Remove(key);
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            _initialKeys.Remove(key);
            return _data.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int index)
        {
            ((ICollection<KeyValuePair<string, object>>)_data).CopyTo(array, index);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> keyValuePair)
        {
            _initialKeys.Add(keyValuePair.Key);
            ((ICollection<KeyValuePair<string, object>>)_data).Add(keyValuePair);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> keyValuePair)
        {
            return ((ICollection<KeyValuePair<string, object>>)_data).Contains(keyValuePair);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> keyValuePair)
        {
            _initialKeys.Remove(keyValuePair.Key);
            return ((ICollection<KeyValuePair<string, object>>)_data).Remove(keyValuePair);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TempDataDictionaryEnumerator(this);
        }

        private sealed class TempDataDictionaryEnumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private IEnumerator<KeyValuePair<string, object>> _enumerator;
            private TempDataDictionary _tempData;

            public TempDataDictionaryEnumerator(TempDataDictionary tempData)
            {
                _tempData = tempData;
                _enumerator = _tempData._data.GetEnumerator();
            }

            public KeyValuePair<string, object> Current
            {
                get
                {
                    KeyValuePair<string, object> kvp = _enumerator.Current;
                    _tempData._initialKeys.Remove(kvp.Key);
                    return kvp;
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            void IDisposable.Dispose()
            {
                _enumerator.Dispose();
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.WebPages.Scope
{
    public class ScopeStorageDictionary : IDictionary<object, object>
    {
        private static readonly StateStorageKeyValueComparer _keyValueComparer = new StateStorageKeyValueComparer();
        private readonly IDictionary<object, object> _baseScope;
        private readonly IDictionary<object, object> _backingStore;

        public ScopeStorageDictionary()
            : this(baseScope: null)
        {
        }

        public ScopeStorageDictionary(IDictionary<object, object> baseScope)
            : this(baseScope: baseScope, backingStore: new Dictionary<object, object>(ScopeStorageComparer.Instance))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeStorageDictionary"/> class.
        /// </summary>
        /// <param name="baseScope">The base scope.</param>
        /// <param name="backingStore">
        /// The dictionary to use as a storage. Since the dictionary would be used as-is, we expect the implementer to 
        /// use the same key-value comparison logic as we do here.
        /// </param>
        internal ScopeStorageDictionary(IDictionary<object, object> baseScope, IDictionary<object, object> backingStore)
        {
            _baseScope = baseScope;
            _backingStore = backingStore;
        }

        protected IDictionary<object, object> BackingStore
        {
            get { return _backingStore; }
        }

        protected IDictionary<object, object> BaseScope
        {
            get { return _baseScope; }
        }

        public virtual ICollection<object> Keys
        {
            get { return GetItems().Select(item => item.Key).ToList(); }
        }

        public virtual ICollection<object> Values
        {
            get { return GetItems().Select(item => item.Value).ToList(); }
        }

        public virtual int Count
        {
            get { return GetItems().Count(); }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public object this[object key]
        {
            get
            {
                object value;
                TryGetValue(key, out value);
                return value;
            }
            set { SetValue(key, value); }
        }

        public virtual void SetValue(object key, object value)
        {
            _backingStore[key] = value;
        }

        public virtual bool TryGetValue(object key, out object value)
        {
            return _backingStore.TryGetValue(key, out value) || (_baseScope != null && _baseScope.TryGetValue(key, out value));
        }

        public virtual bool Remove(object key)
        {
            return _backingStore.Remove(key);
        }

        public virtual IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return GetItems().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(object key, object value)
        {
            SetValue(key, value);
        }

        public virtual bool ContainsKey(object key)
        {
            return _backingStore.ContainsKey(key) || (_baseScope != null && _baseScope.ContainsKey(key));
        }

        public virtual void Add(KeyValuePair<object, object> item)
        {
            SetValue(item.Key, item.Value);
        }

        public virtual void Clear()
        {
            _backingStore.Clear();
        }

        public virtual bool Contains(KeyValuePair<object, object> item)
        {
            return _backingStore.Contains(item) || (_baseScope != null && _baseScope.Contains(item));
        }

        public virtual void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            GetItems().ToList().CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(KeyValuePair<object, object> item)
        {
            return _backingStore.Remove(item);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This call might be expensive depending on how long the chain of contexts is")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This method is implementation specific and is not meant to be exposed as a public API.")]
        protected virtual IEnumerable<KeyValuePair<object, object>> GetItems()
        {
            if (_baseScope == null)
            {
                return _backingStore;
            }
            return Enumerable.Concat(_backingStore, _baseScope).Distinct(_keyValueComparer);
        }

        private class StateStorageKeyValueComparer : IEqualityComparer<KeyValuePair<object, object>>
        {
            private IEqualityComparer<object> _stateStorageComparer = ScopeStorageComparer.Instance;

            public bool Equals(KeyValuePair<object, object> x, KeyValuePair<object, object> y)
            {
                return _stateStorageComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<object, object> obj)
            {
                return _stateStorageComparer.GetHashCode(obj.Key);
            }
        }
    }
}

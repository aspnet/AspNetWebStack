// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;

namespace System.Web.Mvc
{
    public class DictionaryValueProvider<TValue> : IValueProvider, IEnumerableValueProvider
    {
        private PrefixContainer _prefixContainer;
        private readonly Dictionary<string, ValueProviderResult> _values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public DictionaryValueProvider(IDictionary<string, TValue> dictionary, CultureInfo culture)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            foreach (KeyValuePair<string, TValue> entry in dictionary)
            {
                object rawValue = entry.Value;
                string attemptedValue = Convert.ToString(rawValue, culture);
                _values[entry.Key] = new ValueProviderResult(rawValue, attemptedValue, culture);
            }
        }

        private PrefixContainer PrefixContainer
        {
            get
            {
                if (_prefixContainer == null)
                {
                    // Race condition on initialization has no side effects
                    _prefixContainer = new PrefixContainer(_values.Keys);
                }
                return _prefixContainer;
            }
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ValueProviderResult valueProviderResult;
            _values.TryGetValue(key, out valueProviderResult);
            return valueProviderResult;
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return PrefixContainer.GetKeysFromPrefix(prefix);
        }
    }
}

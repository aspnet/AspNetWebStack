// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Web.Mvc
{
    public class ValueProviderCollection : Collection<IValueProvider>, IValueProvider, IUnvalidatedValueProvider, IEnumerableValueProvider
    {
        public ValueProviderCollection()
        {
        }

        public ValueProviderCollection(IList<IValueProvider> list)
            : base(list)
        {
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            // Performance sensitive, so avoid Linq and delegates
            // Saving Count is faster for looping over Collection<T>
            int itemCount = Count;
            for (int i = 0; i < itemCount; i++)
            {
                if (this[i].ContainsPrefix(prefix))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            return GetValue(key, skipValidation: false);
        }

        public virtual ValueProviderResult GetValue(string key, bool skipValidation)
        {
            // Performance sensitive.
            // Caching the count is faster for Collection<T>
            int providerCount = Count;
            for (int i = 0; i < providerCount; i++)
            {
                ValueProviderResult result = GetValueFromProvider(this[i], key, skipValidation);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return (from provider in this
                    let result = GetKeysFromPrefixFromProvider(provider, prefix)
                    where result != null && result.Any()
                    select result).FirstOrDefault() ?? new Dictionary<string, string>();
        }

        internal static ValueProviderResult GetValueFromProvider(IValueProvider provider, string key, bool skipValidation)
        {
            // Since IUnvalidatedValueProvider is a superset of IValueProvider, it's always OK to use the
            // IUnvalidatedValueProvider-supplied members if they're present. Otherwise just call the
            // normal IValueProvider members.

            IUnvalidatedValueProvider unvalidatedProvider = provider as IUnvalidatedValueProvider;
            return (unvalidatedProvider != null) ? unvalidatedProvider.GetValue(key, skipValidation) : provider.GetValue(key);
        }

        internal static IDictionary<string, string> GetKeysFromPrefixFromProvider(IValueProvider provider, string prefix)
        {
            IEnumerableValueProvider enumeratedProvider = provider as IEnumerableValueProvider;
            return (enumeratedProvider != null) ? enumeratedProvider.GetKeysFromPrefix(prefix) : null;
        }

        protected override void InsertItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
    }
}

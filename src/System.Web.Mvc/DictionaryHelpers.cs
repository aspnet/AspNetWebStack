// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    internal static class DictionaryHelpers
    {
        public static IEnumerable<KeyValuePair<string, TValue>> FindKeysWithPrefix<TValue>(IDictionary<string, TValue> dictionary, string prefix)
        {
            TValue exactMatchValue;
            if (dictionary.TryGetValue(prefix, out exactMatchValue))
            {
                yield return new KeyValuePair<string, TValue>(prefix, exactMatchValue);
            }

            foreach (var entry in dictionary)
            {
                string key = entry.Key;

                if (key.Length <= prefix.Length)
                {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                char charAfterPrefix = key[prefix.Length];
                switch (charAfterPrefix)
                {
                    case '[':
                    case '.':
                        yield return entry;
                        break;
                }
            }
        }

        public static bool DoesAnyKeyHavePrefix<TValue>(IDictionary<string, TValue> dictionary, string prefix)
        {
            return FindKeysWithPrefix(dictionary, prefix).Any();
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue @default)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            return @default;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace Microsoft.Web.UnitTestUtil
{
    // just a simple value provider used for unit testing

    public sealed class SimpleValueProvider : Dictionary<string, object>, IValueProvider
    {
        private readonly CultureInfo _culture;

        public SimpleValueProvider()
            : this(null)
        {
        }

        public SimpleValueProvider(CultureInfo culture)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            _culture = culture ?? CultureInfo.InvariantCulture;
        }

        // copied from ValueProviderUtil
        public bool ContainsPrefix(string prefix)
        {
            foreach (string key in Keys)
            {
                if (key != null)
                {
                    if (prefix.Length == 0)
                    {
                        return true; // shortcut - non-null key matches empty prefix
                    }

                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (key.Length == prefix.Length)
                        {
                            return true; // exact match
                        }
                        else
                        {
                            switch (key[prefix.Length])
                            {
                                case '.': // known separator characters
                                case '[':
                                    return true;
                            }
                        }
                    }
                }
            }

            return false; // nothing found
        }

        public ValueProviderResult GetValue(string key)
        {
            object rawValue;
            if (TryGetValue(key, out rawValue))
            {
                return new ValueProviderResult(rawValue, Convert.ToString(rawValue, _culture), _culture);
            }
            else
            {
                // value not found
                return null;
            }
        }
    }
}

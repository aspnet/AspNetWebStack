// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;

namespace System.Web.Http.ValueProviders.Providers
{
    // Represents a value provider that contains a single value.
    internal sealed class ElementalValueProvider : IValueProvider
    {
        public ElementalValueProvider(string name, object rawValue, CultureInfo culture)
        {
            Name = name;
            RawValue = rawValue;
            Culture = culture;
        }

        public CultureInfo Culture { get; private set; }

        public string Name { get; private set; }

        public object RawValue { get; private set; }

        public bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.IsPrefixMatch(prefix, Name);
        }

        public ValueProviderResult GetValue(string key)
        {
            return String.Equals(key, Name, StringComparison.OrdinalIgnoreCase)
                       ? new ValueProviderResult(RawValue, Convert.ToString(RawValue, Culture), Culture)
                       : null;
        }
    }
}

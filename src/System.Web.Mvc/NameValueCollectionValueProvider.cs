// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    [SuppressMessage("StyleCop.CSharp.NamingRules",
                            "SA1305:FieldNamesMustNotUseHungarianNotation",
                            Target = "jQueryToMvcRequestNormalizationRequired",
                            Justification = "jQuery is usually spelled like this. Hence suppressing this message.")]
    public class NameValueCollectionValueProvider : IValueProvider, IUnvalidatedValueProvider, IEnumerableValueProvider
    {
        private PrefixContainer _prefixContainer;
        private NameValueCollection _collection;
        private NameValueCollection _unvalidatedCollection;
        private CultureInfo _culture;
        private bool _jQueryToMvcRequestNormalizationRequired;

        private Dictionary<string, ValueProviderResultPlaceholder> _values = null;
                
        public NameValueCollectionValueProvider(NameValueCollection collection, CultureInfo culture)
            : this(collection, unvalidatedCollection: null, culture: culture)
        {
        }

        public NameValueCollectionValueProvider(
                        NameValueCollection collection, NameValueCollection unvalidatedCollection, CultureInfo culture)
            : this(collection, unvalidatedCollection, culture, jQueryToMvcRequestNormalizationRequired: false)
        {
        }

        /// <summary>
        /// Initializes Name Value collection provider.
        /// </summary>
        /// <param name="collection">Key value collection from request.</param>
        /// <param name="unvalidatedCollection">Unvalidated key value collection from the request.</param>
        /// <param name="culture">Culture with which the values are to be used.</param>
        /// <param name="jQueryToMvcRequestNormalizationRequired">jQuery POST when sending complex Javascript 
        /// objects to server does not encode in the way understandable by MVC. This flag should be set
        /// if the request should be normalized to MVC form - https://aspnetwebstack.codeplex.com/workitem/1564. </param>
        [SuppressMessage("Microsoft.Naming",
                            "CA1704:IdentifiersShouldBeSpelledCorrectly",
                            MessageId = "j",
                            Justification = "jQuery is not accepted as a valid variable name in this class")]
        public NameValueCollectionValueProvider(
                            NameValueCollection collection, 
                            NameValueCollection unvalidatedCollection, 
                            CultureInfo culture, 
                            bool jQueryToMvcRequestNormalizationRequired)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            _unvalidatedCollection = unvalidatedCollection ?? collection;
            _collection = collection;
            _culture = culture;
            _jQueryToMvcRequestNormalizationRequired = jQueryToMvcRequestNormalizationRequired;
        }

        private Dictionary<string, ValueProviderResultPlaceholder> Values
        {
            get
            {
                if (_values == null)
                {
                    _values = InitializeCollectionValues();
                }

                return _values;
            }
        }
        
        private PrefixContainer PrefixContainer
        {
            get
            {
                if (_prefixContainer == null)
                {
                    // Race condition on initialization has no side effects
                    _prefixContainer = new PrefixContainer(Values.Keys);
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
            return GetValue(key, skipValidation: false);
        }

        public virtual ValueProviderResult GetValue(string key, bool skipValidation)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ValueProviderResultPlaceholder placeholder;
            Values.TryGetValue(key, out placeholder);
            if (placeholder == null)
            {
                return null;
            }
            else
            {
                return (skipValidation) ? placeholder.UnvalidatedResult : placeholder.ValidatedResult;
            }
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return PrefixContainer.GetKeysFromPrefix(prefix);
        }

        private Dictionary<string, ValueProviderResultPlaceholder> InitializeCollectionValues()
        {
            Dictionary<string, ValueProviderResultPlaceholder> tempValues =
                            new Dictionary<string, ValueProviderResultPlaceholder>(StringComparer.OrdinalIgnoreCase);

            // Need to read keys from the unvalidated collection, as M.W.I's granular request validation is a bit touchy
            // and validated entries at the time the key or value is looked at. For example, GetKey() will throw if the
            // value fails request validation, even though the value's not being looked at (M.W.I can't tell the difference).

            foreach (string key in _unvalidatedCollection)
            {
                if (key != null)
                {
                    string normalizedKey = key;
                    if (_jQueryToMvcRequestNormalizationRequired)
                    {
                        normalizedKey = NormalizeJQueryToMvc(key);
                    }

                    // need to look up values lazily, as eagerly looking at the collection might trigger validation
                    tempValues[normalizedKey] =
                        new ValueProviderResultPlaceholder(key, _collection, _unvalidatedCollection, _culture);
                }
            }

            return tempValues;
        }

        // This code is borrowed from WebAPI FormDataCollectionExtensions.cs 
        // This is a helper method to use Model Binding over a JQuery syntax. 
        // Normalize from JQuery to MVC keys. The model binding infrastructure uses MVC keys
        // x[] --> x
        // [] --> ""
        // x[12] --> x[12]
        // x[field]  --> x.field, where field is not a number
        private static string NormalizeJQueryToMvc(string key)
        {
            if (key == null)
            {
                return String.Empty;
            }

            StringBuilder sb = null;
            int i = 0;
            while (true)
            {
                int indexOpen = key.IndexOf('[', i);
                if (indexOpen < 0)
                {
                    // Fast path, no normalization needed.
                    // This skips the string conversion and allocating the string builder.
                    if (i == 0)
                    {
                        return key;
                    }

                    sb = sb ?? new StringBuilder();
                    sb.Append(key, i, key.Length - i);
                    break; // no more brackets
                }

                sb = sb ?? new StringBuilder();
                sb.Append(key, i, indexOpen - i); // everything up to "["

                // Find closing bracket.
                int indexClose = key.IndexOf(']', indexOpen);
                if (indexClose == -1)
                {
                    throw Error.Argument("key", MvcResources.JQuerySyntaxMissingClosingBracket);
                }

                if (indexClose == indexOpen + 1)
                {
                    // Empty bracket. Signifies array. Just remove. 
                }
                else
                {
                    if (Char.IsDigit(key[indexOpen + 1]))
                    {
                        // array index. Leave unchanged. 
                        sb.Append(key, indexOpen, indexClose - indexOpen + 1);
                    }
                    else
                    {
                        // Field name.  Convert to dot notation. 
                        sb.Append('.');
                        sb.Append(key, indexOpen + 1, indexClose - indexOpen - 1);
                    }
                }

                i = indexClose + 1;
                if (i >= key.Length)
                {
                    break; // end of string
                }
            }
            return sb.ToString();
        }

        // Placeholder that can store a validated (in relation to request validation) or unvalidated
        // ValueProviderResult for a given key.
        private sealed class ValueProviderResultPlaceholder
        {
            private ValueProviderResult _validatedResult;
            private ValueProviderResult _unvalidatedResult;
            private string _key;
            private NameValueCollection _validatedCollection;
            private NameValueCollection _unvalidatedCollection;
            private CultureInfo _culture;

            public ValueProviderResultPlaceholder(string key, NameValueCollection validatedCollection, NameValueCollection unvalidatedCollection, CultureInfo culture)
            {
                _key = key;
                _validatedCollection = validatedCollection;
                _unvalidatedCollection = unvalidatedCollection;
                _culture = culture;
            }

            public ValueProviderResult ValidatedResult
            {
                get 
                {
                    if (_validatedResult == null)
                    {
                        _validatedResult = GetResultFromCollection(_key, _validatedCollection, _culture);
                    }
                    return _validatedResult;
                }
            }

            public ValueProviderResult UnvalidatedResult
            {
                get 
                {
                    if (_unvalidatedResult == null)
                    {
                        _unvalidatedResult = GetResultFromCollection(_key, _unvalidatedCollection, _culture);
                    }
                    return _unvalidatedResult;
                }
            }

            private static ValueProviderResult GetResultFromCollection(string key, NameValueCollection collection, CultureInfo culture)
            {
                string[] rawValue = collection.GetValues(key);
                string attemptedValue = collection[key];
                return new ValueProviderResult(rawValue, attemptedValue, culture);
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Internal;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Internal
{
    /// <summary>
    ///  NameValueCollection to represent form data and to generate form data output.
    /// </summary>
#if !NETSTANDARD1_3 // NameValueCollection is not serializable in netstandard1.3.
    [Serializable]
#endif
    internal class HttpValueCollection : NameValueCollection
    {
#if !NETSTANDARD1_3 // NameValueCollection is not serializable in netstandard1.3.
        protected HttpValueCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        private HttpValueCollection()
            : base(StringComparer.OrdinalIgnoreCase) // case-insensitive keys
        {
        }

        // Use a builder function instead of a ctor to avoid virtual calls from the ctor.
        // The above condition is only important in the Full .NET fx implementation.
        internal static HttpValueCollection Create()
        {
            return new HttpValueCollection();
        }

        internal static HttpValueCollection Create(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            Contract.Assert(pairs != null);

            var hvc = new HttpValueCollection();

            // Ordering example:
            //   k=A&j=B&k=C --> k:[A,C];j=[B].
            foreach (KeyValuePair<string, string> kvp in pairs)
            {
                hvc.Add(kvp.Key, kvp.Value);
            }

            hvc.IsReadOnly = false;
            return hvc;
        }

        /// <summary>
        /// Adds a name-value pair to the collection.
        /// </summary>
        /// <param name="name">The name to be added as a case insensitive string.</param>
        /// <param name="value">The value to be added.</param>
        public override void Add(string name, string value)
        {
            ThrowIfMaxHttpCollectionKeysExceeded(Count);

            name = name ?? String.Empty;
            value = value ?? String.Empty;

            base.Add(name, value);
        }

        /// <summary>
        /// Converts the content of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance, multiple values with a single key are comma separated.</returns>
        public override string ToString()
        {
            return ToString(true);
        }

        private static void ThrowIfMaxHttpCollectionKeysExceeded(int count)
        {
            if (count >= MediaTypeFormatter.MaxHttpCollectionKeys)
            {
                throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MaxHttpCollectionKeyLimitReached, MediaTypeFormatter.MaxHttpCollectionKeys, typeof(MediaTypeFormatter));
            }
        }

        private string ToString(bool urlEncode)
        {
            if (Count == 0)
            {
                return String.Empty;
            }

            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (string name in this)
            {
                string[] values = GetValues(name);
                if (values == null || values.Length == 0)
                {
                    first = AppendNameValuePair(builder, first, urlEncode, name, String.Empty);
                }
                else
                {
                    foreach (string value in values)
                    {
                        first = AppendNameValuePair(builder, first, urlEncode, name, value);
                    }
                }
            }

            return builder.ToString();
        }

        private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
        {
            string effectiveName = name ?? String.Empty;
            string encodedName = urlEncode ? WebUtility.UrlEncode(effectiveName) : effectiveName;

            string effectiveValue = value ?? String.Empty;
            string encodedValue = urlEncode ? WebUtility.UrlEncode(effectiveValue) : effectiveValue;

            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append("&");
            }

            builder.Append(encodedName);
            if (!String.IsNullOrEmpty(encodedValue))
            {
                builder.Append("=");
                builder.Append(encodedValue);
            }
            return first;
        }
    }
}

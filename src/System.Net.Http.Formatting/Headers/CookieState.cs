// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Net.Http.Formatting.Internal;
using System.Web.Http;

namespace System.Net.Http.Headers
{
    public class CookieState : ICloneable
    {
        private string _name;
        private NameValueCollection _values = HttpValueCollection.Create();

        public CookieState(string name)
            : this(name, String.Empty)
        {
        }

        public CookieState(string name, string value)
        {
            CheckNameFormat(name, "name");
            _name = name;

            CheckValueFormat(value, "value");
            Value = value;
        }

        public CookieState(string name, NameValueCollection values)
        {
            CheckNameFormat(name, "name");
            _name = name;

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }
            Values.Add(values);
        }

        private CookieState(CookieState source)
        {
            Contract.Requires(source != null);

            _name = source._name;
            if (source._values != null)
            {
                Values.Add(source._values);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                CheckNameFormat(value, "value");
                _name = value;
            }
        }

        /// <summary>
        /// If the cookie data is a simple string value then set or retrieve it using the <see cref="Value"/> property.
        /// If the cookie data is structured then use the <see cref="Values"/> property.
        /// </summary>
        public string Value
        {
            get
            {
                return Values.Count > 0 ? Values.AllKeys[0] : String.Empty;
            }

            set
            {
                CheckValueFormat(value, "value");
                if (Values.Count > 0)
                {
                    Values.AllKeys[0] = value;
                }
                else
                {
                    Values.Add(value, String.Empty);
                }
            }
        }

        /// <summary>
        /// If the cookie data is structured then use the <see cref="Values"/> property for setting and getting individual sub-name/value pairs.
        /// If the cookie data is a simple string value then set or retrieve it using the <see cref="Value"/> property.
        /// </summary>
        public NameValueCollection Values
        {
            get { return _values; }
        }

        public string this[string subName]
        {
            get { return Values[subName]; }
            set { Values[subName] = value; }
        }

        public override string ToString()
        {
            return _name + "=" + (_values != null ? _values.ToString() : String.Empty);
        }

        public object Clone()
        {
            return new CookieState(this);
        }

        private static void CheckNameFormat(string name, string parameterName)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (!FormattingUtilities.ValidateHeaderToken(name))
            {
                throw Error.Argument(parameterName, Properties.Resources.CookieInvalidName);
            }
        }

        private static void CheckValueFormat(string value, string parameterName)
        {
            // Empty string is a valid cookie value
            if (value == null)
            {
                throw Error.ArgumentNull(parameterName);
            }
        }
    }
}
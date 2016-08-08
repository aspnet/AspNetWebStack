// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Headers
{
    public class CookieHeaderValue : ICloneable
    {
        private const string ExpiresToken = "expires";
        private const string MaxAgeToken = "max-age";
        private const string DomainToken = "domain";
        private const string PathToken = "path";
        private const string SecureToken = "secure";
        private const string HttpOnlyToken = "httponly";
        private const string DefaultPath = "/";

        private static readonly char[] segmentSeparator = new char[] { ';' };
        private static readonly char[] nameValueSeparator = new char[] { '=' };

        // Use list instead of dictionary since we may have multiple parameters with the same name.
        private Collection<CookieState> _cookies;

        public CookieHeaderValue(string name, string value)
        {
            CookieState cookie = new CookieState(name, value);
            Cookies.Add(cookie);
        }

        public CookieHeaderValue(string name, NameValueCollection values)
        {
            CookieState cookie = new CookieState(name, values);
            Cookies.Add(cookie);
        }

        /// <summary>
        /// Constructor to be used by parser to create a new instance of this type.
        /// </summary>
        protected CookieHeaderValue()
        {
        }

        private CookieHeaderValue(CookieHeaderValue source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }

            Expires = source.Expires;
            MaxAge = source.MaxAge;
            Domain = source.Domain;
            Path = source.Path;
            Secure = source.Secure;
            HttpOnly = source.HttpOnly;

            foreach (CookieState cookie in source.Cookies)
            {
                Cookies.Add(cookie.Clone<CookieState>());
            }
        }

        public Collection<CookieState> Cookies
        {
            get
            {
                if (_cookies == null)
                {
                    _cookies = new Collection<CookieState>();
                }
                return _cookies;
            }
        }

        public DateTimeOffset? Expires { get; set; }

        public TimeSpan? MaxAge { get; set; }

        public string Domain { get; set; }

        public string Path { get; set; }

        public bool Secure { get; set; }

        public bool HttpOnly { get; set; }

        public CookieState this[string name]
        {
            get
            {
                if (String.IsNullOrEmpty(name))
                {
                    return null;
                }

                CookieState cookie = Cookies.FirstOrDefault(c => String.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
                if (cookie == null)
                {
                    cookie = new CookieState(name, String.Empty);
                    Cookies.Add(cookie);
                }
                return cookie;
            }
        }

        public override string ToString()
        {
            StringBuilder header = new StringBuilder();
            bool first = true;

            foreach (CookieState cookie in Cookies)
            {
                first = AppendSegment(header, first, cookie.ToString(), null);
            }

            if (Expires.HasValue)
            {
                first = AppendSegment(header, first, ExpiresToken, FormattingUtilities.DateToString(Expires.Value));
            }

            if (MaxAge.HasValue)
            {
                first = AppendSegment(header, first, MaxAgeToken, ((int)MaxAge.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
            }

            if (Domain != null)
            {
                first = AppendSegment(header, first, DomainToken, Domain);
            }

            if (Path != null)
            {
                first = AppendSegment(header, first, PathToken, Path);
            }

            if (Secure)
            {
                first = AppendSegment(header, first, SecureToken, null);
            }

            if (HttpOnly)
            {
                first = AppendSegment(header, first, HttpOnlyToken, null);
            }

            return header.ToString();
        }

        public object Clone()
        {
            return new CookieHeaderValue(this);
        }

        public static bool TryParse(string input, out CookieHeaderValue parsedValue)
        {
            parsedValue = null;
            if (!String.IsNullOrEmpty(input))
            {
                string[] segments = input.Split(segmentSeparator);
                CookieHeaderValue instance = new CookieHeaderValue();
                foreach (string segment in segments)
                {
                    if (!ParseCookieSegment(instance, segment))
                    {
                        return false;
                    }
                }

                // If we didn't find any cookie state name/value pairs then cookie is not valid
                if (instance.Cookies.Count == 0)
                {
                    return false;
                }

                parsedValue = instance;
                return true;
            }

            return false;
        }

        private static bool AppendSegment(StringBuilder builder, bool first, string name, string value)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append("; ");
            }

            builder.Append(name);
            if (value != null)
            {
                builder.Append("=");
                builder.Append(value);
            }
            return first;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is a try method where we do want to ignore errors.")]
        private static bool ParseCookieSegment(CookieHeaderValue instance, string segment)
        {
            if (String.IsNullOrWhiteSpace(segment))
            {
                return true;
            }

            string[] nameValue = segment.Split(nameValueSeparator, 2);
            if (nameValue.Length < 1 || String.IsNullOrWhiteSpace(nameValue[0]))
            {
                return false;
            }

            string name = nameValue[0].Trim();
            if (String.Equals(name, ExpiresToken, StringComparison.OrdinalIgnoreCase))
            {
                string value = GetSegmentValue(nameValue, null);
                DateTimeOffset expires;
                if (FormattingUtilities.TryParseDate(value, out expires))
                {
                    instance.Expires = expires;
                    return true;
                }
                return false;
            }
            else if (String.Equals(name, MaxAgeToken, StringComparison.OrdinalIgnoreCase))
            {
                string value = GetSegmentValue(nameValue, null);
                int maxAge;
                if (FormattingUtilities.TryParseInt32(value, out maxAge))
                {
                    instance.MaxAge = new TimeSpan(0, 0, maxAge);
                    return true;
                }
                return false;
            }
            else if (String.Equals(name, DomainToken, StringComparison.OrdinalIgnoreCase))
            {
                instance.Domain = GetSegmentValue(nameValue, null);
                return true;
            }
            else if (String.Equals(name, PathToken, StringComparison.OrdinalIgnoreCase))
            {
                instance.Path = GetSegmentValue(nameValue, DefaultPath);
                return true;
            }
            else if (String.Equals(name, SecureToken, StringComparison.OrdinalIgnoreCase))
            {
                string value = GetSegmentValue(nameValue, null);
                if (!String.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
                instance.Secure = true;
                return true;
            }
            else if (String.Equals(name, HttpOnlyToken, StringComparison.OrdinalIgnoreCase))
            {
                string value = GetSegmentValue(nameValue, null);
                if (!String.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
                instance.HttpOnly = true;
                return true;
            }
            else
            {
                string value = GetSegmentValue(nameValue, null);

                // We read the cookie segment as form data
                try
                {
                    FormDataCollection formData = new FormDataCollection(value);
                    NameValueCollection values = formData.ReadAsNameValueCollection();
                    CookieState cookie = new CookieState(name, values);
                    instance.Cookies.Add(cookie);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static string GetSegmentValue(string[] nameValuePair, string defaultValue)
        {
            Contract.Assert(nameValuePair != null);
            return nameValuePair.Length > 1 ? FormattingUtilities.UnquoteToken(nameValuePair[1]) : defaultValue;
        }
    }
}
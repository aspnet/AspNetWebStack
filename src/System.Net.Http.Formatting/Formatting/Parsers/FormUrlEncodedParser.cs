// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers
{
    /// <summary>
    /// Buffer-oriented parsing of HTML form URL-ended, also known as <c>application/x-www-form-urlencoded</c>, data.
    /// </summary>
    internal class FormUrlEncodedParser
    {
        private const int MinMessageSize = 1;
        private long _totalBytesConsumed;
        private long _maxMessageSize;

        private NameValueState _nameValueState;
        private ICollection<KeyValuePair<string, string>> _nameValuePairs;
        private readonly CurrentNameValuePair _currentNameValuePair;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedParser"/> class.
        /// </summary>
        /// <param name="nameValuePairs">The collection to which name value pairs are added as they are parsed.</param>
        /// <param name="maxMessageSize">Maximum length of all the individual name value pairs.</param>
        public FormUrlEncodedParser(ICollection<KeyValuePair<string, string>> nameValuePairs, long maxMessageSize)
        {
            // The minimum length which would be an empty buffer
            if (maxMessageSize < MinMessageSize)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxMessageSize", maxMessageSize, MinMessageSize);
            }

            if (nameValuePairs == null)
            {
                throw Error.ArgumentNull("nameValuePairs");
            }

            _nameValuePairs = nameValuePairs;
            _maxMessageSize = maxMessageSize;
            _currentNameValuePair = new CurrentNameValuePair();
        }

        private enum NameValueState
        {
            Name = 0,
            Value
        }

        /// <summary>
        /// Parse a buffer of URL form-encoded name-value pairs and add them to the collection.
        /// Bytes are parsed in a consuming manner from the beginning of the buffer meaning that the same bytes can not be
        /// present in the buffer.
        /// </summary>
        /// <param name="buffer">Buffer from where data is read</param>
        /// <param name="bytesReady">Size of buffer</param>
        /// <param name="bytesConsumed">Offset into buffer</param>
        /// <param name="isFinal">Indicates whether the end of the URL form-encoded data has been reached.</param>
        /// <returns>State of the parser. Call this method with new data until it reaches a final state.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is translated to parse state.")]
        public ParserState ParseBuffer(
            byte[] buffer,
            int bytesReady,
            ref int bytesConsumed,
            bool isFinal)
        {
            if (buffer == null)
            {
                throw Error.ArgumentNull("buffer");
            }

            ParserState parseStatus = ParserState.NeedMoreData;

            if (bytesConsumed >= bytesReady)
            {
                if (isFinal)
                {
                    parseStatus = CopyCurrent(parseStatus);
                }

                // We either can already tell we need more data or we are done
                return parseStatus;
            }

            try
            {
                parseStatus = ParseNameValuePairs(
                    buffer,
                    bytesReady,
                    ref bytesConsumed,
                    ref _nameValueState,
                    _maxMessageSize,
                    ref _totalBytesConsumed,
                    _currentNameValuePair,
                    _nameValuePairs);

                if (isFinal)
                {
                    parseStatus = CopyCurrent(parseStatus);
                }
            }
            catch (Exception)
            {
                parseStatus = ParserState.Invalid;
            }

            return parseStatus;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This is a parser which cannot be split up for performance reasons.")]
        private static ParserState ParseNameValuePairs(
            byte[] buffer,
            int bytesReady,
            ref int bytesConsumed,
            ref NameValueState nameValueState,
            long maximumLength,
            ref long totalBytesConsumed,
            CurrentNameValuePair currentNameValuePair,
            ICollection<KeyValuePair<string, string>> nameValuePairs)
        {
            Contract.Assert((bytesReady - bytesConsumed) >= 0, "ParseNameValuePairs()|(inputBufferLength - bytesParsed) < 0");
            Contract.Assert(maximumLength <= 0 || totalBytesConsumed <= maximumLength, "ParseNameValuePairs()|Headers already read exceeds limit.");

            // Remember where we started.
            int initialBytesParsed = bytesConsumed;
            int segmentStart;

            // Set up parsing status with what will happen if we exceed the buffer.
            ParserState parseStatus = ParserState.DataTooBig;
            long effectiveMax = maximumLength <= 0 ? Int64.MaxValue : maximumLength - totalBytesConsumed + initialBytesParsed;
            if (bytesReady < effectiveMax)
            {
                parseStatus = ParserState.NeedMoreData;
                effectiveMax = bytesReady;
            }

            Contract.Assert(bytesConsumed < effectiveMax, "We have already consumed more than the max buffer length.");

            switch (nameValueState)
            {
                case NameValueState.Name:
                    segmentStart = bytesConsumed;
                    while (buffer[bytesConsumed] != '=' && buffer[bytesConsumed] != '&')
                    {
                        if (++bytesConsumed == effectiveMax)
                        {
                            string name = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                            currentNameValuePair.Name.Append(name);
                            goto quit;
                        }
                    }

                    if (bytesConsumed > segmentStart)
                    {
                        string name = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentNameValuePair.Name.Append(name);
                    }

                    // Check if we got name=value or just name
                    if (buffer[bytesConsumed] == '=')
                    {
                        // Move part the '='
                        nameValueState = NameValueState.Value;
                        if (++bytesConsumed == effectiveMax)
                        {
                            goto quit;
                        }

                        goto case NameValueState.Value;
                    }
                    else
                    {
                        // Copy parsed name-only to collection
                        currentNameValuePair.CopyNameOnlyTo(nameValuePairs);

                        // Move past the '&' but stay in same state
                        if (++bytesConsumed == effectiveMax)
                        {
                            goto quit;
                        }

                        goto case NameValueState.Name;
                    }

                case NameValueState.Value:
                    segmentStart = bytesConsumed;
                    while (buffer[bytesConsumed] != '&')
                    {
                        if (++bytesConsumed == effectiveMax)
                        {
                            string value = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                            currentNameValuePair.Value.Append(value);
                            goto quit;
                        }
                    }

                    if (bytesConsumed > segmentStart)
                    {
                        string value = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentNameValuePair.Value.Append(value);
                    }

                    // Copy parsed name value pair to collection
                    currentNameValuePair.CopyTo(nameValuePairs);

                    // Move past the '&'
                    nameValueState = NameValueState.Name;
                    if (++bytesConsumed == effectiveMax)
                    {
                        goto quit;
                    }

                    goto case NameValueState.Name;
            }

        quit:
            totalBytesConsumed += bytesConsumed - initialBytesParsed;
            return parseStatus;
        }

        private ParserState CopyCurrent(ParserState parseState)
        {
            // Copy parsed name value pair to collection
            if (_nameValueState == NameValueState.Name)
            {
                if (_totalBytesConsumed > 0)
                {
                    _currentNameValuePair.CopyNameOnlyTo(_nameValuePairs);
                }
            }
            else
            {
                _currentNameValuePair.CopyTo(_nameValuePairs);
            }

            // We are done (or in an error state)
            return parseState == ParserState.NeedMoreData ? ParserState.Done : parseState;
        }

        /// <summary>
        /// Maintains information about the current header field being parsed.
        /// </summary>
        private class CurrentNameValuePair
        {
            private const int DefaultNameAllocation = 128;
            private const int DefaultValueAllocation = 2 * 1024;

            private readonly StringBuilder _name = new StringBuilder(DefaultNameAllocation);
            private readonly StringBuilder _value = new StringBuilder(DefaultValueAllocation);

            /// <summary>
            /// Gets the name of the name value pair.
            /// </summary>
            public StringBuilder Name
            {
                get { return _name; }
            }

            /// <summary>
            /// Gets the value of the name value pair
            /// </summary>
            public StringBuilder Value
            {
                get { return _value; }
            }

            /// <summary>
            /// Copies current name value pair field to the provided collection instance.
            /// </summary>
            /// <param name="nameValuePairs">The collection to copy into.</param>
            public void CopyTo(ICollection<KeyValuePair<string, string>> nameValuePairs)
            {
                string unescapedName = WebUtility.UrlDecode(_name.ToString());
                string escapedValue = _value.ToString();
                string value = WebUtility.UrlDecode(escapedValue);

                nameValuePairs.Add(new KeyValuePair<string, string>(unescapedName, value));

                Clear();
            }

            /// <summary>
            /// Copies current name-only to the provided collection instance.
            /// </summary>
            /// <param name="nameValuePairs">The collection to copy into.</param>
            public void CopyNameOnlyTo(ICollection<KeyValuePair<string, string>> nameValuePairs)
            {
                string unescapedName = WebUtility.UrlDecode(_name.ToString());
                string value = String.Empty;
                nameValuePairs.Add(new KeyValuePair<string, string>(unescapedName, value));
                Clear();
            }

            /// <summary>
            /// Clears this instance.
            /// </summary>
            private void Clear()
            {
                _name.Clear();
                _value.Clear();
            }
        }
    }
}

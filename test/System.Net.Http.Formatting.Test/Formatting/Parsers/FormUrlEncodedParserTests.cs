// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.Parsers
{
    public class FormUrlEncodedParserTests
    {
        private const int MinMessageSize = 1;
        private const int Iterations = 16;

        internal static Collection<KeyValuePair<string, string>> CreateCollection()
        {
            return new Collection<KeyValuePair<string, string>>();
        }

        internal static FormUrlEncodedParser CreateParser(int maxMessageSize, out ICollection<KeyValuePair<string, string>> nameValuePairs)
        {
            nameValuePairs = CreateCollection();
            return new FormUrlEncodedParser(nameValuePairs, maxMessageSize);
        }

        internal static byte[] CreateBuffer(params string[] nameValuePairs)
        {
            StringBuilder buffer = new StringBuilder();
            bool first = true;
            foreach (var h in nameValuePairs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    buffer.Append('&');
                }

                buffer.Append(h);
            }

            return Encoding.UTF8.GetBytes(buffer.ToString());
        }

        internal static ParserState ParseBufferInSteps(FormUrlEncodedParser parser, byte[] buffer, int readsize, out int totalBytesConsumed)
        {
            ParserState state = ParserState.Invalid;
            totalBytesConsumed = 0;
            while (totalBytesConsumed <= buffer.Length)
            {
                int size = Math.Min(buffer.Length - totalBytesConsumed, readsize);
                byte[] parseBuffer = new byte[size];
                Buffer.BlockCopy(buffer, totalBytesConsumed, parseBuffer, 0, size);

                int bytesConsumed = 0;
                state = parser.ParseBuffer(parseBuffer, parseBuffer.Length, ref bytesConsumed, totalBytesConsumed == buffer.Length - size);
                totalBytesConsumed += bytesConsumed;

                if (state != ParserState.NeedMoreData)
                {
                    return state;
                }
            }

            return state;
        }

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<FormUrlEncodedParser>(TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void FormUrlEncodedParserThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => { new FormUrlEncodedParser(null, ParserData.MinHeaderSize); }, "nameValuePairs");
        }

        [Fact]
        public void FormUrlEncodedParserThrowsOnInvalidSize()
        {
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => { new FormUrlEncodedParser(CreateCollection(), MinMessageSize - 1); }, "maxMessageSize", MinMessageSize.ToString(), MinMessageSize - 1);

            FormUrlEncodedParser parser = new FormUrlEncodedParser(CreateCollection(), MinMessageSize);
            Assert.NotNull(parser);

            parser = new FormUrlEncodedParser(CreateCollection(), MinMessageSize + 1);
            Assert.NotNull(parser);
        }

        [Fact]
        public void ParseBufferThrowsOnNullBuffer()
        {
            ICollection<KeyValuePair<string, string>> collection;
            FormUrlEncodedParser parser = CreateParser(128, out collection);
            int bytesConsumed = 0;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed, false); }, "buffer");
        }

        [Fact]
        public void ParseBufferHandlesEmptyBuffer()
        {
            byte[] data = CreateBuffer();
            ICollection<KeyValuePair<string, string>> collection;
            FormUrlEncodedParser parser = CreateParser(MinMessageSize, out collection);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed, true);
            Assert.Equal(ParserState.Done, state);
            Assert.Equal(data.Length, bytesConsumed);
            Assert.Empty(collection);
        }

        [Theory]
        [InlineData("N", "N", "")]
        [InlineData("%26", "&", "")]
        [TestDataSet(typeof(UriQueryTestData), "UriQueryData")]
        public void ParseBufferCorrectly(string segment, string name, string value)
        {
            for (int index = 1; index < Iterations; index++)
            {
                List<string> segments = new List<string>();
                for (int cnt = 0; cnt < index; cnt++)
                {
                    segments.Add(segment);
                }

                byte[] data = CreateBuffer(segments.ToArray());
                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    ICollection<KeyValuePair<string, string>> collection;
                    FormUrlEncodedParser parser = CreateParser(data.Length + 1, out collection);
                    Assert.NotNull(parser);

                    int totalBytesConsumed;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Done, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    Assert.Equal(index, collection.Count());
                    foreach (KeyValuePair<string, string> element in collection)
                    {
                        Assert.Equal(name, element.Key);
                        Assert.Equal(value, element.Value);
                    }
                }
            }
        }

        [Fact]
        public void HeaderParserDataTooBig()
        {
            byte[] data = CreateBuffer("N=V");
            ICollection<KeyValuePair<string, string>> collection;
            FormUrlEncodedParser parser = CreateParser(MinMessageSize, out collection);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed, true);
            Assert.Equal(ParserState.DataTooBig, state);
            Assert.Equal(MinMessageSize, bytesConsumed);
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.Parsers
{
    public class HttpResponseHeaderParserTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpResponseHeaderParser>(TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void HttpResponseHeaderParserConstructorTest()
        {
            HttpUnsortedResponse result = new HttpUnsortedResponse();
            Assert.NotNull(result);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize - 1, ParserData.MinHeaderSize),
                "maxStatusLineSize", ParserData.MinStatusLineSize.ToString(), ParserData.MinStatusLineSize - 1);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize - 1),
                "maxHeaderSize", ParserData.MinHeaderSize.ToString(), ParserData.MinHeaderSize - 1);

            HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);

            Assert.ThrowsArgumentNull(() => { new HttpResponseHeaderParser(null); }, "httpResponse");
        }


        [Fact]
        public void ResponseHeaderParserNullBuffer()
        {
            HttpUnsortedResponse result = new HttpUnsortedResponse();
            HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);
            int bytesConsumed = 0;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed); }, "buffer");
        }

        [Fact]
        public void ResponseHeaderParserMinimumBuffer()
        {
            byte[] data = CreateBuffer("HTTP/1.1", "200", "", null);
            HttpUnsortedResponse result = new HttpUnsortedResponse();
            HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed);
            Assert.Equal(ParserState.Done, state);
            Assert.Equal(data.Length, bytesConsumed);

            ValidateResult(result, new Version("1.1"), HttpStatusCode.OK, "", null);
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "AllHttpStatusCodes")]
        public void ResponseHeaderParserAcceptsStandardStatusCodes(HttpStatusCode status)
        {
            byte[] data = CreateBuffer("HTTP/1.1", ((int)status).ToString(), "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(result, new Version("1.1"), status, "Reason", ParserData.ValidHeaders);
            }
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "CustomHttpStatusCodes")]
        public void ResponseHeaderParserAcceptsCustomStatusCodes(HttpStatusCode status)
        {
            byte[] data = CreateBuffer("HTTP/1.1", ((int)status).ToString(), "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(result, new Version("1.1"), status, "Reason", ParserData.ValidHeaders);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidStatusCodes")]
        public void ResponseHeaderParserRejectsInvalidStatusCodes(string invalidStatus)
        {
            byte[] data = CreateBuffer("HTTP/1.1", invalidStatus, "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidReasonPhrases")]
        public void ResponseHeaderParserRejectsInvalidReasonPhrase(string invalidReason)
        {
            byte[] data = CreateBuffer("HTTP/1.1", "200", invalidReason, ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "Versions")]
        public void ResponseHeaderParserAcceptsValidVersion(Version version)
        {
            byte[] data = CreateBuffer(String.Format("HTTP/{0}", version.ToString(2)), "200", "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(result, version, HttpStatusCode.OK, "Reason", ParserData.ValidHeaders);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidVersions")]
        public void ResponseHeaderParserRejectsInvalidVersion(string invalid)
        {
            byte[] data = CreateBuffer(invalid, "200", "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        private static byte[] CreateBuffer(string version, string statusCode, string reasonPhrase, Dictionary<string, string> headers)
        {
            const string SP = " ";
            const string CRLF = "\r\n";
            string lws = SP;

            StringBuilder response = new StringBuilder();
            response.AppendFormat("{0}{1}{2}{3}{4}{5}", version, lws, statusCode, lws, reasonPhrase, CRLF);
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    response.AppendFormat("{0}: {1}{2}", h.Key, h.Value, CRLF);
                }
            }

            response.Append(CRLF);
            return Encoding.UTF8.GetBytes(response.ToString());
        }

        private static ParserState ParseBufferInSteps(HttpResponseHeaderParser parser, byte[] buffer, int readsize, out int totalBytesConsumed)
        {
            ParserState state = ParserState.Invalid;
            totalBytesConsumed = 0;
            while (totalBytesConsumed <= buffer.Length)
            {
                int size = Math.Min(buffer.Length - totalBytesConsumed, readsize);
                byte[] parseBuffer = new byte[size];
                Buffer.BlockCopy(buffer, totalBytesConsumed, parseBuffer, 0, size);

                int bytesConsumed = 0;
                state = parser.ParseBuffer(parseBuffer, parseBuffer.Length, ref bytesConsumed);
                totalBytesConsumed += bytesConsumed;

                if (state != ParserState.NeedMoreData)
                {
                    return state;
                }
            }

            return state;
        }

        private static void ValidateResult(
            HttpUnsortedResponse statusLine,
            Version version,
            HttpStatusCode statusCode,
            string reasonPhrase,
            Dictionary<string, string> headers)
        {
            Assert.Equal(version, statusLine.Version);
            Assert.Equal(statusCode, statusLine.StatusCode);
            Assert.Equal(reasonPhrase, statusLine.ReasonPhrase);

            if (headers != null)
            {
                Assert.Equal(headers.Count, statusLine.HttpHeaders.Count());
                foreach (var header in headers)
                {
                    Assert.True(statusLine.HttpHeaders.Contains(header.Key), "Parsed header did not contain expected key " + header.Key);
                    Assert.Equal(header.Value, statusLine.HttpHeaders.GetValues(header.Key).ElementAt(0));
                }
            }
        }
    }
}

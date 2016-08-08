// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting.DataSets;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.Parsers
{
    public class HttpStatusLineParserTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpStatusLineParser>(TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void HttpStatusLineParserConstructorTest()
        {
            HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
            Assert.NotNull(statusLine);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpStatusLineParser(statusLine, ParserData.MinStatusLineSize - 1),
                "maxStatusLineSize", ParserData.MinStatusLineSize.ToString(), ParserData.MinStatusLineSize - 1);

            HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, ParserData.MinStatusLineSize);
            Assert.NotNull(parser);

            Assert.ThrowsArgumentNull(() => { new HttpStatusLineParser(null, ParserData.MinStatusLineSize); }, "httpResponse");
        }


        [Fact]
        public void StatusLineParserNullBuffer()
        {
            HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
            HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, ParserData.MinStatusLineSize);
            Assert.NotNull(parser);
            int bytesConsumed = 0;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed); }, "buffer");
        }

        [Fact]
        public void StatusLineParserMinimumBuffer()
        {
            byte[] data = CreateBuffer("HTTP/1.1", "200", "");
            HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
            HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, ParserData.MinStatusLineSize);
            Assert.NotNull(parser);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed);
            Assert.Equal(ParserState.Done, state);
            Assert.Equal(data.Length, bytesConsumed);

            ValidateResult(statusLine, new Version("1.1"), HttpStatusCode.OK, "");
        }

        [Fact]
        public void StatusLineParserRejectsLws()
        {
            byte[] data = CreateBuffer("HTTP/1.1", "200", "Reason", true);
            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, data.Length);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "AllHttpStatusCodes")]
        public void StatusLineParserAcceptsStandardStatusCodes(HttpStatusCode status)
        {
            byte[] data = CreateBuffer("HTTP/1.1", ((int)status).ToString(), "Reason");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, data.Length);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(statusLine, new Version("1.1"), status, "Reason");
            }
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "CustomHttpStatusCodes")]
        public void StatusLineParserAcceptsCustomStatusCodes(HttpStatusCode status)
        {
            byte[] data = CreateBuffer("HTTP/1.1", ((int)status).ToString(), "Reason");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, data.Length);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(statusLine, new Version("1.1"), status, "Reason");
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidStatusCodes")]
        public void StatusLineParserRejectsInvalidStatusCodes(string invalidStatus)
        {
            byte[] data = CreateBuffer("HTTP/1.1", invalidStatus, "Reason");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "ValidReasonPhrases")]
        public void StatusLineParserAcceptsValidReasonPhrase(string validReasonPhrase)
        {
            byte[] data = CreateBuffer("HTTP/1.1", "200", validReasonPhrase);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);

                ValidateResult(statusLine, new Version("1.1"), HttpStatusCode.OK, validReasonPhrase);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "Versions")]
        public void StatusLineParserAcceptsValidVersion(Version version)
        {
            byte[] data = CreateBuffer(String.Format("HTTP/{0}", version.ToString(2)), "200", "Reason");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(statusLine, version, HttpStatusCode.OK, "Reason");
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidVersions")]
        public void StatusLineParserRejectsInvalidVersion(string invalidVersion)
        {
            byte[] data = CreateBuffer(invalidVersion, "200", "Reason");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse statusLine = new HttpUnsortedResponse();
                HttpStatusLineParser parser = new HttpStatusLineParser(statusLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        internal static byte[] CreateBuffer(string version, string statusCode, string reasonPhrase)
        {
            return CreateBuffer(version, statusCode, reasonPhrase, false);
        }

        private static byte[] CreateBuffer(string version, string statusCode, string reasonPhrase, bool withLws)
        {
            const string SP = " ";
            const string HTAB = "\t";
            const string CRLF = "\r\n";

            string lws = SP;
            if (withLws)
            {
                lws = SP + SP + HTAB + SP;
            }

            string statusLine = String.Format("{0}{1}{2}{3}{4}{5}", version, lws, statusCode, lws, reasonPhrase, CRLF);
            return Encoding.UTF8.GetBytes(statusLine);
        }

        private static ParserState ParseBufferInSteps(HttpStatusLineParser parser, byte[] buffer, int readsize, out int totalBytesConsumed)
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

        private static void ValidateResult(HttpUnsortedResponse statusLine, Version version, HttpStatusCode statusCode, string reasonPhrase)
        {
            Assert.Equal(version, statusLine.Version);
            Assert.Equal(statusCode, statusLine.StatusCode);
            Assert.Equal(reasonPhrase, statusLine.ReasonPhrase);
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting.DataSets;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.Parsers
{
    public class HttpRequestLineParserTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpRequestLineParser>(TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void HttpRequestLineParserConstructorTest()
        {
            HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
            Assert.NotNull(requestLine);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpRequestLineParser(requestLine, ParserData.MinRequestLineSize - 1),
                "maxRequestLineSize", ParserData.MinRequestLineSize.ToString(), ParserData.MinRequestLineSize - 1);

            HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, ParserData.MinRequestLineSize);
            Assert.NotNull(parser);

            Assert.ThrowsArgumentNull(() => { new HttpRequestLineParser(null, ParserData.MinRequestLineSize); }, "httpRequest");
        }


        [Fact]
        public void RequestLineParserNullBuffer()
        {
            HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
            HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, ParserData.MinRequestLineSize);
            Assert.NotNull(parser);
            int bytesConsumed = 0;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed); }, "buffer");
        }

        [Fact]
        public void RequestLineParserMinimumBuffer()
        {
            byte[] data = CreateBuffer("G", "/", "HTTP/1.1");
            HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
            HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, ParserData.MinRequestLineSize);
            Assert.NotNull(parser);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed);
            Assert.Equal(ParserState.Done, state);
            Assert.Equal(data.Length, bytesConsumed);

            ValidateResult(requestLine, "G", "/", new Version("1.1"));
        }

        [Fact]
        public void RequestLineParserRejectsLws()
        {
            byte[] data = CreateBuffer("GET", "/", "HTTP/1.1", true);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, data.Length);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "AllHttpMethods")]
        public void RequestLineParserAcceptsStandardMethods(HttpMethod method)
        {
            byte[] data = CreateBuffer(method.ToString(), "/", "HTTP/1.1");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, data.Length);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(requestLine, method.ToString(), "/", new Version("1.1"));
            }
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "CustomHttpMethods")]
        public void RequestLineParserAcceptsCustomMethods(HttpMethod method)
        {
            byte[] data = CreateBuffer(method.ToString(), "/", "HTTP/1.1");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, data.Length);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(requestLine, method.ToString(), "/", new Version("1.1"));
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidMethods")]
        public void RequestLineParserRejectsInvalidMethod(string invalidMethod)
        {
            byte[] data = CreateBuffer(invalidMethod, "/", "HTTP/1.1");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidRequestUris")]
        public void RequestLineParserRejectsInvalidUri(string invalidRequestUri)
        {
            byte[] data = CreateBuffer("GET", invalidRequestUri, "HTTP/1.1");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "Versions")]
        public void RequestLineParserAcceptsValidVersion(Version version)
        {
            byte[] data = CreateBuffer("GET", "/", String.Format("HTTP/{0}", version.ToString(2)));

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(requestLine, "GET", "/", version);
            }
        }

        [Theory]
        [TestDataSet(typeof(ParserData), "InvalidVersions")]
        public void RequestLineParserRejectsInvalidVersion(string invalidVersion)
        {
            byte[] data = CreateBuffer("GET", "/", invalidVersion);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest requestLine = new HttpUnsortedRequest();
                HttpRequestLineParser parser = new HttpRequestLineParser(requestLine, 256);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }

        internal static byte[] CreateBuffer(string method, string address, string version)
        {
            return CreateBuffer(method, address, version, false);
        }

        private static byte[] CreateBuffer(string method, string address, string version, bool withLws)
        {
            const string SP = " ";
            const string HTAB = "\t";
            const string CRLF = "\r\n";

            string lws = SP;
            if (withLws)
            {
                lws = SP + SP + HTAB + SP;
            }

            string requestLine = String.Format("{0}{1}{2}{3}{4}{5}", method, lws, address, lws, version, CRLF);
            return Encoding.UTF8.GetBytes(requestLine);
        }

        private static ParserState ParseBufferInSteps(HttpRequestLineParser parser, byte[] buffer, int readsize, out int totalBytesConsumed)
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

        private static void ValidateResult(HttpUnsortedRequest requestLine, string method, string requestUri, Version version)
        {
            Assert.Equal(new HttpMethod(method), requestLine.Method);
            Assert.Equal(requestUri, requestLine.RequestUri);
            Assert.Equal(version, requestLine.Version);
        }
    }
}
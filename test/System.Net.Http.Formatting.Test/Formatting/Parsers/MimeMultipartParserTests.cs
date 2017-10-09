// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.Parsers
{
    public class MimeMultipartParserTests
    {
        private const string SP = " ";
        private const string LF = "\n";
        private const string CR = "\r";
        private const string CRLF = "\r\n";
        private const string HTAB = "\t";
        private const string DashDash = "--";

        public static TheoryDataSet<string> Boundaries
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "1",
                    "a",
                    "'()+_--./:=?",
                    "--",
                    "----",
                    "9df4e21f-6e6f-4b08-8023-97283d2afeeb",
                    "-----------------------------7d159c1302d0y0",
                    "--------------------01234567890123456789",
                    "--------------------01234567890123456789--------------------",
                    "--A--B--C--D--E--F--",
                };
            }
        }

        public static TheoryDataSet<string> SingleShortBodies
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    "A",
                    "AA",
                };
            }
        }

        public static TheoryDataSet<string[]> MultipleShortBodies
        {
            get
            {
                return new TheoryDataSet<string[]>
                {
                    CreateMultipleShortBodies("", 26),
                    CreateMultipleShortBodies("{0}", 26),
                    CreateMultipleShortBodies("--{0}", 26),
                };
            }
        }

        public static TheoryDataSet<string> SingleLongBodies
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    CreateLongString("1234567890", "A", 128),
                    CreateLongString("1234567890", "--", 128),
                };
            }
        }

        public static TheoryDataSet<string[]> MultipleLongBodies
        {
            get
            {
                string[] result = new string[16];
                for (int count = 0; count < result.Length; count++)
                {
                    string bookend = Char.ConvertFromUtf32(0x41 + count);
                    result[count] = CreateLongString("1234567890", bookend, 16);
                }

                return new TheoryDataSet<string[]>
                {
                    result
                };
            }
        }

        public static TheoryDataSet<string> NearBoundaryBodies
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "AAA" + LF,
                    "AAA" + CR,
                    "AAA" + CRLF,
                    "AAA" + CRLF + CRLF,
                    "AAA" + CRLF + "-",
                    "AAA" + CRLF + "-" + CR,
                    "AAA" + CRLF + "=" + CRLF,
                    CR + "-" + "AAA",
                    CRLF + "-" + "AAA",
                    CRLF + "--" + "AAA" + CR + "AAA",
                    CRLF,
                    "AAA",
                    CRLF + CRLF,
                    CRLF + CRLF + CRLF,
                    "AAA" + "--" + "AAA",
                    CRLF + "AAA" + "--" + "AAA" + "--",
                    CRLF + "--" + "AAA" + CRLF,
                    CRLF + "--" + "AAA" + CRLF + CRLF,
                    CRLF + "--" + "AAA" + "--" + CRLF,
                    CRLF + "--" + "AAA" + "--" + CRLF + CRLF,
                    "--úN$(Os#»Í(Bt$(Dqf(CS'.Â‚æ0j",
                    "--123456",
                    "123--456",
                    "123456--"
                };
            }
        }

        public static TheoryDataSet<bool> TrueAndFalse
        {
            get
            {
                return new TheoryDataSet<bool>
                {
                    true,
                    false,
                };
            }
        }

        [Theory]
        [TestDataSet(typeof(MimeMultipartParserTests), "Boundaries")]
        public void MimeMultipartParserConstructorTest(string boundary)
        {
            MimeMultipartParser parser = new MimeMultipartParser(boundary, ParserData.MinMessageSize);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new MimeMultipartParser("-", ParserData.MinMessageSize - 1),
                "maxMessageSize", ParserData.MinMessageSize.ToString(), ParserData.MinMessageSize - 1);

            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgument(() => { new MimeMultipartParser(empty, ParserData.MinMessageSize); }, "boundary", allowDerivedExceptions: true);
            }

            Assert.ThrowsArgument(() => { new MimeMultipartParser("trailingspace ", ParserData.MinMessageSize); }, "boundary");

            Assert.ThrowsArgumentNull(() => { new MimeMultipartParser(null, ParserData.MinMessageSize); }, "boundary");
        }

        [Fact]
        public void MimeMultipartParser_ThrowsOnTooBigBoundary()
        {
            string maxLegalBoundary = new string('a', 246);
            MimeMultipartParser parser = new MimeMultipartParser(maxLegalBoundary, ParserData.MinMessageSize);

            string minIllegalBoundary = new string('a', 247);
            Assert.ThrowsArgumentLessThanOrEqualTo(() => new MimeMultipartParser(minIllegalBoundary, ParserData.MinMessageSize),
                "boundary", "246", "247");
        }

        [Fact]
        public void MultipartParserNullBuffer()
        {
            MimeMultipartParser parser = CreateMimeMultipartParser("-", 128);

            int bytesConsumed = 0;
            ArraySegment<byte> out1;
            ArraySegment<byte> out2;
            bool isFinal;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed, out out1, out out2, out isFinal); }, "buffer");
        }

        [Theory]
        [TestDataSet(typeof(MimeMultipartParserTests), "Boundaries")]
        public void MultipartParserEmptyBuffer(string boundary)
        {
            byte[] data = CreateBuffer(boundary);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);


                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);
                Assert.Equal(0, bodyParts[1].Length);
            }
        }

        [Theory]
        [TestDataSet(
            typeof(MimeMultipartParserTests), "Boundaries",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "SingleShortBodies")]
        public void MultipartParserSingleShortBodyPart(string boundary, bool withExtraWhitespace, bool withExtraCRLF, string singleShortBody)
        {
            byte[] data = CreateBuffer(boundary, withExtraWhitespace, withExtraCRLF, singleShortBody);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);
                Assert.Equal(singleShortBody, bodyParts[1]);
            }
        }

        [Theory]
        [TestDataSet(
            typeof(MimeMultipartParserTests), "Boundaries",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "MultipleShortBodies")]
        public void MultipartParserMultipleShortBodyParts(string boundary, bool withExtraWhitespace, bool withExtraCRLF, string[] multipleShortBodies)
        {
            byte[] data = CreateBuffer(boundary, withExtraWhitespace, withExtraCRLF, multipleShortBodies);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(multipleShortBodies.Length + 1, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                for (var check = 0; check < multipleShortBodies.Length; check++)
                {
                    Assert.Equal(multipleShortBodies[check], bodyParts[check + 1]);
                }
            }
        }


        [Theory]
        [TestDataSet(
            typeof(MimeMultipartParserTests), "Boundaries",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "SingleLongBodies")]
        public void MultipartParserSingleLongBodyPart(string boundary, bool withExtraWhitespace, bool withExtraCRLF, string singleLongBody)
        {
            byte[] data = CreateBuffer(boundary, withExtraWhitespace, withExtraCRLF, singleLongBody);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                Assert.Equal(singleLongBody.Length, bodyParts[1].Length);
                Assert.Equal(singleLongBody, bodyParts[1]);
            }
        }

        [Theory]
        [TestDataSet(
            typeof(MimeMultipartParserTests), "Boundaries",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "MultipleLongBodies")]
        public void MultipartParserMultipleLongBodyParts(string boundary, bool withExtraWhitespace, bool withExtraCRLF, string[] multipleLongBodies)
        {
            byte[] data = CreateBuffer(boundary, withExtraWhitespace, withExtraCRLF, multipleLongBodies);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(multipleLongBodies.Length + 1, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                for (var check = 0; check < multipleLongBodies.Length; check++)
                {
                    Assert.Equal(multipleLongBodies[check], bodyParts[check + 1]);
                }
            }
        }

        [Theory]
        [TestDataSet(
            typeof(MimeMultipartParserTests), "Boundaries",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "TrueAndFalse",
            typeof(MimeMultipartParserTests), "NearBoundaryBodies")]
        public void MultipartParserNearMatches(string boundary, bool withExtraWhitespace, bool withExtraCRLF, string nearBoundaryBody)
        {
            byte[] data = CreateBuffer(boundary, withExtraWhitespace, withExtraCRLF, nearBoundaryBody);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);
                Assert.Equal(nearBoundaryBody, bodyParts[1]);
            }
        }

        [Theory]
        [TestDataSet(typeof(MimeMultipartParserTests), "Boundaries")]
        public void MultipartParserNesting(string boundary)
        {
            for (var nesting = 0; nesting < 16; nesting++)
            {
                string nested = CreateNestedBuffer(nesting);

                byte[] data = CreateBuffer(boundary, nested);

                for (var readSize = 1; readSize <= data.Length; readSize++)
                {
                    MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                    int totalBytesConsumed;
                    List<string> bodyParts;
                    MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                    Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    Assert.Equal(2, bodyParts.Count);
                    Assert.Equal(0, bodyParts[0].Length);
                    Assert.Equal(nested.Length, bodyParts[1].Length);
                }
            }
        }

        [Theory]
        [TestDataSet(typeof(MimeMultipartParserTests), "Boundaries")]
        public void MimeMultipartParserTestDataTooBig(string boundary)
        {
            byte[] data = CreateBuffer(boundary);

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary, ParserData.MinMessageSize);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.DataTooBig, state);
                Assert.Equal(ParserData.MinMessageSize, totalBytesConsumed);
            }
        }

        [Fact]
        public void MaxMessageSizeIsExact()
        {
            string boundary = "--A";
            byte[] data = CreateBuffer(boundary, "cool");

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary, data.Length);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, 2, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Empty(bodyParts[0]);
            }
        }

        [Theory]
        [TestDataSet(typeof(MimeMultipartParserTests), "Boundaries")]
        public async Task MimeMultipartParserTestMultipartContent(string boundary)
        {
            MultipartContent content = new MultipartContent("mixed", boundary);
            content.Add(new StringContent("A"));
            content.Add(new StringContent("B"));
            content.Add(new StringContent("C"));

            MemoryStream memStream = new MemoryStream();
            await content.CopyToAsync(memStream);
            memStream.Position = 0;
            byte[] data = memStream.ToArray();

            for (var readSize = 1; readSize <= data.Length; readSize++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(boundary);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, readSize, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(4, bodyParts.Count);
                Assert.Empty(bodyParts[0]);

                Assert.EndsWith("A", bodyParts[1]);
                Assert.EndsWith("B", bodyParts[2]);
                Assert.EndsWith("C", bodyParts[3]);
            }
        }

        private static MimeMultipartParser CreateMimeMultipartParser(string boundary)
        {
            return new MimeMultipartParser(boundary, MimeMultipartBodyPartParser.DefaultMaxMessageSize);
        }

        private static MimeMultipartParser CreateMimeMultipartParser(string boundary, int minimumLength)
        {
            return new MimeMultipartParser(boundary, minimumLength);
        }

        internal static byte[] CreateBuffer(string boundary, params string[] bodyparts)
        {
            return CreateBuffer(boundary, false, false, bodyparts);
        }

        internal static string CreateNestedBuffer(int count)
        {
            StringBuilder buffer = new StringBuilder("content");

            for (var readSize = 0; readSize < count; readSize++)
            {
                byte[] nested = CreateBuffer("N" + readSize.ToString(), buffer.ToString());
                var message = Encoding.UTF8.GetString(nested);
                buffer.Length = 0;
                buffer.AppendLine(message);
            }

            return buffer.ToString();
        }

        private static byte[] CreateBuffer(string boundary, bool withExtraWhitespace, bool withTrailingCRLF, params string[] bodyparts)
        {
            string whitespace = String.Empty;
            if (withExtraWhitespace)
            {
                whitespace = SP + SP + HTAB + SP;
            }

            StringBuilder message = new StringBuilder();
            message.Append(DashDash + boundary + whitespace + CRLF);
            for (var i = 0; i < bodyparts.Length; i++)
            {
                message.Append(bodyparts[i]);
                if (i < bodyparts.Length - 1)
                {
                    message.Append(CRLF + DashDash + boundary + whitespace + CRLF);
                }
            }

            message.Append(CRLF + DashDash + boundary + DashDash + whitespace);
            if (withTrailingCRLF)
            {
                message.Append(CRLF);
            }

            return Encoding.UTF8.GetBytes(message.ToString());
        }

        private static MimeMultipartParser.State ParseBufferInSteps(MimeMultipartParser parser, byte[] buffer, int readsize, out List<string> bodyParts, out int totalBytesConsumed)
        {
            MimeMultipartParser.State state = MimeMultipartParser.State.Invalid;
            totalBytesConsumed = 0;
            bodyParts = new List<string>();
            bool isFinal = false;
            byte[] currentBodyPart = new byte[32 * 1024];
            int currentBodyLength = 0;

            while (true)
            {
                int size = Math.Min(buffer.Length - totalBytesConsumed, readsize);
                byte[] parseBuffer = new byte[size];
                Buffer.BlockCopy(buffer, totalBytesConsumed, parseBuffer, 0, size);

                int bytesConsumed = 0;
                ArraySegment<byte> out1;
                ArraySegment<byte> out2;
                state = parser.ParseBuffer(parseBuffer, parseBuffer.Length, ref bytesConsumed, out out1, out out2, out isFinal);
                totalBytesConsumed += bytesConsumed;

                Buffer.BlockCopy(out1.Array, out1.Offset, currentBodyPart, currentBodyLength, out1.Count);
                currentBodyLength += out1.Count;

                Buffer.BlockCopy(out2.Array, out2.Offset, currentBodyPart, currentBodyLength, out2.Count);
                currentBodyLength += out2.Count;

                if (state == MimeMultipartParser.State.BodyPartCompleted)
                {
                    var bPart = new byte[currentBodyLength];
                    Buffer.BlockCopy(currentBodyPart, 0, bPart, 0, currentBodyLength);
                    bodyParts.Add(Encoding.UTF8.GetString(bPart));
                    currentBodyLength = 0;
                    if (isFinal)
                    {
                        break;
                    }
                }
                else if (state != MimeMultipartParser.State.NeedMoreData)
                {
                    return state;
                }
            }

            Assert.True(isFinal, "The last segment is not a final segment.");
            return state;
        }

        private static string[] CreateMultipleShortBodies(string format, int iterations)
        {
            string[] result = new string[iterations];
            for (int count = 0; count < iterations; count++)
            {
                result[count] = string.Format(format, Char.ConvertFromUtf32(0x41 + count));
            }
            return result;
        }

        private static string CreateLongString(string msg, string bookend, int iterations)
        {
            StringBuilder longBody = new StringBuilder();
            if (!String.IsNullOrEmpty(bookend))
            {
                longBody.Append(bookend);
            }

            for (int i = 0; i < iterations; i++)
            {
                longBody.Append(msg);
            }

            if (!String.IsNullOrEmpty(bookend))
            {
                longBody.Append(bookend);
            }

            return longBody.ToString();
        }
    }
}
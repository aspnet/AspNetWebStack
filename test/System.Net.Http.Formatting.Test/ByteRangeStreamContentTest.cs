// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class ByteRangeStreamContentTest
    {
        private MediaTypeHeaderValue _expectedMediatype = MediaTypeHeaderValue.Parse("text/test");

        public static TheoryDataSet<string, int, string> SingleRanges
        {
            get
            {
                // string ranges, int innerStreamLength, string expectedContentRange)
                return new TheoryDataSet<string, int, string>
                {
                    { "bytes=0-9", 10, "bytes 0-9/10" },
                    { "bytes=0-0", 10, "bytes 0-0/10" },
                    { "bytes=9-9", 10, "bytes 9-9/10" },
                    { "bytes=9-20", 10, "bytes 9-9/10" },
                    { "bytes=1-", 10, "bytes 1-9/10" },
                    { "bytes=8-", 10, "bytes 8-9/10" },
                    { "bytes=9-", 10, "bytes 9-9/10" },
                    { "bytes=-1", 10, "bytes 9-9/10" },
                    { "bytes=-2", 10, "bytes 8-9/10" },
                    { "bytes=-20", 10, "bytes 0-9/10" },
                    { "bytes=-9", 10, "bytes 1-9/10" },
                    { "bytes=-10", 10, "bytes 0-9/10" },
                };
            }
        }

        public static TheoryDataSet<string, int, int, string[]> MultiRanges
        {
            get
            {
                // string ranges, int innerStreamLength, int expectedBodyparts, string[] contentRanges)
                return new TheoryDataSet<string, int, int, string[]>
                {
                    { "bytes=0-9,0-0", 10, 2, new string[] { "bytes 0-9/10", "bytes 0-0/10" } },
                    { "bytes=0-0,0-9", 10, 2, new string[] { "bytes 0-0/10", "bytes 0-9/10" } },
                    { "bytes=0-0,9-20", 10, 2, new string[] { "bytes 0-0/10", "bytes 9-9/10" } },
                    { "bytes=0-0,9-9,9-20", 10, 3, new string[] { "bytes 0-0/10", "bytes 9-9/10", "bytes 9-9/10" } },
                    { "bytes=0-0,9-9,10-20", 10, 2, new string[] { "bytes 0-0/10", "bytes 9-9/10" } },
                };
            }
        }

        public static TheoryDataSet<string, int, string> NoOverlappingRanges
        {
            get
            {
                // string ranges, int innerStreamLength, string expectedContentRange)
                return new TheoryDataSet<string, int, string>
                {
                    { "bytes=100-", 10, "bytes */10" },
                    { "bytes=100-,200-,300-", 10, "bytes */10" },
                };
            }
        }

        [Fact]
        public void Ctor_ThrowsOnNullContent()
        {
            RangeHeaderValue range = new RangeHeaderValue();
            Assert.ThrowsArgumentNull(() => new ByteRangeStreamContent(
                content: null,
                range: range,
                mediaType: _expectedMediatype,
                bufferSize: 128),
                "content");
        }

        [Fact]
        public void Ctor_ThrowsIfCantSeekContent()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(false);
            RangeHeaderValue range = new RangeHeaderValue();

            // Act/Assert
            Assert.ThrowsArgument(() => new ByteRangeStreamContent(
                content: mockInnerStream.Object,
                range: range,
                mediaType: _expectedMediatype,
                bufferSize: 128),
                "content");
        }

        [Fact]
        public void Ctor_ThrowsOnNullRange()
        {
            Assert.ThrowsArgumentNull(() => new ByteRangeStreamContent(
                content: Stream.Null,
                range: null,
                mediaType: _expectedMediatype,
                bufferSize: 128),
                "range");
        }

        [Fact]
        public void Ctor_ThrowsOnNullMediaType()
        {
            RangeHeaderValue range = new RangeHeaderValue();
            Assert.ThrowsArgumentNull(() => new ByteRangeStreamContent(
                content: Stream.Null,
                range: range,
                mediaType: (MediaTypeHeaderValue)null,
                bufferSize: 128),
                "mediaType");
        }

        [Fact]
        public void Ctor_ThrowsOnNullMediaTypeString()
        {
            RangeHeaderValue range = new RangeHeaderValue();
            Assert.ThrowsArgument(() => new ByteRangeStreamContent(
                content: Stream.Null,
                range: range,
                mediaType: (String)null,
                bufferSize: 128),
                "mediaType");
        }

        [Fact]
        public void Ctor_ThrowsOnInvalidBufferSize()
        {
            RangeHeaderValue range = new RangeHeaderValue();
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new ByteRangeStreamContent(
                content: Stream.Null,
                range: range,
                mediaType: _expectedMediatype,
                bufferSize: 0),
                "bufferSize", "1", "0");
        }

        [Fact]
        public void Ctor_ThrowsOnNonByteRangeUnit()
        {
            RangeHeaderValue range = RangeHeaderValue.Parse("pages=0-9");
            Assert.ThrowsArgument(() => new ByteRangeStreamContent(
                content: Stream.Null,
                range: range,
                mediaType: _expectedMediatype,
                bufferSize: 128),
                "range");
        }

        [Fact]
        public void Ctor_ThrowsOnNoByteRanges()
        {
            RangeHeaderValue range = new RangeHeaderValue() { Unit = "bytes" };
            Assert.ThrowsArgument(() => new ByteRangeStreamContent(
                content: Stream.Null,
                range: range,
                mediaType: _expectedMediatype,
                bufferSize: 128),
                "range");
        }

        [Theory]
        [InlineData("bytes=-1")]
        [InlineData("bytes=0-")]
        [InlineData("bytes=0-,10-20,9-9")]
        public void RangesOverZeroLengthStream(string ranges)
        {
            // Arrange
            RangeHeaderValue range = RangeHeaderValue.Parse(ranges);

            // Act
            try
            {
                new ByteRangeStreamContent(Stream.Null, range, _expectedMediatype);
            }
            catch (InvalidByteRangeException invalidByteRangeException)
            {
                ContentRangeHeaderValue expectedContentRange = new ContentRangeHeaderValue(length: 0);
                Assert.Equal(expectedContentRange, invalidByteRangeException.ContentRange);
            }
        }

        [Theory]
        [PropertyData("SingleRanges")]
        public void SingleRangeGeneratesNonMultipartContent(string ranges, int innerStreamLength, string contentRange)
        {
            // Arrange
            string data = new String('a', innerStreamLength);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            MemoryStream memStream = new MemoryStream(bytes);
            RangeHeaderValue range = RangeHeaderValue.Parse(ranges);

            // Act
            ByteRangeStreamContent rangeContent = new ByteRangeStreamContent(memStream, range, _expectedMediatype);

            // Assert
            Assert.Equal(_expectedMediatype, rangeContent.Headers.ContentType);
            ContentRangeHeaderValue expectedContentRange = ContentRangeHeaderValue.Parse(contentRange);
            Assert.Equal(expectedContentRange, rangeContent.Headers.ContentRange);
        }

        [Theory]
        [PropertyData("MultiRanges")]
        public async Task MultipleRangesGeneratesMultipartByteRangesContent(string ranges, int innerStreamLength, int expectedBodyparts, string[] contentRanges)
        {
            // Arrange
            string data = new String('a', innerStreamLength);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            MemoryStream memStream = new MemoryStream(bytes);
            RangeHeaderValue range = RangeHeaderValue.Parse(ranges);

            // Act
            ByteRangeStreamContent content = new ByteRangeStreamContent(memStream, range, _expectedMediatype);
            MemoryStream result = new MemoryStream();
            await content.CopyToAsync(result);
            MultipartMemoryStreamProvider multipart = await content.ReadAsMultipartAsync();

            // Assert
            Assert.Equal(expectedBodyparts, multipart.Contents.Count);
            for (int count = 0; count < multipart.Contents.Count; count++)
            {
                MediaTypeHeaderValue contentType = multipart.Contents[count].Headers.ContentType;
                Assert.Equal(_expectedMediatype, contentType);

                ContentRangeHeaderValue expectedContentRange = ContentRangeHeaderValue.Parse(contentRanges[count]);
                ContentRangeHeaderValue contentRange = multipart.Contents[count].Headers.ContentRange;
                Assert.Equal(expectedContentRange, contentRange);
            }
        }

        [Theory]
        [PropertyData("NoOverlappingRanges")]
        public void NoOverlappingRangesThrowException(string ranges, int innerStreamLength, string contentRange)
        {
            // Arrange
            string data = new String('a', innerStreamLength);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            MemoryStream memStream = new MemoryStream(bytes);
            RangeHeaderValue range = RangeHeaderValue.Parse(ranges);

            // Act
            try
            {
                new ByteRangeStreamContent(memStream, range, _expectedMediatype);
            }
            catch (InvalidByteRangeException invalidByteRangeException)
            {
                ContentRangeHeaderValue expectedContentRange = ContentRangeHeaderValue.Parse(contentRange);
                Assert.Equal(expectedContentRange, invalidByteRangeException.ContentRange);
            }
        }
    }
}

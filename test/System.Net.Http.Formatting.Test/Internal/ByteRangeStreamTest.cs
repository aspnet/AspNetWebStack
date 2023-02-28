// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Internal
{
    public class ByteRangeStreamTest
    {
        // from, to, expectedText
        public static TheoryDataSet<long?, long?, string> CopyBoundsData
        {
            get
            {
                return new TheoryDataSet<long?, long?, string>
                {
                    { null, 23, "This is the whole text." },
                    { 0, null, "This is the whole text." },
                    { 0, 22, "This is the whole text." },
                    { 0, 3, "This" },
                    { 12, 16, "whole" },
                    { null, 5, "text." },
                    { 18, null, "text." },
                    { 18, 22, "text." },
                };
            }
        }

        // from, to, innerLength, effectiveLength
        public static TheoryDataSet<int, int, int, int> ReadBoundsData
        {
            get
            {
                return new TheoryDataSet<int, int, int, int>
                {
                    { 0, 9, 20, 10 },
                    { 8, 8, 10, 1 },
                    { 0, 19, 20, 20 },
                    { 0, 29, 40, 30 },
                    { 0, 29, 20, 20 },
                    { 19, 29, 20, 1 },
                };
            }
        }

        // from, to, innerLength, effectiveLength for reads limited by byte[] size.
        public static TheoryDataSet<int, int, int, int> ReadBoundsDataWithLimit
        {
            get
            {
                return new TheoryDataSet<int, int, int, int>
                {
                    { 0, 9, 20, 10 },
                    { 8, 8, 10, 1 },
                    { 0, 19, 20, 20 },
                    { 0, 29, 40, 25 },
                    { 0, 29, 20, 20 },
                    { 19, 29, 20, 1 },
                };
            }
        }

        [Fact]
        public void Ctor_ThrowsOnNullInnerStream()
        {
            var range = new RangeItemHeaderValue(0, 10);
            Assert.ThrowsArgumentNull(() => new ByteRangeStream(innerStream: null, range: range), "innerStream");
        }

        [Fact]
        public void Ctor_ThrowsOnNullRange()
        {
            Assert.ThrowsArgumentNull(() => new ByteRangeStream(innerStream: Stream.Null, range: null), "range");
        }

        [Fact]
        public void Ctor_ThrowsIfCantSeekInnerStream()
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(false);
            var range = new RangeItemHeaderValue(0, 10);

            // Act/Assert
            Assert.ThrowsArgument(() => new ByteRangeStream(mockInnerStream.Object, range), "innerStream");
        }

        [Fact]
        public void Ctor_ThrowsIfLowerRangeExceedsInnerStream()
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(5);
            var range = new RangeItemHeaderValue(10, 20);

            // Act/Assert
            Assert.ThrowsArgumentOutOfRange(() => new ByteRangeStream(mockInnerStream.Object, range), "range",
                "The 'From' value of the range must be less than or equal to 5.", false, 10);
        }

        [Fact]
        public void Ctor_SetsContentRange()
        {
            // Arrange
            var expectedContentRange = new ContentRangeHeaderValue(5, 9, 20);
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(20);
            var range = new RangeItemHeaderValue(5, 9);

            // Act
            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Assert
                Assert.Equal(expectedContentRange, rangeStream.ContentRange);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Ctor_ThrowsIfInnerStreamLengthIsLessThanOne(int innerLength)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            var range = new RangeItemHeaderValue(null, 0);

            // Act/Assert
            Assert.ThrowsArgumentOutOfRange(
                () => new ByteRangeStream(mockInnerStream.Object, range),
                "innerStream",
                "The stream over which 'ByteRangeStream' provides a range view must have a length greater than or " +
                "equal to 1.",
                false,
                innerLength);
        }

        [Theory]
        [PropertyData("ReadBoundsData")]
        public void Ctor_SetsLength(int from, int to, int innerLength, int expectedLength)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            var range = new RangeItemHeaderValue(from, to);

            // Act
            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Assert
                Assert.Equal(expectedLength, rangeStream.Length);
            }
        }

        [Theory]
        [PropertyData("CopyBoundsData")]
        public async Task CopyTo_ReadsSpecifiedRange(long? from, long? to, string expectedText)
        {
            // Arrange
            var originalText = "This is the whole text.";
            var range = new RangeItemHeaderValue(from, to);

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            using (var targetStream = new MemoryStream())
            using (var reader = new StreamReader(targetStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                // Act
                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    rangeStream.CopyTo(targetStream);
                }

                // Assert
                targetStream.Position = 0L;
                var text = await reader.ReadToEndAsync();
                Assert.Equal(expectedText, text);
            }
        }

        [Theory]
        [PropertyData("CopyBoundsData")]
        public async Task CopyToAsync_ReadsSpecifiedRange(long? from, long? to, string expectedText)
        {
            // Arrange
            var originalText = "This is the whole text.";
            var range = new RangeItemHeaderValue(from, to);

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            using (var targetStream = new MemoryStream())
            using (var reader = new StreamReader(targetStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                // Act
                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    await rangeStream.CopyToAsync(targetStream);
                }

                // Assert
                targetStream.Position = 0L;
                var text = await reader.ReadToEndAsync();
                Assert.Equal(expectedText, text);
            }
        }

        [Fact]
        public void Position_ThrowsOnNegativeValue()
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(10L);
            var range = new RangeItemHeaderValue(0, 25L);

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act & Assert
                Assert.Throws<ArgumentOutOfRangeException>(() => rangeStream.Position = -1L);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0L)]
        [InlineData(7L)]
        [InlineData(9L)]
        public void Position_ReturnsZeroInitially(long? from)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(10L);
            var range = new RangeItemHeaderValue(from, 25L);

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                var position = rangeStream.Position;

                // Assert
                Assert.Equal(0L, position);
            }
        }

        [Fact]
        public void Position_CanBeSetAfterLength()
        {
            // Arrange
            var expectedPosition = 300L;
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(10L);
            var range = new RangeItemHeaderValue(0L, 10L);

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                rangeStream.Position = expectedPosition;

                // Assert
                Assert.Equal(expectedPosition, rangeStream.Position);
            }
        }

        [Fact]
        public async Task Position_PositionsNextRead()
        {
            // Arrange
            var originalText = "890123456789";
            var range = new RangeItemHeaderValue(2L, null);

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    // Act
                    rangeStream.Position = 5L;

                    // Assert
                    var read = rangeStream.ReadByte();
                    Assert.Equal('5', (char)read);
                }
            }
        }

#if !Testing_NetStandard1_3 // BeginX and EndX are not supported on Streams in netstandard1.3
        [Theory]
        [PropertyData("ReadBoundsDataWithLimit")]
        public void BeginRead_ReadsEffectiveLengthBytes(int from, int to, int innerLength, int effectiveLength)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            var range = new RangeItemHeaderValue(from, to);
            var data = new byte[25];
            var offset = 5;
            var callback = new AsyncCallback(_ => { });
            var userState = new object();

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                var result = rangeStream.BeginRead(data, offset, data.Length, callback, userState);
                rangeStream.EndRead(result);

                // Assert
                mockInnerStream.Verify(
                    s => s.BeginRead(data, offset, effectiveLength, callback, userState),
                    Times.Once());
                Assert.Equal(effectiveLength, rangeStream.Position);
            }
        }
#endif

        [Fact]
        public async Task BeginRead_CanReadAfterLength()
        {
            // Arrange
            var originalText = "This is the whole text.";
            var range = new RangeItemHeaderValue(0L, null);
            var data = new byte[25];
            var callback = new AsyncCallback(_ => { });
            var userState = new object();

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    rangeStream.Position = 50L;

                    // Act
                    var result = rangeStream.BeginRead(data, 0, data.Length, callback, userState);
                    var read = rangeStream.EndRead(result);

                    // Assert
                    Assert.Equal(0, read);
                }
            }
        }

        [Theory]
        [PropertyData("ReadBoundsDataWithLimit")]
        public void Read_ReadsEffectiveLengthBytes(int from, int to, int innerLength, int effectiveLength)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            var range = new RangeItemHeaderValue(from, to);
            var data = new byte[25];
            var offset = 5;

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                rangeStream.Read(data, offset, data.Length);

                // Assert
                mockInnerStream.Verify(s => s.Read(data, offset, effectiveLength), Times.Once());
                Assert.Equal(effectiveLength, rangeStream.Position);
            }
        }

        [Fact]
        public async Task Read_CanReadAfterLength()
        {
            // Arrange
            var originalText = "This is the whole text.";
            var range = new RangeItemHeaderValue(0L, null);
            var data = new byte[25];

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    rangeStream.Position = 50L;

                    // Act
                    var read = rangeStream.Read(data, 0, data.Length);

                    // Assert
                    Assert.Equal(0, read);
                }
            }
        }

        [Theory]
        [PropertyData("ReadBoundsDataWithLimit")]
        public async Task ReadAsync_ReadsEffectiveLengthBytes(int from, int to, int innerLength, int effectiveLength)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            var range = new RangeItemHeaderValue(from, to);
            var data = new byte[25];
            var offset = 5;

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                await rangeStream.ReadAsync(data, offset, data.Length);

                // Assert
                mockInnerStream.Verify(
                    s => s.ReadAsync(data, offset, effectiveLength, CancellationToken.None),
                    Times.Once());
                Assert.Equal(effectiveLength, rangeStream.Position);
            }
        }

        [Fact]
        public async Task ReadAsync_CanReadAfterLength()
        {
            // Arrange
            var originalText = "This is the whole text.";
            var range = new RangeItemHeaderValue(0L, null);
            var data = new byte[25];

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    rangeStream.Position = 50L;

                    // Act
                    var read = await rangeStream.ReadAsync(data, 0, data.Length);

                    // Assert
                    Assert.Equal(0, read);
                }
            }
        }

        [Theory]
        [PropertyData("ReadBoundsData")]
        public void ReadByte_ReadsEffectiveLengthTimes(int from, int to, int innerLength, int effectiveLength)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            var range = new RangeItemHeaderValue(from, to);
            var counter = 0;

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                while (rangeStream.ReadByte() != -1)
                {
                    counter++;
                }

                // Assert
                mockInnerStream.Verify(s => s.ReadByte(), Times.Exactly(effectiveLength));
                Assert.Equal(effectiveLength, counter);
                Assert.Equal(effectiveLength, rangeStream.Position);
            }
        }

        [Fact]
        public async Task ReadByte_CanReadAfterLength()
        {
            // Arrange
            var originalText = "This is the whole text.";
            var range = new RangeItemHeaderValue(0L, null);

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    rangeStream.Position = 50L;

                    // Act
                    var read = rangeStream.ReadByte();

                    // Assert
                    Assert.Equal(-1, read);
                }
            }
        }

        [Theory]
        [InlineData(-1, SeekOrigin.Begin)]
        [InlineData(-1, SeekOrigin.Current)]
        [InlineData(-11, SeekOrigin.End)]
        public void Seek_ThrowsIfBeforeOrigin(int offset, SeekOrigin origin)
        {
            // Arrange
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(10L);
            var range = new RangeItemHeaderValue(0, 25L);

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act & Assert
                Assert.Throws<IOException>(() => rangeStream.Seek(offset, origin));
            }
        }

        [Theory]
        [InlineData(25, SeekOrigin.Begin)]
        [InlineData(25, SeekOrigin.Current)]
        [InlineData(15, SeekOrigin.End)]
        public void Seek_CanMoveAfterLength(int offset, SeekOrigin origin)
        {
            // Arrange
            var expectedPosition = 25L;
            var mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(10L);
            var range = new RangeItemHeaderValue(0L, 10L);

            using (var rangeStream = new ByteRangeStream(mockInnerStream.Object, range))
            {
                // Act
                var newPosition = rangeStream.Seek(offset, origin);

                // Assert
                Assert.Equal(expectedPosition, newPosition);
                Assert.Equal(expectedPosition, rangeStream.Position);
            }
        }

        [Theory]
        [InlineData(5, SeekOrigin.Begin)]
        [InlineData(5, SeekOrigin.Current)]
        [InlineData(-5, SeekOrigin.End)]
        public async Task Seek_PositionsNextRead(int offset, SeekOrigin origin)
        {
            // Arrange
            var originalText = "890123456789";
            var range = new RangeItemHeaderValue(2L, null);

            using (var innerStream = new MemoryStream())
            using (var writer = new StreamWriter(innerStream))
            {
                await writer.WriteAsync(originalText);
                await writer.FlushAsync();

                using (var rangeStream = new ByteRangeStream(innerStream, range))
                {
                    // Act
                    rangeStream.Seek(offset, origin);

                    // Assert
                    var read = rangeStream.ReadByte();
                    Assert.Equal('5', (char)read);
                }
            }
        }
    }
}

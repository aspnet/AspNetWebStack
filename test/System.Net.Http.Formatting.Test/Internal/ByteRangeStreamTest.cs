// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Internal
{
    public class ByteRangeStreamTest
    {
        [Fact]
        public void Ctor_ThrowsOnNullInnerStream()
        {
            RangeItemHeaderValue range = new RangeItemHeaderValue(0, 10);
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
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(false);
            RangeItemHeaderValue range = new RangeItemHeaderValue(0, 10);

            // Act/Assert
            Assert.ThrowsArgument(() => new ByteRangeStream(mockInnerStream.Object, range), "innerStream");
        }

        [Fact]
        public void Ctor_ThrowsIfLowerRangeExceedsInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(5);
            RangeItemHeaderValue range = new RangeItemHeaderValue(10, 20);

            // Act/Assert
            Assert.ThrowsArgumentOutOfRange(() => new ByteRangeStream(mockInnerStream.Object, range), "range",
                "The 'From' value of the range must be less than or equal to 5.", false, 10);
        }

        [Fact]
        public void Ctor_SetsContentRange()
        {
            // Arrange
            ContentRangeHeaderValue expectedContentRange = new ContentRangeHeaderValue(5, 9, 20);
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(20);
            RangeItemHeaderValue range = new RangeItemHeaderValue(5, 9);

            // Act
            ByteRangeStream rangeStream = new ByteRangeStream(mockInnerStream.Object, range);

            // Assert
            Assert.Equal(expectedContentRange, rangeStream.ContentRange);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Ctor_ThrowsIfInnerStreamLengthIsLessThanOne(int innerLength)
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            RangeItemHeaderValue range = new RangeItemHeaderValue(null, 0);

            // Act/Assert
            Assert.ThrowsArgumentOutOfRange(() => new ByteRangeStream(mockInnerStream.Object, range), "innerStream",
                "The stream over which 'ByteRangeStream' provides a range view must have a length greater than or equal to 1.",
                false, innerLength);
        }

        [Theory]
        [InlineData(0, 9, 20, 10)]
        [InlineData(8, 8, 10, 1)]
        [InlineData(0, 19, 20, 20)]
        public void Ctor_SetsLength(int from, int to, int innerLength, int expectedLength)
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            RangeItemHeaderValue range = new RangeItemHeaderValue(from, to);

            // Act
            ByteRangeStream rangeStream = new ByteRangeStream(mockInnerStream.Object, range);

            // Assert
            Assert.Equal(expectedLength, rangeStream.Length);
        }

        [Theory]
        [InlineData(0, 9, 20, 10)]
        [InlineData(8, 8, 10, 1)]
        [InlineData(0, 19, 20, 20)]
        [InlineData(0, 29, 40, 25)]
        [InlineData(0, 29, 20, 20)]
        [InlineData(19, 29, 20, 1)]
        public void Read_ReadsEffectiveLengthBytes(int from, int to, int innerLength, int effectiveLength)
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            RangeItemHeaderValue range = new RangeItemHeaderValue(from, to);
            byte[] data = new byte[25];
            int offset = 5;

            // Act
            ByteRangeStream rangeStream = new ByteRangeStream(mockInnerStream.Object, range);
            rangeStream.Read(data, offset, data.Length);

            // Assert
            mockInnerStream.Verify(s => s.Read(data, offset, effectiveLength), Times.Once());
        }

        [Theory]
        [InlineData(0, 9, 20, 10)]
        [InlineData(8, 8, 10, 1)]
        [InlineData(0, 19, 20, 20)]
        [InlineData(0, 29, 40, 30)]
        [InlineData(0, 29, 20, 20)]
        [InlineData(19, 29, 20, 1)]
        public void ReadByte_ReadsEffectiveLengthTimes(int from, int to, int innerLength, int effectiveLength)
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>();
            mockInnerStream.Setup(s => s.CanSeek).Returns(true);
            mockInnerStream.Setup(s => s.Length).Returns(innerLength);
            RangeItemHeaderValue range = new RangeItemHeaderValue(from, to);

            // Act
            ByteRangeStream rangeStream = new ByteRangeStream(mockInnerStream.Object, range);
            int counter = 0;
            while (rangeStream.ReadByte() != -1)
            {
                counter++;
            }

            // Assert
            Assert.Equal(effectiveLength, counter);
            mockInnerStream.Verify(s => s.ReadByte(), Times.Exactly(effectiveLength));
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class BinaryBlobTest
    {
        [Fact]
        public void Ctor_BitLength()
        {
            // Act
            BinaryBlob blob = new BinaryBlob(bitLength: 64);
            byte[] data = blob.GetData();

            // Assert
            Assert.Equal(64, blob.BitLength);
            Assert.Equal(64 / 8, data.Length);
            Assert.NotEqual(new byte[64 / 8], data); // should not be a zero-filled array
        }

        [Theory]
        [InlineData(24)]
        [InlineData(33)]
        public void Ctor_BitLength_Bad(int bitLength)
        {
            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new BinaryBlob(bitLength));
            Assert.Equal("bitLength", ex.ParamName);
        }

        [Fact]
        public void Ctor_BitLength_ProducesDifferentValues()
        {
            // Act
            BinaryBlob blobA = new BinaryBlob(bitLength: 64);
            BinaryBlob blobB = new BinaryBlob(bitLength: 64);

            // Assert
            Assert.NotEqual(blobA.GetData(), blobB.GetData());
        }

        [Fact]
        public void Ctor_Data()
        {
            // Arrange
            byte[] expectedData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            // Act
            BinaryBlob blob = new BinaryBlob(32, expectedData);

            // Assert
            Assert.Equal(32, blob.BitLength);
            Assert.Equal(expectedData, blob.GetData());
        }

        [Theory]
        [InlineData((object[])null)]
        [InlineData(new byte[] { 0x01, 0x02, 0x03 })]
        public void Ctor_Data_Bad(byte[] data)
        {
            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new BinaryBlob(32, data));
            Assert.Equal("data", ex.ParamName);
        }

        [Fact]
        public void Equals_DifferentData_ReturnsFalse()
        {
            // Arrange
            object blobA = new BinaryBlob(32, new byte[] { 0x01, 0x02, 0x03, 0x04 });
            object blobB = new BinaryBlob(32, new byte[] { 0x04, 0x03, 0x02, 0x01 });

            // Act & assert
            Assert.NotEqual(blobA, blobB);
        }

        [Fact]
        public void Equals_NotABlob_ReturnsFalse()
        {
            // Arrange
            object blobA = new BinaryBlob(32);
            object blobB = "hello";

            // Act & assert
            Assert.NotEqual(blobA, blobB);
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            object blobA = new BinaryBlob(32);
            object blobB = null;

            // Act & assert
            Assert.NotEqual(blobA, blobB);
        }

        [Fact]
        public void Equals_SameData_ReturnsTrue()
        {
            // Arrange
            object blobA = new BinaryBlob(32, new byte[] { 0x01, 0x02, 0x03, 0x04 });
            object blobB = new BinaryBlob(32, new byte[] { 0x01, 0x02, 0x03, 0x04 });

            // Act & assert
            Assert.Equal(blobA, blobB);
        }

        [Fact]
        public void GetHashCodeTest()
        {
            // Arrange
            byte[] blobData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            int expectedHashCode = BitConverter.ToInt32(blobData, 0);

            BinaryBlob blob = new BinaryBlob(32, blobData);

            // Act
            int actualHashCode = blob.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }
    }
}

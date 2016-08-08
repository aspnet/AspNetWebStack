// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class CryptoUtilTest
    {
        [Theory]
        [InlineData(new byte[0], null)]
        [InlineData(null, new byte[0])]
        [InlineData(new byte[0], new byte[] { 0x00 })]
        [InlineData(new byte[] { 0x01, 0x02 }, new byte[] { 0x02, 0x01 })]
        public void AreByteArraysEqual_False(byte[] a, byte[] b)
        {
            // Act
            bool retVal = CryptoUtil.AreByteArraysEqual(a, b);

            // Assert
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void AreByteArraysEqual_True()
        {
            // Arrange
            byte[] a = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            byte[] b = (byte[])a.Clone();

            // Act
            bool retVal = CryptoUtil.AreByteArraysEqual(a, b);

            // Assert
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestVectors_Empty()
        {
            // Act
            byte[] retVal = CryptoUtil.ComputeSHA256(new string[0]);

            // Assert
            Assert.Equal("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=", Convert.ToBase64String(retVal));
        }

        [Fact]
        public void TestVectors_NonEmpty()
        {
            // Act
            byte[] retVal = CryptoUtil.ComputeSHA256(new string[] { "a parameter", "another parameter" });

            // Assert
            Assert.Equal("Bez9yYh4Zq9jK1H5jD21wh04HTZi/vgxp6yDE7Y6cfo=", Convert.ToBase64String(retVal));
        }
    }
}

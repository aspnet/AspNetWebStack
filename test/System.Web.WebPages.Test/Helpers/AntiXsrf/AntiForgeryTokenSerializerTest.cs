// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class AntiForgeryTokenSerializerTest
    {
        private static readonly AntiForgeryTokenSerializer _testSerializer = new AntiForgeryTokenSerializer(cryptoSystem: CreateIdentityTransformCryptoSystem());

        private static readonly BinaryBlob _claimUid = new BinaryBlob(256, new byte[] { 0x6F, 0x16, 0x48, 0xE9, 0x72, 0x49, 0xAA, 0x58, 0x75, 0x40, 0x36, 0xA6, 0x7E, 0x24, 0x8C, 0xF0, 0x44, 0xF0, 0x7E, 0xCF, 0xB0, 0xED, 0x38, 0x75, 0x56, 0xCE, 0x02, 0x9A, 0x4F, 0x9A, 0x40, 0xE0 });
        private static readonly BinaryBlob _securityToken = new BinaryBlob(128, new byte[] { 0x70, 0x5E, 0xED, 0xCC, 0x7D, 0x42, 0xF1, 0xD6, 0xB3, 0xB9, 0x8A, 0x59, 0x36, 0x25, 0xBB, 0x4C });

        [Theory]
        [InlineData(
            "01" // Version
            + "705EEDCC7D42F1D6B3B9" // SecurityToken
            // (WRONG!) Stream ends too early
            )]
        [InlineData(
            "01" // Version
            + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            + "01" // IsSessionToken
            + "00" // (WRONG!) Too much data in stream
            )]
        [InlineData(
            "02" // (WRONG! - must be 0x01) Version
            + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            + "01" // IsSessionToken
            )]
        [InlineData(
            "01" // Version
            + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            + "00" // IsSessionToken
            + "00" // IsClaimsBased
            + "05" // Username length header
            + "0000" // (WRONG!) Too little data in stream
            )]
        public void Deserialize_BadToken(string serializedToken)
        {
            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => _testSerializer.Deserialize(serializedToken));
            Assert.Equal(@"The anti-forgery token could not be decrypted. If this application is hosted by a Web Farm or cluster, ensure that all machines are running the same version of ASP.NET Web Pages and that the <machineKey> configuration specifies explicit encryption and validation keys. AutoGenerate cannot be used in a cluster.", ex.Message);
        }

        [Fact]
        public void Serialize_FieldToken_WithClaimUid()
        {
            // Arrange
            const string expectedSerializedData =
                "01" // Version
                + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
                + "00" // IsSessionToken
                + "01" // IsClaimsBased
                + "6F1648E97249AA58754036A67E248CF044F07ECFB0ED387556CE029A4F9A40E0" // ClaimUid
                + "05" // AdditionalData length header
                + "E282AC3437"; // AdditionalData ("€47") as UTF8

            AntiForgeryToken token = new AntiForgeryToken()
            {
                SecurityToken = _securityToken,
                IsSessionToken = false,
                ClaimUid = _claimUid,
                AdditionalData = "€47"
            };

            // Act & assert - serialization
            string actualSerializedData = _testSerializer.Serialize(token);
            Assert.Equal(expectedSerializedData, actualSerializedData);

            // Act & assert - deserialization
            AntiForgeryToken deserializedToken = _testSerializer.Deserialize(actualSerializedData);
            AssertTokensEqual(token, deserializedToken);
        }

        [Fact]
        public void Serialize_FieldToken_WithUsername()
        {
            // Arrange
            const string expectedSerializedData =
                "01" // Version
                + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
                + "00" // IsSessionToken
                + "00" // IsClaimsBased
                + "08" // Username length header
                + "4AC3A972C3B46D65" // Username ("Jérôme") as UTF8
                + "05" // AdditionalData length header
                + "E282AC3437"; // AdditionalData ("€47") as UTF8

            AntiForgeryToken token = new AntiForgeryToken()
            {
                SecurityToken = _securityToken,
                IsSessionToken = false,
                Username = "Jérôme",
                AdditionalData = "€47"
            };

            // Act & assert - serialization
            string actualSerializedData = _testSerializer.Serialize(token);
            Assert.Equal(expectedSerializedData, actualSerializedData);

            // Act & assert - deserialization
            AntiForgeryToken deserializedToken = _testSerializer.Deserialize(actualSerializedData);
            AssertTokensEqual(token, deserializedToken);
        }

        [Fact]
        public void Serialize_SessionToken()
        {
            // Arrange
            const string expectedSerializedData =
                "01" // Version
                + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
                + "01"; // IsSessionToken

            AntiForgeryToken token = new AntiForgeryToken()
            {
                SecurityToken = _securityToken,
                IsSessionToken = true
            };

            // Act & assert - serialization
            string actualSerializedData = _testSerializer.Serialize(token);
            Assert.Equal(expectedSerializedData, actualSerializedData);

            // Act & assert - deserialization
            AntiForgeryToken deserializedToken = _testSerializer.Deserialize(actualSerializedData);
            AssertTokensEqual(token, deserializedToken);
        }

        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", b);
            }
            return sb.ToString();
        }

        private static byte[] HexToBytes(string hex)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < hex.Length; i += 2)
            {
                byte b = Byte.Parse(hex.Substring(i, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                bytes.Add(b);
            }
            return bytes.ToArray();
        }

        private static ICryptoSystem CreateIdentityTransformCryptoSystem()
        {
            Mock<MockableCryptoSystem> mockCryptoSystem = new Mock<MockableCryptoSystem>();
            mockCryptoSystem.Setup(o => o.Protect(It.IsAny<byte[]>())).Returns<byte[]>(HexUtil.HexEncode);
            mockCryptoSystem.Setup(o => o.Unprotect(It.IsAny<string>())).Returns<string>(HexUtil.HexDecode);
            return mockCryptoSystem.Object;
        }

        private static void AssertTokensEqual(AntiForgeryToken expected, AntiForgeryToken actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            Assert.Equal(expected.AdditionalData, actual.AdditionalData);
            Assert.Equal(expected.ClaimUid, actual.ClaimUid);
            Assert.Equal(expected.IsSessionToken, actual.IsSessionToken);
            Assert.Equal(expected.SecurityToken, actual.SecurityToken);
            Assert.Equal(expected.Username, actual.Username);
        }
    }
}

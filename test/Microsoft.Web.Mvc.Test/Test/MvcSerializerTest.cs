// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using System.Web.Security;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class MvcSerializerTest
    {
        [Fact]
        public void DeserializeThrowsIfSerializedValueIsCorrupt()
        {
            // Arrange
            IMachineKey machineKey = new MockMachineKey();

            // Act & assert
            Exception exception = Assert.Throws<SerializationException>(
                delegate { MvcSerializer.Deserialize("This is a corrupted value.", machineKey); },
                @"Deserialization failed. Verify that the data is being deserialized using the same SerializationMode with which it was serialized. Otherwise see the inner exception.");

            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void DeserializeThrowsIfSerializedValueIsEmpty()
        {
            // Arrange
            MvcSerializer serializer = new MvcSerializer();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { serializer.Deserialize(""); }, "serializedValue");
        }

        [Fact]
        public void DeserializeThrowsIfSerializedValueIsNull()
        {
            // Arrange
            MvcSerializer serializer = new MvcSerializer();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { serializer.Deserialize(null); }, "serializedValue");
        }

        [Fact]
        public void SerializeAllowsNullValues()
        {
            // Arrange
            IMachineKey machineKey = new MockMachineKey();

            // Act
            string serializedValue = MvcSerializer.Serialize(null, machineKey);

            // Assert
            Assert.Equal(@"Microsoft.Web.Mvc.MvcSerializer.v1-dwdhbnlUeXBlLgNuaWyGCQF6M2h0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vMjAwMy8xMC9TZXJpYWxpemF0aW9uLwkBaSlodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZQE=", serializedValue);
        }

        [Fact]
        public void SerializeAndDeserializeRoundTripsValue()
        {
            // Arrange
            IMachineKey machineKey = new MockMachineKey();

            // Act
            string serializedValue = MvcSerializer.Serialize(42, machineKey);
            object deserializedValue = MvcSerializer.Deserialize(serializedValue, machineKey);

            // Assert
            Assert.Equal(42, deserializedValue);
        }

        private sealed class MockMachineKey : IMachineKey
        {
            public byte[] Unprotect(string protectedData, params string[] purposes)
            {
                string optionString = purposes[0].ToString();
                if (protectedData.StartsWith(optionString, StringComparison.Ordinal))
                {
                    protectedData = protectedData.Substring(optionString.Length + 1);
                }
                else
                {
                    throw new Exception("Corrupted data.");
                }
                return Convert.FromBase64String(protectedData);

            }

            public string Protect(byte[] userData, params string[] purposes)
            {
                return purposes[0].ToString() + "-" + Convert.ToBase64String(userData);
            }
        }
    }
}

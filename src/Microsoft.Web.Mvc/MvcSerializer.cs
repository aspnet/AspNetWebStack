// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Security;
using System.Xml;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    public class MvcSerializer
    {
        private static readonly string[] _machineKeyPurposes = new string[] { "Microsoft.Web.Mvc.MvcSerializer.v1" };

        private static SerializationException CreateSerializationException(Exception innerException)
        {
            return new SerializationException(MvcResources.MvcSerializer_DeserializationFailed, innerException);
        }

        public virtual object Deserialize(string serializedValue)
        {
            return Deserialize(serializedValue, MachineKeyWrapper.Instance);
        }

        internal static object Deserialize(string serializedValue, IMachineKey machineKey)
        {
            if (String.IsNullOrEmpty(serializedValue))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "serializedValue");
            }

            try
            {
                // First, need to decrypt / verify data
                byte[] rawBytes = machineKey.Unprotect(serializedValue, _machineKeyPurposes);

                // Finally, deserialize the object graph
                using (MemoryStream ms = new MemoryStream(rawBytes, 0, rawBytes.Length))
                {
                    return DeserializeGraph(ms);
                }
            }
            catch (Exception ex)
            {
                throw CreateSerializationException(ex);
            }
        }

        // Deserializes a stream to a graph using the NetDataContractSerializer (binary mode)
        private static object DeserializeGraph(Stream rawBytes)
        {
            using (XmlDictionaryReader dr = XmlDictionaryReader.CreateBinaryReader(rawBytes, XmlDictionaryReaderQuotas.Max))
            {
                object deserialized = new NetDataContractSerializer().ReadObject(dr);
                return deserialized;
            }
        }

        public virtual string Serialize(object state)
        {
            return Serialize(state, MachineKeyWrapper.Instance);
        }

        internal static string Serialize(object state, IMachineKey machineKey)
        {
            try
            {
                // First, need to serialize the object graph
                byte[] rawBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    SerializeGraph(ms, state);
                    rawBytes = ms.ToArray();
                }

                // Then, encrypt / sign data
                return machineKey.Protect(rawBytes, _machineKeyPurposes);
            }
            catch (Exception ex)
            {
                throw CreateSerializationException(ex);
            }
        }

        // Serializes a graph to a byte array using the NetDataContractSerializer (binary mode)
        private static void SerializeGraph(Stream outputStream, object graph)
        {
            using (XmlDictionaryWriter dw = XmlDictionaryWriter.CreateBinaryWriter(outputStream, null, null, ownsStream: false))
            {
                new NetDataContractSerializer().WriteObject(dw, graph);
            }
        }
    }
}

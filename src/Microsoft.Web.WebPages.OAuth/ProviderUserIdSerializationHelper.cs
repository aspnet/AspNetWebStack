// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web.Security;

namespace Microsoft.Web.WebPages.OAuth
{
    internal static class ProviderUserIdSerializationHelper
    {
        // Custom message purpose to prevent this data from being readable by a different subsystem.
        private static byte[] _padding = new byte[] { 0x85, 0xC5, 0x65, 0x72 };

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The instances are disposed correctly.")]
        public static string ProtectData(string providerName, string providerUserId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(providerName);
                bw.Write(providerUserId);
                bw.Flush();
                byte[] serializedWithPadding = new byte[ms.Length + _padding.Length];
                Buffer.BlockCopy(_padding, 0, serializedWithPadding, 0, _padding.Length);
                Buffer.BlockCopy(ms.GetBuffer(), 0, serializedWithPadding, _padding.Length, (int)ms.Length);
#pragma warning disable 0618 // Encode is [Obsolete] in 4.5
                return MachineKey.Encode(serializedWithPadding, MachineKeyProtection.All);
#pragma warning restore 0618
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The instances are disposed correctly.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exception are being caught on purpose.")]
        public static bool UnprotectData(string protectedData, out string providerName, out string providerUserId)
        {
            providerName = null;
            providerUserId = null;
            if (String.IsNullOrEmpty(protectedData))
            {
                return false;
            }
#pragma warning disable 0618 // Decode is [Obsolete] in 4.5
            byte[] decodedWithPadding = MachineKey.Decode(protectedData, MachineKeyProtection.All);
#pragma warning restore 0618
            if (decodedWithPadding.Length < _padding.Length)
            {
                return false;
            }

            // timing attacks aren't really applicable to this, so we just do the simple check.
            for (int i = 0; i < _padding.Length; i++)
            {
                if (_padding[i] != decodedWithPadding[i])
                {
                    return false;
                }
            }

            using (MemoryStream ms = new MemoryStream(decodedWithPadding, _padding.Length, decodedWithPadding.Length - _padding.Length))
            using (BinaryReader br = new BinaryReader(ms))
            {
                try
                {
                    // use temp variable to keep both out parameters consistent and only set them when the input stream is read completely
                    string name = br.ReadString();
                    string userId = br.ReadString();

                    // make sure that we consume the entire input stream
                    if (ms.ReadByte() == -1)
                    {
                        providerName = name;
                        providerUserId = userId;
                        return true;
                    }
                }
                catch
                {
                    // Any exceptions will result in this method returning false.
                }
            }
            return false;
        }
    }
}
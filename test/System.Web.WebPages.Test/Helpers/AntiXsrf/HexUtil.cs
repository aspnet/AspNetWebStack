// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Web.Helpers.AntiXsrf.Test
{
    internal static class HexUtil
    {
        public static string HexEncode(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", b);
            }
            return sb.ToString();
        }

        public static byte[] HexDecode(string input)
        {
            List<byte> bytes = new List<byte>(input.Length / 2);
            for (int i = 0; i < input.Length; i += 2)
            {
                bytes.Add(Byte.Parse(input.Substring(i, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
            }
            return bytes.ToArray();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;

namespace System.Web.WebPages
{
    internal static class StringWriterExtensions
    {
        public const int BufferSize = 1024;

        // Used to copy data from a string writer to avoid allocating the full string
        // which can end up on LOH (and cause memory fragmentation).
        public static void CopyTo(this StringWriter input, TextWriter output)
        {
            StringBuilder builder = input.GetStringBuilder();

            int remainingChars = builder.Length;
            int bufferSize = Math.Min(builder.Length, BufferSize);

            char[] buffer = new char[bufferSize];
            int currentPosition = 0;

            while (remainingChars > 0)
            {
                int copyLen = Math.Min(bufferSize, remainingChars);

                builder.CopyTo(currentPosition, buffer, 0, copyLen);

                output.Write(buffer, 0, copyLen);

                currentPosition += copyLen;
                remainingChars -= copyLen;
            }
        }
    }
}

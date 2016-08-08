// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Internal
{
    public class ReadOnlyStreamWithEncodingPreambleTest
    {
        [Theory]
        [EncodingData]
        public void StreamWithoutPreamble(Encoding encoding, bool includePreambleInInputStream)
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                // Arrange
                string message = "Hello, world" + Environment.NewLine     // English
                               + "こんにちは、世界" + Environment.NewLine  // Japanese
                               + "مرحبا، العالم";                       // Arabic

                byte[] preamble = encoding.GetPreamble();
                byte[] encodedMessage = encoding.GetBytes(message);

                if (includePreambleInInputStream)
                {
                    inputStream.Write(preamble, 0, preamble.Length);
                }

                inputStream.Write(encodedMessage, 0, encodedMessage.Length);

                byte[] expectedBytes = new byte[preamble.Length + encodedMessage.Length];
                preamble.CopyTo(expectedBytes, 0);
                encodedMessage.CopyTo(expectedBytes, preamble.Length);

                inputStream.Seek(0, SeekOrigin.Begin);

                using (ReadOnlyStreamWithEncodingPreamble wrapperStream = new ReadOnlyStreamWithEncodingPreamble(inputStream, encoding))
                {
                    // Act
                    int totalRead = 0;
                    byte[] readBuffer = new byte[expectedBytes.Length];

                    while (totalRead < readBuffer.Length)
                    {
                        int read = wrapperStream.Read(readBuffer, totalRead, readBuffer.Length - totalRead);
                        totalRead += read;

                        if (read == 0)
                            break;
                    }

                    // Assert
                    Assert.Equal(expectedBytes.Length, totalRead);
                    Assert.Equal(expectedBytes, readBuffer);
                    Assert.Equal(0, wrapperStream.Read(readBuffer, 0, 1));  // Make sure there are no stray bytes left in the stream
                }
            }
        }

        class EncodingDataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                return new MatrixTheoryDataSet<Encoding, bool>(
                    new[] { Encoding.UTF7, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF32, Encoding.ASCII },
                    new[] { false, true }
                );
            }
        }
    }
}

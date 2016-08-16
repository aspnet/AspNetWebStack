// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class StringWriterExtensionsTest
    {
        [Fact]
        public void CopiesResult()
        {
            // Note that a preable is not expected on the generated stream.
            string text = "Hello world";
            Byte[] textInBytes = Encoding.UTF8.GetBytes(text);
            string outputText;

            Byte[] buffer = new Byte[1024];

            using (MemoryStream stream = new MemoryStream(buffer))
            using (StringWriter writer = new StringWriter())
            using (StreamWriter outputWriter = new StreamWriter(stream))
            {
                writer.Write(text);
                writer.CopyTo(outputWriter);

                outputText = writer.ToString();
            }

            Assert.Equal(text, outputText, StringComparer.Ordinal);

            for (int i = 0; i < textInBytes.Length; i++)
            {
                Assert.Equal(textInBytes[i], buffer[i]);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(20000)]
        [InlineData(100000)]
        public void OnlyUsesBufferUpToSize(int count)
        {
            string text = new string('a', count);
            Byte[] textInBytes = Encoding.UTF8.GetBytes(text);

            Mock<StreamWriter> mock;

            Byte[] buffer = new Byte[textInBytes.Length + 100];

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                StringWriter writer = new StringWriter();

                mock = new Mock<StreamWriter>(MockBehavior.Strict, stream) { CallBase = true };
                mock.Setup(sw => sw.Write(It.IsAny<char[]>(),
                                          It.IsAny<int>(),
                                          It.Is<int>(c => c == StringWriterExtensions.BufferSize ||
                                                          c == textInBytes.Length % StringWriterExtensions.BufferSize)))
                    .Verifiable();

                StreamWriter outputWriter = mock.Object;
                writer.Write(text);
                writer.CopyTo(outputWriter);

                mock.Verify();
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1023/7)]
        [InlineData(20000/7)]
        [InlineData(100000/7)]
        public void ProperlyCopiesLargeSetsOfText(int count)
        {
            // The char א turns into a two byte sequence so we end up with a
            // 7 byte sequence that is not a divider or 1024.
            string text = string.Join(string.Empty, Enumerable.Repeat("abcdeא", count));

            Byte[] textInBytes = Encoding.UTF8.GetBytes(text);
            string outputText;

            Byte[] buffer = new Byte[textInBytes.Length + 100];

            using (MemoryStream stream = new MemoryStream(buffer))
            using (StringWriter writer = new StringWriter())
            {
                using (StreamWriter outputWriter = new StreamWriter(stream))
                {
                    writer.Write(text);
                    writer.CopyTo(outputWriter);

                    outputText = writer.ToString();
                }
            }

            Assert.Equal(text, outputText, StringComparer.Ordinal);

            for (int i = 0; i < textInBytes.Length; i++)
            {
                Assert.Equal(textInBytes[i], buffer[i]);
            }
        }
    }
}

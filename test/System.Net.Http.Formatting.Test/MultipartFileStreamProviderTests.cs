// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http.Internal;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class MockMultipartFileStreamProvider : MultipartFileStreamProvider
    {
        public MockMultipartFileStreamProvider()
            : base(Path.GetTempPath())
        {
        }

        public MockMultipartFileStreamProvider(string rootPath)
            : base(rootPath)
        {
        }

        public MockMultipartFileStreamProvider(string rootPath, int bufferSize)
            : base(rootPath, bufferSize)
        {
        }
    }

    public class MultipartFileStreamProviderTests : MultipartStreamProviderTestBase<MockMultipartFileStreamProvider>
    {
        private const int MinBufferSize = 1;
        private const int ValidBufferSize = 0x111;
        private const string ValidPath = @"c:\some\path";

#if !NETCOREAPP // .NET Core does not enforce path validity in many APIs.
        public static TheoryDataSet<string> NotSupportedFilePaths
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "cc:\\a\\b",
                    "123:\\a\\b",
                    "c d:\\a\\b",
                };
            }
        }
#endif

        public static TheoryDataSet<string> InvalidFilePaths
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    " ",
                    "  ",
#if !NETCOREAPP // .NET Core does not enforce path validity in many APIs.
                    "\t\t \n ",
                    "c:\\a<b",
                    "c:\\a>b",
                    "c:\\a\"b",
                    "c:\\a\tb",
                    "c:\\a|b",
                    "c:\\a\bb",
#endif
                    "c:\\a\0b",
                    "c :\\a\0b",
                };
            }
        }

        [Fact]
        public void Constructor_ThrowsOnNullRootPath()
        {
            Assert.ThrowsArgumentNull(() => { new MultipartFileStreamProvider(null); }, "rootPath");
        }

#if !NETCOREAPP // .NET Core does not enforce path validity in many APIs.
        [Theory]
        [PropertyData("NotSupportedFilePaths")]
        public void Constructor_ThrowsOnNotSupportedRootPath(string notSupportedPath)
        {
            Assert.Throws<NotSupportedException>(() => new MultipartFileStreamProvider(notSupportedPath, ValidBufferSize));
        }
#endif

        [Theory]
        [PropertyData("InvalidFilePaths")]
        public void Constructor_ThrowsOnInvalidRootPath(string invalidPath)
        {
            Assert.ThrowsArgument(() => new MultipartFileStreamProvider(invalidPath, ValidBufferSize), null);
        }

        [Fact]
        public void Constructor_InvalidBufferSize()
        {
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new MultipartFileStreamProvider(ValidPath, MinBufferSize - 1),
                "bufferSize", MinBufferSize.ToString(), MinBufferSize - 1);
        }

        [Fact]
        public void FileData_IsEmpty()
        {
            MultipartFileStreamProvider provider = new MultipartFileStreamProvider(ValidPath, ValidBufferSize);
            Assert.Empty(provider.FileData);
        }

        [Fact]
        public void GetStream()
        {
            Stream stream0 = null;
            Stream stream1 = null;

            try
            {
                string tempPath = Path.GetTempPath();
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent("Content 1"), "NoFile");
                content.Add(new StringContent("Content 2"), "File", "Filename");

                MultipartFileStreamProvider provider = new MultipartFileStreamProvider(tempPath);
                stream0 = provider.GetStream(content, content.ElementAt(0).Headers);
                stream1 = provider.GetStream(content, content.ElementAt(1).Headers);

                Assert.IsType<FileStream>(stream0);
                Assert.IsType<FileStream>(stream1);

                Assert.Equal(2, provider.FileData.Count);
                string partialFileName = String.Format("{0}BodyPart_", tempPath);
                Assert.Contains(partialFileName, provider.FileData[0].LocalFileName);
                Assert.Contains(partialFileName, provider.FileData[1].LocalFileName);

                Assert.Same(content.ElementAt(0).Headers.ContentDisposition, provider.FileData[0].Headers.ContentDisposition);
                Assert.Same(content.ElementAt(1).Headers.ContentDisposition, provider.FileData[1].Headers.ContentDisposition);
            }
            finally
            {
                if (stream0 != null)
                {
                    stream0.Close();
                }

                if (stream1 != null)
                {
                    stream1.Close();
                }
            }
        }
    }
}

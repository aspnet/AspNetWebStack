// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class MockMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public MockMultipartFormDataStreamProvider()
            : base(Path.GetTempPath())
        {
        }

        public MockMultipartFormDataStreamProvider(string rootPath)
            : base(rootPath)
        {
        }

        public MockMultipartFormDataStreamProvider(string rootPath, int bufferSize)
            : base(rootPath, bufferSize)
        {
        }
    }

    public class MultipartFormDataStreamProviderTests : MultipartStreamProviderTestBase<MockMultipartFormDataStreamProvider>
    {
        private const int ValidBufferSize = 0x111;
        private const string ValidPath = @"c:\some\path";

        [Fact]
        public void FormData_IsEmpty()
        {
            MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(ValidPath, ValidBufferSize);
            Assert.Empty(provider.FormData);
        }

        [Fact]
        public void GetStream_ThrowsOnNoContentDisposition()
        {
            MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(ValidPath);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            Assert.Throws<InvalidOperationException>(() => { provider.GetStream(content, headers); });
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

                MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(tempPath);
                stream0 = provider.GetStream(content, content.ElementAt(0).Headers);
                stream1 = provider.GetStream(content, content.ElementAt(1).Headers);

                Assert.IsType<MemoryStream>(stream0);
                Assert.IsType<FileStream>(stream1);

                MultipartFileData fileData = Assert.Single(provider.FileData);
                string partialFileName = String.Format("{0}BodyPart_", tempPath);
                Assert.Contains(partialFileName, fileData.LocalFileName);

                Assert.Same(content.ElementAt(1).Headers.ContentDisposition, fileData.Headers.ContentDisposition);
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

        [Fact]
        public async Task PostProcessing_ProcessesFormData()
        {
            // Arrange
            int maxContents = 16;
            string contentFormat = "Content {0}";
            string formNameFormat = "FormName_{0}";

            MultipartFormDataContent multipartContent = new MultipartFormDataContent();

            for (int index = 0; index < maxContents; index++)
            {
                string content = String.Format(contentFormat, index);
                string formName = String.Format(formNameFormat, index);
                multipartContent.Add(new StringContent(content), formName);
            }

            MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(ValidPath);
            foreach (HttpContent content in multipartContent)
            {
                provider.Contents.Add(content);
                provider.GetStream(multipartContent, content.Headers);
            }

            // Act
            await provider.ExecutePostProcessingAsync();

            // Assert
            Assert.Equal(maxContents, provider.FormData.Count);

            for (int index = 0; index < maxContents; index++)
            {
                string content = String.Format(contentFormat, index);
                string formName = String.Format(formNameFormat, index);
                Assert.Equal(content, provider.FormData[formName]);
            }
        }

        [Fact]
        public async Task PostProcessing_ProcessesFormData_WithCustomMultipartFormDataStreamProvider()
        {
            // Arrange
            string tempPath = Path.GetTempPath();
            int maxContents = 16;
            string contentFormat = "Content {0}";
            string formNameFormat = "FormName_{0}";
            string fileNameFormat = "FileName_{0}";

            MultipartFormDataContent multipartContent = new MultipartFormDataContent();

            // Create half contents for form data and the other half for file data.
            for (int index = 0; index < maxContents; index++)
            {
                string content = String.Format(contentFormat, index);
                string formName = String.Format(formNameFormat, index);
                if (index < maxContents/2)
                {
                    multipartContent.Add(new StringContent(content), formName);
                }
                else
                {
                    string fileName = String.Format(fileNameFormat, index);
                    multipartContent.Add(new StringContent(content), formName, fileName);
                }
            }

            CustomMultipartFormDataStreamProvider provider =
                new CustomMultipartFormDataStreamProvider(tempPath);
            foreach (HttpContent content in multipartContent)
            {
                provider.Contents.Add(content);
                using (provider.GetStream(multipartContent, content.Headers)) { }
            }

            // Act
            await provider.ExecutePostProcessingAsync();

            // Assert
            Assert.Equal(maxContents / 2, provider.FormData.Count);

            // half contents for form data
            for (int index = 0; index < maxContents / 2; index++)
            {
                string content = String.Format(contentFormat, index);
                string formName = String.Format(formNameFormat, index);
                Assert.Equal(content, provider.FormData[formName]);
            }

            // the other half for file data
            HttpContent[] contents = multipartContent.ToArray();
            for (int index = maxContents / 2; index < maxContents; index++)
            {
                int fileDataIndex = index - (maxContents / 2);
                string fileName = String.Format(fileNameFormat, index);
                Assert.Equal(fileName, provider.FileData[fileDataIndex].LocalFileName);
                Assert.Same(contents[index].Headers, provider.FileData[fileDataIndex].Headers);
            }
        }

        [Fact]
        public async Task ExecutePostProcessingAsyncWithoutCancellationToken_GetCalledBy_ReadAsMultipartAsync()
        {
            // Arrange
            MultipartFormDataContent multipartContent = new MultipartFormDataContent();
            Mock<MultipartFormDataStreamProvider> mockProvider = new Mock<MultipartFormDataStreamProvider>(ValidPath);
            mockProvider.CallBase = true;

            // Act
            var provider = await multipartContent.ReadAsMultipartAsync(mockProvider.Object);

            // Assert
            mockProvider.Verify(p => p.ExecutePostProcessingAsync(), Times.Once());
        }

        private class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public CustomMultipartFormDataStreamProvider(string rootPath)
                : base(rootPath)
            {
            }

            public CustomMultipartFormDataStreamProvider(string rootPath, int bufferSize)
                : base(rootPath, bufferSize)
            {
            }

            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                Stream stream = null;
                ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
                if (contentDisposition != null)
                {
                    if (!String.IsNullOrEmpty(contentDisposition.FileName))
                    {
                        FileData.Add(new MultipartFileData(headers, contentDisposition.FileName));
                        // Can be replaced with any stream the user want. e.g. Azure Blob Storage Stream.
                        stream = new MemoryStream();
                    }
                    else
                    {
                        stream = base.GetStream(parent, headers);
                    }
                }

                return stream;
            }
        }
    }
}

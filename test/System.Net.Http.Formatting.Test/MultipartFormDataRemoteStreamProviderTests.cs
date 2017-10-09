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
    public class MultipartFormDataRemoteStreamProviderTests :
        MultipartStreamProviderTestBase<CustomMultipartFormDataRemoteStreamProvider>
    {
        [Fact]
        public void FileData_IsEmpty()
        {
            CustomMultipartFormDataRemoteStreamProvider provider = new CustomMultipartFormDataRemoteStreamProvider();
            Assert.Empty(provider.FileData);
        }

        [Fact]
        public void FormData_IsEmpty()
        {
            CustomMultipartFormDataRemoteStreamProvider provider = new CustomMultipartFormDataRemoteStreamProvider();
            Assert.Empty(provider.FormData);
        }

        [Fact]
        public void GetStream_ThrowsOnNoContentDisposition()
        {
            // Arrange
            CustomMultipartFormDataRemoteStreamProvider provider = new CustomMultipartFormDataRemoteStreamProvider();
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => { provider.GetStream(content, headers); });
        }

        [Fact]
        public void GetStream()
        {
            // Arrange
            Stream stream0 = null;
            Stream stream1 = null;

            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent("Content 1"), "NoFile");
                content.Add(new StringContent("Content 2"), "File", "Filename");

                CustomMultipartFormDataRemoteStreamProvider provider =
                    new CustomMultipartFormDataRemoteStreamProvider();

                // Act
                stream0 = provider.GetStream(content, content.ElementAt(0).Headers);
                stream1 = provider.GetStream(content, content.ElementAt(1).Headers);

                // Assert
                Assert.IsType<MemoryStream>(stream0);
                Assert.Single(provider.RemoteStreams, stream1);

                MultipartRemoteFileData fileData = Assert.Single(provider.FileData);
                string expectedUrl = provider.UrlBase + "Filename";
                Assert.Equal(expectedUrl, fileData.Location);

                Assert.Same(content.ElementAt(1).Headers.ContentDisposition,
                    fileData.Headers.ContentDisposition);
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
        [ReplaceCulture]
        public void GetStream_StreamResultNullThrowsException()
        {
            // Arrange
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent("Content"), "File", "Filename");
            CustomMultipartFormDataRemoteStreamProvider provider =
                new CustomMultipartFormDataRemoteStreamProvider(isResultNull: true);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetStream(content, content.ElementAt(0).Headers),
                "The 'GetRemoteStream' method in 'CustomMultipartFormDataRemoteStreamProvider' returned null. " +
                "It must return a RemoteStreamResult instance containing a writable stream and a valid URL."
                );
        }

        [Fact]
        public async Task PostProcessing_ProcessesFormData()
        {
            // Arrange
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

            CustomMultipartFormDataRemoteStreamProvider provider =
                new CustomMultipartFormDataRemoteStreamProvider();
            foreach (HttpContent content in multipartContent)
            {
                provider.Contents.Add(content);
                using (provider.GetStream(multipartContent, content.Headers))
                {
                }
            }

            // Act
            await provider.ExecutePostProcessingAsync();

            // Assert
            Assert.Equal(maxContents/2, provider.FormData.Count);

            // half contents for form data
            for (int index = 0; index < maxContents/2; index++)
            {
                string content = String.Format(contentFormat, index);
                string formName = String.Format(formNameFormat, index);
                Assert.Equal(content, provider.FormData[formName]);
            }

            // the other half for file data
            HttpContent[] contents = multipartContent.ToArray();
            for (int index = maxContents/2; index < maxContents; index++)
            {
                int fileDataIndex = index - (maxContents/2);
                string fileName = String.Format(fileNameFormat, index);
                string url = provider.UrlBase + fileName;
                Assert.Equal(url, provider.FileData[fileDataIndex].Location);
                Assert.Same(contents[index].Headers, provider.FileData[fileDataIndex].Headers);
            }
        }

        [Fact]
        public async Task ExecutePostProcessingAsyncWithoutCancellationToken_GetCalledBy_ReadAsMultipartAsync()
        {
            // Arrange
            MultipartFormDataContent multipartContent = new MultipartFormDataContent();
            Mock<CustomMultipartFormDataRemoteStreamProvider> mockProvider =
                new Mock<CustomMultipartFormDataRemoteStreamProvider>();
            mockProvider.CallBase = true;

            // Act
            await multipartContent.ReadAsMultipartAsync(mockProvider.Object);

            // Assert
            mockProvider.Verify(p => p.ExecutePostProcessingAsync(), Times.Once());
        }
    }
}

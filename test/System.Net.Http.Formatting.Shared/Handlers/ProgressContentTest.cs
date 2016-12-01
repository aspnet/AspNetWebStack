// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Net.Http.Handlers
{
    public class ProgressContentTest
    {
        private const string TestHeader = "TestHeader";
        private const string TestValue = "TestValue";

        [Fact]
        public void Constructor_CopyHeadersFromInnerContent()
        {
            // Arrange
            StringContent innerContent = new StringContent("HelloWorld!");
            innerContent.Headers.Add(TestHeader, TestValue);
            HttpRequestMessage request = new HttpRequestMessage();
            ProgressMessageHandler progressHandler = new ProgressMessageHandler();

            // Act
            ProgressContent progressContent = new ProgressContent(innerContent, progressHandler, request);

            // Assert
            ValidateContentHeader(progressContent);
            Assert.Equal(innerContent.Headers.ContentType, progressContent.Headers.ContentType);
            Assert.Equal(innerContent.Headers.ContentLength, progressContent.Headers.ContentLength);
        }

        [Fact]
        public async Task SerializeToStreamAsync_InsertsProgressStream()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("HelloWorld!");

            MockProgressEventHandler progressEventHandler = new MockProgressEventHandler();
            ProgressMessageHandler progressHandler = MockProgressEventHandler.CreateProgressMessageHandler(out progressEventHandler, sendProgress: true);
            ProgressContent progressContent = new ProgressContent(request.Content, progressHandler, request);
            MemoryStream memStream = new MemoryStream();

            // Act
            await progressContent.CopyToAsync(memStream);

            // Assert
            Assert.True(progressEventHandler.WasInvoked);
            Assert.Equal(request, progressEventHandler.Sender);
            Assert.Equal(request.Content.Headers.ContentLength, progressEventHandler.EventArgs.TotalBytes);
        }

        [Fact]
        public void Dispose_DisposesInnerContent()
        {
            // Arrange
            StringContent innerContent = new StringContent("HelloWorld!");
            HttpRequestMessage request = new HttpRequestMessage();
            ProgressMessageHandler progressHandler = new ProgressMessageHandler();
            ProgressContent progressContent = new ProgressContent(innerContent, progressHandler, request);

            // Act
            progressContent.Dispose();

            // Assert
            Assert.ThrowsObjectDisposed(() => innerContent.LoadIntoBufferAsync(), typeof(StringContent).FullName);
        }

        private static void ValidateContentHeader(HttpContent content)
        {
            IEnumerable<string> values;
            bool headerResult = content.Headers.TryGetValues(TestHeader, out values);
            Assert.True(headerResult);
            Assert.Equal(TestValue, values.First());
        }
    }
}

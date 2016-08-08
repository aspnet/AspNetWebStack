// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class MultipartMemoryStreamProviderTests : MultipartStreamProviderTestBase<MultipartMemoryStreamProvider>
    {
        [Fact]
        public void GetStream_ReturnsNewMemoryStream()
        {
            // Arrange
            MultipartMemoryStreamProvider instance = new MultipartMemoryStreamProvider();
            HttpContent parent = new StringContent(String.Empty);
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            Stream stream1 = instance.GetStream(parent, headers);
            Stream stream2 = instance.GetStream(parent, headers);

            // Assert
            Assert.IsType<MemoryStream>(stream1);
            Assert.Equal(0, stream1.Length);
            Assert.Equal(0, stream1.Position);

            Assert.IsType<MemoryStream>(stream2);
            Assert.Equal(0, stream2.Length);
            Assert.Equal(0, stream2.Position);

            Assert.NotSame(stream1, stream2);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public abstract class MultipartStreamProviderTestBase<TProvider> where TProvider : MultipartStreamProvider, new()
    {
        protected MultipartStreamProviderTestBase()
        {
        }

        [Fact]
        public void Contents_IsEmpty()
        {
            // Arrange
            TProvider provider = new TProvider();

            // Act
            Collection<HttpContent> contents = provider.Contents;

            // Assert
            Assert.Empty(contents);
        }

        [Fact]
        public void PostProcessing_ReturnsCompleteTask()
        {
            // Arrange
            TProvider provider = new TProvider();

            // Act
            Task postProcessing = provider.ExecutePostProcessingAsync();

            // Assert
            Assert.Equal(TaskStatus.RanToCompletion, postProcessing.Status);
        }

        [Fact]
        public void GetStream_ThrowsOnNullParent()
        {
            TProvider provider = new TProvider();
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            Assert.ThrowsArgumentNull(() => provider.GetStream(null, headers), "parent");
        }

        [Fact]
        public void GetStream_ThrowsOnNullHeaders()
        {
            TProvider provider = new TProvider();
            StringContent content = new StringContent(String.Empty);

            Assert.ThrowsArgumentNull(() => provider.GetStream(content, null), "headers");
        }
    }
}

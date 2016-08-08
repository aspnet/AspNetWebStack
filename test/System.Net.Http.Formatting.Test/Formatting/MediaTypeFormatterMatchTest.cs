// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterMatchTest
    {
        [Fact]
        public void Ctor_ThrowsOnNullFormatter()
        {
            Assert.ThrowsArgumentNull(() => new MediaTypeFormatterMatch(null, null, null, MediaTypeFormatterMatchRanking.None), "formatter");
        }

        [Fact]
        public void Ctor_ClonesMediaType()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            MediaTypeHeaderValue mediaType = MediaTypeHeaderValue.Parse("text/test");

            // Act
            MediaTypeFormatterMatch match = new MediaTypeFormatterMatch(formatter, mediaType, null, MediaTypeFormatterMatchRanking.MatchOnCanWriteType);

            // Assert
            Assert.Equal(mediaType, match.MediaType);
            Assert.NotSame(mediaType, match.MediaType);
        }

        [Fact]
        public void Ctor_InitializesDefaultValues()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();

            // Act
            MediaTypeFormatterMatch match = new MediaTypeFormatterMatch(formatter, null, null, MediaTypeFormatterMatchRanking.MatchOnCanWriteType);

            // Assert
            Assert.Same(formatter, match.Formatter);
            Assert.Equal(MediaTypeConstants.ApplicationOctetStreamMediaType, match.MediaType);
            Assert.Equal(FormattingUtilities.Match, match.Quality);
            Assert.Equal(MediaTypeFormatterMatchRanking.MatchOnCanWriteType, match.Ranking);
        }
    }
}

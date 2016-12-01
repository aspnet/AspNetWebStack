// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class MediaTypeConstantsTests
    {
        private static void ValidateClones(MediaTypeHeaderValue clone1, MediaTypeHeaderValue clone2, string charset)
        {
            Assert.NotNull(clone1);
            Assert.NotNull(clone2);
            Assert.NotSame(clone1, clone2);
            Assert.Equal(clone1.MediaType, clone2.MediaType);
            Assert.Equal(charset, clone1.CharSet);
            Assert.Equal(charset, clone2.CharSet);
        }

        [Fact]
        public void ApplicationOctetStreamMediaType_ReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationOctetStreamMediaType, MediaTypeConstants.ApplicationOctetStreamMediaType, null);
        }

        [Fact]
        public void ApplicationXmlMediaType_ReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationXmlMediaType, MediaTypeConstants.ApplicationXmlMediaType, null);
        }

        [Fact]
        public void ApplicationJsonMediaType_ReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationJsonMediaType, MediaTypeConstants.ApplicationJsonMediaType, null);
        }

        [Fact]
        public void TextXmlMediaType_ReturnsClone()
        {
            ValidateClones(MediaTypeConstants.TextXmlMediaType, MediaTypeConstants.TextXmlMediaType, null);
        }

        [Fact]
        public void TextJsonMediaType_ReturnsClone()
        {
            ValidateClones(MediaTypeConstants.TextJsonMediaType, MediaTypeConstants.TextJsonMediaType, null);
        }

        [Fact]
        public void ApplicationFormUrlEncodedMediaType_ReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationFormUrlEncodedMediaType, MediaTypeConstants.ApplicationFormUrlEncodedMediaType, null);
        }


    }
}

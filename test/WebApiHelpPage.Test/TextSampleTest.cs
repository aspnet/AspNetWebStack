// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class TextSampleTest
    {
        [Fact]
        public void Constructor()
        {
            TextSample sample = new TextSample("some text");
            Assert.Equal("some text", sample.Text);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TextSample(null));
        }

        [Fact]
        public void Equals_ReturnsTrue()
        {
            TextSample sample = new TextSample("some text");
            Assert.Equal(new TextSample("some text"), sample);
        }

        [Fact]
        public void ToString_ReturnsText()
        {
            TextSample sample = new TextSample("some text");
            Assert.Equal("some text", sample.ToString());
        }

        [Fact]
        public void GetHashCode_ReturnsTextHashCode()
        {
            TextSample sample = new TextSample("some text");
            Assert.Equal("some text".GetHashCode(), sample.GetHashCode());
        }
    }
}

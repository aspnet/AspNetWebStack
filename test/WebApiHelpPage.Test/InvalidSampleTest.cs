// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class InvalidSampleTest
    {
        [Fact]
        public void Constructor()
        {
            InvalidSample sample = new InvalidSample("something failed");
            Assert.Equal("something failed", sample.ErrorMessage);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new InvalidSample(null));
        }

        [Fact]
        public void Equals_ReturnsTrue()
        {
            InvalidSample sample = new InvalidSample("something failed");
            Assert.Equal(new InvalidSample("something failed"), sample);
        }

        [Fact]
        public void ToString_ReturnsSrc()
        {
            InvalidSample sample = new InvalidSample("something failed");
            Assert.Equal("something failed", sample.ToString());
        }

        [Fact]
        public void GetHashCode_ReturnsSrcHashCode()
        {
            InvalidSample sample = new InvalidSample("something failed");
            Assert.Equal("something failed".GetHashCode(), sample.GetHashCode());
        }
    }
}

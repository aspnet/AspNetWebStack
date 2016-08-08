// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web;
using Microsoft.TestCommon;

namespace System.Web.Test
{
    public class PathHelpersTest
    {
        [Theory]
        [InlineData("foo.Bar", "bar")]
        [InlineData("foo.bar", "bar")]
        [InlineData(".bar", "bar")]
        public void EndsWithExtensionReturnsTrue(string path, string extension)
        {
            Assert.True(PathHelpers.EndsWithExtension(path, extension));
        }

        [Theory]
        [InlineData("foo.Baz", "bar")]
        [InlineData("", "bar")]
        [InlineData("Bar", "bar")]
        [InlineData("fooBar", "bar")]
        public void EndsWithExtensionReturnsFalse(string path, string extension)
        {
            Assert.False(PathHelpers.EndsWithExtension(path, extension));
        }
    }
}

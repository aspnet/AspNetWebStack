// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class AntiForgeryConfigTest
    {
        [Theory]
        [InlineData(null, "__RequestVerificationToken")]
        [InlineData("", "__RequestVerificationToken")]
        [InlineData("/", "__RequestVerificationToken")]
        [InlineData("/path", "__RequestVerificationToken_L3BhdGg1")]
        public void GetAntiForgeryCookieName(string appPath, string expectedCookieName)
        {
            // Act
            string retVal = AntiForgeryConfig.GetAntiForgeryCookieName(appPath);

            // Assert
            Assert.Equal(expectedCookieName, retVal);
        }
    }
}

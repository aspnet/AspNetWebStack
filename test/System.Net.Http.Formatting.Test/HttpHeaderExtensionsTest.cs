// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpHeaderExtensionsTest
    {
        [Fact]
        public void CopyTo_CopiesContentHeaders()
        {
            // Arrange
            HttpContentHeaders source = FormattingUtilities.CreateEmptyContentHeaders();
            source.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf8; parameter=value");
            source.ContentLength = 1234;
            source.ContentLocation = new Uri("http://some.host");
            source.Add("test-name1", "test-value1");
            source.Add("test-name2", "test-value2");

            HttpContentHeaders destination = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            source.CopyTo(destination);

            // Assert
            Assert.Equal(source.ToString(), destination.ToString());
        }
    }
}

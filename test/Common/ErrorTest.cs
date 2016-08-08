// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class ErrorTest
    {
        [Fact]
        public void Format()
        {
            // Arrange
            string expected = "The formatted message";

            // Act
            string actual = Error.Format("The {0} message", "formatted");

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}

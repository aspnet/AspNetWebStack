// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelErrorTest
    {
        [Fact]
        public void ConstructorThrowsIfExceptionIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ModelError((Exception)null); }, "exception");
        }

        [Fact]
        public void ConstructorWithExceptionAndStringArguments()
        {
            // Arrange
            Exception ex = new Exception("some message");

            // Act
            ModelError modelError = new ModelError(ex, "some other message");

            // Assert
            Assert.Equal("some other message", modelError.ErrorMessage);
            Assert.Same(ex, modelError.Exception);
        }

        [Fact]
        public void ConstructorWithExceptionArgument()
        {
            // Arrange
            Exception ex = new Exception("some message");

            // Act
            ModelError modelError = new ModelError(ex);

            // Assert
            Assert.Equal(String.Empty, modelError.ErrorMessage);
            Assert.Same(ex, modelError.Exception);
        }

        [Fact]
        public void ConstructorWithNullStringArgumentCreatesEmptyStringErrorMessage()
        {
            // Act
            ModelError modelError = new ModelError((string)null);

            // Assert
            Assert.Equal(String.Empty, modelError.ErrorMessage);
        }

        [Fact]
        public void ConstructorWithStringArgument()
        {
            // Act
            ModelError modelError = new ModelError("some message");

            // Assert
            Assert.Equal("some message", modelError.ErrorMessage);
            Assert.Null(modelError.Exception);
        }
    }
}

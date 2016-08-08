// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class HttpAntiForgeryExceptionTest
    {
        [Fact]
        public void ConstructorWithMessageAndInnerExceptionParameter()
        {
            // Arrange
            Exception innerException = new Exception();

            // Act
            HttpAntiForgeryException ex = new HttpAntiForgeryException("the message", innerException);

            // Assert
            Assert.Equal("the message", ex.Message);
            Assert.Equal(innerException, ex.InnerException);
        }

        [Fact]
        public void ConstructorWithMessageParameter()
        {
            // Act
            HttpAntiForgeryException ex = new HttpAntiForgeryException("the message");

            // Assert
            Assert.Equal("the message", ex.Message);
        }

        [Fact]
        public void ConstructorWithoutParameters()
        {
            // Act & assert
            Assert.Throws<HttpAntiForgeryException>(
                delegate { throw new HttpAntiForgeryException(); });
        }

        [Fact]
        public void TypeIsSerializable()
        {
            // If this ever fails with SerializationException : Unable to find assembly 'System.Web.Mvc, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
            // (usually when the assembly version is incremented) you need to modify the App.config file in this test project to reference the new version.

            // Arrange
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            HttpAntiForgeryException ex = new HttpAntiForgeryException("the message", new Exception("inner exception"));

            // Act
            formatter.Serialize(ms, ex);
            ms.Position = 0;
            HttpAntiForgeryException deserialized = formatter.Deserialize(ms) as HttpAntiForgeryException;

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("the message", deserialized.Message);
            Assert.NotNull(deserialized.InnerException);
            Assert.Equal("inner exception", deserialized.InnerException.Message);
        }
    }
}

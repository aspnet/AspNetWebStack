// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpFileCollectionValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            HttpFileCollectionValueProviderFactory factory = new HttpFileCollectionValueProviderFactory();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Files.Count).Returns(0);

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.IsType<HttpFileCollectionValueProvider>(valueProvider);
        }

        [Fact]
        public void GetValueProvider_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            HttpFileCollectionValueProviderFactory factory = new HttpFileCollectionValueProviderFactory();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { factory.GetValueProvider(null); }, "controllerContext");
        }
    }
}

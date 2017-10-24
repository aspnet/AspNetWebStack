// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Services
{
    public class ServicesExtensionsTests
    {
        [Fact]
        public void GetValueProviderFactories_Throws_WhenServicesIsNull()
        {
            // Arrange
            ServicesContainer services = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ServicesExtensions.GetValueProviderFactories(services), "services");
        }

        [Fact]
        public void GetExceptionHandler_DelegatesToGetService()
        {
            // Arrange
            IExceptionHandler expectedExceptionHandler = new Mock<IExceptionHandler>(MockBehavior.Strict).Object;

            Mock<ServicesContainer> mock = new Mock<ServicesContainer>(MockBehavior.Strict);
            mock.Setup(s => s.GetService(typeof(IExceptionHandler))).Returns(expectedExceptionHandler);
            ServicesContainer services = mock.Object;

            // Act
            IExceptionHandler exceptionHandler = ServicesExtensions.GetExceptionHandler(services);

            // Assert
            mock.Verify(s => s.GetService(typeof(IExceptionHandler)), Times.Once());
            Assert.Same(expectedExceptionHandler, exceptionHandler);
        }

        [Fact]
        public void GetExceptionLoggers_DelegatesToGetServices()
        {
            // Arrange
            IExceptionLogger expectedExceptionLogger = new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
            IEnumerable<IExceptionLogger> expectedExceptionLoggers =
                new IExceptionLogger[] { expectedExceptionLogger };

            Mock<ServicesContainer> mock = new Mock<ServicesContainer>(MockBehavior.Strict);
            mock.Setup(s => s.GetServices(typeof(IExceptionLogger))).Returns(expectedExceptionLoggers);
            ServicesContainer services = mock.Object;

            // Act
            IEnumerable<IExceptionLogger> exceptionLoggers = ServicesExtensions.GetExceptionLoggers(services);

            // Assert
            mock.Verify(s => s.GetServices(typeof(IExceptionLogger)), Times.Once());
            Assert.NotNull(exceptionLoggers);
            IExceptionLogger exceptionLogger = Assert.Single(exceptionLoggers);
            Assert.Same(expectedExceptionLogger, exceptionLogger);
        }
    }
}

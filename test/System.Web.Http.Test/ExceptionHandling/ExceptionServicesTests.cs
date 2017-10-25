// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionServicesTests
    {
        [Fact]
        public void GetLoggerWithServices_ReturnsCompositeExceptionLoggerWithServicesLoggers()
        {
            // Arrange
            IExceptionLogger expectedLogger = CreateDummyLogger();

            using (ServicesContainer services = CreateServices(expectedLogger))
            {
                // Act
                IExceptionLogger logger = ExceptionServices.GetLogger(services);

                // Assert
                IEnumerable<IExceptionLogger> loggers = Assert.IsType<CompositeExceptionLogger>(logger).Loggers;
                IExceptionLogger actualLogger = Assert.Single(loggers);
                Assert.Same(expectedLogger, actualLogger);
            }
        }

        [Fact]
        public void GetLoggerWithServices_ReturnsSameInstance()
        {
            // Arrange
            IExceptionLogger innerLogger = CreateDummyLogger();

            using (ServicesContainer services = CreateServices(innerLogger))
            {
                IExceptionLogger firstLogger = ExceptionServices.GetLogger(services);

                // Act
                IExceptionLogger secondLogger = ExceptionServices.GetLogger(services);

                // Assert
                Assert.Same(firstLogger, secondLogger);
            }
        }

        [Fact]
        public void GetLoggerWithServices_IfServicesIsNull_Throws()
        {
            // Arrange
            ServicesContainer services = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ExceptionServices.GetLogger(services), "services");
        }

        [Fact]
        public void GetLoggerWithConfiguration_ReturnsCompositeExceptionLoggerWithServicesLoggers()
        {
            // Arrange
            IExceptionLogger expectedLogger = CreateDummyLogger();

            using (HttpConfiguration configuration = CreateConfiguration(expectedLogger))
            {
                // Act
                IExceptionLogger logger = ExceptionServices.GetLogger(configuration);

                // Assert
                IEnumerable<IExceptionLogger> loggers = Assert.IsType<CompositeExceptionLogger>(logger).Loggers;
                IExceptionLogger actualLogger = Assert.Single(loggers);
                Assert.Same(expectedLogger, actualLogger);
            }
        }

        [Fact]
        public void GetLoggerWithConfiguration_IfConfigurationIsNull_Throws()
        {
            // Arrange
            HttpConfiguration configuration = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ExceptionServices.GetLogger(configuration), "configuration");
        }

        [Fact]
        public void GetHandlerWithServices_ReturnsLastChanceHandlerWithServicesInnerHandler()
        {
            // Arrange
            IExceptionHandler expectedHandler = CreateDummyHandler();

            using (ServicesContainer services = CreateServices(expectedHandler))
            {
                // Act
                IExceptionHandler handler = ExceptionServices.GetHandler(services);

                // Assert
                IExceptionHandler innerHandler = Assert.IsType<LastChanceExceptionHandler>(handler).InnerHandler;
                Assert.Same(expectedHandler, innerHandler);
            }
        }

        [Fact]
        public void GetHandlerWithServices_ReturnsSameInstance()
        {
            // Arrange
            IExceptionHandler innerHandler = CreateDummyHandler();

            using (ServicesContainer services = CreateServices(innerHandler))
            {
                IExceptionHandler firstHandler = ExceptionServices.GetHandler(services);

                // Act
                IExceptionHandler secondHandler = ExceptionServices.GetHandler(services);

                // Assert
                Assert.Same(firstHandler, secondHandler);
            }
        }

        [Fact]
        public void GetHandlerWithServices_IfHandlerIsAbsent_ReturnsLastChanceHandlerWithEmptyInnerHandler()
        {
            // Arrange
            IExceptionHandler servicesHandler = null;

            using (ServicesContainer services = CreateServices(servicesHandler))
            {
                // Act
                IExceptionHandler handler = ExceptionServices.GetHandler(services);

                // Assert
                IExceptionHandler innerHandler = Assert.IsType<LastChanceExceptionHandler>(handler).InnerHandler;
                Assert.IsType<EmptyExceptionHandler>(innerHandler);
            }
        }

        [Fact]
        public void GetHandlerWithServices_IfServicesIsNull_Throws()
        {
            // Arrange
            ServicesContainer services = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ExceptionServices.GetHandler(services), "services");
        }

        [Fact]
        public void GetHandlerWithConfiguration_ReturnsLastChanceHandlerWithServicesInnerHandler()
        {
            // Arrange
            IExceptionHandler expectedHandler = CreateDummyHandler();

            using (HttpConfiguration configuration = CreateConfiguration(expectedHandler))
            {
                // Act
                IExceptionHandler handler = ExceptionServices.GetHandler(configuration);

                // Assert
                IExceptionHandler innerHandler = Assert.IsType<LastChanceExceptionHandler>(handler).InnerHandler;
                Assert.Same(expectedHandler, innerHandler);
            }
        }

        [Fact]
        public void GetHandlerWithConfiguration_IfConfigurationIsNull_Throws()
        {
            // Arrange
            HttpConfiguration configuration = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ExceptionServices.GetHandler(configuration), "configuration");
        }

        private static HttpConfiguration CreateConfiguration(IExceptionHandler handler)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IExceptionHandler), handler);
            return configuration;
        }

        private static HttpConfiguration CreateConfiguration(params IExceptionLogger[] loggers)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.ReplaceRange(typeof(IExceptionLogger), loggers);
            return configuration;
        }

        private static IExceptionHandler CreateDummyHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IExceptionLogger CreateDummyLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private static ServicesContainer CreateServices(IExceptionHandler handler)
        {
            Mock<ServicesContainer> mock = new Mock<ServicesContainer>(MockBehavior.Strict);
            mock.Setup(s => s.GetService(typeof(IExceptionHandler))).Returns(handler);
            LastChanceExceptionHandler cached = null;
            mock.Setup(s => s.GetService(typeof(LastChanceExceptionHandler))).Returns(() => cached);
            mock.Protected()
                .Setup("ReplaceSingle", typeof(LastChanceExceptionHandler), ItExpr.IsAny<object>())
                .Callback<Type, object>((i, o) => cached = (LastChanceExceptionHandler)o);
            mock.Setup(s => s.IsSingleService(typeof(LastChanceExceptionHandler))).Returns(true);
            mock.Protected().Setup("ResetCache", typeof(LastChanceExceptionHandler));
            mock.Setup(s => s.Dispose());
            return mock.Object;
        }

        private static ServicesContainer CreateServices(params IExceptionLogger[] loggers)
        {
            Mock<ServicesContainer> mock = new Mock<ServicesContainer>(MockBehavior.Strict);
            mock.Setup(s => s.GetServices(typeof(IExceptionLogger))).Returns(loggers);
            CompositeExceptionLogger cached = null;
            mock.Setup(s => s.GetService(typeof(CompositeExceptionLogger))).Returns(() => cached);
            mock.Protected()
                .Setup("ReplaceSingle", typeof(CompositeExceptionLogger), ItExpr.IsAny<object>())
                .Callback<Type, object>((i, o) => cached = (CompositeExceptionLogger)o);
            mock.Setup(s => s.IsSingleService(typeof(CompositeExceptionLogger))).Returns(true);
            mock.Protected().Setup("ResetCache", typeof(CompositeExceptionLogger));
            mock.Setup(s => s.Dispose());
            return mock.Object;
        }
    }
}

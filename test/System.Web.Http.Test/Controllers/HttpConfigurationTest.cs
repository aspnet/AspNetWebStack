// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpConfigurationTest
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpConfiguration>(TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsDisposable);
        }

        [Fact]
        public void Default_Constructor()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            Assert.Empty(configuration.Filters);
            Assert.NotEmpty(configuration.Formatters);
            Assert.Empty(configuration.MessageHandlers);
            Assert.Empty(configuration.Properties);
            Assert.Empty(configuration.Routes);
            Assert.NotNull(configuration.DependencyResolver);
            Assert.NotNull(configuration.Services);
            Assert.Equal("/", configuration.VirtualPathRoot);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            string path = "/some/path";
            HttpRouteCollection routes = new HttpRouteCollection(path);
            HttpConfiguration configuration = new HttpConfiguration(routes);

            Assert.Same(routes, configuration.Routes);
            Assert.Equal(path, configuration.VirtualPathRoot);
        }

        [Fact]
        public void Dispose_Idempotent()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Dispose();
            configuration.Dispose();
        }

        [Fact]
        public void DependencyResolver_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act & assert
            Assert.ThrowsArgumentNull(() => config.DependencyResolver = null, "value");
        }

        [Fact]
        public void Initializer_Default_Set_By_Ctor()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Assert
            Assert.NotNull(config.Initializer);
        }

        [Fact]
        public void Initializer_Throws_With_Null()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => config.Initializer = null, "value");
        }

        [Fact]
        public void Initializer_Initializes_TraceManager_By_Default()
        {
            // Arrange
            var config = new HttpConfiguration();
            Mock<ITraceManager> mock = new Mock<ITraceManager>() { CallBase = true };
            mock.Setup(m => m.Initialize(config)).Verifiable();
            config.Services.Replace(typeof(ITraceManager), mock.Object);

            // Act
            config.Initializer(config);

            // Assert
            mock.Verify();
        }

        [Fact]
        public void EnsureInitialized_CallsInitializerOnce()
        {
            // Arrange
            int count = 0;
            var config = new HttpConfiguration();
            config.Initializer = _ => { count++; };

            // Act
            config.EnsureInitialized();
            Assert.Equal(1, count);

            config.EnsureInitialized();
            Assert.Equal(1, count);

        }

        [Fact]
        public void Initializer_Sets_Formatter_RequiredMemberSelector_By_Default()
        {
            // Arrange
            var config = new HttpConfiguration();
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            mockFormatter.Object.RequiredMemberSelector = null;

            config.Formatters.Clear();
            config.Formatters.Add(mockFormatter.Object);

            // Act
            config.Initializer(config);

            // Assert
            Assert.NotNull(mockFormatter.Object.RequiredMemberSelector);
        }

        [Fact]
        public void Initialize_Default_Initializer_Can_Be_Removed()
        {
            // Arrange
            var config = new HttpConfiguration();
            config.Initializer = (c) => { };
            bool initializeCalled = false;
            Mock<ITraceManager> mock = new Mock<ITraceManager>() { CallBase = true };
            mock.Setup(m => m.Initialize(config)).Callback(() => initializeCalled = true);
            config.Services.Replace(typeof(ITraceManager), mock.Object);

            // Act
            config.Initializer(config);

            // Assert
            Assert.False(initializeCalled);
        }

        [Fact]
        public void Initialize_Initializer_Can_Be_Reused()
        {
            // Arrange
            HttpConfiguration config1 = new HttpConfiguration();
            HttpConfiguration configPassedToAction = null;
            config1.Initializer = (c) => configPassedToAction = c;

            HttpConfiguration config2 = new HttpConfiguration();
            config2.Initializer = config1.Initializer;

            // Act
            config2.Initializer(config2);

            // Assert
            Assert.Same(config2, configPassedToAction);
        }

        [Fact]
        public void Initialize_Can_Be_Chained()
        {
            // Arrange
            HttpConfiguration config1 = new HttpConfiguration();
            HttpConfiguration configPassedToAction1 = null;
            config1.Initializer = (c) => configPassedToAction1 = c;

            HttpConfiguration config2 = new HttpConfiguration();
            HttpConfiguration configPassedToAction2 = null;
            config2.Initializer = (c) => { config1.Initializer(config1); configPassedToAction2 = c; };

            // Act
            config2.Initializer(config2);

            // Assert
            Assert.Same(config1, configPassedToAction1);
            Assert.Same(config2, configPassedToAction2);
        }

        [Fact]
        public void ApplyControllerSettings_Does_Not_Clone_When_Settings_Are_Not_Initialized()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerSettings settings = new HttpControllerSettings(config);

            // Act
            HttpConfiguration clonedConfig = HttpConfiguration.ApplyControllerSettings(settings, config);

            // Assert
            Assert.Same(config, clonedConfig);
        }

        [Fact]
        public void ApplyControllerSettings_Clones_Configuration_When_Settings_Are_Initialized()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerSettings settings = new HttpControllerSettings(config);
            settings.Formatters.Clear();    // accessing this property will force a clone

            // Act
            HttpConfiguration clonedConfig = HttpConfiguration.ApplyControllerSettings(settings, config);

            // Assert
            Assert.NotNull(clonedConfig);
            Assert.NotSame(config, clonedConfig);
        }

        [Fact]
        public void ApplyControllerSettings_Executes_Original_Initializer_On_Clone()
        {
            // Arrange
            HttpConfiguration configPassedToAction = null;
            HttpConfiguration config = new HttpConfiguration();
            config.Initializer = (c) => configPassedToAction = c;
            HttpControllerSettings settings = new HttpControllerSettings(config);
            settings.Formatters.Clear();    // accessing this property will force a clone

            // Act
            HttpConfiguration clonedConfig = HttpConfiguration.ApplyControllerSettings(settings, config);

            // Assert
            Assert.Same(clonedConfig, configPassedToAction);
        }

        [Fact]
        public void ApplyControllerSettings_Clone_Inherits_Tracing_On_PerController_Services()
        {
            // Arrange
            bool calledTrace = false;
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> mockTracer = new Mock<ITraceWriter>() { CallBase = true };
            mockTracer.Setup(m => m.Trace(It.IsAny<HttpRequestMessage>(),
                                          It.IsAny<string>(),
                                          It.IsAny<TraceLevel>(),
                                          It.IsAny<Action<TraceRecord>>())).Callback(() => { calledTrace = true; });

            config.Services.Replace(typeof(ITraceWriter), mockTracer.Object);
            config.Initializer(config);    // installs tracer on original config

            HttpControllerSettings settings = new HttpControllerSettings(config);
            Mock<IContentNegotiator> mockNegotiator = new Mock<IContentNegotiator>() { CallBase = true };
            settings.Services.Replace(typeof(IContentNegotiator), mockNegotiator.Object);

            // Act
            HttpConfiguration clonedConfig = HttpConfiguration.ApplyControllerSettings(settings, config);
            clonedConfig.Services.GetContentNegotiator().Negotiate(typeof(string), new HttpRequestMessage(), Enumerable.Empty<MediaTypeFormatter>());

            // Assert
            Assert.True(calledTrace);
        }

        [Fact]
        public void ApplyControllerSettings_Clone_Inherits_Tracing_On_PerController_Formatters()
        {
            // Arrange
            bool calledTrace = false;
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> mockTracer = new Mock<ITraceWriter>() { CallBase = true };
            mockTracer.Setup(m => m.Trace(It.IsAny<HttpRequestMessage>(),
                                          It.IsAny<string>(),
                                          It.IsAny<TraceLevel>(),
                                          It.IsAny<Action<TraceRecord>>())).Callback(() => { calledTrace = true; });

            config.Services.Replace(typeof(ITraceWriter), mockTracer.Object);
            config.Initializer(config);    // installs tracer on original config

            HttpControllerSettings settings = new HttpControllerSettings(config);
            Mock<MediaTypeFormatter> mockFormatter = new Mock<MediaTypeFormatter>() { CallBase = true };
            settings.Formatters.Clear();
            settings.Formatters.Add(mockFormatter.Object);

            // Act
            HttpConfiguration clonedConfig = HttpConfiguration.ApplyControllerSettings(settings, config);
            clonedConfig.Formatters[0].GetPerRequestFormatterInstance(typeof(string), new HttpRequestMessage(), new MediaTypeHeaderValue("application/mine"));

            // Assert
            Assert.True(calledTrace);
        }

        [Fact]
        public void ApplyControllerSettings_Clone_Can_Enable_Tracing_When_Original_Disabled_It()
        {
            // Arrange
            bool calledTrace = false;
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> mockTracer = new Mock<ITraceWriter>() { CallBase = true };
            mockTracer.Setup(m => m.Trace(It.IsAny<HttpRequestMessage>(),
                                          It.IsAny<string>(),
                                          It.IsAny<TraceLevel>(),
                                          It.IsAny<Action<TraceRecord>>())).Callback(() => { calledTrace = true; });

            config.Initializer(config);    // ensures TraceManager is called, but it will be a NOP

            HttpControllerSettings settings = new HttpControllerSettings(config);
            settings.Services.Replace(typeof(ITraceWriter), mockTracer.Object);

            // Act
            HttpConfiguration clonedConfig = HttpConfiguration.ApplyControllerSettings(settings, config);
            clonedConfig.Services.GetContentNegotiator().Negotiate(typeof(string), new HttpRequestMessage(), Enumerable.Empty<MediaTypeFormatter>());

            // Assert
            Assert.True(calledTrace);
        }
    }
}

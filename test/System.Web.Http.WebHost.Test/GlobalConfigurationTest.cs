// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.WebHost;
using System.Web.Http.WebHost.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class GlobalConfigurationTest
    {
        [Fact]
        public void ConfigurationRouteCollection_IsHostedHttpRouteCollection()
        {
            // Arrange
            HttpConfiguration configuration = GlobalConfiguration.Configuration;
            Assert.NotNull(configuration); // Guard

            // Act
            HttpRouteCollection routes = configuration.Routes;

            // Assert
            Assert.IsType<HostedHttpRouteCollection>(routes);
            // Note that there is currently no easy way to test that it references RouteTable.Routes.
        }

        [Theory]
        [InlineData(typeof(IAssembliesResolver), typeof(WebHostAssembliesResolver))]
        [InlineData(typeof(IHttpControllerTypeResolver), typeof(WebHostHttpControllerTypeResolver))]
        [InlineData(typeof(IHostBufferPolicySelector), typeof(WebHostBufferPolicySelector))]
        [InlineData(typeof(IExceptionHandler), typeof(WebHostExceptionHandler))]
        public void ConfigurationService_IsWebHost(Type serviceInterfaceType, Type expectedImplementationType)
        {
            // Arrange
            HttpConfiguration configuration = GlobalConfiguration.Configuration;
            Assert.NotNull(configuration); // Guard
            Assert.NotNull(configuration.Services); // Guard

            // Act
            object service = configuration.Services.GetService(serviceInterfaceType);

            // Assert
            Assert.IsType(expectedImplementationType, service);
        }

        [Fact]
        public void ExceptionHandler_WrapsDefaultExceptionHandler()
        {
            // Arrange
            HttpConfiguration configuration = GlobalConfiguration.Configuration;
            Assert.NotNull(configuration); // Guard
            Assert.NotNull(configuration.Services); // Guard

            Type defaultExceptionHandlerType;

            using (HttpConfiguration standardConfiguration = new HttpConfiguration())
            {
                defaultExceptionHandlerType = standardConfiguration.Services.GetExceptionHandler().GetType();
            }

            // Act
            object service = configuration.Services.GetService(typeof(IExceptionHandler));

            // Assert
            WebHostExceptionHandler typedHandler = Assert.IsType<WebHostExceptionHandler>(service); // Guard
            Assert.IsType(defaultExceptionHandlerType, typedHandler.InnerHandler);
        }

        [Fact]
        public void DefaultHandler_IsHttpRoutingDispatcher()
        {
            // Act
            HttpMessageHandler handler = GlobalConfiguration.DefaultHandler;

            // Assert
            Assert.IsType<HttpRoutingDispatcher>(handler);
            // Note that there is currently no easy way to test it references GlobalConfiguration.Configuration.
        }

        [Fact]
        public void DefaultServer_ReferencesConfiguration()
        {
            // Act
            HttpServer server = GlobalConfiguration.DefaultServer;

            // Assert
            Assert.NotNull(server);
            Assert.Same(GlobalConfiguration.Configuration, server.Configuration);
        }

        [Fact]
        public void DefaultServer_ReferencesDefaultHandler()
        {
            // Act
            HttpServer server = GlobalConfiguration.DefaultServer;

            // Assert
            Assert.NotNull(server);
            Assert.Same(GlobalConfiguration.DefaultHandler, server.Dispatcher);
        }

        [Fact]
        public void Configure_Throws_WhenConfigurationCallbackIsNull()
        {
            // Arrange
            using (new GlobalConfigurationContext())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() => { GlobalConfiguration.Configure(null); }, "configurationCallback");
            }
        }

        [Fact]
        public void Configure_CallsCallbackOnceWithGlobalConfiguration()
        {
            // Arrange
            using (new GlobalConfigurationContext())
            {
                int calls = 0;
                HttpConfiguration configuration = null;
                Action<HttpConfiguration> callback = (c) =>
                {
                    calls++;
                    configuration = c;
                };

                // Act
                GlobalConfiguration.Configure(callback);

                // Assert
                Assert.Equal(1, calls);
                Assert.Same(GlobalConfiguration.Configuration, configuration);
            }
        }

        [Fact]
        public void Configure_CallsConfigurationEnsureInitialized()
        {
            // Arrange
            using (new GlobalConfigurationContext())
            {
                bool initialized = false;
                GlobalConfiguration.Configuration.Initializer = (c) =>
                {
                    initialized = true;
                };
                Action<HttpConfiguration> callback = (ignore) => { };

                // Act
                GlobalConfiguration.Configure(callback);

                // Assert
                Assert.True(initialized);
            }
        }

        private sealed class GlobalConfigurationContext : IDisposable
        {
            bool disposed;

            public void Dispose()
            {
                if (!disposed)
                {
                    GlobalConfiguration.Reset();
                    disposed = true;
                }
            }
        }
    }
}

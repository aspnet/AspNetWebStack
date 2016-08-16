// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;
using Moq;
using Owin;

namespace System.Web.Http.Owin
{
    public class WebApiAppBuilderExtensionsTest
    {
        [Fact]
        public void UseWebApiWithConfiguration_IfBuilderIsNull_Throws()
        {
            // Arrange
            IAppBuilder builder = null;

            using (HttpConfiguration configuration = CreateConfiguration())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() => WebApiAppBuilderExtensions.UseWebApi(builder, configuration),
                    "builder");
            }
        }

        [Fact]
        public void UseWebApiWithConfiguration_IfConfigurationIsNull_Throws()
        {
            // Arrange
            IAppBuilder builder = CreateDummyAppBuilder();
            HttpConfiguration configuration = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => WebApiAppBuilderExtensions.UseWebApi(builder, configuration),
                "configuration");
        }

        [Fact]
        public void UseWebApiWithServer_IfBuilderIsNull_Throws()
        {
            // Arrange
            IAppBuilder builder = null;

            using (HttpServer httpServer = CreateServer())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() => WebApiAppBuilderExtensions.UseWebApi(builder, httpServer), "builder");
            }
        }

        [Fact]
        public void UseWebApiWithServer_IfServerIsNull_Throws()
        {
            // Arrange
            IAppBuilder builder = CreateDummyAppBuilder();
            HttpServer httpServer = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => WebApiAppBuilderExtensions.UseWebApi(builder, httpServer), "httpServer");
        }

        [Fact]
        public void UseWebApi_UsesAdapter()
        {
            var config = new HttpConfiguration();
            var appBuilder = new Mock<IAppBuilder>();
            appBuilder
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    It.Is<HttpMessageHandlerOptions>((o) => ((HttpServer)o.MessageHandler).Configuration == config)))
                .Returns(appBuilder.Object)
                .Verifiable();

            IAppBuilder returnedAppBuilder = appBuilder.Object.UseWebApi(config);

            Assert.Equal(appBuilder.Object, returnedAppBuilder);
            appBuilder.Verify();
        }

        [Fact]
        public async Task UseWebApi_UsesAdapterAndConfigServices()
        {
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                var config = new HttpConfiguration();
                var bufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object;
                Mock<IExceptionLogger> loggerMock = new Mock<IExceptionLogger>();
                Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>();
                config.Services.Replace(typeof(IHostBufferPolicySelector), bufferPolicySelector);
                config.Services.Replace(typeof(IExceptionLogger), loggerMock.Object);
                config.Services.Replace(typeof(IExceptionHandler), handlerMock.Object);
                IExceptionLogger exceptionLogger = null;
                IExceptionHandler exceptionHandler = null;
                var appBuilder = new Mock<IAppBuilder>();
                CancellationToken expectedAppDisposing = tokenSource.Token;
                IDictionary<string, object> properties = new Dictionary<string, object>
                {
                    { "host.OnAppDisposing", expectedAppDisposing }
                };
                appBuilder.SetupGet(b => b.Properties).Returns(properties);
                appBuilder
                    .Setup(ab => ab.Use(
                        typeof(HttpMessageHandlerAdapter),
                        It.Is<HttpMessageHandlerOptions>((o) => ((HttpServer)o.MessageHandler).Configuration == config
                            && o.BufferPolicySelector == bufferPolicySelector
                            && o.AppDisposing == expectedAppDisposing)))
                    .Callback<object, object[]>((i, args) =>
                    {
                        HttpMessageHandlerOptions options = (HttpMessageHandlerOptions)args[0];
                        exceptionLogger = options.ExceptionLogger;
                        exceptionHandler = options.ExceptionHandler;
                    })
                    .Returns(appBuilder.Object)
                    .Verifiable();

                IAppBuilder returnedAppBuilder = appBuilder.Object.UseWebApi(config);

                Assert.Equal(appBuilder.Object, returnedAppBuilder);
                appBuilder.Verify();
                await AssertDelegatesToAsync(loggerMock, exceptionLogger);
                await AssertDelegatesToAsync(handlerMock, exceptionHandler);
            }
        }

        [Fact]
        public void UseWebApiWithHttpServer_UsesAdapter()
        {
            // Arrange
            HttpServer httpServer = new Mock<HttpServer>().Object;
            Mock<IAppBuilder> appBuilderMock = new Mock<IAppBuilder>();
            appBuilderMock
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    It.Is<HttpMessageHandlerOptions>((o) => o.MessageHandler == httpServer)))
                .Returns(appBuilderMock.Object)
                .Verifiable();

            // Act
            IAppBuilder returnedAppBuilder = appBuilderMock.Object.UseWebApi(httpServer);

            // Assert
            Assert.Equal(appBuilderMock.Object, returnedAppBuilder);
            appBuilderMock.Verify();
        }

        [Fact]
        public async Task UseWebApiWithHttpServer_UsesAdapterAndConfigServices()
        {
            // Arrange
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                HttpConfiguration config = new HttpConfiguration();
                IHostBufferPolicySelector bufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object;
                Mock<IExceptionLogger> loggerMock = new Mock<IExceptionLogger>();
                Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>();
                config.Services.Replace(typeof(IHostBufferPolicySelector), bufferPolicySelector);
                config.Services.Replace(typeof(IExceptionLogger), loggerMock.Object);
                config.Services.Replace(typeof(IExceptionHandler), handlerMock.Object);
                HttpServer httpServer = new Mock<HttpServer>(config).Object;
                IExceptionLogger exceptionLogger = null;
                IExceptionHandler exceptionHandler = null;
                Mock<IAppBuilder> appBuilderMock = new Mock<IAppBuilder>();
                CancellationToken expectedAppDisposing = tokenSource.Token;
                IDictionary<string, object> properties = new Dictionary<string, object>
                {
                    { "host.OnAppDisposing", expectedAppDisposing }
                };
                appBuilderMock.SetupGet(b => b.Properties).Returns(properties);
                appBuilderMock
                    .Setup(ab => ab.Use(
                        typeof(HttpMessageHandlerAdapter),
                        It.Is<HttpMessageHandlerOptions>((o) => o.MessageHandler == httpServer
                            && o.BufferPolicySelector == bufferPolicySelector
                            && o.AppDisposing == expectedAppDisposing)))
                    .Callback<object, object[]>((i, args) =>
                    {
                        HttpMessageHandlerOptions options = (HttpMessageHandlerOptions)args[0];
                        exceptionLogger = options.ExceptionLogger;
                        exceptionHandler = options.ExceptionHandler;
                    })
                    .Returns(appBuilderMock.Object)
                    .Verifiable();

                // Act
                IAppBuilder returnedAppBuilder = appBuilderMock.Object.UseWebApi(httpServer);

                // Assert
                Assert.Equal(appBuilderMock.Object, returnedAppBuilder);
                appBuilderMock.Verify();
                await AssertDelegatesToAsync(loggerMock, exceptionLogger);
                await AssertDelegatesToAsync(handlerMock, exceptionHandler);
            }
        }

        [Fact]
        public void GetOnAppDisposingProperty_IfPropertiesIsPresent_ReturnsSpecifiedValue()
        {
            // Arrange
            using (CancellationTokenSource tokenSource = CreateCancellationTokenSource())
            {
                CancellationToken expectedOnAppDisposing = tokenSource.Token;
                IDictionary<string, object> properties = CreateStubOnAppDisposingDictionary(expectedOnAppDisposing);
                IAppBuilder builder = CreateStubAppBuilder(properties);

                // Act
                CancellationToken onAppDisposing = builder.GetOnAppDisposingProperty();

                // Assert
                Assert.Equal(expectedOnAppDisposing, onAppDisposing);
            }
        }

        [Fact]
        public void GetOnAppDisposingProperty_IfPropertiesIsNull_ReturnsCancellationTokenNone()
        {
            // Arrange
            IDictionary<string, object> properties = null;
            IAppBuilder builder = CreateStubAppBuilder(properties);

            // Act
            CancellationToken onAppDisposing = WebApiAppBuilderExtensions.GetOnAppDisposingProperty(builder);

            // Assert
            Assert.Equal(CancellationToken.None, onAppDisposing);
        }

        [Fact]
        public void GetOnAppDisposingProperty_IfPropertyIsAbsent_ReturnsCancellationTokenNone()
        {
            // Arrange
            IDictionary<string, object> properties = CreateStubOnAppDisposingDictionary(onAppDisposing: null, hasOnAppDisposing: false);
            IAppBuilder builder = CreateStubAppBuilder(properties);

            // Act
            CancellationToken onAppDisposing = WebApiAppBuilderExtensions.GetOnAppDisposingProperty(builder);

            // Assert
            Assert.Equal(CancellationToken.None, onAppDisposing);
        }

        [Fact]
        public void GetOnAppDisposingProperty_IfPropertyIsNonCancellationToken_ReturnsCancellationTokenNone()
        {
            // Arrange
            object nonCancellationToken = new object();
            IDictionary<string, object> properties = CreateStubOnAppDisposingDictionary(nonCancellationToken);
            IAppBuilder builder = CreateStubAppBuilder(properties);

            // Act
            CancellationToken onAppDisposing = WebApiAppBuilderExtensions.GetOnAppDisposingProperty(builder);

            // Assert
            Assert.Equal(CancellationToken.None, onAppDisposing);
        }

        private static async Task AssertDelegatesToAsync(Mock<IExceptionHandler> expected, IExceptionHandler actual)
        {
            Assert.NotNull(actual);

            ExceptionHandlerContext context = new ExceptionHandlerContext(new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer));
            CancellationToken cancellationToken = CancellationToken.None;

            expected
                .Setup((l) => l.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(CreateCanceledTask());

            await Assert.ThrowsAsync<TaskCanceledException>(() => actual.HandleAsync(context, cancellationToken));

            expected.Verify((l) => l.HandleAsync(context, cancellationToken), Times.Once());
        }

        private static async Task AssertDelegatesToAsync(Mock<IExceptionLogger> expected, IExceptionLogger actual)
        {
            Assert.NotNull(actual);

            ExceptionLoggerContext context = new ExceptionLoggerContext(new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer));
            CancellationToken cancellationToken = CancellationToken.None;

            expected
                .Setup((l) => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(CreateCanceledTask());

            await Assert.ThrowsAsync<TaskCanceledException>(() => actual.LogAsync(context, cancellationToken));

            expected.Verify((l) => l.LogAsync(context, cancellationToken), Times.Once());
        }

        private static Task CreateCanceledTask()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            source.SetCanceled();
            return source.Task;
        }

        private static CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static IAppBuilder CreateDummyAppBuilder()
        {
            return new Mock<IAppBuilder>(MockBehavior.Strict).Object;
        }

        private static HttpServer CreateServer()
        {
            return new HttpServer();
        }

        private static IAppBuilder CreateStubAppBuilder(IDictionary<string, object> properties)
        {
            Mock<IAppBuilder> mock = new Mock<IAppBuilder>(MockBehavior.Strict);
            mock.SetupGet(b => b.Properties).Returns(properties);
            return mock.Object;
        }

        private static IDictionary<string, object> CreateStubOnAppDisposingDictionary(object onAppDisposing)
        {
            return CreateStubOnAppDisposingDictionary(onAppDisposing, hasOnAppDisposing: true);
        }

        private static IDictionary<string, object> CreateStubOnAppDisposingDictionary(object onAppDisposing, bool hasOnAppDisposing)
        {
            Mock<IDictionary<string, object>> mock = new Mock<IDictionary<string, object>>();
            mock.Setup(d => d.TryGetValue("host.OnAppDisposing", out onAppDisposing)).Returns(hasOnAppDisposing);
            return mock.Object;
        }
    }
}

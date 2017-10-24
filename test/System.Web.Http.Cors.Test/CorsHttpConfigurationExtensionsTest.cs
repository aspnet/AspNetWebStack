// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Cors;
using System.Web.Http.Cors.Tracing;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Cors.Test
{
    public class CorsHttpConfigurationExtensionsTest
    {
        [Fact]
        public void EnableCors_NullConfig_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.EnableCors(null);
            },
            "httpConfiguration");
        }

        [Fact]
        public void EnableCors_AddsCorsMessageHandler_DuringInitializerExecution()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Clear();
            config.EnableCors();

            Assert.Empty(config.MessageHandlers);

            config.Initializer(config);

            var handler = Assert.Single(config.MessageHandlers);
            Assert.IsType<CorsMessageHandler>(handler);
        }

        [Fact]
        public void EnableCors_IsIdempotent()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Clear();
            config.EnableCors();
            config.EnableCors();
            config.EnableCors();

            Assert.Empty(config.MessageHandlers);

            config.Initializer(config);

            var handler = Assert.Single(config.MessageHandlers);
            Assert.IsType<CorsMessageHandler>(handler);
        }

        [Fact]
        public void EnableCors_Initializer_IsIdempotent()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Clear();
            config.EnableCors();

            Assert.Empty(config.MessageHandlers);

            config.Initializer(config);
            config.Initializer(config);
            config.Initializer(config);

            var handler = Assert.Single(config.MessageHandlers);
            Assert.IsType<CorsMessageHandler>(handler);
        }

        [Fact]
        public void EnableCors_AddsCorsPolicyProvider()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MessageHandlers.Clear();
            EnableCorsAttribute policyProvider = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");
            config.EnableCors(policyProvider);
            config.Initializer(config);

            ICorsPolicyProviderFactory providerFactory = config.GetCorsPolicyProviderFactory();
            AttributeBasedPolicyProviderFactory attributeProviderFactory = Assert.IsType<AttributeBasedPolicyProviderFactory>(providerFactory);
            Assert.Same(policyProvider, attributeProviderFactory.DefaultPolicyProvider);
        }

        [Fact]
        public void EnableCors_AddsTracers_WhenTracingIsEnabled()
        {
            HttpConfiguration config = new HttpConfiguration();
            ITraceWriter traceMock = new Mock<ITraceWriter>().Object;
            config.Services.Replace(typeof(ITraceWriter), traceMock);
            config.MessageHandlers.Clear();
            EnableCorsAttribute policyProvider = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");
            config.EnableCors(policyProvider);
            config.Initializer(config);

            ICorsPolicyProviderFactory providerFactory = config.GetCorsPolicyProviderFactory();
            Assert.IsType<CorsPolicyProviderFactoryTracer>(providerFactory);
        }

        [Fact]
        public void GetCorsPolicyProviderFactory_NullHttpConfiguration_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.GetCorsPolicyProviderFactory(null);
            },
            "httpConfiguration");
        }

        [Fact]
        public void GetCorsPolicyProviderFactory_ReturnsDefaultCorsPolicyProviderFactory()
        {
            HttpConfiguration config = new HttpConfiguration();
            ICorsPolicyProviderFactory providerFactory = config.GetCorsPolicyProviderFactory();

            Assert.NotNull(providerFactory);
            Assert.IsType<AttributeBasedPolicyProviderFactory>(providerFactory);
        }

        [Fact]
        public void GetCorsPolicyProviderFactory_ReturnsTheCustomCorsPolicyProviderFactory()
        {
            ICorsPolicyProviderFactory mockFactory = new Mock<ICorsPolicyProviderFactory>().Object;
            HttpConfiguration config = new HttpConfiguration();
            config.SetCorsPolicyProviderFactory(mockFactory);
            ICorsPolicyProviderFactory providerFactory = config.GetCorsPolicyProviderFactory();

            Assert.Same(mockFactory, providerFactory);
        }

        [Fact]
        public void GetCorsEngine_NullHttpConfiguration_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.GetCorsEngine(null);
            },
            "httpConfiguration");
        }

        [Fact]
        public void GetCorsEngine_ReturnsDefaultCorsEngine()
        {
            HttpConfiguration config = new HttpConfiguration();
            ICorsEngine corsEngine = config.GetCorsEngine();
            Assert.IsType<CorsEngine>(corsEngine);
        }

        [Fact]
        public void GetCorsEngine_ReturnsTheCustomCorsEngine()
        {
            ICorsEngine mockEngine = new Mock<ICorsEngine>().Object;
            HttpConfiguration config = new HttpConfiguration();
            config.SetCorsEngine(mockEngine);
            ICorsEngine corsEngine = config.GetCorsEngine();
            Assert.Same(mockEngine, corsEngine);
        }

        [Fact]
        public void SetCorsEngine_NullHttpConfiguration_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.SetCorsEngine(null, new CorsEngine());
            },
            "httpConfiguration");
        }

        [Fact]
        public void SetCorsEngine_NullCorsEngine_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.SetCorsEngine(new HttpConfiguration(), null);
            },
            "corsEngine");
        }

        [Fact]
        public void SetCorsPolicyProviderFactory_NullHttpConfiguration_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.SetCorsPolicyProviderFactory(null, new AttributeBasedPolicyProviderFactory());
            },
            "httpConfiguration");
        }

        [Fact]
        public void SetCorsPolicyProviderFactory_NullCorsEngine_Throws()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                CorsHttpConfigurationExtensions.SetCorsPolicyProviderFactory(new HttpConfiguration(), null);
            },
            "corsPolicyProviderFactory");
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Cors;
using System.Web.Http.Cors;
using System.Web.Http.Cors.Tracing;
using System.Web.Http.Tracing;

namespace System.Web.Http
{
    /// <summary>
    /// CORS-related extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CorsHttpConfigurationExtensions
    {
        private const string CorsEngineKey = "MS_CorsEngineKey";
        private const string CorsPolicyProviderFactoryKey = "MS_CorsPolicyProviderFactoryKey";
        private const string CorsEnabledKey = "MS_CorsEnabledKey";

        /// <summary>
        /// Enables the support for CORS.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        public static void EnableCors(this HttpConfiguration httpConfiguration)
        {
            EnableCors(httpConfiguration, null, false);
        }
        
        /// <summary>
        /// Enables the support for CORS.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <param name="rethrowExceptions">Indicates whether upstream exceptions should be rethrown</param>
        public static void EnableCors(this HttpConfiguration httpConfiguration, bool rethrowExceptions)
        {
            EnableCors(httpConfiguration, null, rethrowExceptions);
        }

        /// <summary>
        /// Enables the support for CORS.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <param name="defaultPolicyProvider">The default <see cref="ICorsPolicyProvider"/>.</param>
        public static void EnableCors(this HttpConfiguration httpConfiguration, ICorsPolicyProvider defaultPolicyProvider)
        {
            EnableCors(httpConfiguration, defaultPolicyProvider, false);
        }

        /// <summary>
        /// Enables the support for CORS.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <param name="defaultPolicyProvider">The default <see cref="ICorsPolicyProvider"/>.</param>
        /// <param name="rethrowExceptions">Indicates whether upstream exceptions should be rethrown</param>
        /// <exception cref="System.ArgumentNullException">httpConfiguration</exception>
        public static void EnableCors(this HttpConfiguration httpConfiguration, ICorsPolicyProvider defaultPolicyProvider,
            bool rethrowExceptions)
        {
            if (httpConfiguration == null)
            {
                throw new ArgumentNullException("httpConfiguration");
            }

            if (defaultPolicyProvider != null)
            {
                AttributeBasedPolicyProviderFactory policyProviderFactory = new AttributeBasedPolicyProviderFactory();
                policyProviderFactory.DefaultPolicyProvider = defaultPolicyProvider;
                httpConfiguration.SetCorsPolicyProviderFactory(policyProviderFactory);
            }

            AddCorsMessageHandler(httpConfiguration, rethrowExceptions);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns the disposable object")]
        private static void AddCorsMessageHandler(this HttpConfiguration httpConfiguration, bool rethrowExceptions)
        {
            object corsEnabled;
            if (!httpConfiguration.Properties.TryGetValue(CorsEnabledKey, out corsEnabled))
            {
                Action<HttpConfiguration> defaultInitializer = httpConfiguration.Initializer;
                httpConfiguration.Initializer = config =>
                {
                    if (!config.Properties.TryGetValue(CorsEnabledKey, out corsEnabled))
                    {
                        // Execute this in the Initializer to ensure that the CorsMessageHandler is added last.
                        config.MessageHandlers.Add(new CorsMessageHandler(config, rethrowExceptions));

                        ITraceWriter traceWriter = config.Services.GetTraceWriter();

                        if (traceWriter != null)
                        {
                            ICorsPolicyProviderFactory factory = config.GetCorsPolicyProviderFactory();
                            config.SetCorsPolicyProviderFactory(new CorsPolicyProviderFactoryTracer(factory, traceWriter));
                            ICorsEngine corsEngine = config.GetCorsEngine();
                            config.SetCorsEngine(new CorsEngineTracer(corsEngine, traceWriter));
                        }

                        config.Properties[CorsEnabledKey] = true;
                    }
                    defaultInitializer(config);
                };
            }
        }

        /// <summary>
        /// Sets the <see cref="ICorsEngine"/> on the <see cref="HttpConfiguration"/>.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <param name="corsEngine">The <see cref="ICorsEngine"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// httpConfiguration
        /// or
        /// corsEngine
        /// </exception>
        public static void SetCorsEngine(this HttpConfiguration httpConfiguration, ICorsEngine corsEngine)
        {
            if (httpConfiguration == null)
            {
                throw new ArgumentNullException("httpConfiguration");
            }
            if (corsEngine == null)
            {
                throw new ArgumentNullException("corsEngine");
            }

            httpConfiguration.Properties[CorsEngineKey] = corsEngine;
        }

        /// <summary>
        /// Gets the <see cref="ICorsEngine"/> from the <see cref="HttpConfiguration"/>.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <returns>The <see cref="ICorsEngine"/>.</returns>
        /// <exception cref="System.ArgumentNullException">httpConfiguration</exception>
        public static ICorsEngine GetCorsEngine(this HttpConfiguration httpConfiguration)
        {
            if (httpConfiguration == null)
            {
                throw new ArgumentNullException("httpConfiguration");
            }

            return (ICorsEngine)httpConfiguration.Properties.GetOrAdd(CorsEngineKey, k => new CorsEngine());
        }

        /// <summary>
        /// Sets the <see cref="ICorsPolicyProviderFactory"/> on the <see cref="HttpConfiguration"/>.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <param name="corsPolicyProviderFactory">The <see cref="ICorsPolicyProviderFactory"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// httpConfiguration
        /// or
        /// corsPolicyProviderFactory
        /// </exception>
        public static void SetCorsPolicyProviderFactory(this HttpConfiguration httpConfiguration, ICorsPolicyProviderFactory corsPolicyProviderFactory)
        {
            if (httpConfiguration == null)
            {
                throw new ArgumentNullException("httpConfiguration");
            }
            if (corsPolicyProviderFactory == null)
            {
                throw new ArgumentNullException("corsPolicyProviderFactory");
            }

            httpConfiguration.Properties[CorsPolicyProviderFactoryKey] = corsPolicyProviderFactory;
        }

        /// <summary>
        /// Gets the <see cref="ICorsPolicyProviderFactory"/> from the <see cref="HttpConfiguration"/>.
        /// </summary>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/>.</param>
        /// <returns>The <see cref="ICorsPolicyProviderFactory"/>.</returns>
        /// <exception cref="System.ArgumentNullException">httpConfiguration</exception>
        public static ICorsPolicyProviderFactory GetCorsPolicyProviderFactory(this HttpConfiguration httpConfiguration)
        {
            if (httpConfiguration == null)
            {
                throw new ArgumentNullException("httpConfiguration");
            }

            return (ICorsPolicyProviderFactory)httpConfiguration.Properties.GetOrAdd(CorsPolicyProviderFactoryKey, k => new AttributeBasedPolicyProviderFactory());
        }
    }
}
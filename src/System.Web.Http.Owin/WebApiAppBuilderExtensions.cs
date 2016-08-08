// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Owin;

namespace Owin
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAppBuilder"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebApiAppBuilderExtensions
    {
        private static readonly IHostBufferPolicySelector _defaultBufferPolicySelector =
            new OwinBufferPolicySelector();

        /// <summary>Adds a component to the OWIN pipeline for running a Web API endpoint.</summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure the endpoint.</param>
        /// <returns>The application builder.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "In the success path, HttpMessageHandlerAdapter owns the message handler.")]
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            HttpServer server = new HttpServer(configuration);

            try
            {
                HttpMessageHandlerOptions options = CreateOptions(builder, server, configuration);
                return UseMessageHandler(builder, options);
            }
            catch
            {
                server.Dispose();
                throw;
            }
        }

        /// <summary>Adds a component to the OWIN pipeline for running a Web API endpoint.</summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="httpServer">The http server.</param>
        /// <returns>The application builder.</returns>
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpServer httpServer)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (httpServer == null)
            {
                throw new ArgumentNullException("httpServer");
            }

            HttpConfiguration configuration = httpServer.Configuration;
            Contract.Assert(configuration != null);

            HttpMessageHandlerOptions options = CreateOptions(builder, httpServer, configuration);
            return UseMessageHandler(builder, options);
        }

        private static IAppBuilder UseMessageHandler(this IAppBuilder builder, HttpMessageHandlerOptions options)
        {
            Contract.Assert(builder != null);
            Contract.Assert(options != null);

            return builder.Use(typeof(HttpMessageHandlerAdapter), options);
        }

        private static HttpMessageHandlerOptions CreateOptions(IAppBuilder builder, HttpServer server,
            HttpConfiguration configuration)
        {
            Contract.Assert(builder != null);
            Contract.Assert(server != null);
            Contract.Assert(configuration != null);

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);

            IHostBufferPolicySelector bufferPolicySelector = services.GetHostBufferPolicySelector()
                ?? _defaultBufferPolicySelector;
            IExceptionLogger exceptionLogger = ExceptionServices.GetLogger(services);
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(services);

            return new HttpMessageHandlerOptions
            {
                MessageHandler = server,
                BufferPolicySelector = bufferPolicySelector,
                ExceptionLogger = exceptionLogger,
                ExceptionHandler = exceptionHandler,
                AppDisposing = builder.GetOnAppDisposingProperty()
            };
        }

        internal static CancellationToken GetOnAppDisposingProperty(this IAppBuilder builder)
        {
            Contract.Assert(builder != null);

            IDictionary<string, object> properties = builder.Properties;

            if (properties == null)
            {
                return CancellationToken.None;
            }

            object value;

            if (!properties.TryGetValue("host.OnAppDisposing", out value))
            {
                return CancellationToken.None;
            }

            CancellationToken? token = value as CancellationToken?;

            if (!token.HasValue)
            {
                return CancellationToken.None;
            }

            return token.Value;
        }
    }
}

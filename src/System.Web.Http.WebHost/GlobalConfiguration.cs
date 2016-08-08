// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.WebHost;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Provides a global <see cref="T:System.Web.Http.HttpConfiguration"/> for ASP.NET applications.
    /// </summary>
    public static class GlobalConfiguration
    {
        private static Lazy<HttpConfiguration> _configuration = CreateConfiguration();

        private static Lazy<HttpMessageHandler> _defaultHandler = CreateDefaultHandler();

        private static Lazy<HttpServer> _defaultServer = CreateDefaultServer();

        /// <summary>
        /// Gets the global <see cref="T:System.Web.Http.HttpConfiguration"/>.
        /// </summary>
        public static HttpConfiguration Configuration
        {
            get { return _configuration.Value; }
        }

        /// <summary>
        /// Gets the default message handler that will be called for all requests.
        /// </summary>
        public static HttpMessageHandler DefaultHandler
        {
            get { return _defaultHandler.Value; }
        }

        /// <summary>
        /// Gets the global <see cref="T:System.Web.Http.HttpServer"/>.
        /// </summary>
        public static HttpServer DefaultServer
        {
            get { return _defaultServer.Value; }
        }

        /// <summary>
        /// Performs configuration for <see cref="GlobalConfiguration.Configuration"/> and ensures that it is
        /// initialized.
        /// </summary>
        /// <param name="configurationCallback">The callback that will perform the configuration.</param>
        public static void Configure(Action<HttpConfiguration> configurationCallback)
        {
            if (configurationCallback == null)
            {
                throw new ArgumentNullException("configurationCallback");
            }

            configurationCallback.Invoke(Configuration);
            Configuration.EnsureInitialized();
        }

        internal static void Reset()
        {
            _configuration = CreateConfiguration();
            _defaultHandler = CreateDefaultHandler();
            _defaultServer = CreateDefaultServer();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000",
            Justification = "It does not appear possible for this construction code to throw.")]
        private static Lazy<HttpConfiguration> CreateConfiguration()
        {
            return new Lazy<HttpConfiguration>(() =>
            {
                HttpConfiguration config = new HttpConfiguration(new HostedHttpRouteCollection(RouteTable.Routes));
                ServicesContainer services = config.Services;
                Contract.Assert(services != null);
                services.Replace(typeof(IAssembliesResolver), new WebHostAssembliesResolver());
                services.Replace(typeof(IHttpControllerTypeResolver), new WebHostHttpControllerTypeResolver());
                services.Replace(typeof(IHostBufferPolicySelector), new WebHostBufferPolicySelector());
                services.Replace(typeof(IExceptionHandler),
                    new WebHostExceptionHandler(services.GetExceptionHandler()));
                return config;
            });
        }

        private static Lazy<HttpMessageHandler> CreateDefaultHandler()
        {
            return new Lazy<HttpMessageHandler>(() => new HttpRoutingDispatcher(_configuration.Value));
        }

        private static Lazy<HttpServer> CreateDefaultServer()
        {
            return new Lazy<HttpServer>(() => new HttpServer(_configuration.Value, _defaultHandler.Value));
        }
    }
}
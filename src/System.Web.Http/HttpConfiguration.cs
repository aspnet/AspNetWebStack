// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Services;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;

namespace System.Web.Http
{
    /// <summary>
    /// Configuration of <see cref="HttpServer"/> instances.
    /// </summary>
    public class HttpConfiguration : IDisposable
    {
        private readonly HttpRouteCollection _routes;
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();
        private readonly MediaTypeFormatterCollection _formatters;
        private readonly Collection<DelegatingHandler> _messageHandlers = new Collection<DelegatingHandler>();
        private readonly HttpFilterCollection _filters = new HttpFilterCollection();

        private IDependencyResolver _dependencyResolver = EmptyResolver.Instance;
        private Action<HttpConfiguration> _initializer = DefaultInitializer;
        private bool _initialized;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConfiguration"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The route collection is disposed as part of this class.")]
        public HttpConfiguration()
            : this(new HttpRouteCollection(String.Empty))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConfiguration"/> class.
        /// </summary>
        /// <param name="routes">The <see cref="HttpRouteCollection"/> to associate with this instance.</param>
        public HttpConfiguration(HttpRouteCollection routes)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

            _routes = routes;
            _formatters = DefaultFormatters(this);

            Services = new DefaultServices(this);
            ParameterBindingRules = DefaultActionValueBinder.GetDefaultParameterBinders();
        }

        private HttpConfiguration(HttpConfiguration configuration, HttpControllerSettings settings)
        {
            _routes = configuration.Routes;
            _filters = configuration.Filters;
            _messageHandlers = configuration.MessageHandlers;
            _properties = configuration.Properties;
            _dependencyResolver = configuration.DependencyResolver;
            IncludeErrorDetailPolicy = configuration.IncludeErrorDetailPolicy;

            // per-controller settings
            Services = settings.IsServiceCollectionInitialized ? settings.Services : configuration.Services;
            _formatters = settings.IsFormatterCollectionInitialized ? settings.Formatters : configuration.Formatters;
            ParameterBindingRules = settings.IsParameterBindingRuleCollectionInitialized ? settings.ParameterBindingRules : configuration.ParameterBindingRules;

            // Use the original configuration's initializer so that its Initialize()
            // will perform the same logic on this clone as on the original.
            Initializer = configuration.Initializer;

            // create a new validator cache if the validator providers have changed
            if (settings.IsServiceCollectionInitialized &&
                !settings.Services.GetModelValidatorProviders().SequenceEqual(configuration.Services.GetModelValidatorProviders()))
            {
                ModelValidatorCache validatorCache = new ModelValidatorCache(new Lazy<IEnumerable<ModelValidatorProvider>>(() => Services.GetModelValidatorProviders()));
                settings.Services.Replace(typeof(IModelValidatorCache), validatorCache);
            }
        }

        /// <summary>
        /// Gets or sets the action that will perform final initialization
        /// of the <see cref="HttpConfiguration"/> instance before it is used
        /// to process requests.
        /// </summary>
        /// <remarks>The Action returned by this property will be called to perform
        /// final initialization of an <see cref="HttpConfiguration"/> before it is
        /// used to process a request.
        /// <para>
        /// The <see cref="HttpConfiguration"/> passed to this action should be
        /// considered immutable after the action returns.
        /// </para>
        /// </remarks>
        public Action<HttpConfiguration> Initializer
        {
            get
            {
                return _initializer;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _initializer = value;
            }
        }

        /// <summary>
        /// Gets the list of filters that apply to all requests served using this HttpConfiguration instance.
        /// </summary>
        public HttpFilterCollection Filters
        {
            get { return _filters; }
        }

        /// <summary>
        /// Gets an ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an
        /// <see cref="HttpRequestMessage"/> travels up the stack and an <see cref="HttpResponseMessage"/> travels down in
        /// stack in return. The handlers are invoked in a top-down fashion in the incoming path and bottom-up in the outgoing 
        /// path. That is, the first entry is invoked first for an incoming request message but last for an outgoing 
        /// response message.
        /// </summary>
        /// <value>
        /// The message handler collection.
        /// </value>
        public Collection<DelegatingHandler> MessageHandlers
        {
            get { return _messageHandlers; }
        }

        /// <summary>
        /// Gets the <see cref="HttpRouteCollection"/> associated with this <see cref="HttpServer"/> instance.
        /// </summary>
        /// <value>
        /// The <see cref="HttpRouteCollection"/>.
        /// </value>
        public HttpRouteCollection Routes
        {
            get { return _routes; }
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets the root virtual path. The <see cref="VirtualPathRoot"/> property always returns 
        /// "/" as the first character of the returned value.
        /// </summary>
        public string VirtualPathRoot
        {
            get { return _routes.VirtualPathRoot; }
        }

        /// <summary>
        /// Gets or sets the dependency resolver associated with this <see cref="HttpConfiguration"/>.
        /// </summary>
        public IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolver; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _dependencyResolver = value;
            }
        }

        /// <summary>
        /// Gets the container of default services associated with this <see cref="HttpConfiguration"/>.
        /// Only supports the list of service types documented on <see cref="DefaultServices"/>. For general
        /// purpose types, please use <see cref="DependencyResolver"/>.
        /// </summary>
        public ServicesContainer Services { get; internal set; }

        /// <summary>
        /// Top level hook for how parameters should be bound. 
        /// This should be respected by the IActionValueBinder. If a parameter is not claimed by the list, the IActionValueBinder still binds it. 
        /// </summary>
        public ParameterBindingRulesCollection ParameterBindingRules { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether error details should be included in error messages.
        /// </summary>
        public IncludeErrorDetailPolicy IncludeErrorDetailPolicy { get; set; }

        /// <summary>
        /// Gets the media type formatters.
        /// </summary>
        public MediaTypeFormatterCollection Formatters
        {
            get { return _formatters; }
        }

        private static MediaTypeFormatterCollection DefaultFormatters(HttpConfiguration config)
        {
            var formatters = new MediaTypeFormatterCollection();

            // Basic FormUrlFormatter does not support binding to a T. 
            // Use our JQuery formatter instead.
            formatters.Add(new JQueryMvcFormUrlEncodedFormatter(config));

            return formatters;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns the disposable object")]
        internal static HttpConfiguration ApplyControllerSettings(HttpControllerSettings settings, HttpConfiguration configuration)
        {
            if (!settings.IsFormatterCollectionInitialized && !settings.IsParameterBindingRuleCollectionInitialized && !settings.IsServiceCollectionInitialized)
            {
                return configuration;
            }

            // Create a clone of the original configuration, including its initialization rules.
            // Invoking Initialize therefore initializes the cloned config the same way as the original.
            HttpConfiguration newConfiguration = new HttpConfiguration(configuration, settings);
            newConfiguration.Initializer(newConfiguration);
            return newConfiguration;
        }

        private static void DefaultInitializer(HttpConfiguration configuration)
        {
            // Register the default IRequiredMemberSelector for formatters that haven't been assigned one
            ModelMetadataProvider metadataProvider = configuration.Services.GetModelMetadataProvider();
            IEnumerable<ModelValidatorProvider> validatorProviders = configuration.Services.GetModelValidatorProviders();
            IRequiredMemberSelector defaultRequiredMemberSelector = new ModelValidationRequiredMemberSelector(metadataProvider, validatorProviders);

            foreach (MediaTypeFormatter formatter in configuration.Formatters)
            {
                if (formatter.RequiredMemberSelector == null)
                {
                    formatter.RequiredMemberSelector = defaultRequiredMemberSelector;
                }
            }

            // Initialize the tracing layer.
            // This must be the last initialization code to execute
            // because it alters the configuration and expects no
            // further changes.  As a default service, we know it
            // must be present.
            ITraceManager traceManager = configuration.Services.GetTraceManager();
            Contract.Assert(traceManager != null);
            traceManager.Initialize(configuration);
        }

        /// <summary>
        /// Invoke the Intializer hook. It is considered immutable from this point forward.
        /// It's safe to call this multiple times. 
        /// </summary>
        public void EnsureInitialized()
        { 
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            Initializer(this);            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    _routes.Dispose();
                    DependencyResolver.Dispose();
                }
            }
        }
    }
}

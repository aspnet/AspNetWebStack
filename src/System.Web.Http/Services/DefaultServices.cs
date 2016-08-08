// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.Services
{
    /// <summary>
    ///     <para>
    ///         Represents a container for service instances used by the <see cref="HttpConfiguration"/>. Note that
    ///         this container only supports known types, and methods to get or set arbitrary service types will
    ///         throw <see cref="ArgumentException"/> when called. For creation of arbitrary types, please use
    ///         <see cref="IDependencyResolver"/> instead. The supported types for this container are:
    ///     </para>
    ///     <list type="bullet">
    ///         <item><see cref="IActionValueBinder"/></item>
    ///         <item><see cref="IApiExplorer"/></item>
    ///         <item><see cref="IAssembliesResolver"/></item>
    ///         <item><see cref="IBodyModelValidator"/></item>
    ///         <item><see cref="IContentNegotiator"/></item>
    ///         <item><see cref="IDocumentationProvider"/></item>
    ///         <item><see cref="IExceptionHandler"/></item>
    ///         <item><see cref="IExceptionLogger"/></item>
    ///         <item><see cref="IFilterProvider"/></item>
    ///         <item><see cref="IHostBufferPolicySelector"/></item>
    ///         <item><see cref="IHttpActionInvoker"/></item>
    ///         <item><see cref="IHttpActionSelector"/></item>
    ///         <item><see cref="IHttpControllerActivator"/></item>
    ///         <item><see cref="IHttpControllerSelector"/></item>
    ///         <item><see cref="IHttpControllerTypeResolver"/></item>
    ///         <item><see cref="ITraceManager"/></item>
    ///         <item><see cref="ITraceWriter"/></item>
    ///         <item><see cref="ModelBinderProvider"/></item>
    ///         <item><see cref="ModelMetadataProvider"/></item>
    ///         <item><see cref="ModelValidatorProvider"/></item>
    ///         <item><see cref="ValueProviderFactory"/></item>
    ///     </list>
    ///     <para>
    ///         Passing any type which is not on this to any method on this interface will cause
    ///         an <see cref="ArgumentException"/> to be thrown.
    ///     </para>
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Although this class is not sealed, end users cannot set instances of it so in practice it is sealed.")]
    public class DefaultServices : ServicesContainer
    {
        private ConcurrentDictionary<Type, object[]> _cacheMulti = new ConcurrentDictionary<Type, object[]>();
        private ConcurrentDictionary<Type, object> _cacheSingle = new ConcurrentDictionary<Type, object>();
        private readonly HttpConfiguration _configuration;

        // Mutation operations delegate (throw if applied to wrong set)
        private readonly Dictionary<Type, object> _defaultServicesSingle = new Dictionary<Type, object>();

        private readonly Dictionary<Type, List<object>> _defaultServicesMulti = new Dictionary<Type, List<object>>();
        private IDependencyResolver _lastKnownDependencyResolver;
        private readonly HashSet<Type> _serviceTypesSingle;
        private readonly HashSet<Type> _serviceTypesMulti;

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        protected DefaultServices()
        {
        }

        private void SetSingle<T>(T instance) where T : class
        {
            _defaultServicesSingle[typeof(T)] = instance;
        }
        private void SetMultiple<T>(params T[] instances) where T : class
        {
            var x = (IEnumerable<object>)instances;
            _defaultServicesMulti[typeof(T)] = new List<object>(x);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class needs references to large number of types.")]
        public DefaultServices(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;

            // Initialize the dictionary with all known service types, even if the list for that service type is
            // empty, because we will throw if the developer tries to read or write unsupported types.

            SetSingle<IActionValueBinder>(new DefaultActionValueBinder());
            SetSingle<IApiExplorer>(new ApiExplorer(configuration));
            SetSingle<IAssembliesResolver>(new DefaultAssembliesResolver());
            SetSingle<IBodyModelValidator>(new DefaultBodyModelValidator());
            SetSingle<IContentNegotiator>(new DefaultContentNegotiator());
            SetSingle<IDocumentationProvider>(null); // Missing

            SetMultiple<IFilterProvider>(new ConfigurationFilterProvider(),
                                      new ActionDescriptorFilterProvider());

            SetSingle<IHostBufferPolicySelector>(null);
            SetSingle<IHttpActionInvoker>(new ApiControllerActionInvoker());
            SetSingle<IHttpActionSelector>(new ApiControllerActionSelector());
            SetSingle<IHttpControllerActivator>(new DefaultHttpControllerActivator());
            SetSingle<IHttpControllerSelector>(new DefaultHttpControllerSelector(configuration));
            SetSingle<IHttpControllerTypeResolver>(new DefaultHttpControllerTypeResolver());
            SetSingle<ITraceManager>(new TraceManager());
            SetSingle<ITraceWriter>(null);

            // This is a priority list. So put the most common binders at the top. 
            SetMultiple<ModelBinderProvider>(new TypeConverterModelBinderProvider(),
                                        new TypeMatchModelBinderProvider(),
                                        new KeyValuePairModelBinderProvider(),
                                        new ComplexModelDtoModelBinderProvider(),
                                        new ArrayModelBinderProvider(),
                                        new DictionaryModelBinderProvider(),
                                        new CollectionModelBinderProvider(),
                                        new MutableObjectModelBinderProvider());
            SetSingle<ModelMetadataProvider>(new DataAnnotationsModelMetadataProvider());
            SetMultiple<ModelValidatorProvider>(new DataAnnotationsModelValidatorProvider(),
                                        new DataMemberModelValidatorProvider());

            // This is an ordered list,so put the most common providers at the top. 
            SetMultiple<ValueProviderFactory>(new QueryStringValueProviderFactory(),
                                           new RouteDataValueProviderFactory());

            ModelValidatorCache validatorCache = new ModelValidatorCache(new Lazy<IEnumerable<ModelValidatorProvider>>(() => this.GetModelValidatorProviders()));
            SetSingle<IModelValidatorCache>(validatorCache);

            SetSingle<IExceptionHandler>(new DefaultExceptionHandler());
            SetMultiple<IExceptionLogger>();

            _serviceTypesSingle = new HashSet<Type>(_defaultServicesSingle.Keys);
            _serviceTypesMulti = new HashSet<Type>(_defaultServicesMulti.Keys);

            // Reset the caches and the known dependency scope
            ResetCache();
        }

        public override bool IsSingleService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            return _serviceTypesSingle.Contains(serviceType);
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The first instance of the service, or null if the service is not found.</returns>
        public override object GetService(Type serviceType)
        {
            // Cached read case is very performance-sensitive
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            // Invalidate the cache if the dependency scope has switched
            if (_lastKnownDependencyResolver != _configuration.DependencyResolver)
            {
                ResetCache();
            }

            object result;

            if (_cacheSingle.TryGetValue(serviceType, out result))
            {
                return result;
            }

            // Check input after initial read attempt for performance.
            if (!_serviceTypesSingle.Contains(serviceType))
            {
                throw Error.Argument("serviceType", SRResources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            // Get the service from DI. If we're coming up hot, this might
            // mean we end up creating the service more than once.
            object dependencyService = _lastKnownDependencyResolver.GetService(serviceType);

            if (!_cacheSingle.TryGetValue(serviceType, out result))
            {
                result = dependencyService ?? _defaultServicesSingle[serviceType];
                _cacheSingle.TryAdd(serviceType, result);
            }

            return result;
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The list of service instances of the given type. Returns an empty enumeration if the
        /// service is not found. </returns>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            // Cached read case is very performance-sensitive
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }

            // Invalidate the cache if the dependency scope has switched
            if (_lastKnownDependencyResolver != _configuration.DependencyResolver)
            {
                ResetCache();
            }

            object[] result;

            if (_cacheMulti.TryGetValue(serviceType, out result))
            {
                return result;
            }

            // Check input after initial read attempt for performance.
            if (!_serviceTypesMulti.Contains(serviceType))
            {
                throw Error.Argument("serviceType", SRResources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            // Get the service from DI. If we're coming up hot, this might
            // mean we end up creating the service more than once.
            IEnumerable<object> dependencyServices = _lastKnownDependencyResolver.GetServices(serviceType);

            if (!_cacheMulti.TryGetValue(serviceType, out result))
            {
                result = dependencyServices.Where(s => s != null)
                                            .Concat(_defaultServicesMulti[serviceType])
                                            .ToArray();
                _cacheMulti.TryAdd(serviceType, result);
            }

            return result;
        }

        // Returns the List<object> for the given service type. Also validates serviceType is in the known service type list.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "inherits from base")]
        protected override List<object> GetServiceInstances(Type serviceType)
        {
            Contract.Assert(serviceType != null);

            List<object> result;
            if (!_defaultServicesMulti.TryGetValue(serviceType, out result))
            {
                throw Error.Argument("serviceType", SRResources.DefaultServices_InvalidServiceType, serviceType.Name);
            }

            return result;
        }

        protected override void ClearSingle(Type serviceType)
        {
            _defaultServicesSingle[serviceType] = null;
        }

        protected override void ReplaceSingle(Type serviceType, object service)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            _defaultServicesSingle[serviceType] = service;
        }

        // Removes the cached values for all service types. Called when the dependency scope
        // has changed since the last time we made a request.
        private void ResetCache()
        {
            _cacheSingle = new ConcurrentDictionary<Type, object>();
            _cacheMulti = new ConcurrentDictionary<Type, object[]>();
            _lastKnownDependencyResolver = _configuration.DependencyResolver;
        }

        // Removes the cached values for a single service type. Called whenever the user manipulates
        // the local service list for a given service type.
        protected override void ResetCache(Type serviceType)
        {
            object single;
            _cacheSingle.TryRemove(serviceType, out single);
            object[] multiple;
            _cacheMulti.TryRemove(serviceType, out multiple);
        }
    }
}

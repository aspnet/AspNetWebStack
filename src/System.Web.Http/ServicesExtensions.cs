// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;

namespace System.Web.Http
{
    /// <summary>
    /// This provides a centralized list of type-safe accessors describing where and how we get services.
    /// This also provides a single entry point for each service request. That makes it easy
    /// to see which parts of the code use it, and provides a single place to comment usage.
    /// Accessors encapsulate usage like:
    /// <list type="bullet">
    /// <item>Type-safe using {T} instead of unsafe <see cref="System.Type"/>.</item>
    /// <item>which type do we key off? This is interesting with type hierarchies.</item>
    /// <item>do we ask for singular or plural?</item>
    /// <item>is it optional or mandatory?</item>
    /// <item>what are the ordering semantics</item>
    /// </list>
    /// Expected that any <see cref="IEnumerable{T}"/> we return is non-null, although possibly empty.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServicesExtensions
    {
        public static IEnumerable<ModelBinderProvider> GetModelBinderProviders(this ServicesContainer services)
        {
            return services.GetServices<ModelBinderProvider>();
        }

        public static ModelMetadataProvider GetModelMetadataProvider(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<ModelMetadataProvider>();
        }

        public static IEnumerable<ModelValidatorProvider> GetModelValidatorProviders(this ServicesContainer services)
        {
            return services.GetServices<ModelValidatorProvider>();
        }

        internal static IModelValidatorCache GetModelValidatorCache(this ServicesContainer services)
        {
            return services.GetService<IModelValidatorCache>();
        }

        public static IContentNegotiator GetContentNegotiator(this ServicesContainer services)
        {
            return services.GetService<IContentNegotiator>();
        }

        /// <summary>
        /// Controller activator is used to instantiate an <see cref="IHttpController"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IHttpControllerActivator"/> instance or null if none are registered.
        /// </returns>
        public static IHttpControllerActivator GetHttpControllerActivator(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHttpControllerActivator>();
        }

        public static IHttpActionSelector GetActionSelector(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHttpActionSelector>();
        }

        public static IHttpActionInvoker GetActionInvoker(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHttpActionInvoker>();
        }

        public static IActionValueBinder GetActionValueBinder(this ServicesContainer services)
        {
            return services.GetService<IActionValueBinder>();
        }

        /// <summary>
        /// Get ValueProviderFactories. The order of returned providers is the priority order that we search the factories.
        /// </summary>
        public static IEnumerable<ValueProviderFactory> GetValueProviderFactories(this ServicesContainer services)
        {
            return services.GetServices<ValueProviderFactory>();
        }

        public static IBodyModelValidator GetBodyModelValidator(this ServicesContainer services)
        {
            return services.GetService<IBodyModelValidator>();
        }

        public static IHostBufferPolicySelector GetHostBufferPolicySelector(this ServicesContainer services)
        {
            return services.GetService<IHostBufferPolicySelector>();
        }

        /// <summary>
        /// Get a controller selector, which selects an <see cref="HttpControllerDescriptor"/> given an <see cref="HttpRequestMessage"/>.
        /// </summary>
        public static IHttpControllerSelector GetHttpControllerSelector(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHttpControllerSelector>();
        }

        public static IAssembliesResolver GetAssembliesResolver(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IAssembliesResolver>();
        }

        public static IHttpControllerTypeResolver GetHttpControllerTypeResolver(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IHttpControllerTypeResolver>();
        }

        public static IApiExplorer GetApiExplorer(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<IApiExplorer>();
        }

        public static IDocumentationProvider GetDocumentationProvider(this ServicesContainer services)
        {
            return services.GetService<IDocumentationProvider>();
        }

        /// <summary>Returns the registered unhandled exception handler, if any.</summary>
        /// <param name="services">The services container.</param>
        /// <returns>
        /// The registered unhandled exception hander, if present; otherwise, <see langword="null"/>.
        /// </returns>
        public static IExceptionHandler GetExceptionHandler(this ServicesContainer services)
        {
            return services.GetService<IExceptionHandler>();
        }

        /// <summary>Returns the collection of registered unhandled exception loggers.</summary>
        /// <param name="services">The services container.</param>
        /// <returns>The collection of registered unhandled exception loggers.</returns>
        public static IEnumerable<IExceptionLogger> GetExceptionLoggers(this ServicesContainer services)
        {
            return services.GetServices<IExceptionLogger>();
        }

        public static IEnumerable<IFilterProvider> GetFilterProviders(this ServicesContainer services)
        {
            return services.GetServices<IFilterProvider>();
        }

        public static ITraceManager GetTraceManager(this ServicesContainer services)
        {
            return services.GetService<ITraceManager>();
        }

        public static ITraceWriter GetTraceWriter(this ServicesContainer services)
        {
            return services.GetService<ITraceWriter>();
        }

        internal static IEnumerable<TService> GetServices<TService>(this ServicesContainer services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            return services.GetServices(typeof(TService)).Cast<TService>();
        }

        // Runtime code shouldn't call GetService() directly. Instead, have a wrapper (like the ones above)
        // and call through the wrapper.
        private static TService GetService<TService>(this ServicesContainer services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            return (TService)services.GetService(typeof(TService));
        }

        private static T GetServiceOrThrow<T>(this ServicesContainer services)
        {
            T result = services.GetService<T>();
            if (result == null)
            {
                throw Error.InvalidOperation(SRResources.DependencyResolverNoService, typeof(T).FullName);
            }

            return result;
        }
    }
}

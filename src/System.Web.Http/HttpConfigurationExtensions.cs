// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Register that the given parameter type on an Action is to be bound using the model binder.
        /// </summary>
        /// <param name="configuration">configuration to be updated.</param>
        /// <param name="type">parameter type that binder is applied to</param>
        /// <param name="binder">a model binder</param>
        public static void BindParameter(this HttpConfiguration configuration, Type type, IModelBinder binder)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            // Add a provider so that we can use this type recursively
            // Be sure to insert at position 0 to preempt any eager binders (eg, MutableObjectBinder) that 
            // may eagerly claim all types.
            configuration.Services.Insert(typeof(ModelBinderProvider), 0, new SimpleModelBinderProvider(type, binder));

            // Add the binder to the list of rules. 
            // This ensures that the parameter binding will actually use model binding instead of Formatters.            
            // Without this, the parameter binding system may see the parameter type is complex and choose
            // to use formatters instead, in which case it would ignore the registered model binders. 
            configuration.ParameterBindingRules.Insert(0, type, param => param.BindWithModelBinding(binder));
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        // Corresponds to the MVC implementation of attribute routing in
        // System.Web.Mvc.RouteCollectionAttributeRoutingExtensions.
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            AttributeRoutingMapper.MapAttributeRoutes(configuration, new DefaultInlineConstraintResolver(), new DefaultDirectRouteProvider());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints.
        /// </param>
        // Corresponds to the MVC implementation of attribute routing in
        // System.Web.Mvc.RouteCollectionAttributeRoutingExtensions.
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration,
            IInlineConstraintResolver constraintResolver)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            AttributeRoutingMapper.MapAttributeRoutes(configuration, constraintResolver, new DefaultDirectRouteProvider());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="directRouteProvider">
        /// The <see cref="IDirectRouteProvider"/> to use for discovering and building routes.
        /// </param>
        // Corresponds to the MVC implementation of attribute routing in
        // System.Web.Mvc.RouteCollectionAttributeRoutingExtensions.
        public static void MapHttpAttributeRoutes(
            this HttpConfiguration configuration,
            IDirectRouteProvider directRouteProvider)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (directRouteProvider == null)
            {
                throw new ArgumentNullException("directRouteProvider");
            }

            AttributeRoutingMapper.MapAttributeRoutes(configuration, new DefaultInlineConstraintResolver(), directRouteProvider);
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints.
        /// </param>
        /// <param name="directRouteProvider">
        /// The <see cref="IDirectRouteProvider"/> to use for discovering and building routes.
        /// </param>
        // Corresponds to the MVC implementation of attribute routing in
        // System.Web.Mvc.RouteCollectionAttributeRoutingExtensions.
        public static void MapHttpAttributeRoutes(
            this HttpConfiguration configuration,
            IInlineConstraintResolver constraintResolver,
            IDirectRouteProvider directRouteProvider)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            if (directRouteProvider == null)
            {
                throw new ArgumentNullException("directRouteProvider");
            }

            AttributeRoutingMapper.MapAttributeRoutes(configuration, constraintResolver, directRouteProvider);
        }

        // Test Hook for inspecting the route table generated by MapHttpAttributeRoutes. 
        // MapHttpAttributeRoutes doesn't return the route collection because it's an implementation detail
        // that attr routes even generate a meaningful route collection. 
        // Public APIs can get similar functionality by querying the IHttpRoute for IReadOnlyCollection<IHttpRoute>.
        internal static IReadOnlyCollection<IHttpRoute> GetAttributeRoutes(this HttpConfiguration configuration)
        {
            configuration.EnsureInitialized();

            HttpRouteCollection routes = configuration.Routes;
            foreach (IHttpRoute route in routes)
            {
                var attrRoute = route as IReadOnlyCollection<IHttpRoute>;
                if (attrRoute != null)
                {
                    return attrRoute;
                }
            }
            return null;
        }

        /// <summary>Enables suppression of the host's principal.</summary>
        /// <param name="configuration">The server configuration.</param>
        /// <remarks>
        /// When the host's principal is suppressed, the current principal is set to anonymous upon entering the
        /// <see cref="HttpServer"/>'s first message handler. As a result, any authentication performed by the host is
        /// ignored. The remaining pipeline within the <see cref="HttpServer"/>, including
        /// <see cref="IAuthenticationFilter"/>s, is then the exclusive authority for authentication.
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Message handler should be disposed with parent configuration.")]
        public static void SuppressHostPrincipal(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Contract.Assert(configuration.MessageHandlers != null);
            configuration.MessageHandlers.Insert(0, new SuppressHostPrincipalMessageHandler());
        }
    }
}

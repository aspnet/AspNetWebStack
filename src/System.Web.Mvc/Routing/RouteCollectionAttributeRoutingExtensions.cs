// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc
{
    // Corresponds to the Web API implementation of attribute routing in System.Web.Http.HttpConfigurationExtensions.
    public static class RouteCollectionAttributeRoutingExtensions
    {
        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        public static void MapMvcAttributeRoutes(this RouteCollection routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            AttributeRoutingMapper.MapAttributeRoutes(routes, new DefaultInlineConstraintResolver());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        public static void MapMvcAttributeRoutes(this RouteCollection routes,
            IInlineConstraintResolver constraintResolver)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            AttributeRoutingMapper.MapAttributeRoutes(routes, constraintResolver);
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="directRouteProvider">
        /// The <see cref="IDirectRouteProvider"/> to use for mapping routes.
        /// </param>
        public static void MapMvcAttributeRoutes(
            this RouteCollection routes,
            IDirectRouteProvider directRouteProvider)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (directRouteProvider == null)
            {
                throw new ArgumentNullException("directRouteProvider");
            }

            AttributeRoutingMapper.MapAttributeRoutes(routes, new DefaultInlineConstraintResolver(), directRouteProvider);
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        /// <param name="directRouteProvider">
        /// The <see cref="IDirectRouteProvider"/> to use for mapping routes.
        /// </param>
        public static void MapMvcAttributeRoutes(
            this RouteCollection routes,
            IInlineConstraintResolver constraintResolver,
            IDirectRouteProvider directRouteProvider)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            if (directRouteProvider == null)
            {
                throw new ArgumentNullException("directRouteProvider");
            }

            AttributeRoutingMapper.MapAttributeRoutes(routes, constraintResolver, directRouteProvider);
        }
    }
}

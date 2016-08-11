// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    /// <remarks>
    /// Corresponds to the MVC implementation of attribute routing in System.Web.Mvc.Routing.AttributeRoutingMapper.
    /// </remarks>
    internal static class AttributeRoutingMapper
    {
        // Attribute routing will inject a single top-level route into the route table.
        private const string AttributeRouteName = "MS_attributerouteWebApi";

        public static void MapAttributeRoutes(
            HttpConfiguration configuration,
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

            RouteCollectionRoute aggregateRoute = new RouteCollectionRoute();
            configuration.Routes.Add(AttributeRouteName, aggregateRoute);

            Action<HttpConfiguration> previousInitializer = configuration.Initializer;
            configuration.Initializer = config =>
                {
                    // Chain to the previous initializer hook. Do this before we access the config since
                    // initialization may make last minute changes to the configuration.
                    previousInitializer(config);

                    SubRouteCollection subRoutes = null;

                    // Add a single placeholder route that handles all of attribute routing.
                    // Add an initialize hook that initializes these routes after the config has been initialized.
                    Func<SubRouteCollection> initializer = () =>
                    {
                        subRoutes = new SubRouteCollection();
                        AddRouteEntries(subRoutes, configuration, constraintResolver, directRouteProvider);
                        return subRoutes;
                    };

                    // This won't change config. It wants to pick up the finalized config.
                    aggregateRoute.EnsureInitialized(initializer);

                    if (subRoutes != null)
                    {
                        AddGenerationHooksForSubRoutes(config.Routes, subRoutes.Entries);
                    }
                };
        }

        // Add generation hooks for the Attribute-routing subroutes.
        // This lets us generate urls for routes supplied by attr-based routing.
        private static void AddGenerationHooksForSubRoutes(HttpRouteCollection routeTable,
            IEnumerable<RouteEntry> entries)
        {
            Contract.Assert(entries != null);
            foreach (RouteEntry entry in entries)
            {
                Contract.Assert(entry != null);
                string name = entry.Name;

                if (name != null)
                {
                    IHttpRoute route = entry.Route;
                    Contract.Assert(route != null);
                    IHttpRoute linkGenerationRoute = new LinkGenerationRoute(route);
                    routeTable.Add(name, linkGenerationRoute);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
            Justification = "This method is a coordinator, so this coupling is expected. Also fine unless building Debug configuration.")]
        private static void AddRouteEntries(
            SubRouteCollection collector,
            HttpConfiguration configuration,
            IInlineConstraintResolver constraintResolver,
            IDirectRouteProvider directRouteProvider)
        {
            Contract.Assert(configuration != null);
            Contract.Assert(directRouteProvider != null);

            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            IDictionary<string, HttpControllerDescriptor> controllerMap = controllerSelector.GetControllerMapping();
            if (controllerMap != null)
            {
                foreach (HttpControllerDescriptor controllerDescriptor in controllerMap.Values)
                {
                    IHttpActionSelector actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();

                    ILookup<string, HttpActionDescriptor> actionsByName =
                        actionSelector.GetActionMapping(controllerDescriptor);
                    if (actionsByName == null)
                    {
                        continue;
                    }

                    List<HttpActionDescriptor> actions = actionsByName.SelectMany(g => g).ToList();
                    IReadOnlyCollection<RouteEntry> newEntries =
                        directRouteProvider.GetDirectRoutes(controllerDescriptor, actions, constraintResolver);
                    if (newEntries == null)
                    {
                        throw Error.InvalidOperation(
                            SRResources.TypeMethodMustNotReturnNull,
                            typeof(IDirectRouteProvider).Name, "GetDirectRoutes");
                    }

                    foreach (RouteEntry entry in newEntries)
                    {
                        if (entry == null)
                        {
                            throw Error.InvalidOperation(
                                SRResources.TypeMethodMustNotReturnNull,
                                typeof(IDirectRouteProvider).Name, "GetDirectRoutes");
                        }

                        DirectRouteBuilder.ValidateRouteEntry(entry);

                        // We need to mark each action as only reachable by direct routes so that traditional routes
                        // don't accidentally hit them.
                        HttpControllerDescriptor routeControllerDescriptor = entry.Route.GetTargetControllerDescriptor();
                        if (routeControllerDescriptor == null)
                        {
                            HttpActionDescriptor[] actionDescriptors = entry.Route.GetTargetActionDescriptors();
                            foreach (var actionDescriptor in actionDescriptors)
                            {
                                actionDescriptor.SetIsAttributeRouted(true);
                            }
                        }
                        else
                        {
                            routeControllerDescriptor.SetIsAttributeRouted(true);
                        }
                    }

                    collector.AddRange(newEntries);
                }
            }
        }
    }
}

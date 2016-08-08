// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <remarks>
    /// Corresponds to the Web API implementation of attribute routing in
    /// System.Web.Http.Routing.AttributeRoutingMapper.
    /// </remarks>
    internal static class AttributeRoutingMapper
    {
        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        public static void MapAttributeRoutes(RouteCollection routes, IInlineConstraintResolver constraintResolver)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            MapAttributeRoutes(routes, constraintResolver, new DefaultDirectRouteProvider());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        /// <param name="directRouteProvider">
        /// The <see cref="IDirectRouteProvider"/> to use for mapping routes from controller types.
        /// </param>
        public static void MapAttributeRoutes(
            RouteCollection routes, 
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

            DefaultControllerFactory typesLocator =
                DependencyResolver.Current.GetService<IControllerFactory>() as DefaultControllerFactory
                ?? ControllerBuilder.Current.GetControllerFactory() as DefaultControllerFactory
                ?? new DefaultControllerFactory();

            IReadOnlyList<Type> controllerTypes = typesLocator.GetControllerTypes();

            MapAttributeRoutes(routes, controllerTypes, constraintResolver, directRouteProvider);
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        public static void MapAttributeRoutes(RouteCollection routes, IEnumerable<Type> controllerTypes)
        {
            MapAttributeRoutes(routes, controllerTypes, new DefaultInlineConstraintResolver());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        public static void MapAttributeRoutes(RouteCollection routes, IEnumerable<Type> controllerTypes,
            IInlineConstraintResolver constraintResolver)
        {
            MapAttributeRoutes(routes, controllerTypes, constraintResolver, new DefaultDirectRouteProvider());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        /// <param name="directRouteProvider">
        /// The <see cref="IDirectRouteProvider"/> to use for mapping routes from controller types.
        /// </param>
        public static void MapAttributeRoutes(RouteCollection routes, IEnumerable<Type> controllerTypes,
            IInlineConstraintResolver constraintResolver, IDirectRouteProvider directRouteProvider)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (controllerTypes == null)
            {
                throw new ArgumentNullException("controllerTypes");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            if (directRouteProvider == null)
            {
                throw new ArgumentNullException("directRouteProvider");
            }

            SubRouteCollection subRoutes = new SubRouteCollection();
            AddRouteEntries(subRoutes, controllerTypes, constraintResolver, directRouteProvider);
            IReadOnlyCollection<RouteEntry> entries = subRoutes.Entries;

            if (entries.Count > 0)
            {
                RouteCollectionRoute aggregrateRoute = new RouteCollectionRoute(subRoutes);
                routes.Add(aggregrateRoute);

                // This sort is here to enforce a static ordering for link generation using these routes. 
                // We don't apply dynamic criteria like ActionSelectors on link generation, but we can use the static
                // ones.
                //
                // Routes to actions are placed first because they are considered more specific. A route to an action
                // will only match for link generation if the action name was supplied, so this is essential for
                // correctness. Without this a controller-level route could be 'greedy' and generate a link when
                // the action-level route was intended.
                RouteEntry[] sorted = entries
                    .OrderBy(r => r.Route.GetOrder())
                    .ThenBy(r => r.Route.GetTargetIsAction() ? 0 : 1)
                    .ThenBy(r => r.Route.GetPrecedence())
                    .ToArray();

                AddGenerationHooksForSubRoutes(routes, sorted);
            }
        }

        // just for testing
        internal static IReadOnlyCollection<RouteEntry> GetAttributeRoutes(Type controllerType)
        {
            SubRouteCollection collector = new SubRouteCollection();
            AddRouteEntries(
                collector, 
                new Type[] { controllerType }, 
                new DefaultInlineConstraintResolver(), 
                new DefaultDirectRouteProvider());

            return collector.Entries;
        }

        // Add generation hooks for the Attribute-routing subroutes. 
        // This lets us generate urls for routes supplied by attr-based routing.
        private static void AddGenerationHooksForSubRoutes(RouteCollection routeTable, IList<RouteEntry> entries)
        {
            Contract.Assert(entries != null);

            foreach (RouteEntry entry in entries)
            {
                Contract.Assert(entry != null);
                Route route = entry.Route;
                Contract.Assert(route != null);
                RouteBase linkGenerationRoute = new LinkGenerationRoute(route);
                string name = entry.Name;

                if (name == null)
                {
                    routeTable.Add(linkGenerationRoute);
                }
                else
                {
                    routeTable.Add(name, linkGenerationRoute);
                }
            }
        }

        internal static void AddRouteEntries(SubRouteCollection collector, IEnumerable<Type> controllerTypes,
            IInlineConstraintResolver constraintResolver, IDirectRouteProvider directRouteProvider)
        {
            IEnumerable<ReflectedAsyncControllerDescriptor> controllers = GetControllerDescriptors(controllerTypes);

            foreach (ReflectedAsyncControllerDescriptor controller in controllers)
            {
                List<ActionDescriptor> actions = GetActionDescriptors(controller);

                IReadOnlyCollection<RouteEntry> entries = directRouteProvider.GetDirectRoutes(controller, actions, constraintResolver);
                if (entries == null)
                {
                    throw Error.InvalidOperation(
                        MvcResources.TypeMethodMustNotReturnNull,
                        typeof(IDirectRouteProvider).Name, 
                        "GetDirectRoutes");
                }

                foreach (RouteEntry entry in entries)
                {
                    if (entry == null)
                    {
                        throw Error.InvalidOperation(
                            MvcResources.TypeMethodMustNotReturnNull,
                            typeof(IDirectRouteProvider).Name, 
                            "GetDirectRoutes");
                    }

                    DirectRouteBuilder.ValidateRouteEntry(entry);

                    // This marks the action/controller as unreachable via traditional routing.
                    if (entry.Route.GetTargetIsAction())
                    {
                        var actionDescriptors = entry.Route.GetTargetActionDescriptors();
                        Contract.Assert(actionDescriptors != null && actionDescriptors.Any());

                        foreach (var actionDescriptor in actionDescriptors.OfType<IMethodInfoActionDescriptor>())
                        {
                            var methodInfo = actionDescriptor.MethodInfo;
                            if (methodInfo != null)
                            {
                                controller.Selector.StandardRouteMethods.Remove(methodInfo);
                            }
                        }
                    }
                    else
                    {
                        // This is a controller-level route - no actions in this controller are reachable via
                        // traditional routes.
                        controller.Selector.StandardRouteMethods.Clear();
                    }
                }

                collector.AddRange(entries);
            }
        }

        private static IEnumerable<ReflectedAsyncControllerDescriptor> GetControllerDescriptors(IEnumerable<Type> controllerTypes)
        {
            Contract.Assert(controllerTypes != null);

            Func<Type, ControllerDescriptor> descriptorFactory = ReflectedAsyncControllerDescriptor.DefaultDescriptorFactory;
            ControllerDescriptorCache descriptorsCache = new AsyncControllerActionInvoker().DescriptorCache;

            return 
                controllerTypes
                .Select(type => descriptorsCache.GetDescriptor(type, descriptorFactory, type))
                .Cast<ReflectedAsyncControllerDescriptor>();
        }

        private static List<ActionDescriptor> GetActionDescriptors(ReflectedAsyncControllerDescriptor controller)
        {
            Contract.Assert(controller != null);

            AsyncActionMethodSelector actionSelector = controller.Selector;

            var actions = new List<ActionDescriptor>();
            foreach (MethodInfo method in actionSelector.ActionMethods)
            {
                string actionName = actionSelector.GetActionName(method);
                ActionDescriptorCreator creator = actionSelector.GetActionDescriptorDelegate(method);
                Debug.Assert(creator != null);

                ActionDescriptor action = creator(actionName, controller);
                actions.Add(action);
            }

            return actions;
        }
    }
}
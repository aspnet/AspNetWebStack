// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    public class DefaultDirectRouteProvider : IDirectRouteProvider
    {
        /// <summary>
        /// Gets direct routes for the given controller descriptor and action descriptors based on 
        /// <see cref="IDirectRouteFactory"/> attributes.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="actionDescriptors">The action descriptors for all actions.</param>
        /// <param name="constraintResolver">The constraint resolver.</param>
        /// <returns>A set of route entries.</returns>
        /// <remarks>
        /// The implementation returns route entries for the given controller and actions. 
        /// 
        /// Any actions that have associated <see cref="IDirectRouteFactory"/> instances will produce route
        /// entries that route direct to those actions.
        /// 
        /// Any actions that do not have an associated <see cref="IDirectRouteFactory"/> instances will be
        /// associated with the controller. If the controller has any associated <see cref="IDirectRouteProvider"/>
        /// instances, then route entries will be created for the controller and associated actions.
        /// </remarks>
        public virtual IReadOnlyList<RouteEntry> GetDirectRoutes(
            ControllerDescriptor controllerDescriptor,
            IReadOnlyList<ActionDescriptor> actionDescriptors,
            IInlineConstraintResolver constraintResolver)
        {
            List<RouteEntry> entries = new List<RouteEntry>();

            List<ActionDescriptor> actionsWithoutRoutes = new List<ActionDescriptor>();

            foreach (ActionDescriptor action in actionDescriptors)
            {
                IReadOnlyList<IDirectRouteFactory> factories = GetActionRouteFactories(action);

                if (factories != null && factories.Count > 0)
                {
                    IReadOnlyCollection<RouteEntry> actionEntries = GetActionDirectRoutes(action, factories, constraintResolver);
                    if (actionEntries != null)
                    {
                        entries.AddRange(actionEntries);
                    }
                }
                else
                {
                    // IF there are no routes on the specific action, attach it to the controller routes (if any).
                    actionsWithoutRoutes.Add(action);
                }
            }

            if (actionsWithoutRoutes.Count > 0)
            {
                IReadOnlyList<IDirectRouteFactory> controllerFactories = GetControllerRouteFactories(controllerDescriptor);
                if (controllerFactories != null && controllerFactories.Count > 0)
                {
                    IReadOnlyCollection<RouteEntry> controllerEntries = GetControllerDirectRoutes(
                        controllerDescriptor,
                        actionsWithoutRoutes,
                        controllerFactories,
                        constraintResolver);

                    if (controllerEntries != null)
                    {
                        entries.AddRange(controllerEntries);
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// Gets route factories for the given controller descriptor.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>A set of route factories.</returns>
        /// <remarks>
        /// The implementation returns <see cref="IDirectRouteFactory"/> instances based on attributes on the controller.
        /// </remarks>
        protected virtual IReadOnlyList<IDirectRouteFactory> GetControllerRouteFactories(ControllerDescriptor controllerDescriptor)
        {
            object[] attributes = controllerDescriptor.GetCustomAttributes(inherit: false);
            IEnumerable<IDirectRouteFactory> newFactories = attributes.OfType<IDirectRouteFactory>();
            IEnumerable<IRouteInfoProvider> oldProviders = attributes.OfType<IRouteInfoProvider>();

            List<IDirectRouteFactory> combined = new List<IDirectRouteFactory>();
            combined.AddRange(newFactories);

            foreach (IRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteFactory)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteFactory(oldProvider));
            }

            return combined;
        }

        /// <summary>
        /// Gets a set of route factories for the given action descriptor.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A set of route factories.</returns>
        /// <remarks>
        /// The implementation returns <see cref="IDirectRouteFactory"/> instances based on attributes on the action. Returns
        /// null if the action was defined on a base class of this controller.
        /// </remarks>
        protected virtual IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(ActionDescriptor actionDescriptor)
        {
            // Skip Route attributes on inherited actions.
            IMethodInfoActionDescriptor methodInfoActionDescriptor = actionDescriptor as IMethodInfoActionDescriptor;
            if (methodInfoActionDescriptor != null &&
                methodInfoActionDescriptor.MethodInfo != null &&
                actionDescriptor.ControllerDescriptor != null &&
                methodInfoActionDescriptor.MethodInfo.DeclaringType != actionDescriptor.ControllerDescriptor.ControllerType)
            {
                return null;
            }

            object[] attributes = actionDescriptor.GetCustomAttributes(inherit: false);
            IEnumerable<IDirectRouteFactory> newFactories = attributes.OfType<IDirectRouteFactory>();
            IEnumerable<IRouteInfoProvider> oldProviders = attributes.OfType<IRouteInfoProvider>();

            List<IDirectRouteFactory> combined = new List<IDirectRouteFactory>();
            combined.AddRange(newFactories);

            foreach (IRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteFactory)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteFactory(oldProvider));
            }

            return combined;
        }

        /// <summary>
        /// Creates <see cref="RouteEntry"/> instances based on the provided factories, controller and actions. The route
        /// entries provided direct routing to the provided controller and can reach the set of provided actions.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="actionDescriptors">The action descriptors.</param>
        /// <param name="factories">The direct route factories.</param>
        /// <param name="constraintResolver">The constraint resolver.</param>
        /// <returns>A set of route entries.</returns>
        protected virtual IReadOnlyList<RouteEntry> GetControllerDirectRoutes(
            ControllerDescriptor controllerDescriptor,
            IReadOnlyList<ActionDescriptor> actionDescriptors,
            IReadOnlyList<IDirectRouteFactory> factories,
            IInlineConstraintResolver constraintResolver)
        {
            return CreateRouteEntries(
                GetAreaPrefix(controllerDescriptor),
                GetRoutePrefix(controllerDescriptor),
                factories,
                actionDescriptors,
                constraintResolver,
                targetIsAction: false);
        }

        /// <summary>
        /// Creates <see cref="RouteEntry"/> instances based on the provided factories and action. The route entries
        /// provide direct routing to the provided action.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="factories">The direct route factories.</param>
        /// <param name="constraintResolver">The constraint resolver.</param>
        /// <returns>A set of route entries.</returns>
        protected virtual IReadOnlyList<RouteEntry> GetActionDirectRoutes(
            ActionDescriptor actionDescriptor,
            IReadOnlyList<IDirectRouteFactory> factories,
            IInlineConstraintResolver constraintResolver)
        {
            return CreateRouteEntries(
                GetAreaPrefix(actionDescriptor.ControllerDescriptor),
                GetRoutePrefix(actionDescriptor.ControllerDescriptor),
                factories,
                new ActionDescriptor[] { actionDescriptor },
                constraintResolver,
                targetIsAction: true);
        }

        /// <summary>
        /// Gets the route prefix from the provided controller.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>The route prefix or null.</returns>
        protected virtual string GetRoutePrefix(ControllerDescriptor controllerDescriptor)
        {
            IRoutePrefix[] attributes = controllerDescriptor.GetCustomAttributes(inherit: false).OfType<IRoutePrefix>().ToArray();

            if (attributes == null)
            {
                return null;
            }

            if (attributes.Length > 1)
            {
                string errorMessage = Error.Format(
                    MvcResources.RoutePrefix_CannotSupportMultiRoutePrefix,
                    controllerDescriptor.ControllerType.FullName);
                throw new InvalidOperationException(errorMessage);
            }

            if (attributes.Length == 1)
            {
                IRoutePrefix attribute = attributes[0];

                if (attribute != null)
                {
                    string prefix = attribute.Prefix;
                    if (prefix == null)
                    {
                        string errorMessage = Error.Format(
                            MvcResources.RoutePrefix_PrefixCannotBeNull,
                            controllerDescriptor.ControllerType.FullName);
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (prefix.StartsWith("/", StringComparison.Ordinal)
                        || prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        string errorMessage = Error.Format(
                            MvcResources.RoutePrefix_CannotStartOrEnd_WithForwardSlash, prefix,
                            controllerDescriptor.ControllerName);
                        throw new InvalidOperationException(errorMessage);
                    }

                    return prefix;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the area prefix from the provided controller.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>The area prefix or null.</returns>
        protected virtual string GetAreaPrefix(ControllerDescriptor controllerDescriptor)
        {
            RouteAreaAttribute area = controllerDescriptor.GetAreaFrom();
            string areaName = controllerDescriptor.GetAreaName(area);
            string areaPrefix = area != null ? area.AreaPrefix ?? area.AreaName : null;

            ValidateAreaPrefixTemplate(areaPrefix, areaName, controllerDescriptor);

            return areaPrefix;
        }

        private static IReadOnlyList<RouteEntry> CreateRouteEntries(
            string areaPrefix,
            string controllerPrefix,
            IReadOnlyCollection<IDirectRouteFactory> factories,
            IReadOnlyCollection<ActionDescriptor> actions,
            IInlineConstraintResolver constraintResolver,
            bool targetIsAction)
        {
            List<RouteEntry> entries = new List<RouteEntry>();
            foreach (IDirectRouteFactory factory in factories)
            {
                RouteEntry entry = CreateRouteEntry(areaPrefix, controllerPrefix, factory, actions, constraintResolver, targetIsAction);
                entries.Add(entry);
            }

            return entries;
        }

        // Internal for testing
        internal static RouteEntry CreateRouteEntry(
            string areaPrefix,
            string controllerPrefix,
            IDirectRouteFactory factory,
            IReadOnlyCollection<ActionDescriptor> actions,
            IInlineConstraintResolver constraintResolver,
            bool targetIsAction)
        {
            Contract.Assert(factory != null);

            DirectRouteFactoryContext context = new DirectRouteFactoryContext(
                areaPrefix, 
                controllerPrefix, 
                actions, 
                constraintResolver, 
                targetIsAction);

            RouteEntry entry = factory.CreateRoute(context);

            if (entry == null)
            {
                throw Error.InvalidOperation(
                    MvcResources.TypeMethodMustNotReturnNull,
                    typeof(IDirectRouteFactory).Name, 
                    "CreateRoute");
            }

            DirectRouteBuilder.ValidateRouteEntry(entry);

            return entry;
        }

        private static void ValidateAreaPrefixTemplate(string areaPrefix, string areaName, ControllerDescriptor controllerDescriptor)
        {
            if (areaPrefix != null && areaPrefix.EndsWith("/", StringComparison.Ordinal))
            {
                string errorMessage = Error.Format(MvcResources.RouteAreaPrefix_CannotEnd_WithForwardSlash,
                                                   areaPrefix, areaName, controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}

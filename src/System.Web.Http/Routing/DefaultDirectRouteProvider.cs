// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// A default implementation of <see cref="IDirectRouteProvider"/>.
    /// </summary>
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
            HttpControllerDescriptor controllerDescriptor, 
            IReadOnlyList<HttpActionDescriptor> actionDescriptors,
            IInlineConstraintResolver constraintResolver)
        {
            List<RouteEntry> entries = new List<RouteEntry>();

            List<HttpActionDescriptor> actionsWithoutRoutes = new List<HttpActionDescriptor>();

            foreach (HttpActionDescriptor action in actionDescriptors)
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
        protected virtual IReadOnlyList<IDirectRouteFactory> GetControllerRouteFactories(HttpControllerDescriptor controllerDescriptor)
        {
            Collection<IDirectRouteFactory> newFactories = controllerDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: false);

            Collection<IHttpRouteInfoProvider> oldProviders = controllerDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            List<IDirectRouteFactory> combined = new List<IDirectRouteFactory>();
            combined.AddRange(newFactories);

            foreach (IHttpRouteInfoProvider oldProvider in oldProviders)
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
        protected virtual IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
        {
            // Ignore the Route attributes from inherited actions.
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null &&
                reflectedActionDescriptor.MethodInfo != null &&
                reflectedActionDescriptor.MethodInfo.DeclaringType != actionDescriptor.ControllerDescriptor.ControllerType)
            {
                return null;
            }

            Collection<IDirectRouteFactory> newFactories = actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: false);

            Collection<IHttpRouteInfoProvider> oldProviders = actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            List<IDirectRouteFactory> combined = new List<IDirectRouteFactory>();
            combined.AddRange(newFactories);

            foreach (IHttpRouteInfoProvider oldProvider in oldProviders)
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
            HttpControllerDescriptor controllerDescriptor,
            IReadOnlyList<HttpActionDescriptor> actionDescriptors,
            IReadOnlyList<IDirectRouteFactory> factories,
            IInlineConstraintResolver constraintResolver)
        {
            return CreateRouteEntries(
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
            HttpActionDescriptor actionDescriptor, 
            IReadOnlyList<IDirectRouteFactory> factories,
            IInlineConstraintResolver constraintResolver)
        {
            return CreateRouteEntries(
                GetRoutePrefix(actionDescriptor.ControllerDescriptor), 
                factories, 
                new HttpActionDescriptor[] { actionDescriptor }, 
                constraintResolver, 
                targetIsAction: true);
        }

        /// <summary>
        /// Gets the route prefix from the provided controller.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>The route prefix or null.</returns>
        protected virtual string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            Collection<IRoutePrefix> attributes = controllerDescriptor.GetCustomAttributes<IRoutePrefix>(inherit: false);

            if (attributes == null)
            {
                return null;
            }

            if (attributes.Count > 1)
            {
                string errorMessage = Error.Format(SRResources.RoutePrefix_CannotSupportMultiRoutePrefix, controllerDescriptor.ControllerType.FullName);
                throw new InvalidOperationException(errorMessage);
            }

            if (attributes.Count == 1)
            {
                IRoutePrefix attribute = attributes[0];

                if (attribute != null)
                {
                    string prefix = attribute.Prefix;
                    if (prefix == null)
                    {
                        string errorMessage = Error.Format(
                            SRResources.RoutePrefix_PrefixCannotBeNull,
                            controllerDescriptor.ControllerType.FullName);
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidPrefix, prefix,
                            controllerDescriptor.ControllerName);
                    }

                    return prefix;
                }
            }

            return null;
        }

        private static IReadOnlyList<RouteEntry> CreateRouteEntries(
            string prefix,
            IReadOnlyCollection<IDirectRouteFactory> factories,
            IReadOnlyCollection<HttpActionDescriptor> actions, 
            IInlineConstraintResolver constraintResolver, 
            bool targetIsAction)
        {
            List<RouteEntry> entries = new List<RouteEntry>();
            foreach (IDirectRouteFactory factory in factories)
            {
                RouteEntry entry = CreateRouteEntry(prefix, factory, actions, constraintResolver, targetIsAction);
                entries.Add(entry);
            }

            return entries;
        }

        private static RouteEntry CreateRouteEntry(
            string prefix,
            IDirectRouteFactory factory,
            IReadOnlyCollection<HttpActionDescriptor> actions,
            IInlineConstraintResolver constraintResolver,
            bool targetIsAction)
        {
            Contract.Assert(factory != null);

            DirectRouteFactoryContext context = new DirectRouteFactoryContext(prefix, actions, constraintResolver, targetIsAction);
            RouteEntry entry = factory.CreateRoute(context);

            if (entry == null)
            {
                throw Error.InvalidOperation(SRResources.TypeMethodMustNotReturnNull,
                    typeof(IDirectRouteFactory).Name, "CreateRoute");
            }

            DirectRouteBuilder.ValidateRouteEntry(entry);

            return entry;
        }
    }
}

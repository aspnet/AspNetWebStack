// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    internal static class DirectRouteExtensions
    {
        /// <summary>
        /// Gets the precedence or this route.
        /// </summary>
        public static decimal GetPrecedence(this RouteData routeData)
        {
            return GetRouteDataTokenValue<decimal>(routeData, RouteDataTokenKeys.Precedence);
        }

        /// <summary>
        /// Gets the precedence or this route.
        /// </summary>
        public static decimal GetPrecedence(this Route route)
        {
            return GetRouteDataTokenValue<decimal>(route, RouteDataTokenKeys.Precedence);
        }

        /// <summary>
        /// Sets the precedence of this Route.
        /// </summary>
        public static void SetPrecedence(this Route route, decimal precedence)
        {
            SetRouteDataTokenValue(route, RouteDataTokenKeys.Precedence, precedence);
        }

        /// <summary>
        /// Gets the order or this route.
        /// </summary>
        public static int GetOrder(this RouteData routeData)
        {
            return GetRouteDataTokenValue<int>(routeData, RouteDataTokenKeys.Order);
        }

        /// <summary>
        /// Gets the order or this route.
        /// </summary>
        public static int GetOrder(this Route route)
        {
            return GetRouteDataTokenValue<int>(route, RouteDataTokenKeys.Order);
        }

        /// <summary>
        /// Sets the order of this Route.
        /// </summary>
        public static void SetOrder(this Route route, int order)
        {
            SetRouteDataTokenValue(route, RouteDataTokenKeys.Order, order);
        }

        /// <summary>
        /// Gets a value indicating whether or not the route is a direct route to an action.
        /// </summary>
        public static bool GetTargetIsAction(this RouteData routeData)
        {
            return GetRouteDataTokenValue<bool>(routeData, RouteDataTokenKeys.TargetIsAction);
        }

        /// <summary>
        /// Gets a value indicating whether or not the route is a direct route to an action.
        /// </summary>
        public static bool GetTargetIsAction(this Route route)
        {
            return GetRouteDataTokenValue<bool>(route, RouteDataTokenKeys.TargetIsAction);
        }

        /// <summary>
        /// Sets a value indicating whether or not the route is a direct route to an action.
        /// </summary>
        public static void SetTargetIsAction(this Route route, bool targetIsAction)
        {
            SetRouteDataTokenValue(route, RouteDataTokenKeys.TargetIsAction, targetIsAction);
        }

        /// <summary>
        /// Gets the ControllerDescriptor that matches this Route.
        /// </summary>
        public static ControllerDescriptor GetTargetControllerDescriptor(this Route route)
        {
            var actions = GetTargetActionDescriptors(route);
            ControllerDescriptor controller = null;

            foreach (var action in actions)
            {
                if (controller == null)
                {
                    controller = action.ControllerDescriptor;
                }
                else if (controller != action.ControllerDescriptor)
                {
                    // Don't provide a single controller descriptor if multiple controllers match.
                    return null;
                }
            }

            return controller;
        }

        /// <summary>
        /// Gets the ControllerDescriptor that matches this Route.
        /// </summary>
        public static ControllerDescriptor GetTargetControllerDescriptor(this RouteData routeData)
        {
            var actions = GetTargetActionDescriptors(routeData);
            ControllerDescriptor controller = null;

            foreach (var action in actions)
            {
                if (controller == null)
                {
                    controller = action.ControllerDescriptor;
                }
                else if (controller != action.ControllerDescriptor)
                {
                    // Don't provide a single controller descriptor if multiple controllers match.
                    return null;
                }
            }

            return controller;
        }

        public static Type GetTargetControllerType(this RouteData routeData)
        {
            ControllerDescriptor controllerDescriptor = routeData.GetTargetControllerDescriptor();
            return controllerDescriptor == null ? null : controllerDescriptor.ControllerType;
        }

        /// <summary>
        /// Gets the target actions that can be matched if this route is matched.
        /// </summary>
        public static ActionDescriptor[] GetTargetActionDescriptors(this RouteData routeData)
        {
            return GetRouteDataTokenValue<ActionDescriptor[]>(routeData, RouteDataTokenKeys.Actions);
        }

        /// <summary>
        /// Gets the target actions that can be matched if this route is matched.
        /// </summary>
        public static ActionDescriptor[] GetTargetActionDescriptors(this Route route)
        {
            return GetRouteDataTokenValue<ActionDescriptor[]>(route, RouteDataTokenKeys.Actions);
        }

        /// <summary>
        /// Sets the target actions that can be matched if this route is matched.
        /// </summary>
        public static void SetTargetActionDescriptors(this Route route, ActionDescriptor[] actionDescriptors)
        {
            if (actionDescriptors == null || actionDescriptors.Length == 0)
            {
                throw Error.ParameterCannotBeNullOrEmpty("actionDescriptors");
            }

            SetRouteDataTokenValue(route, RouteDataTokenKeys.Actions, actionDescriptors);
        }

        public static bool HasDirectRouteMatch(this RouteData routeData)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            return routeData.Values.ContainsKey(RouteDataTokenKeys.DirectRouteMatches);
        }

        public static IEnumerable<RouteData> GetDirectRouteMatches(this RouteData routeData)
        {
            return GetRouteDataValue<IEnumerable<RouteData>>(routeData, RouteDataTokenKeys.DirectRouteMatches) ?? Enumerable.Empty<RouteData>();
        }

        public static void SetDirectRouteMatches(this RouteData routeData, IEnumerable<RouteData> matches)
        {
            if (matches == null || !matches.Any())
            {
                throw Error.ParameterCannotBeNullOrEmpty("matches");
            }

            SetRouteDataValue(routeData, RouteDataTokenKeys.DirectRouteMatches, matches);
        }

        public static bool IsDirectRoute(this RouteBase routeBase)
        {
            Route route = routeBase as Route;
            if (route == null)
            {
                return false;
            }
            else
            {
                // All direct routes need to have actions associated.
                return route.GetTargetActionDescriptors() != null;
            }
        }

        private static T GetRouteDataTokenValue<T>(this Route route, string key)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            T value;
            if (route.DataTokens != null && route.DataTokens.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(T);
            }
        }

        private static T GetRouteDataTokenValue<T>(this RouteData routeData, string key)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            return GetRouteDataTokenValue<T>(routeData.Route as Route, key);
        }

        private static void SetRouteDataTokenValue<T>(this Route route, string key, T value)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            route.DataTokens[key] = value;
        }

        private static T GetRouteDataValue<T>(this RouteData routeData, string key)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            T value;
            if (routeData.Values.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(T);
            }
        }

        private static void SetRouteDataValue<T>(this RouteData routeData, string key, T value)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            routeData.Values[key] = value;
        }
    }
}
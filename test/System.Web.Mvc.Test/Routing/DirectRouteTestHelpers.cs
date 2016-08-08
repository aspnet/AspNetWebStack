// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Mvc.Routing;

namespace System.Web.Routing.Test
{
    internal static class DirectRouteTestHelpers
    {
        public static RouteCollectionRoute BuildDirectRouteFromMethod<T>(Expression<Action<T>> methodCall)
        {
            SubRouteCollection collector = new SubRouteCollection();
            AddDirectRouteFromMethod(collector, methodCall);
            return new RouteCollectionRoute(collector);
        }

        public static void AddDirectRouteFromMethod<T>(SubRouteCollection collector, Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            var attributes = method.GetCustomAttributes(false).OfType<IRouteInfoProvider>();

            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(method.DeclaringType);
            var actionDescriptor = new ReflectedActionDescriptor(method, method.Name, controllerDescriptor);

            foreach (var attribute in attributes)
            {
                var subRoute = new Route(attribute.Template, routeHandler: null);
                subRoute.SetTargetActionDescriptors(new ActionDescriptor[] { actionDescriptor });
                collector.Add(new RouteEntry(null, subRoute));
            }
        }

        public static RouteCollectionRoute BuildDirectRouteFromController<T>()
        {
            SubRouteCollection collector = new SubRouteCollection();
            AddDirectRouteFromController<T>(collector);
            return new RouteCollectionRoute(collector);
        }

        public static void AddDirectRouteFromController<T>(SubRouteCollection collector)
        {
            var controllerType = typeof(T);
            AttributeRoutingMapper.AddRouteEntries(
                collector, 
                new Type[] { controllerType },
                new DefaultInlineConstraintResolver(),
                new DefaultDirectRouteProvider());
        }

        public static void AddDirectRouteMatches(this RouteData routeData, Func<RouteBase, RouteData, bool> selector = null)
        {
            RouteCollectionRoute route = (RouteCollectionRoute)routeData.Route;

            List<RouteData> matches = new List<RouteData>();
            foreach (var subRoute in route)
            {
                RouteData match = new RouteData() { Route = subRoute };
                bool isMatch = selector == null ? true : selector(subRoute, match);
                if (isMatch)
                {
                    matches.Add(match);
                }
            }

            if (matches.Any())
            {
                routeData.SetDirectRouteMatches(matches);
            }
        }
    }
}
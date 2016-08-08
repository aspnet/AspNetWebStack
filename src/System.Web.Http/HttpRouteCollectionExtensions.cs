// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Batch;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpRouteCollection"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRouteCollectionExtensions
    {
        /// <summary>
        /// Maps the specified route template.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults: null, constraints: null, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default constraints.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults, constraints: null, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default route values and constraints.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults, constraints, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default route values, constraints, and end-point message handler.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        /// <param name="handler">The handler to which the request will be dispatched.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

            HttpRouteValueDictionary defaultsDictionary = new HttpRouteValueDictionary(defaults);
            HttpRouteValueDictionary constraintsDictionary = new HttpRouteValueDictionary(constraints);
            IHttpRoute route = routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary, dataTokens: null, handler: handler);
            routes.Add(name, route);
            return route;
        }

        /// <summary>
        /// Maps the specified route for handling HTTP batch requests.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="batchHandler">The <see cref="HttpBatchHandler"/> for handling batch requests.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want the handler to be a batch handler.")]
        public static IHttpRoute MapHttpBatchRoute(this HttpRouteCollection routes, string routeName, string routeTemplate, HttpBatchHandler batchHandler)
        {
            return routes.MapHttpRoute(routeName, routeTemplate, defaults: null, constraints: null, handler: batchHandler);
        }

        /// <summary>
        /// Ignores the specified route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to ignore.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        public static IHttpRoute IgnoreRoute(this HttpRouteCollection routes, string routeName, string routeTemplate)
        {
            return IgnoreRoute(routes, routeName, routeTemplate, constraints: null);
        }

        /// <summary>
        /// Ignores the specified route.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="routeName">The name of the route to ignore.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "The handler instance is owned by the Route. StopRoutingHandler.Dispose() doesn't do anything, so we don't care if we throw and fail to dispose it. Currently it will never be disposed, see https://aspnetwebstack.codeplex.com/workitem/1393.")]
        public static IHttpRoute IgnoreRoute(this HttpRouteCollection routes, string routeName, string routeTemplate, object constraints)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (routeName == null)
            {
                throw new ArgumentNullException("routeName");
            }
            if (routeTemplate == null)
            {
                throw new ArgumentNullException("routeTemplate");
            }

            IgnoreHttpRouteInternal route = new IgnoreHttpRouteInternal(routeTemplate, new HttpRouteValueDictionary(constraints), new StopRoutingHandler());
            routes.Add(routeName, route);
            return route;
        }

        private sealed class IgnoreHttpRouteInternal : HttpRoute
        {
            public IgnoreHttpRouteInternal(string routeTemplate, HttpRouteValueDictionary constraints, HttpMessageHandler handler)
                : base(routeTemplate, constraints: constraints, handler: handler, dataTokens: null, defaults: null)
            {
            }

            public override IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
            {
                // Never match during route generation. This avoids the scenario where an IgnoreRoute with
                // fairly relaxed constraints ends up eagerly matching all generated URLs.
                return null;
            }
        }
    }
}

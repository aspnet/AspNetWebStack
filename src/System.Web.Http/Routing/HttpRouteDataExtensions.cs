// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Web.Http.Routing
{
    public static class HttpRouteDataExtensions
    {
        /// <summary>
        /// Remove all optional parameters that do not have a value from the route data. 
        /// </summary>
        /// <param name="routeData">route data, to be mutated in-place.</param>
        public static void RemoveOptionalRoutingParameters(this IHttpRouteData routeData)
        {
            RemoveOptionalRoutingParameters(routeData.Values);

            var subRouteData = routeData.GetSubRoutes();
            if (subRouteData != null)
            {
                foreach (IHttpRouteData sub in subRouteData)
                {
                    RemoveOptionalRoutingParameters(sub);
                }
            }
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValueDictionary)
        {
            Contract.Assert(routeValueDictionary != null);

            // Get all keys for which the corresponding value is 'Optional'.
            // Having a separate array is necessary so that we don't manipulate the dictionary while enumerating.
            // This is on a hot-path and linq expressions are showing up on the profile, so do array manipulation.
            int max = routeValueDictionary.Count;
            int i = 0;
            string[] matching = new string[max];
            foreach (KeyValuePair<string, object> kv in routeValueDictionary)
            {
                if (kv.Value == RouteParameter.Optional)
                {
                    matching[i] = kv.Key;
                    i++;
                }
            }
            for (int j = 0; j < i; j++)
            {
                string key = matching[j];
                routeValueDictionary.Remove(key);
            }
        }

        /// <summary>
        /// If a route is really a union of other routes, return the set of sub routes. 
        /// </summary>
        /// <param name="routeData">a union route data</param>
        /// <returns>set of sub soutes contained within this route</returns>
        public static IEnumerable<IHttpRouteData> GetSubRoutes(this IHttpRouteData routeData)
        {
            IHttpRouteData[] subRoutes = null;
            if (routeData.Values.TryGetValue(RouteCollectionRoute.SubRouteDataKey, out subRoutes))
            {
                return subRoutes;
            }
            return null;
        }

        // If routeData is from an attribute route, get the action descriptors, order and precedence that it may match
        // to. Caller still needs to run action selection to pick the specific action.
        // Else return null.
        internal static CandidateAction[] GetDirectRouteCandidates(this IHttpRouteData routeData)
        {
            Contract.Assert(routeData != null);
            IEnumerable<IHttpRouteData> subRoutes = routeData.GetSubRoutes();

            // Possible this is being called on a subroute. This can happen after ElevateRouteData. Just chain. 
            if (subRoutes == null)
            {
                if (routeData.Route == null)
                {
                    // If the matched route is a System.Web.Routing.Route (in web host) then routeData.Route
                    // will be null. Normally a System.Web.Routing.Route match would go through an MVC handler
                    // but we can get here through HttpRoutingDispatcher in WebAPI batching. If that happens, 
                    // then obviously it's not a WebAPI attribute routing match.
                    return null;
                }
                else
                {
                    return routeData.Route.GetDirectRouteCandidates();
                }
            }

            var list = new List<CandidateAction>();

            foreach (IHttpRouteData subData in subRoutes)
            {
                CandidateAction[] candidates = subData.Route.GetDirectRouteCandidates();
                if (candidates != null)
                {
                    list.AddRange(candidates);
                }
            }
            return list.ToArray();
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// A single route that is the composite of multiple "sub routes".  
    /// </summary>
    /// <remarks>
    /// Corresponds to the Web API implementation of attribute routing in System.Web.Http.Routing.RouteCollectionRoute.
    /// </remarks>
    internal class RouteCollectionRoute : RouteBase, IReadOnlyCollection<RouteBase>
    {
        private readonly IReadOnlyCollection<RouteBase> _subRoutes;

        public RouteCollectionRoute(IReadOnlyCollection<RouteBase> subRoutes)
        {
            if (subRoutes == null)
            {
                throw new ArgumentNullException("subRoutes");
            }

            _subRoutes = subRoutes;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            List<RouteData> matches = new List<RouteData>();
            foreach (RouteBase route in _subRoutes)
            {
                var match = route.GetRouteData(httpContext);
                if (match != null)
                {
                    matches.Add(match);
                }
            }

            return CreateDirectRouteMatch(this, matches);
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            // Link generation is not supported via the RouteCollectionRoute - see LinkGenerationRoute.
            return null;
        }

        public int Count
        {
            get { return _subRoutes.Count; }
        }

        public IEnumerator<RouteBase> GetEnumerator()
        {
            return _subRoutes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _subRoutes.GetEnumerator();
        }

        public static RouteData CreateDirectRouteMatch(RouteBase route, List<RouteData> matches)
        {
            if (matches.Count == 0)
            {
                return null;
            }
            else
            {
                var routeData = new RouteData();
                routeData.Route = route;
                routeData.RouteHandler = new MvcRouteHandler();
                routeData.SetDirectRouteMatches(matches);

                // At a few points in the code (MvcRouteHandler, MvcHandler) we need to look up the controller
                // by name. For the purposes of error handling/debugging, it's helpful if we can have a name
                // in this code to pass through.
                //
                // Inside the DefaultControllerFactory we'll double check the route data and throw if we have
                // multiple controller matches, but for now let's just use the controller of the first match.
                ControllerDescriptor controllerDescriptor = matches[0].GetTargetControllerDescriptor();
                if (controllerDescriptor != null)
                {
                    routeData.Values[RouteDataTokenKeys.Controller] = controllerDescriptor.ControllerName;
                }

                return routeData;
            }
        }
    }
}
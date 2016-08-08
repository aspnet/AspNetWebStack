// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Adapts a named direct route into a top-level entry in the route table that can only be used
    /// for generating a link (GetVirtualPath). We use these because the subroutes produced by direct
    /// routing, don't go into the main collection and so can't be matched by name.
    /// </summary>
    /// <remarks>
    /// Parallel to the Web API implementation of attribute routing in System.Web.Http.Routing.LinkGenerationRoute.
    /// </remarks>
    internal class LinkGenerationRoute : Route
    {
        private readonly Route _innerRoute;

        public LinkGenerationRoute(Route innerRoute)
            : base(innerRoute.Url, innerRoute.Defaults, innerRoute.Constraints, innerRoute.DataTokens,
            innerRoute.RouteHandler)
        {
            if (innerRoute == null)
            {
                throw Error.ArgumentNull("innerRoute");
            }

            _innerRoute = innerRoute;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            // Claims no routes
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return _innerRoute.GetVirtualPath(requestContext, values);
        }
    }
}

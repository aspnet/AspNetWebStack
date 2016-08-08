// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Defines an abstraction for constraining a route.
    /// </summary>
    public interface IHttpRouteConstraint
    {
        /// <summary>
        /// Attempts to match the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="route">The route that is being constrained.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="values">The route value dictionary.</param>
        /// <param name="routeDirection">The direction of the routing, i.e. URI resolution or URI generation.</param>
        /// <returns><c>false</c> if the route should not match the specified request, or <c>true</c> otherwise</returns>
        bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection);
    }
}

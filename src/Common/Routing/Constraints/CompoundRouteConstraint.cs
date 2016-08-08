// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
#if ASPNETWEBAPI
using System.Net.Http;
#else
using System.Web.Routing;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing.Constraints
#else
namespace System.Web.Mvc.Routing.Constraints
#endif
{
    /// <summary>
    /// Constrains a route by several child constraints.
    /// </summary>
#if ASPNETWEBAPI
    public class CompoundRouteConstraint : IHttpRouteConstraint
#else
    public class CompoundRouteConstraint : IRouteConstraint
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundRouteConstraint" /> class.
        /// </summary>
        /// <param name="constraints">The child constraints that must match for this constraint to match.</param>
#if ASPNETWEBAPI
        public CompoundRouteConstraint(IList<IHttpRouteConstraint> constraints)
#else
        public CompoundRouteConstraint(IList<IRouteConstraint> constraints)
#endif
        {
            if (constraints == null)
            {
                throw Error.ArgumentNull("constraints");
            }

            Constraints = constraints;
        }

        /// <summary>
        /// Gets the child constraints that must match for this constraint to match.
        /// </summary>
#if ASPNETWEBAPI
        public IEnumerable<IHttpRouteConstraint> Constraints { get; private set; }
#else
        public IEnumerable<IRouteConstraint> Constraints { get; private set; }
#endif

        /// <inheritdoc />
#if ASPNETWEBAPI
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
#else
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
#endif
        {
            foreach (var constraint in Constraints)
            {
#if ASPNETWEBAPI
                if (!constraint.Match(request, route, parameterName, values, routeDirection))
#else
                if (!constraint.Match(httpContext, route, parameterName, values, routeDirection))
#endif
                {
                    return false;
                }
            }
            return true;
        }
    }
}
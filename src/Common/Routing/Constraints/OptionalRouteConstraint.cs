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
    /// Constrains a route by an inner constraint that doesn't fail when an optional parameter is set to its default value.
    /// </summary>
#if ASPNETWEBAPI
    public class OptionalRouteConstraint : IHttpRouteConstraint
#else
    public class OptionalRouteConstraint : IRouteConstraint
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalRouteConstraint" /> class.
        /// </summary>
        /// <param name="innerConstraint">The inner constraint to match if the parameter is not an optional parameter without a value</param>
#if ASPNETWEBAPI
        public OptionalRouteConstraint(IHttpRouteConstraint innerConstraint)
#else
        public OptionalRouteConstraint(IRouteConstraint innerConstraint)
#endif
        {
            if (innerConstraint == null)
            {
                throw Error.ArgumentNull("innerConstraint");
            }

            InnerConstraint = innerConstraint;
        }

        /// <summary>
        /// Gets the inner constraint to match if the parameter is not an optional parameter without a value.
        /// </summary>
#if ASPNETWEBAPI
        public IHttpRouteConstraint InnerConstraint { get; private set; }
#else
        public IRouteConstraint InnerConstraint { get; private set; }
#endif

        /// <inheritdoc />
#if ASPNETWEBAPI
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
#else
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
#endif
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (parameterName == null)
            {
                throw Error.ArgumentNull("parameterName");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            // If the parameter is optional and has no value, then pass the constraint
            object defaultValue;
#if ASPNETWEBAPI
            var optionalParameter = RouteParameter.Optional;
#else
            var optionalParameter = UrlParameter.Optional;
#endif
            if (route.Defaults.TryGetValue(parameterName, out defaultValue) && defaultValue == optionalParameter)
            {
                object value;
                if (values.TryGetValue(parameterName, out value) && value == optionalParameter)
                {
                    return true;
                }
            }

#if ASPNETWEBAPI
            return InnerConstraint.Match(request, route, parameterName, values, routeDirection);
#else
            return InnerConstraint.Match(httpContext, route, parameterName, values, routeDirection);
#endif
        }
    }
}
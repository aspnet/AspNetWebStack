// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
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
    /// Constrains a route parameter to be a string with a maximum length.
    /// </summary>
#if ASPNETWEBAPI
    public class MinLengthRouteConstraint : IHttpRouteConstraint
#else
    public class MinLengthRouteConstraint : IRouteConstraint
#endif
    {
        public MinLengthRouteConstraint(int minLength)
        {
            if (minLength < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("minLength", minLength, 0);
            }

            MinLength = minLength;
        }

        /// <summary>
        /// Gets the minimum length of the route parameter.
        /// </summary>
        public int MinLength { get; private set; }

        /// <inheritdoc />
#if ASPNETWEBAPI
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
#else
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
#endif
        {
            if (parameterName == null)
            {
                throw Error.ArgumentNull("parameterName");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            object value;
            if (values.TryGetValue(parameterName, out value) && value != null)
            {
                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return valueString.Length >= MinLength;
            }
            return false;
        }
    }
}
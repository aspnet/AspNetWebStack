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
    public class MaxLengthRouteConstraint : IHttpRouteConstraint
#else
    public class MaxLengthRouteConstraint : IRouteConstraint
#endif
    {
        public MaxLengthRouteConstraint(int maxLength)
        {
            if (maxLength < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxLength", maxLength, 0);
            }

            MaxLength = maxLength;
        }

        /// <summary>
        /// Gets the maximum length of the route parameter.
        /// </summary>
        public int MaxLength { get; private set; }

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
                return valueString.Length <= MaxLength;
            }
            return false;
        }
    }
}

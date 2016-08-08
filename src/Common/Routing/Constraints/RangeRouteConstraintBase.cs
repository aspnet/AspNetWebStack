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
    /// Constraints a route parameter to be an integer within a given range of values.
    /// </summary>
#if ASPNETWEBAPI
    public class RangeRouteConstraint : IHttpRouteConstraint
#else
    public class RangeRouteConstraint : IRouteConstraint
#endif
    {
        public RangeRouteConstraint(long min, long max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Gets the minimum value of the route parameter.
        /// </summary>
        public long Min { get; private set; }

        /// <summary>
        /// Gets the maximum value of the route parameter.
        /// </summary>
        public long Max { get; private set; }

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
                long longValue;
                if (value is long)
                {
                    longValue = (long)value;
                    return longValue >= Min && longValue <= Max;
                }

                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (Int64.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
                {
                    return longValue >= Min && longValue <= Max;
                }
            }
            return false;
        }
    }
}
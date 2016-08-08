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
    /// Constrains a route parameter to be a long with a minimum value.
    /// </summary>
#if ASPNETWEBAPI
    public class MinRouteConstraint : IHttpRouteConstraint
#else
    public class MinRouteConstraint : IRouteConstraint
#endif
    {
        public MinRouteConstraint(long min)
        {
            Min = min;
        }

        /// <summary>
        /// Gets the minimum value of the route parameter.
        /// </summary>
        public long Min { get; private set; }

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
                    return longValue >= Min;
                }

                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (Int64.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
                {
                    return longValue >= Min;
                }
            }
            return false;
        }
    }
}
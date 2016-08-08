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
    /// Constrains a route parameter to represent only decimal values.
    /// </summary>
#if ASPNETWEBAPI
    public class DecimalRouteConstraint : IHttpRouteConstraint
#else
    public class DecimalRouteConstraint : IRouteConstraint
#endif
    {
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
                if (value is decimal)
                {
                    return true;
                }

                decimal result;
                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return Decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
            }
            return false;
        }
    }
}
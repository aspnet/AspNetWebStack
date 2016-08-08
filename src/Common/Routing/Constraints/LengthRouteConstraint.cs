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
    /// Constrains a route parameter to be a string of a given length or within a given range of lengths.
    /// </summary>
#if ASPNETWEBAPI
    public class LengthRouteConstraint : IHttpRouteConstraint
#else
    public class LengthRouteConstraint : IRouteConstraint
#endif
    {
        public LengthRouteConstraint(int length)
        {
            if (length < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("length", length, 0);
            }

            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthRouteConstraint" /> class that constrains
        /// a route parameter to be a string of a given length.
        /// </summary>
        /// <param name="minLength">The minimum length of the route parameter.</param>
        /// <param name="maxLength">The maximum length of the route parameter.</param>
        public LengthRouteConstraint(int minLength, int maxLength)
        {
            if (minLength < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("minLength", minLength, 0);
            }

            if (maxLength < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxLength", maxLength, 0);
            }

            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <summary>
        /// Gets the length of the route parameter, if one is set.
        /// </summary>
        public int? Length { get; private set; }

        /// <summary>
        /// Gets the minimum length of the route parameter, if one is set.
        /// </summary>
        public int? MinLength { get; private set; }

        /// <summary>
        /// Gets the maximum length of the route parameter, if one is set.
        /// </summary>
        public int? MaxLength { get; private set; }

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
                int length = valueString.Length;
                if (Length.HasValue)
                {
                    return length == Length.Value;
                }
                else
                {
                    return length >= MinLength.Value && length <= MaxLength.Value;
                }
            }
            return false;
        }
    }
}
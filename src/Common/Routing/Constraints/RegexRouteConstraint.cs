// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
#if ASPNETWEBAPI
using System.Net.Http;
using System.Text.RegularExpressions;
#else
using System.Text.RegularExpressions;
using System.Web.Routing;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing.Constraints
#else
namespace System.Web.Mvc.Routing.Constraints
#endif
{
    /// <summary>
    /// Constrains a route parameter to match a regular expression.
    /// </summary>
#if ASPNETWEBAPI
    public class RegexRouteConstraint : IHttpRouteConstraint
#else
    public class RegexRouteConstraint : IRouteConstraint
#endif
    {
        private readonly Regex _regex;

        public RegexRouteConstraint(string pattern)
        {
            Pattern = pattern;
            _regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets the regular expression pattern to match.
        /// </summary>
        public string Pattern { get; private set; }

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
                return _regex.IsMatch(valueString);
            }
            return false;
        }
    }
}

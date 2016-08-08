// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProvider : NameValuePairsValueProvider
    {
        public RouteDataValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(GetRouteValues(actionContext.ControllerContext.RouteData), culture)
        {
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetRouteValues(IHttpRouteData routeData)
        {
            foreach (KeyValuePair<string, object> pair in routeData.Values)
            {
                string value = (pair.Value == null) ? null : pair.Value.ToString();
                yield return new KeyValuePair<string, string>(pair.Key, value);
            }
        }
    }
}

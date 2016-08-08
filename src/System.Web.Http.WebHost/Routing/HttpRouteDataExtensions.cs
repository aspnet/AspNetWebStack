// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal static class HttpRouteDataExtensions
    {
        public static RouteData ToRouteData(this IHttpRouteData httpRouteData)
        {
            if (httpRouteData == null)
            {
                throw Error.ArgumentNull("httpRouteData");
            }

            HostedHttpRouteData hostedHttpRouteData = httpRouteData as HostedHttpRouteData;
            if (hostedHttpRouteData != null)
            {
                return hostedHttpRouteData.OriginalRouteData;
            }

            Route route = httpRouteData.Route.ToRoute();
            RouteData result = new RouteData(route, HttpControllerRouteHandler.Instance);
            foreach (KeyValuePair<string, object> pair in httpRouteData.Values)
            {
                result.Values.Add(pair.Key, pair.Value);
            }
            return result;
        }
    }
}

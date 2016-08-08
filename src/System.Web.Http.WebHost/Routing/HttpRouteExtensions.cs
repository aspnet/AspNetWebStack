// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal static class HttpRouteExtensions
    {
        public static Route ToRoute(this IHttpRoute httpRoute)
        {
            if (httpRoute == null)
            {
                throw Error.ArgumentNull("httpRoute");
            }

            HostedHttpRoute hostedHttpRoute = httpRoute as HostedHttpRoute;
            if (hostedHttpRoute != null)
            {
                return hostedHttpRoute.OriginalRoute;
            }

            IRouteHandler handler =
                (httpRoute.Handler is System.Web.Http.Routing.StopRoutingHandler) ? (new System.Web.Routing.StopRoutingHandler() as IRouteHandler) : HttpControllerRouteHandler.Instance;

            return new HttpWebRoute(
                httpRoute.RouteTemplate,
                MakeRouteValueDictionary(httpRoute.Defaults),
                MakeRouteValueDictionary(httpRoute.Constraints),
                MakeRouteValueDictionary(httpRoute.DataTokens),
                handler,
                httpRoute);
        }

        private static RouteValueDictionary MakeRouteValueDictionary(IDictionary<string, object> dictionary)
        {
            return dictionary == null
                ? new RouteValueDictionary()
                : new RouteValueDictionary(dictionary);
        }
    }
}

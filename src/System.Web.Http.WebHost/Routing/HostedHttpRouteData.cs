// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpRouteData : IHttpRouteData
    {
        public HostedHttpRouteData(RouteData routeData)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            OriginalRouteData = routeData;

            HttpWebRoute route = routeData.Route as HttpWebRoute;
            Route = route == null ? null : route.HttpRoute;
        }

        public IHttpRoute Route { get; private set; }

        public IDictionary<string, object> Values
        {
            get { return OriginalRouteData.Values; }
        }

        internal RouteData OriginalRouteData { get; private set; }
    }
}

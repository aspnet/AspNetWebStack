// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc
{
    internal static class AreaHelpers
    {
        public static string GetAreaName(RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            Route castRoute = route as Route;
            if (castRoute != null && castRoute.DataTokens != null)
            {
                return castRoute.DataTokens[RouteDataTokenKeys.Area] as string;
            }

            return null;
        }

        public static string GetAreaName(RouteData routeData)
        {
            object area;
            if (routeData.DataTokens.TryGetValue(RouteDataTokenKeys.Area, out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }
    }
}

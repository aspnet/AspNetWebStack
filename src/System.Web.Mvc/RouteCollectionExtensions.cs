// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public static class RouteCollectionExtensions
    {
        // This method returns a new RouteCollection containing only routes that matched a particular area.
        // The Boolean out parameter is just a flag specifying whether any registered routes were area-aware.
        private static RouteCollection FilterRouteCollectionByArea(RouteCollection routes, string areaName, out bool usingAreas)
        {
            if (areaName == null)
            {
                areaName = String.Empty;
            }

            usingAreas = false;

            // Ensure that we continue using the same settings as the previous route collection
            // if we are using areas and the route collection is exchanged
            RouteCollection filteredRoutes = new RouteCollection()
            {
                AppendTrailingSlash = routes.AppendTrailingSlash,
                LowercaseUrls = routes.LowercaseUrls,
                RouteExistingFiles = routes.RouteExistingFiles
            };

            using (routes.GetReadLock())
            {
                foreach (RouteBase route in routes)
                {
                    string thisAreaName = AreaHelpers.GetAreaName(route) ?? String.Empty;
                    usingAreas |= (thisAreaName.Length > 0);
                    if (String.Equals(thisAreaName, areaName, StringComparison.OrdinalIgnoreCase))
                    {
                        filteredRoutes.Add(route);
                    }
                }
            }

            // if areas are not in use, the filtered route collection might be incorrect
            return (usingAreas) ? filteredRoutes : routes;
        }

        public static VirtualPathData GetVirtualPathForArea(this RouteCollection routes, RequestContext requestContext, RouteValueDictionary values)
        {
            return GetVirtualPathForArea(routes, requestContext, null /* name */, values);
        }

        public static VirtualPathData GetVirtualPathForArea(this RouteCollection routes, RequestContext requestContext, string name, RouteValueDictionary values)
        {
            bool usingAreas; // don't care about this value
            return GetVirtualPathForArea(routes, requestContext, name, values, out usingAreas);
        }

        internal static VirtualPathData GetVirtualPathForArea(this RouteCollection routes, RequestContext requestContext, string name, RouteValueDictionary values, out bool usingAreas)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (!String.IsNullOrEmpty(name))
            {
                // the route name is a stronger qualifier than the area name, so just pipe it through
                usingAreas = false;
                return routes.GetVirtualPath(requestContext, name, values);
            }

            string targetArea = null;
            if (values != null)
            {
                object targetAreaRawValue;
                if (values.TryGetValue("area", out targetAreaRawValue))
                {
                    targetArea = targetAreaRawValue as string;
                }
                else
                {
                    // set target area to current area
                    if (requestContext != null)
                    {
                        targetArea = AreaHelpers.GetAreaName(requestContext.RouteData);
                    }
                }
            }

            // need to apply a correction to the RVD if areas are in use
            RouteValueDictionary correctedValues = values;
            RouteCollection filteredRoutes = FilterRouteCollectionByArea(routes, targetArea, out usingAreas);
            if (usingAreas)
            {
                correctedValues = new RouteValueDictionary(values);
                correctedValues.Remove("area");
            }

            VirtualPathData vpd = filteredRoutes.GetVirtualPath(requestContext, correctedValues);
            return vpd;
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static void IgnoreRoute(this RouteCollection routes, string url)
        {
            IgnoreRoute(routes, url, null /* constraints */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static void IgnoreRoute(this RouteCollection routes, string url, object constraints)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            IgnoreRouteInternal route = new IgnoreRouteInternal(url)
            {
                Constraints = CreateRouteValueDictionaryUncached(constraints)
            };

            ConstraintValidation.Validate(route);

            routes.Add(route);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static Route MapRoute(this RouteCollection routes, string name, string url)
        {
            return MapRoute(routes, name, url, null /* defaults */, (object)null /* constraints */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static Route MapRoute(this RouteCollection routes, string name, string url, object defaults)
        {
            return MapRoute(routes, name, url, defaults, (object)null /* constraints */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static Route MapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            return MapRoute(routes, name, url, defaults, constraints, null /* namespaces */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static Route MapRoute(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return MapRoute(routes, name, url, null /* defaults */, null /* constraints */, namespaces);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static Route MapRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            return MapRoute(routes, name, url, defaults, null /* constraints */, namespaces);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public static Route MapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            Route route = new Route(url, new MvcRouteHandler())
            {
                Defaults = CreateRouteValueDictionaryUncached(defaults),
                Constraints = CreateRouteValueDictionaryUncached(constraints),
                DataTokens = new RouteValueDictionary()
            };

            ConstraintValidation.Validate(route);

            if ((namespaces != null) && (namespaces.Length > 0))
            {
                route.DataTokens[RouteDataTokenKeys.Namespaces] = namespaces;
            }

            routes.Add(name, route);

            return route;
        }

        /// <summary>
        /// The callers to this method are used at startup only, thus it's a bit better to use
        /// the uncached method because it will run faster for the first few times, and will not
        /// consume memory long term.
        /// </summary>
        private static RouteValueDictionary CreateRouteValueDictionaryUncached(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            if (dictionary != null)
            {
                return new RouteValueDictionary(dictionary);
            }

            return TypeHelper.ObjectToDictionaryUncached(values);
        }

        private sealed class IgnoreRouteInternal : Route
        {
            public IgnoreRouteInternal(string url)
                : base(url, new StopRoutingHandler())
            {
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues)
            {
                // Never match during route generation. This avoids the scenario where an IgnoreRoute with
                // fairly relaxed constraints ends up eagerly matching all generated URLs.
                return null;
            }
        }
    }
}

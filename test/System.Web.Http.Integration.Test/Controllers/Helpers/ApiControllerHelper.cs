// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    public class ApiControllerHelper
    {
        public static HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            ApiControllerActionSelector selector = new ApiControllerActionSelector();
            HttpActionDescriptor descriptor = selector.SelectAction(controllerContext);
            return descriptor;
        }

        public static HttpControllerContext CreateControllerContext(string httpMethod, string requestUrl, string routeUrl, object routeDefault = null)
        {
            string baseAddress = "http://localhost/";
            HttpConfiguration config = new HttpConfiguration();
            HttpRoute route = routeDefault != null ? new HttpRoute(routeUrl, new HttpRouteValueDictionary(routeDefault)) : new HttpRoute(routeUrl);
            config.Routes.Add("test", route);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethodHelper.GetHttpMethod(httpMethod), baseAddress + requestUrl);

            IHttpRouteData routeData = config.Routes.GetRouteData(request);
            if (routeData == null)
            {
                throw new InvalidOperationException("Could not dispatch to controller based on the route.");
            }

            RemoveOptionalRoutingParameters(routeData.Values);

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(config, routeData, request);
            return controllerContext;
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValueDictionary)
        {
            // Get all keys for which the corresponding value is 'Optional'.
            // ToArray() necessary so that we don't manipulate the dictionary while enumerating.
            string[] matchingKeys = (from entry in routeValueDictionary
                                     where entry.Value == RouteParameter.Optional
                                     select entry.Key).ToArray();

            foreach (string key in matchingKeys)
            {
                routeValueDictionary.Remove(key);
            }
        }
    }
}

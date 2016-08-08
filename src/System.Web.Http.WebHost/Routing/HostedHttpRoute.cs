// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpRoute : IHttpRoute
    {
        public HostedHttpRoute(string uriTemplate, IDictionary<string, object> defaults, IDictionary<string, object> constraints, IDictionary<string, object> dataTokens, HttpMessageHandler handler)
        {
            RouteValueDictionary routeDefaults = defaults != null ? new RouteValueDictionary(defaults) : null;
            RouteValueDictionary routeConstraints = constraints != null ? new RouteValueDictionary(constraints) : null;
            RouteValueDictionary routeDataTokens = dataTokens != null ? new RouteValueDictionary(dataTokens) : null;
            OriginalRoute = new HttpWebRoute(uriTemplate, routeDefaults, routeConstraints, routeDataTokens, HttpControllerRouteHandler.Instance, this);
            Handler = handler;
        }

        public string RouteTemplate
        {
            get { return OriginalRoute.Url; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return OriginalRoute.Defaults; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return OriginalRoute.Constraints; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return OriginalRoute.DataTokens; }
        }

        public HttpMessageHandler Handler { get; private set; }

        internal Route OriginalRoute { get; private set; }

        public IHttpRouteData GetRouteData(string rootVirtualPath, HttpRequestMessage request)
        {
            if (rootVirtualPath == null)
            {
                throw Error.ArgumentNull("rootVirtualPath");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpContextBase httpContextBase = request.GetHttpContext();
            if (httpContextBase == null)
            {
                httpContextBase = new HttpRequestMessageContextWrapper(rootVirtualPath, request);
            }

            RouteData routeData = OriginalRoute.GetRouteData(httpContextBase);
            if (routeData != null)
            {
                return new HostedHttpRouteData(routeData);
            }

            return null;
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpContextBase httpContextBase = request.GetHttpContext();
            if (httpContextBase != null)
            {
                HostedHttpRouteData routeData = request.GetRouteData() as HostedHttpRouteData;
                if (routeData != null)
                {
                    RequestContext requestContext = new RequestContext(httpContextBase, routeData.OriginalRouteData);
                    VirtualPathData virtualPathData = OriginalRoute.GetVirtualPath(requestContext, new RouteValueDictionary(values));
                    if (virtualPathData != null)
                    {
                        return new HostedHttpVirtualPathData(virtualPathData, routeData.Route);
                    }
                }
            }

            return null;
        }
    }
}

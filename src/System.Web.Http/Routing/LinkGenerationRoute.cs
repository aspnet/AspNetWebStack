// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Route that generates a virtual path, but does not claim any routes. 
    /// This can be used with RouteCollectionRoute to provide generation by names. 
    /// Delegates to an inner route to do actual generation.
    /// </summary>
    /// <remarks>
    /// Parallel to the MVC implementation of attribute routing in System.Web.Mvc.Routing.LinkGenerationRoute.
    /// </remarks>
    internal class LinkGenerationRoute : IHttpRoute
    {
        private readonly IHttpRoute _innerRoute;

        public LinkGenerationRoute(IHttpRoute innerRoute)
        {
            if (innerRoute == null)
            {
                throw new ArgumentNullException("innerRoute");
            }

            _innerRoute = innerRoute;
        }

        public string RouteTemplate
        {
            get { return _innerRoute.RouteTemplate; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return _innerRoute.Defaults; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return _innerRoute.Constraints; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return _innerRoute.DataTokens; }
        }

        public HttpMessageHandler Handler
        {
            get { return null; }
        }

        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            // Claims no routes
            return null;
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            return _innerRoute.GetVirtualPath(request, values);
        }
    }
}
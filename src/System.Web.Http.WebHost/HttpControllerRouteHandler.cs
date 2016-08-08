// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// A <see cref="IRouteHandler"/> that returns instances of <see cref="HttpControllerHandler"/> that
    /// can pass requests to a given <see cref="HttpServer"/> instance.
    /// </summary>
    public class HttpControllerRouteHandler : IRouteHandler
    {
        private static readonly Lazy<HttpControllerRouteHandler> _instance =
            new Lazy<HttpControllerRouteHandler>(() => new HttpControllerRouteHandler(), isThreadSafe: true);

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerRouteHandler"/> class.
        /// </summary>
        protected HttpControllerRouteHandler()
        {
        }

        /// <summary>
        /// Gets the singleton <see cref="HttpControllerRouteHandler"/> instance.
        /// </summary>
        public static HttpControllerRouteHandler Instance
        {
            get { return _instance.Value; }
        }

        /// <summary>
        /// Provides the object that processes the request.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the request.</param>
        /// <returns>
        /// An object that processes the request.
        /// </returns>
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            return GetHttpHandler(requestContext);
        }

        /// <summary>
        /// Provides the object that processes the request.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the request.</param>
        /// <returns>
        /// An object that processes the request.
        /// </returns>
        protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new HttpControllerHandler(requestContext.RouteData);
        }
    }
}

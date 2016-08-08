// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Represents a factory for creating URLs.
    /// </summary>
    public class UrlHelper
    {
        private HttpRequestMessage _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlHelper"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public UrlHelper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlHelper"/> class.
        /// </summary>
        /// <param name="request">The HTTP request message containing the context under which the URLs are generated.</param>
        public UrlHelper(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Request = request;
        }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/> of the current <see cref="UrlHelper"/>.
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _request = value;
            }
        }

        /// <summary>
        /// Creates an absolute URL using the specified path.
        /// </summary>
        /// <param name="path">The URL path, which may be a relative URL, a rooted URL, or a virtual path.</param>
        /// <returns>The generated URL.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public virtual string Content(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw Error.ArgumentNullOrEmpty("path");
            }

            if (Request == null)
            {
                throw Error.InvalidOperation(SRResources.RequestIsNull, "UrlHelper");
            }

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                // This is a virtual path, we need to combine it with the virtual path root
                string virtualPathRoot;
                HttpRequestContext requestContext = Request.GetRequestContext();

                if (requestContext != null)
                {
                    virtualPathRoot = requestContext.VirtualPathRoot;
                }
                else
                {
                    HttpConfiguration configuration = Request.GetConfiguration();
                    if (configuration == null)
                    {
                        throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoConfiguration);
                    }

                    virtualPathRoot = configuration.VirtualPathRoot;
                }

                if (virtualPathRoot == null)
                {
                    virtualPathRoot = "/";
                }

                if (!virtualPathRoot.StartsWith("/", StringComparison.Ordinal))
                {
                    virtualPathRoot = "/" + virtualPathRoot;
                }
                if (!virtualPathRoot.EndsWith("/", StringComparison.Ordinal))
                {
                    virtualPathRoot += "/";
                }

                return new Uri(Request.RequestUri, virtualPathRoot + path.Substring("~/".Length)).AbsoluteUri;
            }
            else
            {
                return new Uri(Request.RequestUri, path).AbsoluteUri;
            }
        }

        /// <summary>
        /// Creates a relative URL using the specified route and route data.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>The generated URL.</returns>
        public virtual string Route(string routeName, object routeValues)
        {
            return Route(routeName, new HttpRouteValueDictionary(routeValues));
        }

        /// <summary>
        /// Creates a relative URL using the specified route and route data.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>The generated URL.</returns>
        public virtual string Route(string routeName, IDictionary<string, object> routeValues)
        {
            return GetVirtualPath(Request, routeName, routeValues);
        }

        /// <summary>
        /// Creates an absolute URL using the given route and route data.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>The generated URL.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public virtual string Link(string routeName, object routeValues)
        {
            return Link(routeName, new HttpRouteValueDictionary(routeValues));
        }

        /// <summary>
        /// Creates an absolute URL using the specified route and route data.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>The generated URL.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "It is safe to pass string here")]
        public virtual string Link(string routeName, IDictionary<string, object> routeValues)
        {
            string link = Route(routeName, routeValues);
            if (!String.IsNullOrEmpty(link))
            {
                link = new Uri(Request.RequestUri, link).AbsoluteUri;
            }

            return link;
        }

        private static string GetVirtualPath(HttpRequestMessage request, string routeName, IDictionary<string, object> routeValues)
        {
            if (routeValues == null)
            {
                // If no route values were passed in at all we have to create a new dictionary
                // so that we can add the extra "httproute" key.
                routeValues = new HttpRouteValueDictionary();
                routeValues.Add(HttpRoute.HttpRouteKey, true);
            }
            else
            {
                // Copy the dictionary so that we can guarantee that routeValues uses an OrdinalIgnoreCase comparer
                // and to add the extra "httproute" key used by all Web API routes to disambiguate them from other MVC routes.
                routeValues = new HttpRouteValueDictionary(routeValues);
                if (!routeValues.ContainsKey(HttpRoute.HttpRouteKey))
                {
                    routeValues.Add(HttpRoute.HttpRouteKey, true);
                }
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoConfiguration);
            }

            IHttpVirtualPathData vpd = configuration.Routes.GetVirtualPath(
                request: request,
                name: routeName,
                values: routeValues);
            if (vpd == null)
            {
                return null;
            }
            return vpd.VirtualPath;
        }
    }
}

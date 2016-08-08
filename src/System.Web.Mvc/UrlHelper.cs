// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Mvc.Properties;
using System.Web.Routing;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public class UrlHelper
    {
        /// <summary>
        /// Key used to signify that a route URL generation request should include HTTP routes (e.g. Web API).
        /// If this key is not specified then no HTTP routes will match.
        /// </summary>
        private const string HttpRouteKey = "httproute";

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlHelper"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public UrlHelper()
        {
        }

        public UrlHelper(RequestContext requestContext)
            : this(requestContext, RouteTable.Routes)
        {
        }

        public UrlHelper(RequestContext requestContext, RouteCollection routeCollection)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (routeCollection == null)
            {
                throw new ArgumentNullException("routeCollection");
            }
            RequestContext = requestContext;
            RouteCollection = routeCollection;
        }

        public RequestContext RequestContext { get; private set; }

        public RouteCollection RouteCollection { get; private set; }

        public virtual string Action()
        {
            return RequestContext.HttpContext.Request.RawUrl;
        }

        public virtual string Action(string actionName)
        {
            return GenerateUrl(null /* routeName */, actionName, null, (RouteValueDictionary)null /* routeValues */);
        }

        public virtual string Action(string actionName, object routeValues)
        {
            return GenerateUrl(null /* routeName */, actionName, null /* controllerName */, TypeHelper.ObjectToDictionary(routeValues));
        }

        public virtual string Action(string actionName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(null /* routeName */, actionName, null /* controllerName */, routeValues);
        }

        public virtual string Action(string actionName, string controllerName)
        {
            return GenerateUrl(null /* routeName */, actionName, controllerName, (RouteValueDictionary)null /* routeValues */);
        }

        public virtual string Action(string actionName, string controllerName, object routeValues)
        {
            return GenerateUrl(null /* routeName */, actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues));
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(null /* routeName */, actionName, controllerName, routeValues);
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues, string protocol)
        {
            return GenerateUrl(null /* routeName */, actionName, controllerName, protocol, null /* hostName */, null /* fragment */, routeValues, RouteCollection, RequestContext, true /* includeImplicitMvcValues */);
        }

        public virtual string Action(string actionName, string controllerName, object routeValues, string protocol)
        {
            return GenerateUrl(null /* routeName */, actionName, controllerName, protocol, null /* hostName */, null /* fragment */, TypeHelper.ObjectToDictionary(routeValues), RouteCollection, RequestContext, true /* includeImplicitMvcValues */);
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues, string protocol, string hostName)
        {
            return GenerateUrl(null /* routeName */, actionName, controllerName, protocol, hostName, null /* fragment */, routeValues, RouteCollection, RequestContext, true /* includeImplicitMvcValues */);
        }

        public virtual string Content(string contentPath)
        {
            return GenerateContentUrl(contentPath, RequestContext.HttpContext);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public static string GenerateContentUrl(string contentPath, HttpContextBase httpContext)
        {
            if (String.IsNullOrEmpty(contentPath))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "contentPath");
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            if (contentPath[0] == '~')
            {
                return UrlUtil.GenerateClientUrl(httpContext, contentPath);
            }
            else
            {
                return contentPath;
            }
        }

        //REVIEW: Should we have an overload that takes Uri?
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Needs to take same parameters as HttpUtility.UrlEncode()")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public virtual string Encode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        private string GenerateUrl(string routeName, string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(routeName, actionName, controllerName, routeValues, RouteCollection, RequestContext, true /* includeImplicitMvcValues */);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public static string GenerateUrl(string routeName, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, RouteCollection routeCollection, RequestContext requestContext, bool includeImplicitMvcValues)
        {
            string url = GenerateUrl(routeName, actionName, controllerName, routeValues, routeCollection, requestContext, includeImplicitMvcValues);

            if (url != null)
            {
                if (!String.IsNullOrEmpty(fragment))
                {
                    url = url + "#" + fragment;
                }

                if (!String.IsNullOrEmpty(protocol) || !String.IsNullOrEmpty(hostName))
                {
                    Uri requestUrl = requestContext.HttpContext.Request.Url;
                    protocol = (!String.IsNullOrEmpty(protocol)) ? protocol : Uri.UriSchemeHttp;
                    hostName = (!String.IsNullOrEmpty(hostName)) ? hostName : requestUrl.Host;

                    string port = String.Empty;
                    string requestProtocol = requestUrl.Scheme;

                    if (String.Equals(protocol, requestProtocol, StringComparison.OrdinalIgnoreCase))
                    {
                        port = requestUrl.IsDefaultPort ? String.Empty : (":" + Convert.ToString(requestUrl.Port, CultureInfo.InvariantCulture));
                    }

                    url = protocol + Uri.SchemeDelimiter + hostName + port + url;
                }
            }

            return url;
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public static string GenerateUrl(string routeName, string actionName, string controllerName, RouteValueDictionary routeValues, RouteCollection routeCollection, RequestContext requestContext, bool includeImplicitMvcValues)
        {
            if (routeCollection == null)
            {
                throw new ArgumentNullException("routeCollection");
            }

            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            RouteValueDictionary mergedRouteValues = RouteValuesHelpers.MergeRouteValues(actionName, controllerName, requestContext.RouteData.Values, routeValues, includeImplicitMvcValues);

            VirtualPathData vpd = routeCollection.GetVirtualPathForArea(requestContext, routeName, mergedRouteValues);
            if (vpd == null)
            {
                return null;
            }

            string modifiedUrl = UrlUtil.GenerateClientUrl(requestContext.HttpContext, vpd.VirtualPath);
            return modifiedUrl;
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Response.Redirect() takes its URI as a string parameter.")]
        public virtual bool IsLocalUrl(string url)
        {
            // TODO this should call the System.Web.dll API once it gets added to the framework and MVC takes a dependency on it.
            return RequestExtensions.IsUrlLocalToHost(RequestContext.HttpContext.Request, url);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(object routeValues)
        {
            return RouteUrl(null /* routeName */, routeValues);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(RouteValueDictionary routeValues)
        {
            return RouteUrl(null /* routeName */, routeValues);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(string routeName)
        {
            return RouteUrl(routeName, (object)null /* routeValues */);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(string routeName, object routeValues)
        {
            return RouteUrl(routeName, routeValues, null /* protocol */);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(string routeName, RouteValueDictionary routeValues)
        {
            return RouteUrl(routeName, routeValues, null /* protocol */, null /* hostName */);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(string routeName, object routeValues, string protocol)
        {
            return GenerateUrl(routeName, null /* actionName */, null /* controllerName */, protocol, null /* hostName */, null /* fragment */, TypeHelper.ObjectToDictionary(routeValues), RouteCollection, RequestContext, false /* includeImplicitMvcValues */);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string RouteUrl(string routeName, RouteValueDictionary routeValues, string protocol, string hostName)
        {
            return GenerateUrl(routeName, null /* actionName */, null /* controllerName */, protocol, hostName, null /* fragment */, routeValues, RouteCollection, RequestContext, false /* includeImplicitMvcValues */);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string HttpRouteUrl(string routeName, object routeValues)
        {
            return HttpRouteUrl(routeName, TypeHelper.ObjectToDictionary(routeValues));
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "As the return value will used only for rendering, string return value is more appropriate.")]
        public virtual string HttpRouteUrl(string routeName, RouteValueDictionary routeValues)
        {
            if (routeValues == null)
            {
                // If no route values were passed in at all we have to create a new dictionary
                // so that we can add the extra "httproute" key.
                routeValues = new RouteValueDictionary();
                routeValues.Add(HttpRouteKey, true);
            }
            else
            {
                // Copy the dictionary to add the extra "httproute" key used by all Web API routes to
                // disambiguate them from other MVC routes.
                routeValues = new RouteValueDictionary(routeValues);
                if (!routeValues.ContainsKey(HttpRouteKey))
                {
                    routeValues.Add(HttpRouteKey, true);
                }
            }

            return GenerateUrl(routeName,
                actionName: null,
                controllerName: null,
                protocol: null,
                hostName: null,
                fragment: null,
                routeValues: routeValues,
                routeCollection: RouteCollection,
                requestContext: RequestContext,
                includeImplicitMvcValues: false);
        }
    }
}

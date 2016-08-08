// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc.Properties;
using System.Web.Routing;
using System.Web.WebPages;

namespace System.Web.Mvc.Html
{
    public static class LinkExtensions
    {
        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, new RouteValueDictionary(), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, TypeHelper.ObjectToDictionary(routeValues), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, routeValues, htmlAttributes);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }
            return MvcHtmlString.Create(HtmlHelper.GenerateLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, null /* routeName */, actionName, controllerName, routeValues, htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, protocol, hostName, fragment, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }
            return MvcHtmlString.Create(HtmlHelper.GenerateLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, null /* routeName */, actionName, controllerName, protocol, hostName, fragment, routeValues, htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, object routeValues)
        {
            return RouteLink(htmlHelper, linkText, TypeHelper.ObjectToDictionary(routeValues));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, RouteValueDictionary routeValues)
        {
            return RouteLink(htmlHelper, linkText, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName)
        {
            return RouteLink(htmlHelper, linkText, routeName, (object)null /* routeValues */);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, object routeValues)
        {
            return RouteLink(htmlHelper, linkText, routeName, TypeHelper.ObjectToDictionary(routeValues));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, RouteValueDictionary routeValues)
        {
            return RouteLink(htmlHelper, linkText, routeName, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, object routeValues, object htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, null /* routeName */, routeValues, htmlAttributes);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, object routeValues, object htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, routeName, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }
            return MvcHtmlString.Create(HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, routeName, routeValues, htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, routeName, protocol, hostName, fragment, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }
            return MvcHtmlString.Create(HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, routeName, protocol, hostName, fragment, routeValues, htmlAttributes));
        }
    }
}

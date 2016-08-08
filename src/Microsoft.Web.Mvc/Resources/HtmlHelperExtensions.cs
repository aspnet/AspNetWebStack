// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Microsoft.Web.Mvc.Resources
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Generates the Form preamble, defaulting the link for the Retrieve action
        /// </summary>
        /// <param name="html"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static MvcForm BeginResourceForm(this HtmlHelper html, string controllerName, object routeValues)
        {
            return html.BeginResourceForm(controllerName, routeValues, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates the Form preamble
        /// </summary>
        /// <param name="html"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static MvcForm BeginResourceForm(this HtmlHelper html, string controllerName, object routeValues, ActionType actionType)
        {
            return html.BeginResourceForm(controllerName, routeValues, null, actionType);
        }

        /// <summary>
        /// Generates the Form preamble
        /// </summary>
        /// <param name="html"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static MvcForm BeginResourceForm(this HtmlHelper html, string controllerName, object routeValues, object htmlAttributes, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.GetUpdateForm:
                    return html.BeginRouteForm(controllerName + "-editForm", routeValues, FormMethod.Post, htmlAttributes);
                case ActionType.GetCreateForm:
                    return html.BeginRouteForm(controllerName + "-createForm", FormMethod.Post, htmlAttributes);
                case ActionType.Retrieve:
                case ActionType.Delete:
                case ActionType.Update:
                    return html.BeginRouteForm(controllerName, routeValues, FormMethod.Post, htmlAttributes);
                case ActionType.Create:
                    return html.BeginRouteForm(controllerName + "-create", FormMethod.Post, htmlAttributes);
                case ActionType.Index:
                    return html.BeginRouteForm(controllerName + "-index", FormMethod.Post, htmlAttributes);
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
        }

        /// <summary>
        /// Generates a link to the resource controller, defaulting to the Retrieve action
        /// </summary>
        /// <param name="html"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static MvcHtmlString ResourceLink(this HtmlHelper html, string controllerName, object routeValues)
        {
            return html.ResourceLink(controllerName, controllerName, routeValues, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates a link to the resource controller, defaulting to the Retrieve action
        /// </summary>
        /// <param name="html"></param>
        /// <param name="controllerName"></param>
        /// <param name="linkText"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static MvcHtmlString ResourceLink(this HtmlHelper html, string controllerName, string linkText, object routeValues)
        {
            return html.ResourceLink(controllerName, linkText, routeValues, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates a link to the resource controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="controllerName"></param>
        /// <param name="linkText"></param>
        /// <param name="routeValues"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static MvcHtmlString ResourceLink(this HtmlHelper html, string controllerName, string linkText, object routeValues, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.GetUpdateForm:
                    return html.RouteLink(linkText, controllerName + "-editForm", routeValues);
                case ActionType.GetCreateForm:
                    return html.RouteLink(linkText, controllerName + "-createForm", routeValues);
                case ActionType.Retrieve:
                case ActionType.Delete:
                case ActionType.Update:
                    return html.RouteLink(linkText, controllerName, routeValues);
                case ActionType.Create:
                    return html.RouteLink(linkText, controllerName + "-create", routeValues);
                case ActionType.Index:
                    return html.RouteLink(linkText, controllerName + "-index", routeValues);
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
        }

        /// <summary>
        /// Emits a hidden form variable for X-Http-Method-Override. The only valid values for actionType
        /// are ActionType.Delete and ActionType.Update
        /// </summary>
        /// <param name="html"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static MvcHtmlString HttpMethodOverride(this HtmlHelper html, ActionType actionType)
        {
            if (actionType != ActionType.Delete && actionType != ActionType.Update)
            {
                throw new ArgumentOutOfRangeException("actionType");
            }
            return html.HttpMethodOverride(actionType == ActionType.Delete ? "DELETE" : "PUT");
        }
    }
}

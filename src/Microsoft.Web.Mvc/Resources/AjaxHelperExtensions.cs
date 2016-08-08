// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;

namespace Microsoft.Web.Mvc.Resources
{
    public static class AjaxHelperExtensions
    {
        /// <summary>
        /// Generates the Form preamble, defaulting the link for the Retrieve action
        /// </summary>
        /// <param name="ajax"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="ajaxOptions"></param>
        /// <returns></returns>
        public static MvcForm BeginResourceForm(this AjaxHelper ajax, string controllerName, object routeValues, AjaxOptions ajaxOptions)
        {
            return ajax.BeginResourceForm(controllerName, routeValues, ajaxOptions, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates the Form preamble
        /// </summary>
        /// <param name="ajax"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="ajaxOptions"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static MvcForm BeginResourceForm(this AjaxHelper ajax, string controllerName, object routeValues, AjaxOptions ajaxOptions, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.GetUpdateForm:
                    return ajax.BeginRouteForm(controllerName + "-editForm", routeValues, ajaxOptions);
                case ActionType.GetCreateForm:
                    return ajax.BeginRouteForm(controllerName + "-createForm", ajaxOptions);
                case ActionType.Retrieve:
                case ActionType.Delete:
                case ActionType.Update:
                    // can we use ajaxOptions to either add the header?
                    MvcForm form = ajax.BeginRouteForm(controllerName, routeValues, ajaxOptions);
                    return form;
                case ActionType.Create:
                    return ajax.BeginRouteForm(controllerName + "-create", ajaxOptions);
                case ActionType.Index:
                    return ajax.BeginRouteForm(controllerName + "-index", ajaxOptions);
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
        }

        /// <summary>
        /// Generates a link to the resource controller, defaulting to the Retrieve action
        /// </summary>
        /// <param name="ajax"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="ajaxOptions"></param>
        /// <returns></returns>
        public static MvcHtmlString ResourceLink(this AjaxHelper ajax, string controllerName, object routeValues, AjaxOptions ajaxOptions)
        {
            return ajax.ResourceLink(controllerName, controllerName, routeValues, ajaxOptions, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates a link to the resource controller, defaulting to the Retrieve action
        /// </summary>
        /// <param name="ajax"></param>
        /// <param name="controllerName"></param>
        /// <param name="linkText"></param>
        /// <param name="routeValues"></param>
        /// <param name="ajaxOptions"></param>
        /// <returns></returns>
        public static MvcHtmlString ResourceLink(this AjaxHelper ajax, string controllerName, string linkText, object routeValues, AjaxOptions ajaxOptions)
        {
            return ajax.ResourceLink(linkText, controllerName, routeValues, ajaxOptions, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates a link to the resource controller
        /// </summary>
        /// <param name="ajax"></param>
        /// <param name="controllerName"></param>
        /// <param name="linkText"></param>
        /// <param name="routeValues"></param>
        /// <param name="ajaxOptions"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static MvcHtmlString ResourceLink(this AjaxHelper ajax, string controllerName, string linkText, object routeValues, AjaxOptions ajaxOptions, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.GetUpdateForm:
                    return ajax.RouteLink(linkText, controllerName + "-editForm", routeValues, ajaxOptions);
                case ActionType.GetCreateForm:
                    return ajax.RouteLink(linkText, controllerName + "-createForm", ajaxOptions);
                case ActionType.Retrieve:
                case ActionType.Delete:
                case ActionType.Update:
                    return ajax.RouteLink(linkText, controllerName, routeValues, ajaxOptions);
                case ActionType.Create:
                    return ajax.RouteLink(linkText, controllerName + "-create", ajaxOptions);
                case ActionType.Index:
                    return ajax.RouteLink(linkText, controllerName + "-index", ajaxOptions);
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.Routing;

namespace Microsoft.Web.Mvc.Resources
{
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds the routes to enable RESTful routing of requests to specified controller. The controllerName is the URL prefix to the controller.
        /// The routeSuffix is used for more specific routing in the resource. For example, a controllerName of "books" and a routeSuffix of "{id}" will
        /// result in the following routes being registered for the controller:
        /// ~/books/, ~/books/{id} to the resource,
        /// ~/books/CreateForm to the CreateForm controller action,
        /// ~/books/{id}/EditForm to the EditForm controller action
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeSuffix"></param>
        public static void MapResourceRoute(this RouteCollection routes, string controllerName, string routeSuffix)
        {
            routes.MapResourceRoute(controllerName, null, routeSuffix, null);
        }

        /// <summary>
        /// Adds the routes to enable RESTful routing of requests to specified controller. The controllerName is the URL prefix to the controller.
        /// The routeSuffix is used for more specific routing in the resource. For example, a controllerName of "books" and a routeSuffix of "{id}" will
        /// result in the following routes being registered for the controller:
        /// ~/books/, ~/books/{id} to the resource,
        /// ~/books/CreateForm to the CreateForm controller action,
        /// ~/books/{id}/EditForm to the EditForm controller action
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeSuffix"></param>
        /// <param name="constraints"></param>
        public static void MapResourceRoute(this RouteCollection routes, string controllerName, string routeSuffix, object constraints)
        {
            routes.MapResourceRoute(controllerName, null, routeSuffix, constraints);
        }

        /// <summary>
        /// Adds the routes to enable RESTful routing of requests to specified controller. The routePrefix is the URL prefix to the controller.
        /// The routeSuffix is used for more specific routing in the resource. For example, a routePrefix of "books" and a routeSuffix of "{id}" will
        /// result in the following routes being registered for the controller:
        /// ~/books/, ~/books/{id} to the resource,
        /// ~/books/CreateForm to the CreateForm controller action,
        /// ~/books/{id}/EditForm to the EditForm controller action
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerName"></param>
        /// <param name="routePrefix"></param>
        /// <param name="routeSuffix"></param>
        public static void MapResourceRoute(this RouteCollection routes, string controllerName, string routePrefix, string routeSuffix)
        {
            routes.MapResourceRoute(controllerName, routePrefix, routeSuffix, null);
        }

        /// <summary>
        /// Adds the routes to enable RESTful routing of requests to specified controller. The routePrefix is the URL prefix to the controller.
        /// The routeSuffix is used for more specific routing in the resource. For example, a routePrefix of "books" and a routeSuffix of "{id}" will
        /// result in the following routes being registered for the controller:
        /// ~/books/, ~/books/{id} to the resource,
        /// ~/books/CreateForm to the CreateForm controller action,
        /// ~/books/{id}/EditForm to the EditForm controller action
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerName"></param>
        /// <param name="routePrefix"></param>
        /// <param name="routeSuffix"></param>
        /// <param name="constraints"></param>
        public static void MapResourceRoute(this RouteCollection routes, string controllerName, string routePrefix, string routeSuffix, object constraints)
        {
            if (String.IsNullOrEmpty(routePrefix))
            {
                routePrefix = controllerName;
            }
            else
            {
                routePrefix = routePrefix + "/" + controllerName;
            }
            if (!String.IsNullOrEmpty(routeSuffix))
            {
                routeSuffix = "/" + routeSuffix;
            }

            routes.MapRoute(
                controllerName + "-editForm",
                routePrefix + routeSuffix + "/EditForm",
                new { controller = controllerName, action = "EditForm" },
                constraints);
            routes.MapRoute(
                controllerName + "-createForm",
                routePrefix + "/CreateForm",
                new { controller = controllerName, action = "CreateForm" });
            routes.MapRoute(
                controllerName,
                routePrefix + routeSuffix,
                new { controller = controllerName },
                constraints);
            routes.MapRoute(
                controllerName + "-create",
                routePrefix,
                new { controller = controllerName, action = "Create" },
                new { postOnly = new HttpMethodConstraint("POST") });
            routes.MapRoute(
                controllerName + "-index",
                routePrefix,
                new { controller = controllerName, action = "Index" });
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "controller", Justification = "This is an extension method, the parameter is necessary to provide a place to hook the method")]
        public static string GetResourceRouteName(this Controller controller, string controllerName, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.GetUpdateForm:
                    return controllerName + "-editForm";
                case ActionType.GetCreateForm:
                    return controllerName + "-createForm";
                case ActionType.Retrieve:
                case ActionType.Delete:
                case ActionType.Update:
                    return controllerName;
                case ActionType.Create:
                    return controllerName + "-create";
                case ActionType.Index:
                    return controllerName + "-index";
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
        }
    }
}

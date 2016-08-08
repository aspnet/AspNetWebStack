// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    public static class UriHelperExtensions
    {
        /// <summary>
        /// Generates the route URL for the resource controller's Retrieve action
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an Extension Method which allows the user to provide a strongly-typed argument via Expression"), SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Need to be sure the passed-in argument is of type Controller::Action")]
        public static string ResourceUrl(this UrlHelper url, string controllerName, object routeValues)
        {
            return url.ResourceUrl(controllerName, routeValues, ActionType.Retrieve);
        }

        /// <summary>
        /// Generates the route URL for the resource
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an Extension Method which allows the user to provide a strongly-typed argument via Expression"), SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Need to be sure the passed-in argument is of type Controller::Action")]
        public static string ResourceUrl(this UrlHelper url, string controllerName, object routeValues, ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.GetUpdateForm:
                    return url.RouteUrl(controllerName + "-editForm", routeValues);
                case ActionType.GetCreateForm:
                    return url.RouteUrl(controllerName + "-createForm");
                case ActionType.Retrieve:
                case ActionType.Delete:
                case ActionType.Update:
                    return url.RouteUrl(controllerName, routeValues);
                case ActionType.Create:
                    return url.RouteUrl(controllerName + "-create");
                case ActionType.Index:
                    return url.RouteUrl(controllerName + "-index");
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
        }
    }
}

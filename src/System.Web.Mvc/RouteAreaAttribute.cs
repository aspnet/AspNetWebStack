// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Defines the area to set for all the routes defined in this controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RouteAreaAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAreaAttribute" /> class.
        /// </summary>
        /// <remarks>An attempt will be made to infer the area name from the target controller's namespace.</remarks>
        public RouteAreaAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAreaAttribute" /> class.
        /// </summary>
        /// <param name="areaName">The name of the area. 
        /// If the value is null, an attempt will be made to infer the area name from the target controller's namespace.
        /// </param>
        public RouteAreaAttribute(string areaName)
        {
            AreaName = areaName;
        }

        /// <summary>
        /// The area name to set for all the routes defined in the controller.
        /// If the value is null, an attempt will be made to infer the area name from the target controller's namespace.
        /// </summary>
        public string AreaName { get; private set; }

        /// <summary>
        /// The URL prefix to apply to the routes of this area. Defaults to the area's name.
        /// </summary>
        public string AreaPrefix { get; set; }
    }
}
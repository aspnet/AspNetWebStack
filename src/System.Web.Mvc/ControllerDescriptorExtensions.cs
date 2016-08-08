// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    internal static class ControllerDescriptorExtensions
    {
        public static string GetAreaName(this ControllerDescriptor controllerDescriptor, RouteAreaAttribute area)
        {
            if (area == null)
            {
                return null;
            }

            if (area.AreaName != null)
            {
                return area.AreaName;
            }
            if (controllerDescriptor.ControllerType.Namespace != null)
            {
                return controllerDescriptor.ControllerType.Namespace.Split('.').Last();
            }

            throw Error.InvalidOperation(MvcResources.AttributeRouting_CouldNotInferAreaNameFromMissingNamespace, controllerDescriptor.ControllerName);
        }

        public static RouteAreaAttribute GetAreaFrom(this ControllerDescriptor controllerDescriptor)
        {
            RouteAreaAttribute areaAttribute =
                controllerDescriptor.GetCustomAttributes(typeof(RouteAreaAttribute), inherit: true)
                                    .Cast<RouteAreaAttribute>()
                                    .FirstOrDefault();
            return areaAttribute;
        }
    }
}
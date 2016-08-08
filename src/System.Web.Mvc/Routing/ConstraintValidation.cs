// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Validates that the constraints on a Route are of a type that can be processed by <see cref="System.Web.Routing.Route" />.
    /// </summary>
    /// <remarks>
    /// This validation is only applicable when the <see cref="System.Web.Routing.Route" /> is one that we created. A user-defined
    /// type that is derived from <see cref="System.Web.Routing.RouteBase" /> may have different semantics.
    /// 
    /// The logic here is duplicated from System.Web, but we need it to validate correctness of routes on startup. Since we can't 
    /// change System.Web, this just lives in a static class for MVC.
    /// </remarks>
    internal static class ConstraintValidation
    {
        public static void Validate(Route route)
        {
            Contract.Assert(route != null);
            Contract.Assert(route.Url != null);

            if (route.Constraints == null)
            {
                return;
            }

            foreach (var kvp in route.Constraints)
            {
                if (kvp.Value is string)
                {
                    continue;
                }

                if (kvp.Value is IRouteConstraint)
                {
                    continue;
                }

                throw Error.InvalidOperation(
                    MvcResources.Route_InvalidConstraint,
                    kvp.Key,
                    route.Url,
                    typeof(IRouteConstraint).FullName);
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
#if ASPNETWEBAPI
using System.Web.Http.Controllers;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// Defines a provider for routes that directly target action descriptors (attribute routes).
    /// </summary>
    public interface IDirectRouteProvider
    {
        /// <summary>Gets the direct routes for a controller.</summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="actionDescriptors">The action descriptors.</param>
        /// <param name="constraintResolver">The inline constraint resolver.</param>
        /// <returns>A set of route entries for the controller.</returns>
#if ASPNETWEBAPI
        IReadOnlyList<RouteEntry> GetDirectRoutes(
            HttpControllerDescriptor controllerDescriptor, 
            IReadOnlyList<HttpActionDescriptor> actionDescriptors,
            IInlineConstraintResolver constraintResolver);
#else
        IReadOnlyList<RouteEntry> GetDirectRoutes(
            ControllerDescriptor controllerDescriptor,
            IReadOnlyList<ActionDescriptor> actionDescriptors,
            IInlineConstraintResolver constraintResolver);
#endif
    }
}

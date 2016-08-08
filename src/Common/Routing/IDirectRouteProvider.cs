// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
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
        IReadOnlyList<RouteEntry> GetDirectRoutes(
            HttpControllerDescriptor controllerDescriptor, 
            IReadOnlyList<HttpActionDescriptor> actionDescriptors,
            IInlineConstraintResolver constraintResolver);
    }
}

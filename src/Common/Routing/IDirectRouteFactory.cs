// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// Defines a factory that creates a route directly to a set of action descriptors (an attribute route).
    /// </summary>
    public interface IDirectRouteFactory
    {
        /// <summary>Creates a direct route entry.</summary>
        /// <param name="context">The context to use to create the route.</param>
        /// <returns>The direct route entry.</returns>
        RouteEntry CreateRoute(DirectRouteFactoryContext context);
    }
}

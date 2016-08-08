// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#if ASPNETWEBAPI
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TRouteDictionary = System.Collections.Generic.IDictionary<string, object>;
#else
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Defines a builder that creates direct routes to actions (attribute routes).</summary>
    public interface IDirectRouteBuilder
    {
        /// <summary>Gets or sets the route name, if any; otherwise <see langword="null"/>.</summary>
        string Name { get; set; }

        /// <summary>Gets or sets the route template.</summary>
        /// <remarks>
        /// This value is the core route template that remains after resolving and removing any inline constraints.
        /// </remarks>
        string Template { get; set; }

        /// <summary>Gets or sets the route defaults.</summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Null and empty values are legitimate, separate options when constructing a route.")]
        TRouteDictionary Defaults { get; set; }

        /// <summary>Gets or sets the route constraints.</summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Null and empty values are legitimate, separate options when constructing a route.")]
        TRouteDictionary Constraints { get; set; }

        /// <summary>Gets or sets the route data tokens.</summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Null and empty values are legitimate, separate options when constructing a route.")]
        TRouteDictionary DataTokens { get; set; }

        /// <summary>Gets or sets the route order.</summary>
        /// <remarks>
        /// The route order disambiguates multiple matching routes and overrides precedence.
        /// The intended use of order is for an explicitly provided precedence override value.
        /// </remarks>
        int Order { get; set; }

        /// <summary>Gets or sets the route precedence.</summary>
        /// <remarks>
        /// The route order disambiguates multiple matching routes with the same order.
        /// The intended use of precedence is for default, automatically computed disambiguation based on inline
        /// constraint types.
        /// </remarks>
        decimal Precedence { get; set; }

        /// <summary>Gets the action descriptors to which to create a route.</summary>
        IReadOnlyCollection<TActionDescriptor> Actions { get; }

        /// <summary>
        /// Gets a value indicating whether the route is configured at the action or controller level.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> when the route is configured at the action level; otherwise <see langword="false"/>
        /// (if the route is configured at the controller level).
        /// </remarks>
        bool TargetIsAction { get; }

        /// <summary>Creates a route entry based on the current property values.</summary>
        /// <returns>The route entry created.</returns>
        RouteEntry Build();
    }
}

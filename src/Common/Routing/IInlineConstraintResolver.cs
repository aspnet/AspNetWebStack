// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNETWEBAPI
using TConstraint = System.Web.Http.Routing.IHttpRouteConstraint;
#else
using TConstraint = System.Web.Routing.IRouteConstraint;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// Defines an abstraction for resolving inline constraints as instances of <see cref="TConstraint"/>.
    /// </summary>
    public interface IInlineConstraintResolver
    {
        /// <summary>
        /// Resolves the inline constraint.
        /// </summary>
        /// <param name="inlineConstraint">The inline constraint to resolve.</param>
        /// <returns>The <see cref="TConstraint"/> the inline constraint was resolved to.</returns>
        TConstraint ResolveConstraint(string inlineConstraint);
    }
}
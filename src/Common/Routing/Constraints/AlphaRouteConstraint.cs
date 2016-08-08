// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing.Constraints
#else
namespace System.Web.Mvc.Routing.Constraints
#endif
{
    /// <summary>
    /// Constrains a route parameter to contain only lowercase or uppercase letters A through Z in the English alphabet.
    /// </summary>
    public class AlphaRouteConstraint : RegexRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaRouteConstraint" /> class.
        /// </summary>
        public AlphaRouteConstraint() : base(@"^[a-z]*$")
        {
        }
    }
}
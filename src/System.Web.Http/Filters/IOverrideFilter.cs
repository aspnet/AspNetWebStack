// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Filters
{
    /// <summary>Defines a filter that overrides other filters.</summary>
    public interface IOverrideFilter : IFilter
    {
        /// <summary>Gets the type of filters to override.</summary>
        /// <remarks>
        /// The following types of filters may be overridden:
        /// <list type="bullet">
        /// <item><description><see cref="IActionFilter"/></description></item>
        /// <item><description><see cref="IAuthenticationFilter"/></description></item>
        /// <item><description><see cref="IAuthorizationFilter"/></description></item>
        /// <item><description><see cref="IExceptionFilter"/></description></item>
        /// </list>
        /// </remarks>
        Type FiltersToOverride { get; }
    }
}

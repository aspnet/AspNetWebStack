// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Filters;

namespace System.Web.Mvc
{
    /// <summary>Represents a filter attribute that overrides action filters defined at a higher level.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class OverrideActionFiltersAttribute : FilterAttribute, IOverrideFilter
    {
        /// <inheritdoc />
        public Type FiltersToOverride
        {
            get { return typeof(IActionFilter); }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Filters;

namespace System.Web.Http
{
    /// <summary>
    /// Represents a filter attribute that overrides authentication filters defined at a higher level.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class OverrideAuthenticationAttribute : Attribute, IOverrideFilter
    {
        public bool AllowMultiple
        {
            get { return false; }
        }

        public Type FiltersToOverride
        {
            get { return typeof(IAuthenticationFilter); }
        }
    }
}

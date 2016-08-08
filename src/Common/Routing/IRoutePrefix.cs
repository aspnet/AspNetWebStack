// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Defines a route prefix.</summary>
    public interface IRoutePrefix
    {
        /// <summary>Gets the route prefix.</summary>
        string Prefix { get; }
    }
}
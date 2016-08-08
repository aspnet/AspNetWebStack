// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    /// <summary>Provides keys for looking up route values.</summary>
    internal static class RouteValueKeys
    {
        // Used to provide the action and controller name
        public const string Action = "action";
        public const string Controller = "controller";
    }
}
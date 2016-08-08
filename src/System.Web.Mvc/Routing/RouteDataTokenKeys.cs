// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc.Routing
{
    internal class RouteDataTokenKeys
    {
        public const string UseNamespaceFallback = "UseNamespaceFallback";
        public const string Namespaces = "Namespaces";
        public const string Area = "area";
        public const string Controller = "controller";

        // Used to provide the action descriptors to consider for attribute routing
        public const string Actions = "MS_DirectRouteActions";

        // Used to allow customer-provided disambiguation between multiple matching attribute routes
        public const string Order = "MS_DirectRouteOrder";

        // Used to prioritize routes to actions for link generation
        public const string TargetIsAction = "MS_DirectRouteTargetIsAction";

        // Used to allow URI constraint-based disambiguation between multiple matching attribute routes
        public const string Precedence = "MS_DirectRoutePrecedence";

        public const string DirectRouteMatches = "MS_DirectRouteMatches";
    }
}
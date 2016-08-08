// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Parameter class for URL matching in routing.
    /// </summary>
    internal class RoutingContext
    {
        private static readonly RoutingContext CachedInvalid = new RoutingContext() { IsValid = false };

        public static RoutingContext Invalid()
        {
            return CachedInvalid;
        }

        public static RoutingContext Valid(List<string> pathSegments)
        {
            return new RoutingContext()
            {
                PathSegments = pathSegments,
                IsValid = true,
            };
        }

        private RoutingContext()
        {
        }

        public bool IsValid
        {
            get;
            private set;
        }

        public List<string> PathSegments
        {
            get;
            private set;
        }
    }
}

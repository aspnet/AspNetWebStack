// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace System.Web.Mvc
{
    public sealed class RouteDataValueProvider : DictionaryValueProvider<object>
    {
        // RouteData should use the invariant culture since it's part of the URL, and the URL should be
        // interpreted in a uniform fashion regardless of the origin of a particular request.
        public RouteDataValueProvider(ControllerContext controllerContext)
            : base(controllerContext.RouteData.Values, CultureInfo.InvariantCulture)
        {
        }
    }
}

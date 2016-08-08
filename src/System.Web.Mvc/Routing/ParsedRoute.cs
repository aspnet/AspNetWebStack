// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Web.Mvc.Routing
{
    internal class ParsedRoute
    {
        public ParsedRoute(IList<PathSegment> pathSegments)
        {
            {
                Contract.Assert(pathSegments != null);
                PathSegments = pathSegments;
            }
        }

        public IList<PathSegment> PathSegments { get; private set; }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    // Represents a subsegment of a ContentPathSegment such as a parameter or a literal.
    internal abstract class PathSubsegment
    {
#if ROUTE_DEBUGGING
        public abstract string LiteralText
        {
            get;
        }
#endif
    }
}

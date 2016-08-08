// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    // Represents a parameter subsegment of a ContentPathSegment
    internal sealed class PathParameterSubsegment : PathSubsegment
    {
        public PathParameterSubsegment(string parameterName)
        {
            if (parameterName.StartsWith("*", StringComparison.Ordinal))
            {
                ParameterName = parameterName.Substring(1);
                IsCatchAll = true;
            }
            else
            {
                ParameterName = parameterName;
            }
        }

        public bool IsCatchAll { get; private set; }

        public string ParameterName { get; private set; }

#if ROUTE_DEBUGGING
        public override string LiteralText
        {
            get
            {
                return "{" + (IsCatchAll ? "*" : String.Empty) + ParameterName + "}";
            }
        }

        public override string ToString()
        {
            return "{" + (IsCatchAll ? "*" : String.Empty) + ParameterName + "}";
        }
#endif
    }
}

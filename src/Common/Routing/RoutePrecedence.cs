// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

#if ASPNETWEBAPI
using TParsedRoute = System.Web.Http.Routing.HttpParsedRoute;
#else
using TParsedRoute = System.Web.Mvc.Routing.ParsedRoute;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    internal static class RoutePrecedence
    {
        // Segments have the following order:
        // 1 - Literal segments
        // 2 - Constrained parameter segments / Multi-part segments
        // 3 - Unconstrained parameter segments
        // 4 - Constrained wildcard parameter segments
        // 5 - Unconstrained wildcard parameter segments
        internal static int ComputeDigit(PathContentSegment segment, IDictionary<string, object> constraints)
        {
            if (segment.Subsegments.Count > 1)
            {
                // Multi-part segments should appear after literal segments but before parameter segments
                return 2;
            }

            PathSubsegment subsegment = segment.Subsegments[0];
            // Literal segments always go first
            if (subsegment is PathLiteralSubsegment)
            {
                return 1;
            }
            else
            {
                PathParameterSubsegment parameterSegment = subsegment as PathParameterSubsegment;
                Contract.Assert(parameterSegment != null);
                int order = parameterSegment.IsCatchAll ? 5 : 3;

                // If there is a route constraint for the parameter, reduce order by 1
                // Constrained parameters end up with order 2, Constrained catch alls end up with order 4
                if (constraints != null && constraints.ContainsKey(parameterSegment.ParameterName))
                {
                    order--;
                }

                return order;
            }
        }

        public static decimal Compute(TParsedRoute parsedRoute, IDictionary<string, object> constraints)
        {
            // Each precedence digit corresponds to one decimal place. For example, 3 segments with precedences 2, 1,
            // and 4 results in a combined precedence of 2.14 (decimal).
            IList<PathContentSegment> segments = parsedRoute.PathSegments.OfType<PathContentSegment>().ToArray();

            decimal precedence = 0;
            uint divisor = 1; // The first digit occupies the one's place.

            for (int i = 0; i < segments.Count; i++)
            {
                PathContentSegment segment = segments[i];

                int digit = ComputeDigit(segment, constraints);
                Contract.Assert(digit >= 0 && digit < 10);

                precedence = precedence + Decimal.Divide(digit, divisor);

                // The next digit occupies the subsequent place (always after the decimal point and growing to the
                // right).
                divisor *= 10;
            }

            return precedence;
        }
    }
}

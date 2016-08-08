// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel;

namespace System.Web.Http.SelfHost.ServiceModel
{
    internal static class HostNameComparisonModeHelper
    {
        public static bool IsDefined(HostNameComparisonMode value)
        {
            return
                value == HostNameComparisonMode.StrongWildcard
                || value == HostNameComparisonMode.Exact
                || value == HostNameComparisonMode.WeakWildcard;
        }

        public static void Validate(HostNameComparisonMode value, string parameterName)
        {
            if (!IsDefined(value))
            {
                throw Error.InvalidEnumArgument(parameterName, (int)value, typeof(HostNameComparisonMode));
            }
        }
    }
}

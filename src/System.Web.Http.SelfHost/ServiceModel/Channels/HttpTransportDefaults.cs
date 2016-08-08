// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal static class HttpTransportDefaults
    {
        internal const HostNameComparisonMode HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        internal const TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;
    }
}

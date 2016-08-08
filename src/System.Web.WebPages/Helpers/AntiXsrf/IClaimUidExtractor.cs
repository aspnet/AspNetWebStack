// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Helpers.AntiXsrf
{
    // Can extract unique identifers for a claims-based identity
    internal interface IClaimUidExtractor
    {
        BinaryBlob ExtractClaimUid(IIdentity identity);
    }
}

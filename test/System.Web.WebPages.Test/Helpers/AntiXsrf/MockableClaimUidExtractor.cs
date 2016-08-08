// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Helpers.AntiXsrf.Test
{
    // An IClaimUidExtractor that can be passed to MoQ
    public abstract class MockableClaimUidExtractor : IClaimUidExtractor
    {
        public abstract object ExtractClaimUid(IIdentity identity);

        BinaryBlob IClaimUidExtractor.ExtractClaimUid(IIdentity identity)
        {
            return (BinaryBlob)ExtractClaimUid(identity);
        }
    }
}

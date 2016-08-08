// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf.Test
{
    // An ICryptoSystem that can be passed to MoQ
    public abstract class MockableCryptoSystem : ICryptoSystem
    {
        public abstract string Protect(byte[] data);
        public abstract byte[] Unprotect(string protectedData);
    }
}

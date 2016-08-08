// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf
{
    // Provides an abstraction around the cryptographic subsystem for the anti-XSRF helpers.
    internal interface ICryptoSystem
    {
        string Protect(byte[] data);
        byte[] Unprotect(string protectedData);
    }
}

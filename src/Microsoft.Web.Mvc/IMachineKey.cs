// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Security;

namespace Microsoft.Web.Mvc
{
    // Used for mocking out the static MachineKey type

    internal interface IMachineKey
    {
        byte[] Unprotect(string protectedData, params string[] purposes);
        string Protect(byte[] userData, params string[] purposes);
    }
}

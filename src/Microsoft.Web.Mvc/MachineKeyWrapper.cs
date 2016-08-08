// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;
using System.Web.Security;

namespace Microsoft.Web.Mvc
{
    // Concrete implementation of IMachineKey that talks to the static MachineKey type

    internal sealed class MachineKeyWrapper : IMachineKey
    {
        private static readonly MachineKeyWrapper _singletonInstance = new MachineKeyWrapper();

        public static MachineKeyWrapper Instance
        {
            get
            {
                return _singletonInstance;
            }
        }

        public byte[] Unprotect(string protectedData, params string[] purposes)
        {
            byte[] protectedBytes = Convert.FromBase64String(protectedData);
            return MachineKey.Unprotect(protectedBytes, purposes);
        }

        public string Protect(byte[] userData, params string[] purposes)
        {
            byte[] protectedBytes = MachineKey.Protect(userData, purposes);
            return Convert.ToBase64String(protectedBytes);
        }
    }
}

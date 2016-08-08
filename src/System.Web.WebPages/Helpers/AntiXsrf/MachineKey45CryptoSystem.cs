// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Security;

namespace System.Web.Helpers.AntiXsrf
{
    // Interfaces with the System.Web.MachineKey static class using the 4.5 Protect / Unprotect methods.
    internal sealed class MachineKey45CryptoSystem : ICryptoSystem
    {
        private static readonly string[] _purposes = new string[] { "System.Web.Helpers.AntiXsrf.AntiForgeryToken.v1" };
        private static readonly MachineKey45CryptoSystem _singletonInstance = GetSingletonInstance();

        public static MachineKey45CryptoSystem Instance
        {
            get
            {
                return _singletonInstance;
            }
        }

        private static MachineKey45CryptoSystem GetSingletonInstance()
        {
            return new MachineKey45CryptoSystem();
        }

        public string Protect(byte[] data)
        {
            byte[] rawProtectedBytes = MachineKey.Protect(data, _purposes);
            return HttpServerUtility.UrlTokenEncode(rawProtectedBytes);
        }

        public byte[] Unprotect(string protectedData)
        {
            byte[] rawProtectedBytes = HttpServerUtility.UrlTokenDecode(protectedData);
            return MachineKey.Unprotect(rawProtectedBytes, _purposes);
        }
    }
}

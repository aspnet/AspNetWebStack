﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// Used to retrieve the currently running platform.
    /// </summary>
    public static class PlatformInfo
    {
        private const string _net45TypeName = "System.IWellKnownStringEqualityComparer, mscorlib, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089";
        private const string _netCore20TypeName = "System.OrdinalCaseSensitiveComparer, system.private.corelib, Version=4.0.0.0, PublicKeyToken=7cec85d7bea7798e";
        private static Lazy<Platform> _platform = new Lazy<Platform>(GetPlatform, isThreadSafe: true);

        /// <summary>
        /// Gets the platform that the unit test is currently running on.
        /// </summary>
        public static Platform Platform
        {
            get { return _platform.Value; }
        }

        private static Platform GetPlatform()
        {
            if (Type.GetType(_netCore20TypeName, throwOnError: false) != null)
            {
                // Treat .NET Core 2.1 as a .NET 4.5 superset though internal types are different.
                return Platform.Net45;
            }

            if (Type.GetType(_net45TypeName, throwOnError: false) != null)
            {
                return Platform.Net45;
            }

            return Platform.Net40;
        }
    }
}

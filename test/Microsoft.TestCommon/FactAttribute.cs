// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// An override of <see cref="Xunit.FactAttribute"/> that provides extended capabilities.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Microsoft.TestCommon.FactDiscoverer", "Microsoft.TestCommon")]
    public class FactAttribute : Xunit.FactAttribute
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="FactAttribute"/>.
        /// </summary>
        public FactAttribute()
        {
            Platforms = Platform.All;
            PlatformJustification = "Unsupported platform (test runs on {0}, current platform is {1})";
        }

        /// <summary>
        /// Gets or set the platforms that the unit test is compatible with. Defaults to
        /// <see cref="Platform.All"/>.
        /// </summary>
        public Platform Platforms { get; set; }

        /// <summary>
        /// Gets or sets the platform skipping justification. This message can receive
        /// the supported platforms as {0}, and the current platform as {1}.
        /// </summary>
        public string PlatformJustification { get; set; }
    }
}
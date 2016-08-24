// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// An override of <see cref="Xunit.TheoryAttribute"/> that provides extended capabilities.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Microsoft.TestCommon.TheoryDiscoverer", "Microsoft.TestCommon")]
    public class TheoryAttribute : Xunit.TheoryAttribute
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="TheoryAttribute"/>.
        /// </summary>
        public TheoryAttribute()
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
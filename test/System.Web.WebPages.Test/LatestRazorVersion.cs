// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public static class LatestRazorVersion
    {
        private static readonly Version LatestVersion = VersionTestHelper.GetVersionFromAssembly("System.Web.Razor", typeof(ParserResults));

        public static readonly string MajorMinor = LatestVersion.Major + "." + LatestVersion.Minor;
    }
}

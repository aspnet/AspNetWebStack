// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Deployment.Test
{
    public static class LatestRazorVersion
    {
        public static readonly Version LatestVersion = VersionTestHelper.GetVersionFromAssembly("System.Web.Razor", typeof(ParserResults));
    }
}

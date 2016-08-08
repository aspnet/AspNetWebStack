// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.WebPages.Deployment.Test
{
    internal static class DeploymentUtil
    {
        public static string GetBinDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Path.Combine(tempDirectory, "bin");
        }
    }
}

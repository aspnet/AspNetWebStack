// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web;

namespace Microsoft.Web.Helpers
{
    internal sealed class VirtualPathUtilityWrapper : VirtualPathUtilityBase
    {
        public override string Combine(string basePath, string relativePath)
        {
            return VirtualPathUtility.Combine(basePath, relativePath);
        }

        public override string ToAbsolute(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }
    }
}

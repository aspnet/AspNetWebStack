// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web;

namespace Microsoft.Internal.Web.Utils
{
    internal sealed class VirtualPathUtilityWrapper : IVirtualPathUtility
    {
        public string Combine(string basePath, string relativePath)
        {
            return VirtualPathUtility.Combine(basePath, relativePath);
        }

        public string ToAbsolute(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }
    }
}

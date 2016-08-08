// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Internal.Web.Utils
{
    internal interface IVirtualPathUtility
    {
        string Combine(string basePath, string relativePath);

        string ToAbsolute(string virtualPath);
    }
}

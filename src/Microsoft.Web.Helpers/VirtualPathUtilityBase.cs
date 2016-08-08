// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Web.Helpers
{
    public abstract class VirtualPathUtilityBase
    {
        public abstract string Combine(string basePath, string relativePath);

        public abstract string ToAbsolute(string virtualPath);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    internal sealed class NullViewLocationCache : IViewLocationCache
    {
        #region IViewLocationCache Members

        public string GetViewLocation(HttpContextBase httpContext, string key)
        {
            return null;
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath)
        {
        }

        #endregion
    }
}

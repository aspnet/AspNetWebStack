// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public interface IViewLocationCache
    {
        string GetViewLocation(HttpContextBase httpContext, string key);
        void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath);
    }
}
